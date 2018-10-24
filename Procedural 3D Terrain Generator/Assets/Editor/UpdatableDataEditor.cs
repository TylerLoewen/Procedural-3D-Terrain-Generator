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
/// Allows data in the Unity Inspector to be updated without being in "play mode" by clicking the newly created "Update" button
/// </summary>
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor {

	/// <summary>
	/// Checks when the "Update" button is pressed in the Unity inspector and notifies to update the information
	/// </summary>
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		UpdatableData data = (UpdatableData)target;

		// creates an update button in the Unity Inspector
		if (GUILayout.Button("Update"))
		{
			// notifies that there modified values in the Inspector that need to be updated
			data.NotifyOfUpdatedValues();
			EditorUtility.SetDirty(target);
		}
	}
}