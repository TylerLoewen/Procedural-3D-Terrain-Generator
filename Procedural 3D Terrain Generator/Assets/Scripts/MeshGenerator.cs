using UnityEngine;

/// <summary>
/// Generates a mesh for a terrain chunk
/// </summary>
public static class MeshGenerator
{
	/// <summary>
	/// Generates a mesh for one chunk (Note: Runs on seperate thread)
	/// </summary>
	/// <param name="heightMap"> Map containing all height data </param>
	/// <param name="heightMultiplier"> Multiplier that modified the y scale of all vertices </param>
	/// <param name="heightCurve"> Curve used for modified height values at a specific range </param>
	/// <param name="levelOfDetail"> The LOD to be used for the mesh </param>
	/// <param name="useFlatShading"> Determines whether flat shading is used or not </param>
	/// <returns> Returns a MeshData class containing all of its mesh data </returns>
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading)
	{
		// creates a new heightCurve variable for each thread so that they do not interfere with eachother
		AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

		// scales the amount to iterate through the array of vertices wich increases/decreases vertex count of the mesh
		// level of detail multiplied by 2 to ensure it is always an even number so the traversal of vertex array is never incomplete
		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

		// borderSize is used to create a border of vertices surounding the mesh vertices so the normals can be properly calculated at the vertices at the edge of the mesh (i.e. edge of chunk)
		int borderedSize = heightMap.GetLength(0);
		int meshSize = borderedSize - 2 * meshSimplificationIncrement; // scales the map up based on how simplified the LOD is so its the correct scale
		int meshSizeUnsimplified = borderedSize - 2; // the "correct" value of the size used for vertex and other calculations

		// ensures that the points are centered on the height map using the mesh width and height
		float topLeftX = (meshSizeUnsimplified - 1) / -2f;
		float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

		// levelOfDetail can't be 0 because it will cause  infinite for loops
		// simplification increment determines the amount of vertices to skip over when creating a mesh
		// higher the simplification increment the less vertices the mesh will contain (1 = all vertices, 2 ~ half the amount of vertices etc.)
		int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

		// verticesPerline can change depending on the level of detail of the mesh
		MeshData meshData = new MeshData(verticesPerLine, useFlatShading);

		// keeps track of what incides the mesh has at each vertex
		int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
		int meshVertexIndex = 0;
		int borderVertexIndex = -1;

		// fills the vertex indices map with negative or positive indices depending on if the current point is in the border (-) or the mesh (+)
		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
			{
				// checks to see if the loop is currently iterating through the normal mesh or the mesh border
				bool isBorderVertex = (y == 0) || (y == borderedSize - 1) || (x == 0) || (x == borderedSize - 1);

				// adds the loops current index to ther vertexIndicesMap array
				if (isBorderVertex)
				{
					vertexIndicesMap[x, y] = borderVertexIndex;
					borderVertexIndex--;
				} else
				{
					vertexIndicesMap[x, y] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}

		// iterates through each point in the height map
		// meshSimplificationIncrement controls if and how many vertices the loop skips over, changing the LOD
		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
			{
				int vertexIndex = vertexIndicesMap[x, y];
				
				Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);

				float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

				// calculates the x, y and z coordinates of each vertex using the height map
				Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

				meshData.AddVertex(vertexPosition, percent, vertexIndex);

				// do not traverse the last row or column in the mesh because there are no triangles to add from those indices
				if (x < borderedSize-1 && y < borderedSize -1)
				{
					// points of two adjacent triangles
					int a = vertexIndicesMap[x, y];
					int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
					int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
					int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

					// adds triangles to the whole mesh
					meshData.AddTriangle(a, d, c);
					meshData.AddTriangle(d, a, b);
				}
				vertexIndex++;
			}
		}
		// calculates normals
		meshData.ProcessMesh();
		return meshData;
	}
}

