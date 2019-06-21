using UnityEngine;

/// <summary>
/// Provides orbit transform modificatons to any object the script is attached to
/// </summary>
public class OrbitRotation : MonoBehaviour
{
	[SerializeField]
	public int objectRotationSpeed;

	// Update is called once per frame
	void Update ()
	{
		// rotates the objects (sun/moon) around the specified location (center of map)
		transform.RotateAround(Vector3.zero, Vector3.right, Time.deltaTime * objectRotationSpeed);
		transform.LookAt(Vector3.zero);
	}
}
