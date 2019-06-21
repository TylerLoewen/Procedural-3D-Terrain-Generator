using UnityEngine;

/// <summary>
/// Generates a new Texture2D from noise and colour maps
/// </summary>
public static class TextureGenerator
{
	/// <summary>
	/// Creates a new texture and sets its pixel color values to the values in the colour map
	/// </summary>
	/// <param name="colourMap"> Map of colour values between black and white </param>
	/// <param name="width"> Width of the colourMap </param>
	/// <param name="height"> Height of the Colour Map</param>
	/// <returns> Returns a coloured texture of type Texture2D</returns>
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
	{
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colourMap);
		texture.Apply();
		return texture;
	}

	/// <summary>
	/// Converts a height map to a colour map and then to a 2D texture
	/// </summary>
	/// <param name="heightMap"> 2D float array containing x and y values between 0 and 1 of the heightMap </param>
	/// <returns> Returns a 2D texture using the colours generated for the colourMap </returns>
	public static Texture2D TextureFromHeightMap(float[,] heightMap)
	{
		// gets the lentgh of the x dimension
		int width = heightMap.GetLength(0);

		// gets the length of the y dimension
		int height = heightMap.GetLength(1);

		Color[] colourMap = new Color[width * height];

		// iterates throught the noise map and generates colour values for the colourMap
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// sets the color value of each point in the colour map to a colour between black and white
				// noiseMap x and y values range from 0-1
				colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
			}
		}

		// converts the colour map values to a 2D Texture
		return TextureFromColourMap(colourMap, width, height);
	}
}
