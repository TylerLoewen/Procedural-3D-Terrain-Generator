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

using UnityEngine;

/// <summary>
/// Generates a falloff map which contains randomized values near the center and low values around the edges
/// </summary>
public static class FallOffMapGenerator
{
	/// <summary>
	/// Generates a new Falloff Map
	/// </summary>
	/// <param name="size"> The size of the Falloff map as type int </param>
	/// <returns> Points of the falloff map as a 2d float array </returns>
	public static float[,] GenerateFalloffMap(int size)
	{
		float[,] map = new float[size, size];

		// iterates through every point in the map and gives it a value
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				// creates values between 0 and 1 based on indices of for loops
				float x = i / (float)size * 2 -1;
				float y = j / (float)size * 2 - 1;

				// sets value to the bigger of the two between x and y
				float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

				// Applies the formula used to determine the distribution of the falloff map
				map[i, j] = Evaluate(value);
			}
		}
		return map;
	}

	/// <summary>
	/// Applies the formula used to determine the distribution of the falloff map
	/// </summary>
	/// <param name="value"> Input value between 0 and 1 that changes the output of the function </param>
	/// <returns> Returns the calculated value as a float </returns>
	static float Evaluate(float value)
	{
		// function constants
		float a = 3;
		float b = 2.2f;

		// formula that creates a curve to get the desired distribution of values in the falloff map
		return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
	}
}