/// <summary>
/// Holds data related to a mesh
/// </summary>
public class MeshData
{
	Vector3[] vertices;
	Vector2[] uvs;
	Vector3[] bakedNormals;
	int[] triangles;
	int triangleIndex;

	Vector3[] borderVertices;
	int[] borderTriangles;
	int borderTriangleIndex;

	bool useFlatShading;

	/// <summary>
	/// Initializes the data needed for the mesh
	/// </summary>
	/// <param name="verticesPerLine"> The amount of vertices in one line on the mesh (less if higher LOD value) </param>
	/// <param name="useFlatShading"> Determines if flat shading is used </param>
	public MeshData(int verticesPerLine, bool useFlatShading)
	{
		this.useFlatShading = useFlatShading;

		// calculates amount of the vertices, triangles and uvs arrays in the mesh
		vertices = new Vector3[verticesPerLine * verticesPerLine];
		uvs = new Vector2[verticesPerLine * verticesPerLine];
		triangles = new int[(verticesPerLine-1) * (verticesPerLine-1) * 6];

		// calculates amount of vertices and triangles on the border mesh (4 edges + 4 corners)
		borderVertices = new Vector3[verticesPerLine * 4 + 4];
		borderTriangles = new int[24 * verticesPerLine];
	}

	/// <summary>
	/// Adds the provided vertices to either the border or normal vertices array
	/// </summary>
	/// <param name="vertexPosition"> The position of the vertex </param>
	/// <param name="uv"> The uv value of the vertex </param>
	/// <param name="vertexIndex"> The index of the vertex </param>
	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
	{
		// if the vertex is a border vertex or mesh vertex (<0 border, >0 mesh)
		if (vertexIndex < 0)
		{
			// vertexIndex must be multiplied by - and subtracted by 1 because it is a negative value that starts at -1 by default
			borderVertices[-vertexIndex - 1] = vertexPosition;
		} else
		{
			vertices[vertexIndex] = vertexPosition;
			uvs[vertexIndex] = uv;
		}
	}

	/// <summary>
	/// Adds a new triangle to either the triangles or borderTriangles array
	/// </summary>
	/// <param name="a"> First point of a triangle </param>
	/// <param name="b"> Second point of a triangle </param>
	/// <param name="c"> Third point of a triangle </param>
	public void AddTriangle(int a, int b, int c)
	{
		// if points are less than 0 it is a border tirangle
		if (a < 0 || b < 0 || c < 0)
		{
			borderTriangles[borderTriangleIndex] = a;
			borderTriangles[borderTriangleIndex + 1] = b;
			borderTriangles[borderTriangleIndex + 2] = c;
			borderTriangleIndex += 3;
		} else
		{
			triangles[triangleIndex] = a;
			triangles[triangleIndex + 1] = b;
			triangles[triangleIndex + 2] = c;
			triangleIndex += 3;
		}
	}

