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
