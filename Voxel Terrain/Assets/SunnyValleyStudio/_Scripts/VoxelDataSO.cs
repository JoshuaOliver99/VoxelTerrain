using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    [CreateAssetMenu(fileName = "Block Data", menuName = "Data/Block Data")]
    public class VoxelDataSO : ScriptableObject
    {
        public float textureSizeX, textureSizeY;
        public List<TextureData> textureDataList;
    }

    [Serializable]
    public class TextureData
    {
        public VoxelType voxelType;
        public Vector2Int up, down, side; // (uv coordinates)
        public bool isSolid = true;
        public bool generatesCollider = true;
    }
}
// Source: https://www.youtube.com/watch?v=qWx1YPx-IpI&ab_channel=SunnyValleyStudio