/*
<Procedural 3D Terrain Generator is a seamless chunk based terrain generator with customizable terrain types>
Copyright (C) <2018>  <Tyler Loewen>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/gpl.txt>.

Contact at tylerscottloewen@gmail.com
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
	// constance size and format of the textures that are being used
	const int textureSize = 512;
	const TextureFormat textureFormat = TextureFormat.RGB565;

	public Layer[] layers;

	float savedMinHeight;
	float savedMaxHeight;

	/// <summary>
	/// Applies the saved min and max height of the mesh to the provided material
	/// Min and Max values are saved so the shader can update when the "Generate" button is clicked in Unity Inspector
	/// </summary>
	/// <param name="material"></param>
	public void ApplyToMaterial(Material material)
	{
		// passes these arrays and values to the shader
		material.SetInt("layerCount", layers.Length);
		material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
		material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
		material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
		material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
		material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

		Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
		material.SetTexture("baseTextures", texturesArray);

		UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
	}

	/// <summary>
	/// Sends the min and max height of the mesh to the shader
	/// </summary>
	/// <param name="material"> Material to have the values sent to </param>
	/// <param name="minHeight"> Minimum height of the mesh </param>
	/// <param name="maxHeight"> Maximum height of the mesh </param>
	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
	{
		savedMinHeight = minHeight;
		savedMaxHeight = maxHeight;
		
		// sends the minimum and maximum mesh values to the shader
		material.SetFloat("minHeight", minHeight);
		material.SetFloat("maxHeight", maxHeight);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="textures"></param>
	/// <returns></returns>
	Texture2DArray GenerateTextureArray(Texture2D[] textures)
	{
		// creates a new array to store all of the textures
		Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

		for (int i = 0; i < textures.Length; i++)
		{
			textureArray.SetPixels(textures[i].GetPixels(), i);
		}
		textureArray.Apply();
		return textureArray;
	}


	[System.Serializable]
	public class Layer
	{
		public Texture2D texture;
		public Color tint;
		[Range(0,1)]
		public float tintStrength;
		[Range(0, 1)]
		public float startHeight;
		[Range(0, 1)]
		public float blendStrength;
		public float textureScale;
	}
}
