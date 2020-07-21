using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TechCraftEngine.WorldEngine.Generators
{
    class SimpleTerrain : IRegionBuilder
    {
        public void build(Region chunk)
        {
            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                int worldX = chunk.Position.x + x + WorldSettings.SEED ;

                for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                {
                    int worldZ = chunk.Position.z + z;
                    generateTerrain(chunk, x, z, worldX, worldZ, WorldSettings.MAPHEIGHT);
                }
            }
            chunk.BuildVertexBuffers();
            chunk.Dirty = false;

        }

        protected virtual void generateTerrain(Region chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldY, int worldDepthInBlocks)
        {
            // The lower ground level is at least this high.
            int minimumGroundheight = worldDepthInBlocks / 4;
            int minimumGroundDepth = (int)(worldDepthInBlocks * 0.75f);

            float octave1 = PerlinSimplexNoise.noise(worldX * 0.0001f, worldY * 0.0001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise(worldX * 0.0005f, worldY * 0.0005f) * 0.25f;
            float octave3 = PerlinSimplexNoise.noise(worldX * 0.005f, worldY * 0.005f) * 0.12f;
            float octave4 = PerlinSimplexNoise.noise(worldX * 0.01f, worldY * 0.01f) * 0.12f;
            float octave5 = PerlinSimplexNoise.noise(worldX * 0.03f, worldY * 0.03f) * octave4;
            float lowerGroundHeight = octave1 + octave2 + octave3 + octave4 + octave5;
            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;
            bool sunlit = true;
            BlockType blockType = BlockType.None;
            for (int y = worldDepthInBlocks - 1; y >= 0; y--)
            {

                if (y <= lowerGroundHeight)
                {
                    if (sunlit)
                    {
                        blockType = BlockType.Grass;
                        sunlit = false;
                    }
                    else
                    {
                        blockType = BlockType.Rock;
                    }
                }

                chunk.AddBlock(blockXInChunk, y, blockZInChunk, blockType);
               // Debug.WriteLine(string.Format("chunk {0} : ({1},{2},{3})={4}", chunk.Position, blockXInChunk, y, blockZInChunk, blockType));

            }
        }
    }
}
