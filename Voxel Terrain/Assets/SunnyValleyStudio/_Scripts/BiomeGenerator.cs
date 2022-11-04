using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class BiomeGenerator : MonoBehaviour
    {
        public int waterThreshold = 50;

        public NoiseDataSO biomeNoiseData;

        public VoxelLayerHandler startLayerHandler;

        public List<VoxelLayerHandler> additionalLayerHandlers;

        public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset)
        {
            biomeNoiseData.worldOffset = mapSeedOffset;
            int groundPosition = GetSurfaceHeightNoise(data.worldPosition.x + x, data.worldPosition.z + z, data.chunkHeight);

            for (int y = 0; y < data.chunkHeight; y++)
            {
                startLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            }

            foreach (var layer in additionalLayerHandlers)
            {
                layer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
            }
            return data;
        }

        private int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
        {
            float terrainHeight = MyNoise.OctavePerlin(x, z, biomeNoiseData);
            terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseData);
            int surfaceHeight = MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight);
            return surfaceHeight;
        }
    }
}

// Source: https://www.youtube.com/watch?v=TOLlDa2XTbQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=3&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=JNNxMyu0jkM&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=5&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=tf8x5R5RU-E&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=6&ab_channel=SunnyValleyStudio
// Srouce: https://www.youtube.com/watch?v=8A-D2VZJU4c&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=8&ab_channel=SunnyValleyStudio

// Source: https://adrianb.io/2014/08/09/perlinnoise.html
// Source: http://www.nolithius.com/articles/world-generation/world-generation-techniques-domain-warping