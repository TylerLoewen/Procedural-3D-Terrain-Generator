using UnityEngine;

/// <summary>
/// Hides the mesh used for testing values once the "Play Mode" button is pressed
/// </summary>
public class HideTestMeshOnPlay : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		// Hides this game object once play mode is entered
		gameObject.SetActive(false);
	}
}
