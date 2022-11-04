using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class WaterLayerHandler : VoxelLayerHandler
    {
        public int waterLevel = 1;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (y > surfaceHeightNoise && y <= waterLevel)
            {
                Vector3Int pos = new Vector3Int(x, y, z);
                Chunk.SetVoxel(chunkData, pos, VoxelType.Water);

                if (y == surfaceHeightNoise + 1)
                {
                    pos.y = surfaceHeightNoise;
                    Chunk.SetVoxel(chunkData, pos, VoxelType.Sand);
                }

                return true;
            }

            return false;
        }
    }
}

// Source: https://www.youtube.com/watch?v=dupqptGrgwc&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=7&ab_channel=SunnyValleyStudio