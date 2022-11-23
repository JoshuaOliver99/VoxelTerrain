using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class ChunkData
    {
        // NOTE: Change everything from public
        public VoxelType[] voxel;
        public int chunkSize = 16;
        public int chunkHeight = 100;
        public World worldReference;
        public Vector3Int worldPosition;

        public bool modifiedByThePlayer = false; // NOTE: potential rename "ChunkDirty"?
        public TreeData treeData;

        public ChunkData(int chunkSize, int chunkHeight, World world, Vector3Int worldPosition)        {
            this.chunkSize = chunkSize; // NOTE: SunnyValleyStudio has these swapped...
            this.chunkHeight = chunkHeight; // NOTE: SunnyValleyStudio has these swapped...
            this.worldReference = world;
            this.worldPosition = worldPosition;
            voxel = new VoxelType[chunkSize * chunkHeight * chunkSize];
        }

    }
}

// Source: https://www.youtube.com/watch?v=OObDevIzwcQ&ab_channel=SunnyValleyStudio
// Source: S3 - P7 Adding Trees P1 https://www.youtube.com/watch?v=iifH1zHjxA4&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio
// Source: S3 - P8 Adding Trees P2 https://www.youtube.com/watch?v=IPjzsLV8jd8&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio


