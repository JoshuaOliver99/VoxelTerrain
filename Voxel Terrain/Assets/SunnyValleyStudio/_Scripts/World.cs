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

        internal bool SetVoxel(RaycastHit hit, VoxelType voxelType)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hardcoded...
            ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
            if (chunk == false)
                return false;

            Vector3Int pos = GetVoxelPos(hit);

            WorldDataHelper.SetVoxel(chunk.ChunkData.worldReference, pos, voxelType);
            chunk.ModifiedByThePlayer = true;

            if (Chunk.IsOnEdge(chunk.ChunkData, pos))
            {
                List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
                foreach (ChunkData neighbourData in neighbourDataList)
                {
                    // NOTE: BUG: it seems like chunks < 0 are not getting spawned
                    print($"TEST FAILING > {neighbourData.worldPosition}");
                    print($"TEST FAILING > {neighbourData.worldReference.name}");

                    //neighbourData.modifiedByThePlayer = true;
                    ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);

                    print($"TEST FAILING > {neighbourData}");

                    if (chunkToUpdate != null)
                        chunkToUpdate.UpdateChunk();
                }
            }

            chunk.UpdateChunk();
            return true;
        }

        private Vector3Int GetVoxelPos(RaycastHit hit)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hardcoded...
            Vector3 pos = new Vector3(
                GetVoxelPositionIn(hit.point.x, hit.normal.x),
                GetVoxelPositionIn(hit.point.y, hit.normal.y),
                GetVoxelPositionIn(hit.point.z, hit.normal.z));

            return Vector3Int.RoundToInt(pos);
        }

        private float GetVoxelPositionIn(float pos, float normal)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hardcoded...
            if (Mathf.Abs(pos % 1) == 0.5f)
                pos -= (normal / 2);

            return (float)pos;
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
// Source: S2 - P16 https://www.youtube.com/watch?v=-PhTCTX0q5c&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=16&ab_channel=SunnyValleyStudio
// Source: S2 - P17 https://www.youtube.com/watch?v=aP6N245OjEQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=17&ab_channel=SunnyValleyStudio

