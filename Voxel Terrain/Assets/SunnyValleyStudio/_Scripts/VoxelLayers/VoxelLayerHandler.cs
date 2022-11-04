using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public abstract class VoxelLayerHandler : MonoBehaviour
    {
        [SerializeField]
        private VoxelLayerHandler next;

        public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (TryHandling(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset))
                return true;

            if (next != null)
                return next.Handle(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset);

            return false;
        }

        protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset);

    }
}

// Note: Chain of responsibility pattern
// Source: https://www.youtube.com/watch?v=tf8x5R5RU-E&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=6&ab_channel=SunnyValleyStudio
