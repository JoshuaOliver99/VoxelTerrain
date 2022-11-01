using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class ChunkData
    {
        // NOTE: Change everything from public
        public BlockType[] blocks;
        public int chunkSize = 16;
        public int chunkHeight = 128;
        public World worldReference;
        public Vector3Int worldPosition;

        public bool modifiedByThePlayer = false; // NOTE: potential rename "ChunkDirty"?

        public ChunkData(int chunkSize, int chunkHeight, World world, Vector3Int worldPosition)        {
            this.chunkSize = chunkSize; // NOTE: SunnyValleyStudio has these swapped...
            this.chunkHeight = chunkHeight; // NOTE: SunnyValleyStudio has these swapped...
            this.worldReference = world;
            this.worldPosition = worldPosition;
            blocks = new BlockType[chunkSize * chunkHeight * chunkSize];
        }

    }
}

// Source: https://www.youtube.com/watch?v=OObDevIzwcQ&ab_channel=SunnyValleyStudio