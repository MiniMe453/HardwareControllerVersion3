using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlekGames.Placer.Shared
{
	public static class PerlinNoise
	{
		public static float[,] get2DNoise(noiseSettings settings) //this function is based on Sebastians Lague tutoraial on procedural terrain. no it is not copied, i did a lot of work in it myself
		{
			int width = settings.width;
			int height = settings.height;
			float scale = settings.scale;
			int octaves = settings.octaves;
			float persistance = settings.persistance;
			float lacunarity = settings.lacunarity;
			Vector2 offset = settings.offset;

			float[,] noiseMap = new float[width, height];

			System.Random prng = new System.Random(settings.seed);
			Vector2[] octaveOffsets = new Vector2[octaves];

			float maxPossibleHeight = 0;
			float amplitude = 1;
			float frequency;

			for (int i = 0; i < octaves; i++)
			{
				float offsetX = prng.Next(-100000, 100000) + offset.x;
				float offsetY = prng.Next(-100000, 100000) - offset.y;
				octaveOffsets[i] = new Vector2(offsetX, offsetY);

				maxPossibleHeight += amplitude;
				amplitude *= persistance;
			}

			float halfWidth = width / 2f;
			float halfHeight = height / 2f;

			float biggestHeight = 0;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{

					amplitude = 1;
					frequency = 1;
					float noiseHeight = 0;

					for (int i = 0; i < octaves; i++)
					{
						float sampleX = octaveOffsets[i].x + (x - halfWidth) / scale * frequency; //- halfWidth/- halfHeight to scale to center
						float sampleY = octaveOffsets[i].y + (y - halfHeight) / scale * frequency;

						float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
						noiseHeight += perlinValue * amplitude; // amplitude is always less than 1

						amplitude *= persistance;
						frequency *= lacunarity;

						if (noiseHeight > biggestHeight) biggestHeight = noiseHeight;
					}

					noiseHeight = Mathf.Clamp(noiseHeight, 0, maxPossibleHeight);

					noiseMap[x, y] = noiseHeight;
				}
			}

			if(settings.proportionClamp01)
				for (int y = 0; y < height; y++)			
					for (int x = 0; x < width; x++)				
						noiseMap[x, y] /= biggestHeight;
				
			

			return noiseMap;
        }

		[System.Serializable]
        public class noiseSettings
        {
			[Min(1), Tooltip("noise width (x axis)")]
			public int width = 100;
			[Min(1), Tooltip("noise height (z axis)")]
			public int height = 100;
			[Tooltip("noise seed")]
			public int seed;
			[Min(0.001f), Tooltip("noise scale (zoom)")]
			public float scale = 15;
			[Min(1), Tooltip("ammount of noise adddons to the noise")]
			public int octaves = 3;
			[Range(0, 1), Tooltip("affect of octave in comparison to previous one")]
			public float persistance = 0.8f;
			[Tooltip("affect of octave scale in comparison to previous one")]
			public float lacunarity = 0.5f;
			[Tooltip("noise offset")]
			public Vector2 offset;
			[Tooltip("if should clamp noise scale to be between 0-1, while keeping proportions.")]
			public bool proportionClamp01 = true;
		}

    }
}
