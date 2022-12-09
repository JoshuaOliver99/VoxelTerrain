using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class EmptyLayerHandler : VoxelLayerHandler
    {
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (y <= surfaceHeightNoise)
            {
                Vector3Int pos = new Vector3Int(x, y, z);
                Chunk.SetVoxel(chunkData, pos, VoxelType.Dirt);

                return true;
            }

            return false;
        }
    }
}