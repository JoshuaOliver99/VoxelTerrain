using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class BiomeGenerator : MonoBehaviour
    {
        public int waterThreshold = 50;
        public float noiseScale = 0.03f;

        public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset)
        {
            float noiseValue = Mathf.PerlinNoise((mapSeedOffset.x + data.worldPosition.x + x) * noiseScale, (mapSeedOffset.y + data.worldPosition.z + z) * noiseScale);
            int groundPosition = Mathf.RoundToInt(noiseValue * data.chunkHeight);

            for (int y = 0; y < data.chunkHeight; y++)
            {
                VoxelType voxelType = VoxelType.Dirt;

                if (y > groundPosition)
                {
                    if (y < waterThreshold)
                    {
                        voxelType = VoxelType.Water;
                    }
                    else
                    {
                        voxelType = VoxelType.Air;
                    }
                }
                else if (y == groundPosition)
                {
                    voxelType = VoxelType.Grass_Dirt;
                }

                Chunk.SetVoxel(data, new Vector3Int(x, y, z), voxelType);
            }

            return data;
        }
    }
}

// Source: https://www.youtube.com/watch?v=TOLlDa2XTbQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=3&ab_channel=SunnyValleyStudio