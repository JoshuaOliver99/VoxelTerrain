using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public static class DataProcessing
    {
        public static List<Vector2Int> directions = new List<Vector2Int>
        {
            // NOTE: reading these directions, they dont seem correct...
            new Vector2Int(0, 1),   // N
            new Vector2Int(1, 1),   // NE
            new Vector2Int(1, 0),   // E
            new Vector2Int(-1, 1),  // SE
            new Vector2Int(-1, 0),  // S
            new Vector2Int(-1, -1), // SW
            new Vector2Int(0, -1),  // W
            new Vector2Int(1, -1)   // NW
        };

        public static List<Vector2Int> FindLocalMaxima(float[,] dataMatrix, int xCoord, int zCoord)
        {
            List<Vector2Int> maximas = new List<Vector2Int>();

            for (int x = 0; x < dataMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < dataMatrix.GetLength(1); y++)
                {
                    float noiseVal = dataMatrix[x, y];
                    if (CheckNeighbours(dataMatrix, x, y, (neighbourNoise) => neighbourNoise < noiseVal))
                    {
                        maximas.Add(new Vector2Int(xCoord + x, zCoord + y));
                    }
                }
            }
            return maximas;
        }

        private static bool CheckNeighbours(float[,] dataMatrix, int x, int y, Func<float, bool> successCondition)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPost = new Vector2Int(x + dir.x, y + dir.y);

                // Note: "*this can be why trees will appear near each other"
                if (newPost.x < 0 || newPost.x >= dataMatrix.GetLength(0) ||
                    newPost.y < 0 || newPost.y >= dataMatrix.GetLength(0))
                {
                    continue;
                }

                if (successCondition(dataMatrix[x + dir.x, y + dir.y]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

// Source: S3 - P7 Adding Trees P1 https://www.youtube.com/watch?v=iifH1zHjxA4&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio
// Source: S3 - P8 Adding Trees P2 https://www.youtube.com/watch?v=IPjzsLV8jd8&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio

