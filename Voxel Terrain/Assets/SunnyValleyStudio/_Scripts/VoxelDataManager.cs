using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class VoxelDataManager : MonoBehaviour
    {
        public static float textureOffset = 0.001f;
        public static float tileSezeX, tileSezeY;
        public static Dictionary<VoxelType, TextureData> voxelTextureDataDictionary = new Dictionary<VoxelType, TextureData>();
        public VoxelDataSO textureData;

        private void Awake()
        {
            foreach (var item in textureData.textureDataList)
            {
                if (voxelTextureDataDictionary.ContainsKey(item.voxelType) == false)
                {
                    voxelTextureDataDictionary.Add(item.voxelType, item);
                }
            }
            tileSezeX = textureData.textureSizeX;
            tileSezeY = textureData.textureSizeY;
        }
    }
}

// Source: https://www.youtube.com/watch?v=VcR8pW3YNQI&ab_channel=SunnyValleyStudio