using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls which chunks need to be displayed along with their properties (LOD)
/// </summary>
public class EndlessTerrain : MonoBehaviour
{
	// the distance from the player to the next chunk before the chunk updates
	const float distanceThresholdForChunkUpdate = 25f;
	const float sqrDistanceThresholdForChunkUpdate = distanceThresholdForChunkUpdate * distanceThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDst;
	int chunkSize;
	int visibleChunks;

	public Material mapMaterial;
	public Transform viewer;
	public static Vector2 viewerPosition;
	private Vector2 oldViewerPosition;
	static MapGenerator mapGenerator;
	
	// a new dictionary that stores terrain chunks in a vector array
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunksvisibleLastUpdate = new List<TerrainChunk>();

	/// <summary>
	/// Runs at the start of program execution
	/// </summary>
	void Start()
	{
		// gets the mapGenerator game object and sets the size of terrain chunks
		mapGenerator = FindObjectOfType<MapGenerator>();
		chunkSize = mapGenerator.mapChunkSize - 1;

		// sets the maximum view distance to the visible distance value of the last "level of detail" struct in the array (will be furthest away)
		maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

		// calculates number of terrain blocks around the viewer that should be generated based on the view distance and size of chunks
		visibleChunks = Mathf.RoundToInt(maxViewDst / chunkSize);

		UpdateVisibleChunks();
	}

