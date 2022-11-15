using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SunnyValleyStudio
{
    public class World : MonoBehaviour
    {
        public int mapSizeChunks = 6;
        public int chunkSize = 16;
        public int chunkHeight = 100;
        public int chunkDrawingRange = 8;

        public GameObject chunkPrefab;

        public TerrainGenerator terrainGenerator;
        public Vector2Int mapSeedOffset;

        //Dictionary<Vector3Int, ChunkData> chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();
        //Dictionary<Vector3Int, ChunkRenderer> chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();

        public UnityEvent OnWorldCreated, OnNewChunksGenerated;

        public WorldData worldData { get; private set; }

        private void Awake()
        {
            worldData = new WorldData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };
        }

        public void GenerateWorld()
        {
            GenerateWorld(Vector3Int.zero);
        }

        private void GenerateWorld(Vector3Int position)
        {
            WorldGenerationData worldGenerationData = GetPositionsThatPlayerSees(position);

            // Remove the old chunks...
            foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
                WorldDataHelper.RemoveChunk(this, pos);

            // Remove the old chunks data...
            foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
                WorldDataHelper.RemoveChunkData(this, pos);

            // Generate the chunks and voxel data...
            foreach (Vector3Int pos in worldGenerationData.chunkDataPositionsToCreate)
            {
                ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                worldData.chunkDataDictionary.Add(pos, newData);
            }

            // Render the chunks...
            foreach (Vector3Int pos in worldGenerationData.chunkPositionsToCreate)
            {
                ChunkData data = worldData.chunkDataDictionary[pos];
                MeshData meshData = Chunk.GetChunkMeshData(data);
                GameObject chunkObject = Instantiate(chunkPrefab, pos, Quaternion.identity);
                ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();
                worldData.chunkDictionary.Add(pos, chunkRenderer);
                chunkRenderer.InitializeChunk(data);
                chunkRenderer.UpdateChunk(meshData); 
            }

            OnWorldCreated?.Invoke();
        }

        internal void RemoveChunk(ChunkRenderer chunk)
        {
            chunk.gameObject.SetActive(false);
        }

        private WorldGenerationData GetPositionsThatPlayerSees(Vector3Int playerPosition)
        {
            List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);
            List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, playerPosition);
            
            List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositionsToCreate(worldData, allChunkPositionsNeeded, playerPosition);
            List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositionsToCreate(worldData, allChunkDataPositionsNeeded, playerPosition);

            List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnneededChunks(worldData, allChunkPositionsNeeded);
            List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnneededData(worldData, allChunkDataPositionsNeeded);

            WorldGenerationData data = new WorldGenerationData
            {
                chunkPositionsToCreate = chunkPositionsToCreate,
                chunkDataPositionsToCreate = chunkDataPositionsToCreate,
                chunkPositionsToRemove = chunkPositionsToRemove,
                chunkDataToRemove = chunkDataToRemove
            };
            return data;
        }

        internal void LoadAdditionalChunkRequest(GameObject player)
        {
            Debug.Log($"[{name}] Load more chunks");
            GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
            OnNewChunksGenerated?.Invoke();
        }


        /// <summary>
        /// V1 (deprecated)
        /// Generate the different types of voxels.
        /// Replaced by TerrainGenerator.GenerateChunkData()
        /// </summary>
        /// <param name="data"></param>
#if false
        private void GenerateVoxels(ChunkData data)
        {
            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
                    // NOTE: From here has been seperated...
                    // Replaced by BiomeGenerator.ProcessChunkColumn()

                    float noiseValue = Mathf.PerlinNoise((data.worldPosition.x + x) * noiseScale, (data.worldPosition.z + z) * noiseScale);
                    int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);

                    for (int y = 0; y < chunkHeight; y++)
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
                }
            }
        }
#endif

        internal VoxelType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
        {
            Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
            ChunkData containerChunk = null;

            worldData.chunkDataDictionary   .TryGetValue(pos, out containerChunk);

            if (containerChunk == null)
                return VoxelType.Nothing;

            Vector3Int blockInChunkCoordinates = Chunk.GetVoxelInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
            return Chunk.GetVoxelFromChunkCoordinates(containerChunk, blockInChunkCoordinates);
        }

        public struct WorldGenerationData
        {
            public List<Vector3Int> chunkPositionsToCreate;
            public List<Vector3Int> chunkDataPositionsToCreate;
            public List<Vector3Int> chunkPositionsToRemove;
            public List<Vector3Int> chunkDataToRemove;
        }

        public struct WorldData
        {
            public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
            public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
            public int chunkSize;
            public int chunkHeight;
        }
    }
}

// Source: https://www.youtube.com/watch?v=OObDevIzwcQ&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=L5obsaFeJPQ&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=TOLlDa2XTbQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=3&ab_channel=SunnyValleyStudio
// Source: S2 - P12 https://www.youtube.com/watch?v=ev4Nm50Ujok&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=12&ab_channel=SunnyValleyStudio
// Source: S2 - P13 https://www.youtube.com/watch?v=AHkh5WNq528&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=13&ab_channel=SunnyValleyStudio
// Source: S2 - P14 https://www.youtube.com/watch?v=AvowpcZssxU&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=14&ab_channel=SunnyValleyStudio
// Source: S2 - P15 https://www.youtube.com/watch?v=qOcJDH0FfsY&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=15&ab_channel=SunnyValleyStudio


