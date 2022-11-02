using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SunnyValleyStudio
{
    public class World : MonoBehaviour
    {
        public int mapSizeChunks = 6;
        public int chunkSize = 16;
        public int chunkHeight = 100;
        public int waterThreshold = 50; // (water level)
        public float noiseScale = 0.03f;
        public GameObject chunkPrefab;

        Dictionary<Vector3Int, ChunkData> chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();
        Dictionary<Vector3Int, ChunkRenderer> chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();

        public void GenerateWorld()
        {
            chunkDataDictionary.Clear();

            foreach (ChunkRenderer chunk in chunkDictionary.Values)
                Destroy(chunk.gameObject);

            chunkDictionary.Clear();

            for (int x = 0; x < mapSizeChunks; x++)
            {
                for (int z = 0; z < mapSizeChunks; z++)
                {
                    ChunkData data = new ChunkData(chunkSize, chunkHeight, this, new Vector3Int(x * chunkSize, 0, z * chunkSize));
                    GenerateVoxels(data);
                    chunkDataDictionary.Add(data.worldPosition, data);
                }
            }

            foreach (ChunkData data in chunkDataDictionary.Values)
            {
                MeshData meshData = Chunk.GetChunkMeshData(data);
                GameObject chunkObject = Instantiate(chunkPrefab, data.worldPosition, Quaternion.identity);
                ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();

                chunkDictionary.Add(data.worldPosition, chunkRenderer);
                chunkRenderer.InitializeChunk(data);
                chunkRenderer.UpdateChunk(meshData);
            }
        }

        

        private void GenerateVoxels(ChunkData data)
        {
            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
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

        internal VoxelType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
        {
            Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
            ChunkData containerChunk = null;

            chunkDataDictionary.TryGetValue(pos, out containerChunk);

            if (containerChunk == null)
                return VoxelType.Nothing;

            Vector3Int blockInChunkCoordinates = Chunk.GetVoxelInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
            return Chunk.GetVoxelFromChunkCoordinates(containerChunk, blockInChunkCoordinates);
        }
    }
}

// Source: https://www.youtube.com/watch?v=OObDevIzwcQ&ab_channel=SunnyValleyStudio
// Source: https://www.youtube.com/watch?v=L5obsaFeJPQ&ab_channel=SunnyValleyStudio