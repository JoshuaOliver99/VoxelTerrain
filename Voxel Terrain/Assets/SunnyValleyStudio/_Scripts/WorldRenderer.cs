using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class WorldRenderer: MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject chunkPrefab;

        [SerializeField, Tooltip("Where this WorldRenderer will spawn chunks.")]
        private Transform worldParent;
        public Transform WorldParent { get => worldParent; }

        public Queue<ChunkRenderer> chunkPool = new Queue<ChunkRenderer>();


        private void Start()
        {
            // Error Handling
            if (chunkPrefab == null) Debug.LogWarning($"[{name}] {nameof(chunkPrefab)} == null");
            if (worldParent == null) Debug.LogWarning($"[{name}] {nameof(worldParent)} == null");
        }


        public void Clear(WorldData worldData)
        {
            foreach (var item in worldData.chunkDictionary.Values)
            {
                Destroy(item.gameObject);
            }
            chunkPool.Clear();
        }

        internal ChunkRenderer RenderChunk(WorldData worldData, Vector3Int worldPos, MeshData meshData)
        {
            ChunkRenderer newChunk = null;
            if (chunkPool.Count > 0)
            {
                newChunk = chunkPool.Dequeue();
                newChunk.transform.position = worldPos;
            }
            else
            {
                GameObject chunkObject;

                if (worldParent == null)
                    chunkObject = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
                else
                    chunkObject = Instantiate(chunkPrefab, worldPos, Quaternion.identity, worldParent);
                
                newChunk = chunkObject.GetComponent<ChunkRenderer>();
            }

            newChunk.InitializeChunk(worldData.chunkDataDictionary[worldPos]);
            newChunk.UpdateChunk(meshData);
            newChunk.gameObject.SetActive(true);
            return newChunk;
        }

        public void RemoveChunk(ChunkRenderer chunk)
        {
            chunk.gameObject.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
    }
}

// Source: S3 - P6 Object Pooling chunks https://www.youtube.com/watch?v=qc73lfMirw8&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=6&ab_channel=SunnyValleyStudio

// Source: https://gameprogrammingpatterns.com/object-pool.html

