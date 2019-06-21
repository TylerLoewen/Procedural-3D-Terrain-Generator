using UnityEngine;

/// <summary>
/// Manages application controls
/// </summary>
public class GameManager : MonoBehaviour
{
	// Update is called once per frame
	void Update()
	{
		// Quits the game if the escape key is pressed
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
