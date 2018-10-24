//<'Procedural Terrain Generator' is a seamless chunk based terrain generator with customizable terrain types>
//Copyright(C) <2018> <Tyler Loewen>
//
//This program is free software : you can redistribute it and / or modify
//it under the terms of the GNU Affero General Public License as published
//by the Free Software Foundation, either version 3 of the License, or any
//later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU Affero General Public License for more details.
//
//You should have received a copy of the GNU Affero General Public License
//along with this program.If not, see <https://www.gnu.org/licenses/>.
//
//I can be reached at tylerscottloewen@gmail.com

Shader "Custom/Terrain" 
{	
	Properties
	{
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.5 target, to get nicer looking lighting
		#pragma target 3.5

		// ADDED MY OWN CODE STARTING HERE

		// the amount of different height layers on the texture (e.g. water, sand, grass, rocks, etc)
		const static int maxLayerCount = 8;

		// small number used to prevent division by 0
		const static float smallValue = 0.000001f;

		// Input variables from other scripts
		float minHeight;
		float maxHeight;
		int layerCount;
		float3 baseColours[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColourStrength[maxLayerCount];
		float baseTextureScales[maxLayerCount];
	
		// creates the Texture2DArray that will be imported
		UNITY_DECLARE_TEX2DARRAY(baseTextures);

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		// Calculates a value between 0 and 1 based on the world y value and the min/max value of the mesh
		float inverseLerp(float a, float b, float value)
		{
			// difference between value and minimum value, divided by difference between maximum and minimum value
			// saturate clamps the result between 0 and 1
			return saturate((value - a) / (b - a));
		}

		// Calculates a texture projected onto each axis so it doesn't appear stretched when mapped to terrain with a steep slope
		float3 triplaner(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
		{
			float3 scaledWorldPos = worldPos / scale;

			// calculates the texture projected in the direction of each axis to use for different angled surfaces
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
			return (xProjection + yProjection + zProjection);
		}

		// MAIN SHADER RUNTIME METHOD
		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			// calculates the color value based on the height of the point
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
			float3 blendAxes = abs(IN.worldNormal);

			// makes sure that the blendAxes value does not exced 1 which would make the texture brighter
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			// iterates through each pre-determined layer set in the Unity Inspector
			for (int i = 0; i < layerCount; i++)
			{
				// calulates the strength that the colour will draw
				// 0 if the pixel is half the base blends height below the starting height
				// 1 if the pixel is half the base blends height above the starting height
				// interpolates from 0 to 1 between these two distances
				float drawStrength = inverseLerp(-baseBlends[i] / 2 - smallValue, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);
				float3 baseColour = baseColours[i] * baseColourStrength[i];
				float3 textureColour = triplaner(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);

				// sets the new colour and ensures if the drawStrength is 0 that the current albedo remains the same by being multiplied by 1
				o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
			}
		}
		// END OF CODE I ADDED

		ENDCG
	}
	FallBack "Diffuse"
}