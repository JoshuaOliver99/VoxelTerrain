using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class UndergroundLayerHandler : VoxelLayerHandler
    {
        public VoxelType undergroundBlockType;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (y < surfaceHeightNoise)
            {
                Vector3Int localPos = new Vector3Int(x, y - chunkData.worldPosition.y, z); // Note: converting y to chunk coordinates
                Chunk.SetVoxel(chunkData, localPos, undergroundBlockType);

                return true;
            }

            return false;
        }
    }
}

// Source: https://www.youtube.com/watch?v=dupqptGrgwc&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=7&ab_channel=SunnyValleyStudio