#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NewTake;
#endregion

namespace NewTake.model.terrain
{
    class DualLayerTerrainWithMediumValleysForRivers : SimpleTerrain
    {

        private float lowerGroundHeight;
        private int upperGroundHeight;

        public override void Generate(Chunk chunk)
        {
            base.Generate(chunk);
            GenerateWaterSandLayer(chunk);
            GenerateTreesFlowers(chunk);
            chunk.State = ChunkState.AwaitingBuild;
        }

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, byte blockXInChunk, byte blockZInChunk, uint worldX, uint worldZ)
        {
            lowerGroundHeight = GetLowerGroundHeight(chunk, worldX, worldZ);
            upperGroundHeight = GetUpperGroundHeight(chunk, worldX, worldZ, lowerGroundHeight);

            bool sunlit = true;

            for (int y = Chunk.MAX.Y; y >= 0; y--)
            {

                // Everything above ground height...is air.
                BlockType blockType;
                if (y > upperGroundHeight)
                {
                    blockType = BlockType.None;
                }
                // Are we above the lower ground height?
                else if (y > lowerGroundHeight)
                {
                    // Let's see about some caves er valleys!
                    float caveNoise = PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.01f) * (0.015f * y) + 0.1f;
                    caveNoise += PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.1f) * 0.06f + 0.1f;
                    caveNoise += PerlinSimplexNoise.noise(worldX * 0.2f, worldZ * 0.2f, y * 0.2f) * 0.03f + 0.01f;
                    // We have a cave, draw air here.
                    if (caveNoise > 0.2f)
                    {
                        blockType = BlockType.None;
                    }
                    else
                    {
                        blockType = BlockType.None;
                        if (sunlit)
                        {
                            if (y > SNOWLEVEL + r.Next(3))
                            {
                                blockType = BlockType.Snow;
                            }
                            else
                            {
                                blockType = BlockType.Grass;
                            }
                            sunlit = false;
                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                        }
                    }
                }
                else
                {
                    // We are at the lower ground level
                    if (sunlit)
                    {
                        blockType = BlockType.Grass;
                        sunlit = false;
                    }
                    else
                    {
                        blockType = BlockType.Dirt;
                    }
                }

                if (blockType == BlockType.None && y <= WATERLEVEL)
                {
                    //if (y <= WATERLEVEL)
                    //{
                        blockType = BlockType.Lava;
                        sunlit = false;
                    //}
                }
                chunk.setBlock(blockXInChunk, (byte)y, blockZInChunk, new Block(blockType));
            }
        }


        #endregion

        #region GenerateWaterSandLayer
        private void GenerateWaterSandLayer(Chunk chunk)
        {
            //BlockType blockType;
            //bool sunlit = true;

            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y;
                    //for (byte y = WATERLEVEL + 9; y >= MINIMUMGROUNDHEIGHT; y--)
                    for (byte y = WATERLEVEL + 9; y >= (byte)lowerGroundHeight; y--)
                    {
                        //blockType = chunk.Blocks[offset + y].Type;
                        if (chunk.Blocks[offset + y].Type == BlockType.None)
                        {
                            chunk.setBlock(x, y, z, new Block(BlockType.Water));
                            //blockType = BlockType.Water;
                        }
                        //else
                        //{
                        //    if (chunk.Blocks[offset + y].Type == BlockType.Grass)
                        //    {
                        //        blockType = BlockType.Sand;
                        //        //if (y <= WATERLEVEL)
                        //        //{
                        //        //    sunlit = false;
                        //        //}
                        //    }
                        //    break;
                        //}
                        //chunk.setBlock(x, y, z, new Block(blockType));
                    }
                    for (byte y = WATERLEVEL + 11; y >= WATERLEVEL; y--)
                    {
                        if ((chunk.Blocks[offset + y].Type == BlockType.Dirt) || (chunk.Blocks[offset + y].Type == BlockType.Grass) || (chunk.Blocks[offset + y].Type == BlockType.Lava))
                        {
                            chunk.setBlock(x, y, z, new Block(BlockType.Sand));
                        }
                    }
                }
            }
        }
        #endregion

        #region GenerateTreesFlowers
        private void GenerateTreesFlowers(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y;
                    for (int y = upperGroundHeight+1; y >= WATERLEVEL + 9; y--)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.Grass)
                        {
                            if (r.Next(700) == 1)
                            {
                                base.BuildTree(chunk, x, (byte)y, z);
                            }
                            else if (r.Next(50) == 1)
                            {
                                y++;
                                chunk.setBlock(x, (byte)y, z, new Block(BlockType.RedFlower));
                            }
                            //else if (r.Next(2) == 1)
                            //{
                            //    y++;
                            //    chunk.setBlock(x, y, z, new Block(BlockType.LongGrass));
                            //}
                        }
                    }
                }
            }
        }
        #endregion

        #region GetUpperGroundHeight
        private static int GetUpperGroundHeight(Chunk chunk, uint blockX, uint blockY, float lowerGroundHeight)
        {
            float octave1 = PerlinSimplexNoise.noise((blockX + 100) * 0.001f, blockY * 0.001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise((blockX + 100) * 0.002f, blockY * 0.002f) * 0.25f;
            float octave3 = PerlinSimplexNoise.noise((blockX + 100) * 0.01f, blockY * 0.01f) * 0.25f;
            float octaveSum = octave1 + octave2 + octave3;

            return (int)(octaveSum * (Chunk.SIZE.Y / 2f)) + (int)(lowerGroundHeight);
        }
        #endregion

        #region GetLowerGroundHeight
        private static float GetLowerGroundHeight(Chunk chunk, uint blockX, uint blockY)
        {
            int minimumGroundheight = Chunk.SIZE.Y / 4;
            int minimumGroundDepth = (int)(Chunk.SIZE.Y * 0.5f);

            float octave1 = PerlinSimplexNoise.noise(blockX * 0.0001f, blockY * 0.0001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise(blockX * 0.0005f, blockY * 0.0005f) * 0.35f;
            float octave3 = PerlinSimplexNoise.noise(blockX * 0.02f, blockY * 0.02f) * 0.15f;
            float lowerGroundHeight = octave1 + octave2 + octave3;

            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

            return lowerGroundHeight;
        }
        #endregion

    }
}
