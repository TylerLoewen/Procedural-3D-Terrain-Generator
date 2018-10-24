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

public class UpdatableData : ScriptableObject
{
	public event System.Action OnValuesUpdated;
	public bool autoUpdate;

#if UNITY_EDITOR

	/// <summary>
	/// Sends a request to update the map in the Editor
	/// </summary>
	protected virtual void OnValidate()
	{
		if (autoUpdate)
		{
			// delays the calling of NotifyOfUpdatedValues until after all of the scripts are compiled
			UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
		}
	}

	/// <summary>
	/// Executes all of the methods that are subscribed to the OnValuesUpdated event
	/// </summary>
	public void NotifyOfUpdatedValues()
	{
		UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
		if (OnValuesUpdated != null)
		{
			OnValuesUpdated();
		}
	}
#endif
}
