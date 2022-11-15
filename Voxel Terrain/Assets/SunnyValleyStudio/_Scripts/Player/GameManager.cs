using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class GameManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        private GameObject player;
        public Vector3Int currentPlayerChunkPosition;
        private Vector3Int currentChunkCentre = Vector3Int.zero;

        public World world;

        public float detectionTime = 1;
        public CinemachineVirtualCamera camera_VM;

        public void SpawnPlayer()
        {
            if (player != null)
                return;

            Vector3Int raycastStartPosition = new Vector3Int(world.chunkSize / 2, 100, world.chunkSize / 2);
            RaycastHit hit;
            if (Physics.Raycast(raycastStartPosition, Vector3.down, out hit, 120))
            {
                player = Instantiate(playerPrefab, hit.point + Vector3Int.up, Quaternion.identity);
                camera_VM.Follow = player.transform.GetChild(0);
                StartCheckTheMap();
            }
        }

        public void StartCheckTheMap()
        {
            SetCurrentChunkCoordinated();
            StopAllCoroutines();
            StartCoroutine(CheckIfShouldLoadNextPosition());
        }

        IEnumerator CheckIfShouldLoadNextPosition()
        {
            yield return new WaitForSeconds(detectionTime);
            if (Mathf.Abs(currentChunkCentre.x - player.transform.position.x) > world.chunkSize ||
                Mathf.Abs(currentChunkCentre.z - player.transform.position.z) > world.chunkSize ||
                (Mathf.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeight))
            {
                world.LoadAdditionalChunkRequest(player);
            }
            else
            {
                StartCoroutine(CheckIfShouldLoadNextPosition());
            }

        }

        private void SetCurrentChunkCoordinated()
        {
            currentPlayerChunkPosition = WorldDataHelper.ChunkPositionFromBlockCoords(world, Vector3Int.RoundToInt(player.transform.position));
            currentChunkCentre.x = currentPlayerChunkPosition.x + world.chunkSize / 2;
            currentChunkCentre.z = currentPlayerChunkPosition.z + world.chunkSize / 2;
        }
    }
}

// Source: S2 - P12 https://www.youtube.com/watch?v=ev4Nm50Ujok&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=12&ab_channel=SunnyValleyStudio