	/// <summary>
	/// Runs once each frame
	/// </summary>
	private void Update()
	{
		// updates the position of the viewer every frame based on the scale of the map
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

		// only updates the visible chunkes when the viewer moves a specified distance
		if ((oldViewerPosition - viewerPosition).sqrMagnitude > sqrDistanceThresholdForChunkUpdate)
		{
			oldViewerPosition = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	/// <summary>
	/// Updates the chunks that are visible depending of the players position
	/// </summary>
	void UpdateVisibleChunks()
	{
		// cycles through the hold chunks that are still active and disables them
		for (int i = 0; i < terrainChunksvisibleLastUpdate.Count; i++)
		{
			terrainChunksvisibleLastUpdate[i].SetVisible(false);
		}

		// makes sure the list is empty
		terrainChunksvisibleLastUpdate.Clear();

		// calculates the coordinates of the terrain chunk that the player is currently on
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		// loops through all of the chunks surrounding the player
		for (int yOffset = -visibleChunks; yOffset <= visibleChunks; yOffset++)
		{
			for (int xOffset = -visibleChunks; xOffset <= visibleChunks; xOffset++)
			{
				// stores the x and y value of the chunk that was viewed
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				// checks if the chunk is already viewed and adds it to the dictionary if it is not already viewed
				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					// gets the viewed coordinate and updates the chunk
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
				} else
				{
					// creates a new chunk and adds the chunk to the dictionary if it wasn't there already
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, this.transform, mapMaterial));
				}
			}
		}
	}

	/// <summary>
	/// Contains all information relevant to a terrainChunk
	/// </summary>
	public class TerrainChunk
	{
		GameObject meshObject;
		Vector2 position;
		Bounds bounds; // a bounding box
		HeightMap mapData;
		bool mapDataRecieved;
		int previousLODIndex = -1;

		// mesh Renderer, Filter and Collider of the terrain chunk
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		MeshCollider meshCollider;

		// LOD detail related components
		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		LODMesh collisionLODMesh;

		/// <summary>
		/// Creates required components of the terrain chunk
		/// </summary>
		/// <param name="coord"> x and y coordinate of the terrain chunk </param>
		/// <param name="size"> Size of the terrain chunk </param>
		/// <param name="detailLevels"> Level of Detail of the terrain chunk </param>
		/// <param name="parent"> Instantiates each terrain chunk as a child to this game object </param>
		/// <param name="material"> Material used for the terrain chunk </param>
		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
		{
			this.detailLevels = detailLevels;
			position = coord * size;

			// converts the 2D position of the chunk into 3D coordinates
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			// creates a bounding box with the given position and size
			bounds = new Bounds(position, Vector2.one * size);

			// creates an terrain chunck game object and sets its position and scale
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = material;
			meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
			SetVisible(false);

			// creates an array big enough to fit the amount of "lod" levels that are specified in the Unity Inspector
			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++)
			{
				// fills the array of level of detail meshes with the level of detail values
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);

				// sets the collisison mesh to the specified level of detail mesh (for simpler collider calculations)
				if (detailLevels[i].useForCollider)
				{
					collisionLODMesh = lodMeshes[i];
				}
			}
			// sends a request to get the map and mesh data
			mapGenerator.RequestMapData(position, OnMapDataRecieved);
		}

		/// <summary>
		/// Sends a request to get the mesh data
		/// </summary>
		/// <param name="mapData"> map data of HeightMap type containing height map </param>
		void OnMapDataRecieved(HeightMap mapData)
		{
			this.mapData = mapData;
			mapDataRecieved = true;

			// updates the terrain chunks when they get a new map
			UpdateTerrainChunk();
		}

		/// <summary>
		/// Enables or disables the meshObject depending on its distance from the viewers position
		/// </summary>
		public void UpdateTerrainChunk()
		{
			if (mapDataRecieved)
			{
				// gets the closest distance from the viewer and the edge of the bounding box (chunk)
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

				// calculates if the chunk should be visible to the player or not
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible)
				{
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++)
					{
						// if the view distance to the closest edge is greater than the visible threshold, then increase the lod value (decreases detail)
						if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
						{
							lodIndex = i + 1;
						} else
						{
							break;
						}
					}

					if (lodIndex != previousLODIndex)
					{
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh)
						{
							// update the mesh of the chunk with the new mesh of the determined lodIndex
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						} else if (!lodMesh.hasRequestedMesh)
						{
							// get a mesh if there is no lodMesh
							lodMesh.RequestMesh(mapData);
						}
					}

					// if the chunk is close to the player than generate a collision mesh
					if (lodIndex == 0)
					{
						if (collisionLODMesh.hasMesh)
						{
							meshCollider.sharedMesh = collisionLODMesh.mesh;
						} else if (!collisionLODMesh.hasRequestedMesh)
						{
							collisionLODMesh.RequestMesh(mapData); 
						}
						
					}

					// adds this terrain chunk to the list of previously updated terrain chunks
					terrainChunksvisibleLastUpdate.Add(this);
				}
				SetVisible(visible);
			}
		}

		/// <summary>
		/// Sets the visibility of the chunk based on the visible parameter
		/// </summary>
		/// <param name="visible"> Takes a boolean determining the visibility of the mesh </param>
		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}

		/// <summary>
		/// Checks if meshObject is visible or not
		/// </summary>
		/// <returns> Returns a boolean whether the object is visible or not </returns>
		public bool IsVisible()
		{
			return meshObject.activeSelf;
		}
	}

	/// <summary>
	/// Mesh based on the LOD value
	/// </summary>
	class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;

		// stores a method to update terrain chunks
		System.Action updateCallback;

		/// <summary>
		/// Initializes the LODMesh with level of detail value
		/// </summary>
		/// <param name="lod"> Level of Detail of the mesh as Integer type </param>
		public LODMesh(int lod, System.Action updateCallback)
		{
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		/// <summary>
		/// Sends a request to MapGenerator class to create a new mesh using the given mapData parameter
		/// </summary>
		/// <param name="mapData"> HeightMap type struct containing map properties </param>
		public void RequestMesh(HeightMap mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
		}

		/// <summary>
		/// Creates a mesh using given meshData struct
		/// </summary>
		/// <param name="meshData"> MeshData type struct containing mesh properties </param>
		void OnMeshDataRecieved(MeshData meshData)
		{
			// creates the new mesh
			mesh = meshData.CreateMesh();
			hasMesh = true;

			// once the chunk has a mesh call the terrain chunk update method to update the chunks
			updateCallback();
		}
	}

	/// <summary>
	/// Struct containing the LOD related mesh info
	/// </summary>
	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDstThreshold; // distance at which the LOD value changes
		public bool useForCollider; 
	}
}
