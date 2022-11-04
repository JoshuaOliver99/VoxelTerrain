using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    [CreateAssetMenu(fileName = "Noise Settings", menuName = "Data/Noise Data")]
    public class NoiseDataSO : ScriptableObject
    {
        public float noiseZoom;
        public int octaves;
        public Vector2Int offest;
        public Vector2Int worldOffset;
        public float persistance;
        public float redistributionModifier;
        public float exponent;
    }
}

// Source: https://www.youtube.com/watch?v=nXGlbG3jcKM&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=4&ab_channel=SunnyValleyStudio
