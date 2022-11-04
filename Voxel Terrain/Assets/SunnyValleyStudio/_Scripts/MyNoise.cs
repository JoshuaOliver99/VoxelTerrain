using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public static class MyNoise
    {
        public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
        {
            return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
        }

        public static float RemapValue01(float value, float outputMin, float outputMax)
        {
            return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
        }

        public static int RemapValue01ToInt(float value, float outputMin, float outputMax)
        {
            return (int)RemapValue01(value, outputMin, outputMax);
        }

        public static float Redistribution(float noise, NoiseDataSO settings)
        {
            return Mathf.Pow(noise * settings.redistributionModifier, settings.exponent);
        }

        public static float OctavePerlin(float x, float z, NoiseDataSO settings)
        {
            x *= settings.noiseZoom;
            z *= settings.noiseZoom;
            x += settings.noiseZoom;
            z += settings.noiseZoom;

            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
            for (int i = 0; i < settings.octaves; i++)
            {
                total += Mathf.PerlinNoise((settings.offest.x + settings.worldOffset.x + x) * frequency, (settings.offest.y + settings.worldOffset.y + z) * frequency) * amplitude;

                amplitudeSum += amplitude;

                amplitude *= settings.persistance;
                frequency *= 2;
            }

            return total / amplitudeSum;
        }
    }
}

// Source: https://www.youtube.com/watch?v=nXGlbG3jcKM&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=4&ab_channel=SunnyValleyStudio
