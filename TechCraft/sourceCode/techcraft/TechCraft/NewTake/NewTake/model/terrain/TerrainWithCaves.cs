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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NewTake.model;

namespace NewTake.model.terrain
{
    class TerrainWithCaves : SimpleTerrain
    {

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, byte x, byte z, uint blockX, uint blockZ)
        {
            int groundHeight = (int)GetBlockNoise(blockX, blockZ);
            if (groundHeight < 1)
            {
                groundHeight = 1;
            }
            else if (groundHeight > 128)
            {
                groundHeight = 96;
            }

            // Default to sunlit.. for caves
            bool sunlit = true;

            BlockType blockType = BlockType.None;

            //chunk.Blocks[x, groundHeight, z] = new Block(BlockType.Grass,true);
            //chunk.Blocks[x, 0, z] = new Block(BlockType.Dirt, true);

            int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y;
            chunk.Blocks[offset + groundHeight] = new Block(BlockType.Grass);
            chunk.Blocks[offset] = new Block(BlockType.Dirt);

            for (int y = Chunk.MAX.Y; y >=0; y--)
            {
                if (y > groundHeight)
                {
                    blockType = BlockType.None;
                }
                // Or we at or below ground height?
                else if (y < groundHeight)
                {
                    // Since we are at or below ground height, let's see if we need
                    // to make
                    // a cave
                    uint noiseX = (blockX + (uint)World.SEED);
                    float octave1 = PerlinSimplexNoise.noise(noiseX * 0.009f, blockZ * 0.009f, y * 0.009f) * 0.25f;

                    float initialNoise = octave1 + PerlinSimplexNoise.noise(noiseX * 0.04f, blockZ * 0.04f, y * 0.04f) * 0.15f;
                    initialNoise += PerlinSimplexNoise.noise(noiseX * 0.08f, blockZ * 0.08f, y * 0.08f) * 0.05f;

                    if (initialNoise > 0.2f)
                    {
                        blockType = BlockType.None;
                    }
                    else
                    {
                        // We've placed a block of dirt instead...nothing below us
                        // will be sunlit
                        if (sunlit)
                        {
                            sunlit = false;
                            blockType = BlockType.Grass;
                            //chunk.addGrassBlock(x,y,z);

                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                            if (octave1 < 0.2f)
                            {
                                blockType = BlockType.Rock;
                            }
                        }
                    }
                }
                
                chunk.setBlock(x, (byte)y, z, new Block(blockType));
                
            }
        }
        #endregion

        private float GetBlockNoise(uint blockX, uint blockZ)
        {
            float mediumDetail = PerlinSimplexNoise.noise(blockX / 300.0f, blockZ / 300.0f, 20);
            float fineDetail = PerlinSimplexNoise.noise(blockX / 80.0f, blockZ / 80.0f, 30);
            float bigDetails = PerlinSimplexNoise.noise(blockX / 800.0f, blockZ / 800.0f);
            float noise = bigDetails * 64.0f + mediumDetail * 32.0f + fineDetail * 16.0f; // *(bigDetails
            // *
            // 64.0f);

            return noise + 16;
        }
    }
}
