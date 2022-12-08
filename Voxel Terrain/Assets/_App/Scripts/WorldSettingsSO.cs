using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "Voxel World Settings/World Settings")]
public class WorldSettingsSO : ScriptableObject
{
    [SerializeField, Tooltip("")]
    private int mapSizeChunks = 6;
    [SerializeField, Tooltip("")]
    private int chunkSize = 16;
    [SerializeField, Tooltip("")]
    private int chunkHeight = 64;
    [SerializeField, Tooltip("")]
    private int chunkDrawingRange = 8;

    [SerializeField, Tooltip("")]
    private Vector2Int mapSeedOffset;

    public int MapSizeChunks { get => mapSizeChunks; }
    public int ChunkSize { get => chunkSize; }
    public int ChunkHeight { get => chunkHeight; }
    public int ChunkDrawingRange { get => chunkDrawingRange; }
    public Vector2Int MapSeedOffset { get => mapSeedOffset; }
}
