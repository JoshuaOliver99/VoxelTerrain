using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public static class Chunk
    {
        public static void LoopThroughTheVoxels(ChunkData chunkData, Action<int, int, int> actionToPreform)
        {
            for (int index = 0; index < chunkData.voxel.Length; index++)
            {
                var position = GetPositionFromIndex(chunkData, index);
                actionToPreform(position.x, position.y, position.z);
            }
        }

        private static Vector3Int GetPositionFromIndex(ChunkData chunkData, int index)
        {
            int x = index % chunkData.chunkSize;
            int y = (index / chunkData.chunkSize) % chunkData.chunkHeight;
            int z = index / (chunkData.chunkSize * chunkData.chunkHeight);
            return new Vector3Int(x, y, z);
        }

        // In chunk coordinate system
        private static bool InRange(ChunkData chunkData, int axisCoordinate)
        {
            if (axisCoordinate < 0 || axisCoordinate >= chunkData.chunkSize)
                return false;

            return true;
        }

        // In chunk coordinate system
        private static bool InRangeHeight(ChunkData chunkData, int yCoordinate)
        {
            if (yCoordinate < 0 || yCoordinate >= chunkData.chunkHeight)
                return false;

            return true;
        }

        public static VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, Vector3Int chunkCoordinates)
        {
            return GetVoxelFromChunkCoordinates(chunkData, chunkCoordinates.x, chunkCoordinates.y, chunkCoordinates.z);
        }

        public static VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
        {
            if (InRange(chunkData, x) && 
                InRangeHeight(chunkData, y) && 
                InRange(chunkData, z))
            {
                int index = GetIndexFromPosition(chunkData, x, y, z);
                return chunkData.voxel[index];
            }

            return chunkData.worldReference.GetBlockFromChunkCoordinates(chunkData,
                chunkData.worldPosition.x + x, chunkData.worldPosition.y + y, chunkData.worldPosition.z + z);
        }
        
        public static void SetVoxel(ChunkData chunkData, Vector3Int localPosition, VoxelType voxel)
        {
            if (InRange(chunkData, localPosition.x) && 
                InRangeHeight(chunkData, localPosition.y) && 
                InRange(chunkData, localPosition.z))
            {
                int index = GetIndexFromPosition(chunkData, localPosition.x, localPosition.y, localPosition.z);
                chunkData.voxel[index] = voxel;
            }
            else
            {
                WorldDataHelper.SetVoxel(chunkData.worldReference, localPosition + chunkData.worldPosition, voxel);
            }
        }

        private static int GetIndexFromPosition(ChunkData chunkData, int x, int y, int z)
        {
            return x + chunkData.chunkSize * y + chunkData.chunkSize * chunkData.chunkHeight * z;
        }

        public static Vector3Int GetVoxelInChunkCoordinates(ChunkData chunkData, Vector3Int pos)
        {
            return new Vector3Int
            {
                x = pos.x - chunkData.worldPosition.x,
                y = pos.y - chunkData.worldPosition.y,
                z = pos.z - chunkData.worldPosition.z,
            };
        }


        public static MeshData GetChunkMeshData(ChunkData chunkData)
        {
            MeshData meshData = new MeshData(true);

            LoopThroughTheVoxels(chunkData,
                (x, y, z) => meshData = VoxelHelper.GetMeshData(chunkData, x, y, z, meshData, chunkData.voxel[GetIndexFromPosition(chunkData, x, y, z)]));

            return meshData;
        }

        internal static List<ChunkData> GetEdgeNeighbourChunk(ChunkData chunkData, Vector3Int worldPosition)
        {
            Vector3Int chunkPosition = GetVoxelInChunkCoordinates(chunkData, worldPosition);
            List<ChunkData> neighboursToUpdate = new List<ChunkData>();

            if (chunkPosition.x == 0)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.right));
            if (chunkPosition.x == chunkData.chunkSize - 1)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.right));
            
            if (chunkPosition.y == 0)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.up));
            if (chunkPosition.y == chunkData.chunkHeight - 1)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.up));

            if (chunkPosition.z == 0)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.forward));
            if (chunkPosition.z == chunkData.chunkSize - 1)
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.forward));

            return neighboursToUpdate;
        }

        internal static bool IsOnEdge(ChunkData chunkData, Vector3Int worldPosition)
        {
            Vector3Int chunkPosition = GetVoxelInChunkCoordinates(chunkData, worldPosition);

            // If on the chunk edge...
            if (chunkPosition.x == 0 || chunkPosition.x == chunkData.chunkSize - 1 ||
                chunkPosition.y == 0 || chunkPosition.y == chunkData.chunkHeight - 1 ||
                chunkPosition.z == 0 || chunkPosition.z == chunkData.chunkSize - 1)
                return true;

            return false;
        }

        internal static Vector3Int ChunkPositionFromBlockCoords(World world, int x, int y, int z)
        {
            Vector3Int pos = new Vector3Int
            {
                x = Mathf.FloorToInt(x / (float)world.WorldSettings.ChunkSize) * world.WorldSettings.ChunkSize,
                y = Mathf.FloorToInt(y / (float)world.WorldSettings.ChunkHeight) * world.WorldSettings.ChunkHeight,
                z = Mathf.FloorToInt(z / (float)world.WorldSettings.ChunkSize) * world.WorldSettings.ChunkSize
            };
            return pos;
        }
    }
}

// Source: https://www.youtube.com/watch?v=vsdIKEAuH1I&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=s5mAf-VMgCM&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=L5obsaFeJPQ&ab_channel=SunnyValleyStudio
// Source: S2 - P17 https://www.youtube.com/watch?v=aP6N245OjEQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=17&ab_channel=SunnyValleyStudio
// Source: S3 - P9 Adding Trees P3 https://www.youtube.com/watch?v=Pth2WPDDdqI&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=9&ab_channel=SunnyValleyStudio

