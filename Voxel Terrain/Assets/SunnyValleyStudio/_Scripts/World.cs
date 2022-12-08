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
    [RequireComponent(typeof(WorldRenderer))]
    public class World : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("")]
        private WorldSettingsSO worldSettings;
        public WorldSettingsSO WorldSettings { get => worldSettings; }


        //[SerializeField]
        //private GameObject chunkPrefab; // Note: Seemingly unused here

        [Header("References")]
        private WorldRenderer worldRenderer;
        public WorldRenderer WorldRenderer { get => worldRenderer; }
        
        [SerializeField]
        private TerrainGenerator terrainGenerator;


        [Header("Data")]
        [Tooltip("")]
        private CancellationTokenSource taskTokenSource = new CancellationTokenSource();
        public WorldData worldData { get; private set; }
        public bool IsWorldCreated { get; private set; }


        [Header("Unity Events")]
        public UnityEvent OnWorldCreated;
        public UnityEvent OnNewChunksGenerated;


        private void Awake()
        {
            worldRenderer = GetComponent<WorldRenderer>();

            worldData = new WorldData
            {
                chunkHeight = worldSettings.ChunkHeight,
                chunkSize = worldSettings.ChunkSize,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };
        }

        private void OnDisable()
        {
            taskTokenSource.Cancel();
        }


        public async void GenerateWorld()
        {
            await GenerateWorld(Vector3Int.zero);
        }

        private async Task GenerateWorld(Vector3Int position)
        {
            terrainGenerator.GenerateBiomePoints(position, worldSettings.ChunkDrawingRange, worldSettings.ChunkSize, worldSettings.MapSeedOffset);

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


            // Generate structures
            // (after all chunks are generated... because these cross chunks(?))
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
            ConcurrentDictionary<Vector3Int, MeshData> dictionary = new();

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
            ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new();

            return Task.Run(() =>
            {
                foreach (Vector3Int pos in chunkDataPositionsToCreate)
                {
                    if (taskTokenSource.Token.IsCancellationRequested)
                        taskTokenSource.Token.ThrowIfCancellationRequested();

                    ChunkData data = new(worldSettings.ChunkSize, worldSettings.ChunkHeight, this, pos);
                    ChunkData newData = terrainGenerator.GenerateChunkData(data, worldSettings.MapSeedOffset);
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
                CreateChunkRenderer(worldData, item.Key, item.Value);
                yield return new WaitForEndOfFrame();
            }

            if (IsWorldCreated == false)
            {
                IsWorldCreated = true;
                OnWorldCreated?.Invoke();
            }
        }

        public void CreateChunkRenderer(WorldData worldData, Vector3Int worldPos, MeshData meshData)
        {
            ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, worldPos, meshData);
            worldData.chunkDictionary.Add(worldPos, chunkRenderer);
        }

        internal bool SetVoxel(RaycastHit hit, VoxelType voxelType)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hard coded...
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

        #region Josh Tests
        /// <summary>
        /// TEST Josh mod 
        /// </summary>
        public void CreateNewChunk(RaycastHit hit, VoxelType voxelType)
        {
            WorldData newWorldData = new WorldData
            {
                chunkHeight = worldSettings.ChunkHeight,
                chunkSize = worldSettings.ChunkSize,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };

            // Try round to nearest multiple
            int x = (int)Math.Round((hit.point.x / (double)worldSettings.ChunkSize), MidpointRounding.AwayFromZero) * worldSettings.ChunkSize;
            int y = (int)Math.Round((hit.point.y / (double)worldSettings.ChunkHeight), MidpointRounding.AwayFromZero) * worldSettings.ChunkHeight;
            int z = (int)Math.Round((hit.point.z / (double)worldSettings.ChunkSize), MidpointRounding.AwayFromZero) * worldSettings.ChunkSize;

            Vector3Int hitRoundedToChunk = new(x, y, z);
            Debug.Log(hitRoundedToChunk);

            ChunkData data = new ChunkData(worldSettings.ChunkSize, worldSettings.ChunkHeight, this, hitRoundedToChunk);

            newWorldData.chunkDataDictionary.Add(hitRoundedToChunk, data);


            Vector3Int newChunkPos = new Vector3Int
            {
                x = Mathf.RoundToInt(hit.point.x),
                y = Mathf.RoundToInt(hit.point.y),
                z = Mathf.RoundToInt(hit.point.z),
            };

            MeshData newMeshData = new MeshData(true);

            ChunkRenderer newChunkRenderer = CreateChunkRendererAndReturn(newWorldData, newChunkPos, newMeshData);

            SetVoxelOnChunk(hit, voxelType, newChunkRenderer);
        }

        internal bool SetVoxelOnChunk(RaycastHit hit, VoxelType voxelType, ChunkRenderer chunkRender)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hard coded...
            
            //ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
            //if (chunk == null)
            //    return false;
            
            Vector3Int pos = GetVoxelPos(hit);

            WorldDataHelper.SetVoxel(chunkRender.ChunkData.worldReference, pos, voxelType);
            chunkRender.ModifiedByThePlayer = true;

            if (Chunk.IsOnEdge(chunkRender.ChunkData, pos))
            {
                List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunkRender.ChunkData, pos);
                foreach (ChunkData neighbourData in neighbourDataList)
                {
                    //neighbourData.modifiedByThePlayer = true;
                    ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);

                    if (chunkToUpdate != null)
                        chunkToUpdate.UpdateChunk();
                }
            }

            chunkRender.UpdateChunk();
            return true;
        }

        public ChunkRenderer CreateChunkRendererAndReturn(WorldData worldData, Vector3Int worldPos, MeshData meshData)
        {
            ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, worldPos, meshData);
            worldData.chunkDictionary.Add(worldPos, chunkRenderer);
            return chunkRenderer;
        }
        #endregion



        private Vector3Int GetVoxelPos(RaycastHit hit)
        {
            // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
            // Hard coded...
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
// Source: S3 - P11 Different biomes theory https://www.youtube.com/watch?v=NIiREmJnAX0&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=12&ab_channel=SunnyValleyStudio



// Source: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-7.0
// Source: https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation



