using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class SurfaceLayerHandler : VoxelLayerHandler
    {
        public VoxelType surfaceVoxelType;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (y == surfaceHeightNoise)
            {
                Vector3Int pos = new Vector3Int(x, y, z);
                Chunk.SetVoxel(chunkData, pos, surfaceVoxelType);

                return true;
            }

            return false;
        }
    }
}

// Source: https://www.youtube.com/watch?v=dupqptGrgwc&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=7&ab_channel=SunnyValleyStudio