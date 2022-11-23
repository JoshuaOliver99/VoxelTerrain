using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public WorldRenderer worldRenderer;

        public TerrainGenerator terrainGenerator;
        public Vector2Int mapSeedOffset;

        CancellationTokenSource taskTokenSource = new CancellationTokenSource();

        //Dictionary<Vector3Int, ChunkData> chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();
        //Dictionary<Vector3Int, ChunkRenderer> chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();

        public UnityEvent OnWorldCreated, OnNewChunksGenerated;

        public WorldData worldData { get; private set; }
        public bool IsWorldCreated { get; private set; }

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

        public void OnDisable()
        {
            taskTokenSource.Cancel();
        }

        public async void GenerateWorld()
        {
            await GenerateWorld(Vector3Int.zero);
        }

        private async Task GenerateWorld(Vector3Int position)
        {
            WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsThatPlayerSees(position), taskTokenSource.Token);


            // Remove the old chunks...
            foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
                WorldDataHelper.RemoveChunk(this, pos);


            // Remove the old chunks data...
            foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
                WorldDataHelper.RemoveChunkData(this, pos);


            // Generate the ChunkData...
            ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;
            try
            {
                dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
            }
            catch (Exception)
            {
                Debug.Log("CalculateWorldChunkData Task Canceled");
                return;
            }

            foreach (KeyValuePair<Vector3Int, ChunkData> calculatedData in dataDictionary)
            {
                worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
            }

            // Done after all chunks are generated
            foreach (var chunkData in worldData.chunkDataDictionary.Values)
            {
                AddTreeLeafs(chunkData);
            }


            // Generate the MeshData...
            ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

            List<ChunkData> dataToRender = worldData.chunkDataDictionary
                .Where(KeyValuePair => worldGenerationData.chunkPositionsToCreate.Contains(KeyValuePair.Key))
                .Select(keyValuePair => keyValuePair.Value)
                .ToList();

            try
            {
                meshDataDictionary = await CreateMeshDataAsync(dataToRender);
            }
            catch (Exception)
            {
                Debug.Log("CreateMeshDataAsync Task Canceled");
                return;
            }



            StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
        }

        private void AddTreeLeafs(ChunkData chunkData)
        {
            foreach (Vector3Int treeLeafes in chunkData.treeData.treeLeafsSolid)
            {
                Chunk.SetVoxel(chunkData, treeLeafes, VoxelType.TreeLeavesSolid);
            }
        }

        private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
        {
            ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

            return Task.Run(() =>
            {
                foreach (ChunkData data in dataToRender)
                {
                    if (taskTokenSource.Token.IsCancellationRequested)
                        taskTokenSource.Token.ThrowIfCancellationRequested();

                    MeshData meshData = Chunk.GetChunkMeshData(data);
                    dictionary.TryAdd(data.worldPosition, meshData);
                }
                return dictionary;
            },
            taskTokenSource.Token);
        }

        private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
        {
            ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

            return Task.Run(() =>
            {
                foreach (Vector3Int pos in chunkDataPositionsToCreate)
                {
                    if (taskTokenSource.Token.IsCancellationRequested)
                        taskTokenSource.Token.ThrowIfCancellationRequested();

                    ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                    ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                    dictionary.TryAdd(pos, newData);
                }
                return dictionary;
            }, 
            taskTokenSource.Token);
        }

        IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary)
        {
            foreach (var item in meshDataDictionary)
            {
                CreateChunk(worldData, item.Key, item.Value);
                yield return new WaitForEndOfFrame();
            }
            
            if (IsWorldCreated == false)
            {
                IsWorldCreated = true;
                OnWorldCreated?.Invoke();
            }


        }

        private void CreateChunk(WorldData worldData, Vector3Int worldPos, MeshData meshData)
        {
            ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, worldPos, meshData);
            worldData.chunkDictionary.Add(worldPos, chunkRenderer);
        }

        internal bool SetVoxel(RaycastHit hit, VoxelType voxelType)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hardcoded...
            ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
            if (chunk == null)
                return false;

            Vector3Int pos = GetVoxelPos(hit);

            WorldDataHelper.SetVoxel(chunk.ChunkData.worldReference, pos, voxelType);
            chunk.ModifiedByThePlayer = true;

            if (Chunk.IsOnEdge(chunk.ChunkData, pos))
            {
                List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
                foreach (ChunkData neighbourData in neighbourDataList)
                {
                    //neighbourData.modifiedByThePlayer = true;
                    ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);

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
                chunkDataToRemove = chunkDataToRemove,
            };
            return data;
        }

        internal async void LoadAdditionalChunkRequest(GameObject player)
        {
            Debug.Log($"[{name}] Loading more chunks");
            await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
            OnNewChunksGenerated?.Invoke();
        }

        internal VoxelType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
        {
            Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
            ChunkData containerChunk = null;

            worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

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
    }

    public struct WorldData
    {
        public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
        public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
        public int chunkSize;
        public int chunkHeight;
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
// Source: S3 - P1 Intro to multithreading https://www.youtube.com/watch?v=oWFJl56IL4Y&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=1&ab_channel=SunnyValleyStudio
// Source: S3 - P2 Async & Await in Unity https://www.youtube.com/watch?v=Jgwd7IDmcSA&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=3&ab_channel=SunnyValleyStudio
// Source: S3 - P3 Making our code multithreaded P1 https://www.youtube.com/watch?v=RfFKm7UY2q4&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=3&ab_channel=SunnyValleyStudio
// Source: S3 - P4 Making our code multithreaded P2 https://www.youtube.com/watch?v=eQSZLJaiVBs&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=4&ab_channel=SunnyValleyStudio
// Source: S3 - P5 Stopping async Tasks https://www.youtube.com/watch?v=Wyl5vE-5-2I&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=5&ab_channel=SunnyValleyStudio
// Source: S3 - P6 Object Pooling chunks https://www.youtube.com/watch?v=qc73lfMirw8&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=6&ab_channel=SunnyValleyStudio
// Source: S3 - P9 Adding Trees P3 https://www.youtube.com/watch?v=Pth2WPDDdqI&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=9&ab_channel=SunnyValleyStudio


// Source: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-7.0
// Source: https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation



