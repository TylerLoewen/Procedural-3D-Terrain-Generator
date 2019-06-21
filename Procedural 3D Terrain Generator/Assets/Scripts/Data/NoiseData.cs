using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
	public Noise.NormalizeMode normalizeMode;

	[Range(0, 1)]
	public float persistance;

	public float noiseScale;
	public int octaves;
	public float lacunarity;
	public int seed;
	public Vector2 offset;

#if UNITY_EDITOR
	/// <summary>
	/// Clamps the specified values to within their desired range
	/// </summary>
	protected override void OnValidate()
	{
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0)
		{
			octaves = 0;
		}

		base.OnValidate();
	}

#endif
}