	/// <summary>
	/// Calculates the normal vectors of every triangle
	/// </summary>
	/// <returns> Returns a Vector3 array containing all of the calculated normals of the triangles </returns>
	Vector3[] CalculateNormals()
	{
		Vector3[] vertexNormals = new Vector3[vertices.Length];

		// 3 points make up one triangle
		int triangleCount = triangles.Length / 3;

		// calculates the normals for all mesh triangles
		for (int i = 0; i < triangleCount; i++)
		{
			int normalTriangleIndex = i * 3;

			// gets the index in the triangles array of each 3 vertices of one triangle
			int vertexIndexA = triangles[normalTriangleIndex];
			int vertexIndexB = triangles[normalTriangleIndex + 1];
			int vertexIndexC = triangles[normalTriangleIndex + 2];

			// calculates the normal vector of the current triangle in the for loop iteration
			Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

			// adds the triangles normal vector to each vertex of the current triangle in the for loop iteration
			vertexNormals[vertexIndexA] += triangleNormal;
			vertexNormals[vertexIndexB] += triangleNormal;
			vertexNormals[vertexIndexC] += triangleNormal;
		}

		int borderTriangleCount = borderTriangles.Length / 3;

		// calculates the normals for all border mesh triangles
		for (int i = 0; i < borderTriangleCount; i++)
		{
			int normalTriangleIndex = i * 3;

			// gets the index in the triangles array of each 3 vertices of one triangle
			int vertexIndexA = borderTriangles[normalTriangleIndex];
			int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
			int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

			// calculates the normal vector of the current triangle in the for loop iteration
			Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

			// adds the normal vector of the border triangles if their index is greater or equal to 0
			if (vertexIndexA >= 0)
			{
				vertexNormals[vertexIndexA] += triangleNormal;
			}
			if (vertexIndexB >= 0)
			{
				vertexNormals[vertexIndexB] += triangleNormal;
			}
			if (vertexIndexC >= 0)
			{
				vertexNormals[vertexIndexC] += triangleNormal;
			}
		}

		// normalizes all normal vectors in the array of vertex normals
		for (int i = 0; i < vertexNormals.Length; i++)
		{
			vertexNormals[i].Normalize();
		}
		return vertexNormals;
	}

	/// <summary>
	/// Calculates the surface normals of the traingle made up from the given vertex indices
	/// </summary>
	/// <param name="indexA"> Index of vertex A </param>
	/// <param name="indexB"> Index of vertex B </param>
	/// <param name="indexC"> Index of vertex C </param>
	/// <returns> Returns a normal vector of the specified triangle </returns>
	Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
	{
		Vector3 pointA, pointB, pointC;

		// gets the value of the vertices of one triangle from either border or mesh vertices
		if (indexA >= 0)
		{
			pointA = vertices[indexA];
		} else
		{
			pointA = borderVertices[-indexA - 1];
		}

		if (indexB >= 0)
		{
			pointB = vertices[indexB];
		} else
		{
			pointB = borderVertices[-indexB - 1];
		}

		if (indexC >= 0)
		{
			pointC = vertices[indexC];
		} else
		{
			pointC = borderVertices[-indexC - 1];
		}

		// creats vectors for two sides of the triangle and calculates its normal using cross product
		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;
		return Vector3.Cross(sideAB, sideAC).normalized;
	}

	/// <summary>
	/// Changes the meshes vertices and uv array to either flatshading or normal arrays
	/// </summary>
	public void ProcessMesh()
	{
		if (useFlatShading)
		{
			FlatShading();
		} else
		{
			BakeNormals();
		}
	}

	/// <summary>
	/// Calculates the normals of each triangle in a thread
	/// </summary>
	void BakeNormals()
	{
		bakedNormals = CalculateNormals();
	}

	/// <summary>
	/// Changes the vertices and uv's array to flatshading, if two triangles are touching, the vertices they share will have two seperate normals
	/// This allows the lighting to be calculated for each face seperately instead of interpolating the colours over multiple triangles
	/// </summary>
	void FlatShading()
	{
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		// coppies the normal vertices and uvs to the flatshaded arrays
		for (int i = 0; i < triangles.Length; i++)
		{
			flatShadedVertices[i] = vertices[triangles[i]];
			flatShadedUvs[i] = uvs[triangles[i]];
			triangles[i] = i;
		}

		// this is what changes from normal to flatshading when they are later applied in CreateMesh()
		vertices = flatShadedVertices;
		uvs = flatShadedUvs;
	}

	/// <summary>
	/// Sets the mesh normals bases on flat or normal shading
	/// </summary>
	/// <returns></returns>
	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		if (useFlatShading)
		{
			// calculates two normals at shared vertices between triangles
			mesh.RecalculateNormals();
		} else
		{
			// use custom baked normals to remove lighting discontinuity between chunks
			// calculates one interpolated normal at each shared triangle vertiex
			mesh.normals = bakedNormals;
		}
		return mesh;
	}
}