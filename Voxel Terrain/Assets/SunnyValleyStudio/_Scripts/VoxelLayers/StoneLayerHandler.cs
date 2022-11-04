using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class StoneLayerHandler : VoxelLayerHandler
    {
        [Range(0, 1)]
        public float stoneThreshold = 0.5f;

        [SerializeField]
        private NoiseDataSO stoneNoiseData;

        public DomainWarping domainWarping;

        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (chunkData.worldPosition.y > surfaceHeightNoise)
                return false;

            stoneNoiseData.worldOffset = mapSeedOffset;
            //float stoneNoise = MyNoise.OctavePerlin(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z, stoneNoiseData);
            float stoneNoise = domainWarping.GenerateDomainNoise(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z, stoneNoiseData);

            int endPosition = surfaceHeightNoise;
            if (chunkData.worldPosition.y < 0)
            {
                endPosition = chunkData.worldPosition.y + chunkData.chunkHeight;
            }

            if (stoneNoise > stoneThreshold)
            {
                for (int i = chunkData.worldPosition.y; i <= endPosition; i++)
                {
                    Vector3Int pos = new Vector3Int(x, i, z);
                    Chunk.SetVoxel(chunkData, pos, VoxelType.Stone);
                }
                return true;
            }
            return false;
        }
    }
}

// Source: https://www.youtube.com/watch?v=8A-D2VZJU4c&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=8&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=Pdmw3I0TjK4&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=9&ab_channel=SunnyValleyStudio