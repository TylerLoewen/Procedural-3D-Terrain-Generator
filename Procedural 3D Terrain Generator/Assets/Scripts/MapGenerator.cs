using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class MapGenerator : MonoBehaviour
{
	// Allows toggling of drawing options in the inspector
	public enum DrawMode {NoiseMap, Mesh, FalloffMap};

	float[,] falloffMap;

	public TerrainData terrainData;
	public NoiseData noiseData;
	public TextureData textureData;
	public Material terrainMaterial;

	// Queues used to store structs holding information that is used in a thread
	Queue<MapThreadInfo<HeightMap>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	// EDITOR OPTIONS ///
	public DrawMode drawMode;

	[Range(0,6)]
	public int editorPreviewLOD;

	public bool autoUpdate;
	// END EDITOR OPTIONS ///

	/// <summary>
	/// Runs once after all objects are initialized
	/// </summary>
	void Awake()
	{
		// applies the terrain material to the texture
		textureData.ApplyToMaterial(terrainMaterial);

		// updates the terrain height for the shader to use in its texture
		textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
	}

	/// <summary>
	/// Update method that is called every frame
	/// </summary>
	void Update()
	{
		// empties the queueu of mapData structs
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				// removes the MapThreadInfo struct from the queue and sets it to threadInfo
				MapThreadInfo<HeightMap> threadInfo = mapDataThreadInfoQueue.Dequeue();

				// calls the internal method along with its parameter which it gets from within the threadInfo struct as well
				threadInfo.callback(threadInfo.parameter);
			}
		}

		// empties the queueu full of meshData structs
		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				// removes the MapThreadInfo struct from the queue and sets it to threadInfo
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();

				// calls the internal method along with its parameter which it gets from within the threadInfo struct as well
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	/// <summary>
	/// Re-Draws the map in the editor
	/// </summary>
	void OnValuesUpdated()
	{
		if (!Application.isPlaying)
		{
			DrawMapInEditor();
		}
	}

	/// <summary>
	/// Applies the terrain material to the texture(shader)
	/// </summary>
	void OnTextureValuesUpdated()
	{
		textureData.ApplyToMaterial(terrainMaterial);
	}

	/// <summary>
	/// Gets the map chunk size depending on if flatshading is on
	/// </summary>
	public int mapChunkSize
	{
		get
		{
			if (terrainData.useFlatShading)
			{
				return 95;
			} else
			{
				return 239;
			}
		}
	}

	/// <summary>
	/// Generates noise and colour map and draws the specified map in the editor
	/// </summary>
	public void DrawMapInEditor()
	{
		// updates the mesh height for the shader
		textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

		HeightMap mapData = GenerateMapData(Vector2.zero);
		MapDisplay display = FindObjectOfType<MapDisplay>();

		// Draws the specified map depending on the state of the DrawMode enum
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		} else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
		} else if (drawMode == DrawMode.FalloffMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffMapGenerator.GenerateFalloffMap(mapChunkSize)));
		}
	}

	/// <summary>
	/// Starts a new thread for updating HeightMap
	/// </summary>
	/// <param name="center"> Center of the map as a Vector2 </param>
	/// <param name="callback"> Delegate that contains a method to update map data </param>
	public void RequestMapData(Vector2 center, Action<HeightMap> callback)
	{
		// initializes a new thread
		ThreadStart threadStart = delegate
		{
			// requests map data generation
			MapDataThread(center, callback);
		};

		// starts the thread
		new Thread(threadStart).Start();
	}

	/// <summary>
	/// Generates the mapData and adds it and the callback method to a struct, and then a queue
	/// </summary>
	/// <param name="callback"> Delegate that contains a method to update map data </param>
	void MapDataThread(Vector2 center, Action<HeightMap> callback)
	{
		// generates a noise map within this thread
		HeightMap mapData = GenerateMapData(center);

		// prevents mapDataThreadInfoQueue from being called from anywhere outside this thread
		lock (mapDataThreadInfoQueue)
		{
			// adds the mapData struct and the callback method to a queue
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, mapData));
		}
	}

	/// <summary>
	/// Starts a new thread for creating MeshData
	/// </summary>
	/// <param name="mapData"> HeightMap used to generat mesh </param>
	/// <param name="lod"> Specified LOD of the mesh </param>
	/// <param name="callback"> Delegate that contains a method to create a new mesh </param>
	public void RequestMeshData(HeightMap mapData, int lod, Action<MeshData> callback)
	{
		// initializes a new thread
		ThreadStart threadStart = delegate
		{
			// generates a new mesh on this thread
			MeshDataThread(mapData, lod, callback);
		};

		// starts the thread
		new Thread(threadStart).Start();
	}

	/// <summary>
	/// Generates the meshData and adds it and the callback method to a struct, and then a queue
	/// </summary>
	/// <param name="mapData"> Map data of type HeightMap which is used for its heightmap value </param>
	/// <param name="callback"> Delegate that contains a method to create a new mesh </param>
	void MeshDataThread(HeightMap mapData, int lod, Action<MeshData> callback)
	{
		// generates a mesh within this thread
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);

		// prevents mapDataThreadInfoQueue from being called from anywhere outside this thread
		lock (mapDataThreadInfoQueue)
		{
			// adds the mapData struct and the callback method to a queue
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	/// <summary>
	/// Generates a noise map
	/// </summary>
	/// <param name="center"> The center of the chunk </param>
	/// <returns> Struct of HeightMap type containing a noise map </returns>
	HeightMap GenerateMapData(Vector2 center)
	{
		// generates the noise map
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

		// if the falloff map option is on generate one
		if (terrainData.useFalloff)
		{
			if (falloffMap == null)
			{
				falloffMap = FallOffMapGenerator.GenerateFalloffMap(mapChunkSize + 2);
			}

			// modifies the existing noise map to account for the falloff map values
			for (int y = 0; y < mapChunkSize + 2; y++)
			{
				for (int x = 0; x < mapChunkSize + 2; x++)
				{
					// subtracts the falloffmaps values from the noise maps values which makes the values near the edge of the chunk smaller than the center values
					noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x,y]);
				}
			}
		}
		return new HeightMap(noiseMap);
	}


	/// <summary>
	/// Subscribes and unsubscribes to the respctive events
	/// </summary>
	private void OnValidate()
	{
		if (terrainData != null)
		{
			// keeps the subscrption count from going above 1
			terrainData.OnValuesUpdated -= OnValuesUpdated;
			// subscribes to OnValuesUpdated event
			terrainData.OnValuesUpdated += OnValuesUpdated;
		}
		if (noiseData != null)
		{
			// keeps the subscrption count from going above 1
			noiseData.OnValuesUpdated -= OnValuesUpdated;
			// subscribes to OnValuesUpdated event
			noiseData.OnValuesUpdated += OnValuesUpdated;
		}

		if (textureData != null)
		{
			// keeps the subscrption count from going above 1
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			// subscribes to OnValuesUpdated event
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
	}

	/// <summary>
	/// Struct containing map information that will be needed in a thread
	/// </summary>
	/// <typeparam name="T"> Generic Struct </typeparam>
	struct MapThreadInfo<T>
	{
		// readonly so they don't change while they are in the thread
		public readonly Action<T> callback;
		public readonly T parameter;

		/// <summary>
		/// Initializes the MapThreadInfo struct with a callbck and a paramater
		/// </summary>
		/// <param name="callback"> Generic delegate used for callback </param>
		/// <param name="parameter"> Generic parameter, either meshData class or mapData struct </param>
		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}

/// <summary>
/// Contains the height map
/// </summary>
public struct HeightMap
{
	public readonly float[,] heightMap;

	/// <summary>
	/// Initializes the HeightMap struct with a given heightmap
	/// </summary>
	/// <param name="heightMap"> 2D float array containing height values of the map </param>
	public HeightMap(float[,] heightMap)
	{
		this.heightMap = heightMap;
	}
}
