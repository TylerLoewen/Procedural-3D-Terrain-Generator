using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
	// scales the whole generated map
	public float uniformScale = 3f;

	public bool useFlatShading;
	public bool useFalloff;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	/// <summary>
	/// Gets the minimum height of the terrain
	/// </summary>
	public float minHeight
	{
		get
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
		}
	}

	/// <summary>
	/// Gets the maximum height of the terrain
	/// </summary>
	public float maxHeight
	{
		get
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
		}
	}
}
