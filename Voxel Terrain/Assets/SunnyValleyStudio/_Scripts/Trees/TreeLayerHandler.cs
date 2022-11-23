using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class TreeLayerHandler : VoxelLayerHandler
    {
        public float terrainHeightLimit = 25; // Note: rename tree height limit?

        public static List<Vector3Int> treeLeafesStaticLayout = new List<Vector3Int>
        {
            new Vector3Int(-2, 0, -2),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(-2, 0, 0),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, 2),
            new Vector3Int(-1, 0, -2),
            new Vector3Int(-1, 0, -1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(0, 0, -2),
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(1, 0, -1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(2, 0, -2),
            new Vector3Int(2, 0, -1),
            new Vector3Int(2, 0, 0),
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, 2),
            new Vector3Int(-1, 1, -1),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, 1, 1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 1, -1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 2, 0)
        };

        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
        {
            if (chunkData.worldPosition.y < 0)
                return false;

            if (surfaceHeightNoise < terrainHeightLimit 
                && chunkData.treeData.treePositions.Contains(new Vector2Int(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z)))
            {
                Vector3Int chunkCoordinates = new Vector3Int(x, surfaceHeightNoise, z);
                VoxelType type = Chunk.GetVoxelFromChunkCoordinates(chunkData, chunkCoordinates);
                
                if (type == VoxelType.Grass_Dirt)
                {
                    Chunk.SetVoxel(chunkData, chunkCoordinates, VoxelType.Dirt);

                    for (int i = 1; i < 5; i++)
                    {
                        chunkCoordinates.y = surfaceHeightNoise + i;
                        Chunk.SetVoxel(chunkData, chunkCoordinates, VoxelType.TreeTrunk);
                    }

                    foreach (Vector3Int leafPosition in treeLeafesStaticLayout)
                    {
                        chunkData.treeData.treeLeafsSolid.Add(new Vector3Int(x + leafPosition.x, surfaceHeightNoise + 5 + leafPosition.y, z + leafPosition.z));
                    }
                }
            }

            return false;
        }
    }
}

// Source: S3 - P8 Adding Trees P2 https://www.youtube.com/watch?v=IPjzsLV8jd8&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio
// Source: S3 - P9 Adding Trees P3 https://www.youtube.com/watch?v=Pth2WPDDdqI&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=9&ab_channel=SunnyValleyStudio

