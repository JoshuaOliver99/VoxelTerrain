using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class TerrainGenerator : MonoBehaviour
    {
        public BiomeGenerator biomeGenerator;

        public ChunkData GenerateChunkData(ChunkData data, Vector2Int mapSeedOffset)
        {
            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
                    data = biomeGenerator.ProcessChunkColumn(data, x, z, mapSeedOffset);
                }
            }
            return data;
        }
    }
}

// Source: https://www.youtube.com/watch?v=TOLlDa2XTbQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=3&ab_channel=SunnyValleyStudio