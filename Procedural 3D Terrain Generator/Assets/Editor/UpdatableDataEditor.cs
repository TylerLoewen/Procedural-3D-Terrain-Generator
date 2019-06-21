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