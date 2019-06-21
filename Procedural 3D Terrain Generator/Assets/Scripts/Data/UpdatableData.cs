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
