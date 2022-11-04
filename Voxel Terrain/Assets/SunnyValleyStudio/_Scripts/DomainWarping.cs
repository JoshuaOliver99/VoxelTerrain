using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class DomainWarping : MonoBehaviour
    {
        public NoiseDataSO noiseDomainX, noiseDomainY;
        public int amplitudeX = 20, amplitudeY = 20;

        public float GenerateDomainNoise(int x, int z, NoiseDataSO defaultNoiseData)
        {
            Vector2 domainOffset = GenerateDomainOffset(x, z);
            return MyNoise.OctavePerlin(x + domainOffset.x, z + domainOffset.y, defaultNoiseData);
        }

        public Vector2 GenerateDomainOffset(int x, int z)
        {
            var noiseX = MyNoise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
            var noiseY = MyNoise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
            return new Vector2(noiseX, noiseY);
        }

        public Vector2Int GenerateDomainOffsetInt(int x, int z)
        {
            return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
        }
    }
}

// Source: https://www.youtube.com/watch?v=Pdmw3I0TjK4&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=9&ab_channel=SunnyValleyStudio