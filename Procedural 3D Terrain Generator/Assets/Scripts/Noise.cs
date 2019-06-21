using UnityEngine;

/// <summary>
/// Generates Perlin noise
/// </summary>
public static class Noise
{
	// toggles between using local min/max values or global max values
	public enum NormalizeMode {Local, Global};

	/// <summary>
	/// Generates values of the noise map and returns them in a 2D array
	/// </summary>
	/// <param name="mapWidth"> Width of the map </param>
	/// <param name="mapHeight"> Height of the map </param>
	/// <param name="seed"> Map seed used for generation </param>
	/// <param name="scale"> Scales the map </param>
	/// <param name="octaves"> Controls the amount of detail </param>
	/// <param name="persistance"> Determines how much the amplitude decreases between octaves </param>
	/// <param name="lacunarity"> Determines how much the frequency increases between octave </param>
	/// <param name="offset"> x and y offset of the noise map </param>
	/// <param name="normalizeMode"> Mode used to normalize the height values of the map </param>
	/// <returns> Returns a 2D float array of x and y values of the map </returns>
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
	{
		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;
		float maxLocalNoiseHeight = float.MaxValue;
		float minLocalNoiseHeight = float.MinValue;

		// allows the noise scale value to scale from the center of the texture not the corner
		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		// array containing points making up the noise map
		float[,] noiseMap = new float[mapWidth, mapHeight];
		Vector2[] octaveOffsets = new Vector2[octaves];

		// sudo-random number generator using a given seed value
		System.Random randomValue = new System.Random(seed);

		// iterates through all of the octaves
		for (int i = 0; i < octaves; i++)
		{
			// returns a random number between the two parameters +/- the offset
			// randomly offsets the octaves so that they are not perfectly overlapping
			float offsetX = randomValue.Next(-100000, 100000) + offset.x;
			float offsetY = randomValue.Next(-100000, 100000) - offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			// calculates the highest point with all of the octaves combines
			// used to make seamless borders between chunks
			maxPossibleHeight += amplitude;

			// decreses amplitude for each consecutive octave
			amplitude *= persistance;
		}

		// insures the scale is never less than or equal to zero for dividing
		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		// generates every point for the noise map
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				// generates a noise map for each different octave
				for (int i = 0; i < octaves; i++)
				{
					// higher the frequency the more rapidly the perlin noise will change
					// offsets the octaves to add more variation
					float sampleX = (x- halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y- halfHeight + octaveOffsets[i].y) / scale * frequency;

					// generates a perlin value with a range of -1 to 1 from the given x and y coordinates
					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					// decreses amplitude each iteration
					amplitude *= persistance;

					// increases frequency each iteration
					frequency *= lacunarity;
				}

				// updates the new max and min noise heights
				if (noiseHeight > maxLocalNoiseHeight)
				{
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight)
				{
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}

		// iterates through each point on the map
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				if (normalizeMode == NormalizeMode.Local)
				{
					// normalize the min and max height to be within the range of the largest and smallest float value
					noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
				} else if (normalizeMode == NormalizeMode.Global)
				{
					// clamps the height value to between 0 and largest integer
					float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);
					noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
				}
			}
		}
		return noiseMap;
	}
}
