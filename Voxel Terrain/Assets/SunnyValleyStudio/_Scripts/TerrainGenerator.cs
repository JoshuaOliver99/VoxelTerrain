using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class TerrainGenerator : MonoBehaviour
    {
        public BiomeGenerator biomeGenerator;

        [SerializeField]
        List<Vector3Int> biomeCentres = new List<Vector3Int>();
        List<float> biomeNoise = new List<float>();

        [SerializeField]
        private NoiseDataSO biomeNoiseSettings;

        public DomainWarping biomeDomainWarping;

        [SerializeField]
        private List<BiomeData> biomeGeneratorsData = new List<BiomeData>();

        public ChunkData GenerateChunkData(ChunkData data, Vector2Int mapSeedOffset)
        {
            BiomeGeneratorSelection biomeSelection = SelectBiomeGenerator(data.worldPosition, data, false);

            //TreeData treeData = biomeGenerator.GetTreeData(data, mapSeedOffset);
            data.treeData = biomeSelection.biomeGenerator.GetTreeData(data, mapSeedOffset);

            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
                    biomeSelection = SelectBiomeGenerator(new Vector3Int(data.worldPosition.x + x, 0, data.worldPosition.z + z), data);
                    data = biomeSelection.biomeGenerator.ProcessChunkColumn(data, x, z, mapSeedOffset, biomeSelection.terrainSurfaceNoise);
                }
            }
            return data;
        }

        private BiomeGeneratorSelection SelectBiomeGenerator(Vector3Int worldPositon, ChunkData data, bool useDomainWarping = true)
        {
            if (useDomainWarping == true)
            {
                Vector2Int domainOffset = Vector2Int.RoundToInt(biomeDomainWarping.GenerateDomainOffset(worldPositon.x, worldPositon.z));
                worldPositon += new Vector3Int(domainOffset.x, 0, domainOffset.y);
            }

            List<BiomeSelectionHelper> biomeSelectionHelpers = GetBiomeGeneratorSelectionHelpers(worldPositon);
            BiomeGenerator generator_1 = SelectBiome(biomeSelectionHelpers[0].Index);
            BiomeGenerator generator_2 = SelectBiome(biomeSelectionHelpers[1].Index);

            float distance = Vector3.Distance(
                biomeCentres[biomeSelectionHelpers[0].Index], 
                biomeCentres[biomeSelectionHelpers[1].Index]);

            float weight_0 = biomeSelectionHelpers[1].Distance / distance;
            float weight_1 = 1 - weight_0;
            int terrainHeightNoise_0 = generator_1.GetSurfaceHeightNoise(worldPositon.x, worldPositon.z, data.chunkHeight);
            int terrainHeightNoise_1 = generator_2.GetSurfaceHeightNoise(worldPositon.x, worldPositon.z, data.chunkHeight);

            return new BiomeGeneratorSelection(generator_1, Mathf.RoundToInt(terrainHeightNoise_0 * weight_0 + terrainHeightNoise_1 * weight_1));
        }

        private BiomeGenerator SelectBiome(int index)
        {
            float temp = biomeNoise[index];
            foreach (var data in biomeGeneratorsData)
            {
                if (temp >= data.temperatureStartThreshold && temp < data.temperatureEndThreshold)
                    return data.biomeTerrainGenerator;
            }
            return biomeGeneratorsData[0].biomeTerrainGenerator;
        }

        private List<BiomeSelectionHelper> GetBiomeGeneratorSelectionHelpers(Vector3Int position)
        {
            position.y = 0;
            return GetClosestBiomeIndex(position);
        }

        private struct BiomeSelectionHelper
        {
            public int Index;
            public float Distance;
        }

        private List<BiomeSelectionHelper> GetClosestBiomeIndex(Vector3Int position)
        {
            return biomeCentres.Select((centre, index) => 
            new BiomeSelectionHelper
            {
                Index = index,
                Distance = Vector3.Distance(centre, position)
            })
                .OrderBy(helper => helper.Distance)
                .Take(4)
                .ToList();
        }

        public void GenerateBiomePoints(Vector3 playerPosition, int drawRange, int mapSize, Vector2Int mapSeedOffset)
        {
            biomeCentres = new List<Vector3Int>();
            biomeCentres = BiomeCentreFinder.CalculateBiomeCentres(playerPosition, drawRange, mapSize);

            for (int i = 0; i < biomeCentres.Count; i++)
            {
                Vector2Int domainWarpingOffset = biomeDomainWarping.GenerateDomainOffsetInt(biomeCentres[i].x, biomeCentres[i].y);
                biomeCentres[i] += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
            }

            biomeNoise = CalculateBiomeNoise(biomeCentres, mapSeedOffset);
        }

        private List<float> CalculateBiomeNoise(List<Vector3Int> biomeCentres, Vector2Int mapSeedOffset)
        {
            biomeNoiseSettings.worldOffset = mapSeedOffset;

            return biomeCentres.Select(centre => MyNoise.OctavePerlin(centre.x, centre.y, biomeNoiseSettings)).ToList();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            foreach (var biomeCentrePoint in biomeCentres)
            {
                Gizmos.DrawLine(biomeCentrePoint, biomeCentrePoint + Vector3.up * 255);
            }
        }
    }

    [Serializable]
    public struct BiomeData
    {
        [Range(0f, 1f)]
        public float temperatureStartThreshold, temperatureEndThreshold;
        public BiomeGenerator biomeTerrainGenerator;
    }

    public class BiomeGeneratorSelection
    {
        public BiomeGenerator biomeGenerator = null;
        public int? terrainSurfaceNoise = null;

        public BiomeGeneratorSelection(BiomeGenerator biomeGenerator, int? terrainSurfaceNoise = null)
        {
            this.biomeGenerator = biomeGenerator;
            this.terrainSurfaceNoise = terrainSurfaceNoise;
        }
    }
}

// Source: https://www.youtube.com/watch?v=TOLlDa2XTbQ&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=3&ab_channel=SunnyValleyStudio
// Source: S3 - P7 Adding Trees P1 https://www.youtube.com/watch?v=iifH1zHjxA4&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=8&ab_channel=SunnyValleyStudio
// Source: S3 - P11 Different biomes theory https://www.youtube.com/watch?v=NIiREmJnAX0&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=12&ab_channel=SunnyValleyStudio
// Source: S3 - P12 Creating desert biome https://www.youtube.com/watch?v=ePAASGHndNM&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=12&ab_channel=SunnyValleyStudio
// Source: S3 - P13 Biome selection algorithm P1 https://www.youtube.com/watch?v=ctMBEDc_-Sw&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=13&ab_channel=SunnyValleyStudio
// Source: S3 - P14 Biome selection algorithm P2 https://www.youtube.com/watch?v=OiwjJ9UI9KM&list=PLcRSafycjWFceHTT-m5wU51oVlJySCJbr&index=14&ab_channel=SunnyValleyStudio