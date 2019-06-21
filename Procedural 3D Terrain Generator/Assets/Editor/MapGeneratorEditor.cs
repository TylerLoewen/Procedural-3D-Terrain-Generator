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
