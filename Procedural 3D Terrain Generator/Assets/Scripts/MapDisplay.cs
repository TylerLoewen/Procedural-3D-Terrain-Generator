using UnityEngine;

/// <summary>
/// Contains an interface for displaying a map
/// </summary>
public class MapDisplay : MonoBehaviour {

	public Renderer textureRenderer;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

	/// <summary>
	/// Applies the given texture to the TextureRenderer
	/// </summary>
	/// <param name="texture"> 2D texture </param>
	public void DrawTexture(Texture2D texture)
	{
		// applies the texture and sets the scale
		textureRenderer.sharedMaterial.mainTexture = texture;
		textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
	}

	/// <summary>
	/// Creates a new mesh with the given meshData and applies it to the meshFilter
	/// </summary>
	/// <param name="meshData"> MeshData containing data of a mesh </param>
	public void DrawMesh(MeshData meshData)
	{
		// creates and applies a mesh which allows the mesh data to be changed outside of unity play mode and sets scale
		meshFilter.sharedMesh = meshData.CreateMesh();
		meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().terrainData.uniformScale;
	}
}
