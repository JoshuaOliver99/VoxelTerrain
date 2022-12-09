using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunnyValleyStudio;
using System;

/// <summary>
/// Variation of the World class to be used for player specific view & functionality
/// </summary>
public class KeeperWorld : World
{



    public void TagVoxel(Vector3 worldPosition)
    {
        // if voxel is already tagged => Untag
        // else => tag


        // Get voxel type...
        Vector3Int newWorldPosition = new(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), Mathf.RoundToInt(worldPosition.z));
        ChunkData chunkData = WorldDataHelper.GetChunkData(this, newWorldPosition);

        Vector3Int voxelChunkPos = new(
            newWorldPosition.x - chunkData.worldPosition.x,
            newWorldPosition.y - chunkData.worldPosition.z,
            newWorldPosition.z - chunkData.worldPosition.y);

        VoxelType targetVoxel = Chunk.GetVoxelFromChunkCoordinates(chunkData, voxelChunkPos);

        Debug.Log(
            $"world positon > {newWorldPosition}, " +
            $"chunk positon > {voxelChunkPos}, " +
            $"voxel > {targetVoxel}");


        // (ensure we have the renderer...)
        Vector3Int chunkPosition = WorldDataHelper.ChunkPositionFromVoxelCoords(this, voxelChunkPos);
        ChunkRenderer chunkRenderer = WorldDataHelper.GetChunk(this, chunkPosition);


        // Determine if tagged and set
        if (targetVoxel == VoxelType.Nothing || targetVoxel == VoxelType.Air)
        {
            Debug.Log($"Tagging!");
            SetVoxel(chunkRenderer, newWorldPosition, VoxelType.TreeLeavesSolid);
        }
        else
        {
            Debug.Log($"Untagging!");
            SetVoxel(chunkRenderer, newWorldPosition, VoxelType.Air);
        }

    }

    public void CheckVoxelTagStatus()
    {
        // if tagged and World voxel was deleted => untag 
    }


    internal bool SetVoxel(ChunkRenderer chunkRenderer, Vector3Int worldPositon, VoxelType voxelType)
    {
        // Note: S2 - P16: I dont think this implementation will work for voxels != 1m ...
        // Hard coded...

        //if (chunk == null)
        //    return false;

        Vector3Int pos = worldPositon;
        
        WorldDataHelper.SetVoxel(chunkRenderer.ChunkData.worldReference, pos, voxelType);
        chunkRenderer.ModifiedByThePlayer = true;

        if (Chunk.IsOnEdge(chunkRenderer.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunkRenderer.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);

                if (chunkToUpdate != null)
                    chunkToUpdate.UpdateChunk();
            }
        }

        chunkRenderer.UpdateChunk();
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
            chunkHeight = WorldSettings.ChunkHeight,
            chunkSize = WorldSettings.ChunkSize,
            chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
            chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
        };

        // Try round to nearest multiple
        int x = (int)Math.Round((hit.point.x / (double)WorldSettings.ChunkSize), MidpointRounding.AwayFromZero) * WorldSettings.ChunkSize;
        int y = (int)Math.Round((hit.point.y / (double)WorldSettings.ChunkHeight), MidpointRounding.AwayFromZero) * WorldSettings.ChunkHeight;
        int z = (int)Math.Round((hit.point.z / (double)WorldSettings.ChunkSize), MidpointRounding.AwayFromZero) * WorldSettings.ChunkSize;

        Vector3Int hitRoundedToChunk = new(x, y, z);
        Debug.Log(hitRoundedToChunk);

        ChunkData data = new ChunkData(WorldSettings.ChunkSize, WorldSettings.ChunkHeight, this, hitRoundedToChunk);

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
        ChunkRenderer chunkRenderer = WorldRenderer.RenderChunk(worldData, worldPos, meshData);
        worldData.chunkDictionary.Add(worldPos, chunkRenderer);
        return chunkRenderer;
    }
    #endregion
}
