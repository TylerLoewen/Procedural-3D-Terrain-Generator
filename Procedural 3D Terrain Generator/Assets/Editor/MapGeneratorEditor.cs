/*
<Procedural Terrain Generator is a seamless chunk based terrain generator with customizable terrain types>
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
using UnityEditor;

/// <summary>
/// Allows data in the Unity Inspector to be updated without being in "play mode" by clicking the newly created "Generate" button
/// </summary>
[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
	/// <summary>
	/// overrides the default inspector to add a custom button
	/// </summary>
	public override void OnInspectorGUI()
	{
		MapGenerator mapGen = (MapGenerator)target;

		if (DrawDefaultInspector())
		{
			// only auto update the noise map if the option is on in the inspector
			if (mapGen.autoUpdate)
			{
				mapGen.DrawMapInEditor();
			}
		}
	
		// creates a generate button in the Unity Inspector
		if (GUILayout.Button("Generate"))
		{
			// draws the map with the latest information
			mapGen.DrawMapInEditor();
		}
	}
}
