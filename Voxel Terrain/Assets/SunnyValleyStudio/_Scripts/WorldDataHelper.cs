using SunnyValleyStudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SunnyValleyStudio
{
    public static class WorldDataHelper
    {
        public static Vector3Int ChunkPositionFromVoxelCoords(World world, Vector3Int worldPos)
        {
            return new Vector3Int
            {
                x = Mathf.FloorToInt(worldPos.x / (float)world.WorldSettings.ChunkSize) * world.WorldSettings.ChunkSize,
                y = Mathf.FloorToInt(worldPos.y / (float)world.WorldSettings.ChunkHeight) * world.WorldSettings.ChunkHeight,
                z = Mathf.FloorToInt(worldPos.z / (float)world.WorldSettings.ChunkSize) * world.WorldSettings.ChunkSize
            };
        }

        internal static List<Vector3Int> GetChunkPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            int startX = playerPosition.x - (world.WorldSettings.ChunkDrawingRange) * world.WorldSettings.ChunkSize;
            int startZ = playerPosition.z - (world.WorldSettings.ChunkDrawingRange) * world.WorldSettings.ChunkSize;
            int endX = playerPosition.x + (world.WorldSettings.ChunkDrawingRange) * world.WorldSettings.ChunkSize;
            int endZ = playerPosition.z + (world.WorldSettings.ChunkDrawingRange) * world.WorldSettings.ChunkSize;

            List<Vector3Int> chunkPositionsToCreate = new List<Vector3Int>();
            for (int x = startX; x <= endX; x += world.WorldSettings.ChunkSize)
            {
                for (int z = startZ; z <= endZ; z += world.WorldSettings.ChunkSize)
                {
                    Vector3Int chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, 0, z));
                    chunkPositionsToCreate.Add(chunkPos);
                    if (x >= playerPosition.x - world.WorldSettings.ChunkSize &&
                        x <= playerPosition.x + world.WorldSettings.ChunkSize &&
                        z >= playerPosition.z - world.WorldSettings.ChunkSize &&
                        z <= playerPosition.z + world.WorldSettings.ChunkSize)
                    {
                        for (int y = -world.WorldSettings.ChunkHeight; y >= playerPosition.y - world.WorldSettings.ChunkHeight * 2; y -= world.WorldSettings.ChunkHeight)
                        {
                            chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, y, z));
                            chunkPositionsToCreate.Add(chunkPos);
                        }
                    }
                }
            }

            return chunkPositionsToCreate;
        }

        internal static void RemoveChunkData(World world, Vector3Int pos)
        {
            world.worldData.chunkDataDictionary.Remove(pos);
        }

        internal static void RemoveChunk(World world, Vector3Int pos)
        {
            ChunkRenderer chunk = null;
            if (world.worldData.chunkDictionary.TryGetValue(pos, out chunk))
            {
                world.WorldRenderer.RemoveChunk(chunk);
                world.worldData.chunkDictionary.Remove(pos);
            }
        }

        internal static List<Vector3Int> GetDataPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            int startX = playerPosition.x - (world.WorldSettings.ChunkDrawingRange + 1) * world.WorldSettings.ChunkSize;
            int startZ = playerPosition.z - (world.WorldSettings.ChunkDrawingRange + 1) * world.WorldSettings.ChunkSize;
            int endX = playerPosition.x + (world.WorldSettings.ChunkDrawingRange + 1) * world.WorldSettings.ChunkSize;
            int endZ = playerPosition.z + (world.WorldSettings.ChunkDrawingRange + 1) * world.WorldSettings.ChunkSize;

            List<Vector3Int> chunkDataPosiionsToCreate = new List<Vector3Int>();
            for (int x = startX; x <= endX; x += world.WorldSettings.ChunkSize)
            {
                for (int z = startZ; z <= endZ; z += world.WorldSettings.ChunkSize)
                {
                    Vector3Int chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, 0, z));
                    chunkDataPosiionsToCreate.Add(chunkPos);
                    if (x >= playerPosition.x - world.WorldSettings.ChunkSize &&
                        x <= playerPosition.x + world.WorldSettings.ChunkSize &&
                        z >= playerPosition.z - world.WorldSettings.ChunkSize &&
                        z <= playerPosition.z + world.WorldSettings.ChunkSize)
                    {
                        for (int y = -world.WorldSettings.ChunkHeight; y >= playerPosition.y - world.WorldSettings.ChunkHeight * 2; y -= world.WorldSettings.ChunkHeight)
                        {
                            chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, y, z));
                            chunkDataPosiionsToCreate.Add(chunkPos);
                        }
                    }
                }
            }

            return chunkDataPosiionsToCreate;
        }

        internal static ChunkRenderer GetChunk(World worldReference, Vector3Int worldPosition)
        {
            if (worldReference.worldData.chunkDictionary.ContainsKey(worldPosition))
                return worldReference.worldData.chunkDictionary[worldPosition];
            
            return null;
        }

        internal static void SetVoxel(World worldReference, Vector3Int worldVoxelPos, VoxelType voxelType)
        {
            ChunkData chunkData = GetChunkData(worldReference, worldVoxelPos);
            if (chunkData != null)
            {
                Vector3Int localPosition = Chunk.GetVoxelInChunkCoordinates(chunkData, worldVoxelPos);
                Chunk.SetVoxel(chunkData, localPosition, voxelType);
            }
        }

        public static ChunkData GetChunkData(World worldReference, Vector3Int worldVoxelPos)
        {
            Vector3Int chunkPosition = ChunkPositionFromVoxelCoords(worldReference, worldVoxelPos);

            ChunkData containerChunk = null;

            worldReference.worldData.chunkDataDictionary.TryGetValue(chunkPosition, out containerChunk);

            return containerChunk;
        }

        internal static List<Vector3Int> GetUnneededData(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded)
        {
            return worldData.chunkDataDictionary.Keys
                .Where(pos => allChunkDataPositionsNeeded.Contains(pos) == false && worldData.chunkDataDictionary[pos].modifiedByThePlayer == false)
                .ToList();
        }

        internal static List<Vector3Int> GetUnneededChunks(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded)
        {
            List<Vector3Int> positionToRemove = new List<Vector3Int>();

            // Discard all worldData chunks not in allChunkPositionsNeeded...
            foreach (var pos in worldData.chunkDictionary.Keys
                .Where(pos => allChunkPositionsNeeded.Contains(pos) == false))
            {
                if (worldData.chunkDictionary.ContainsKey(pos)) // Note: 'this might be redundant'
                {
                    positionToRemove.Add(pos);
                }
            }

            return positionToRemove;
        }

        internal static List<Vector3Int> SelectPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded, Vector3Int playerPosition)
        {
            return allChunkPositionsNeeded
                .Where(pos => worldData.chunkDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPosition, pos))
                .ToList();
        }

        internal static List<Vector3Int> SelectDataPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded, Vector3Int playerPosition)
        {
            return allChunkDataPositionsNeeded
                .Where(pos => worldData.chunkDataDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPosition, pos))
                .ToList();
        }
    }
}

// Source: S2 - P12 https://www.youtube.com/watch?v=ev4Nm50Ujok&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=12&ab_channel=SunnyValleyStudio
// Source: S2 - P13 https://www.youtube.com/watch?v=AHkh5WNq528&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=13&ab_channel=SunnyValleyStudio
// Source: S2 - P14 https://www.youtube.com/watch?v=AvowpcZssxU&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=14&ab_channel=SunnyValleyStudio
// Source: S2 - P15 https://www.youtube.com/watch?v=qOcJDH0FfsY&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=15&ab_channel=SunnyValleyStudio
// Source: S2 - P16 https://www.youtube.com/watch?v=-PhTCTX0q5c&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=16&ab_channel=SunnyValleyStudio
// Source: S2 - P17 https://www.youtube.com/watch?v=aP6N245OjEQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=17&ab_channel=SunnyValleyStudio
// Source: S3 - P9 Adding Trees P3 https://www.youtube.com/watch?v=Pth2WPDDdqI&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=9&ab_channel=SunnyValleyStudio

