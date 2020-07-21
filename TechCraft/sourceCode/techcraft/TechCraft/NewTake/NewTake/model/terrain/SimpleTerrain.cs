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

using NewTake.model;
#endregion

namespace NewTake.model.terrain
{
    class SimpleTerrain : IChunkGenerator
    {

        #region Fields

        public const int WATERLEVEL = 64; //Chunk.SISE.Y/2
        public const int SNOWLEVEL = 95;
        public const int MINIMUMGROUNDHEIGHT = 32; //Chunk.SIZE.Y / 4;

        public Random r = new Random(World.SEED);

        #endregion

        #region build
        public virtual void Generate(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                uint worldX = (uint)chunk.Position.X + x + (uint)World.SEED ;

                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    uint worldZ = (uint)chunk.Position.Z + z;
                    generateTerrain(chunk, x, z, worldX, worldZ);
                }
            }
            chunk.State = ChunkState.AwaitingBuild;
            //chunk.generated = true;
        }
        #endregion

        #region generateTerrain
        protected virtual void generateTerrain(Chunk chunk, byte blockXInChunk, byte blockZInChunk, uint worldX, uint worldY)
        {
            // The lower ground level is at least this high.
            int minimumGroundheight = Chunk.SIZE.Y / 4;
            int minimumGroundDepth = (int)(Chunk.SIZE.Y * 0.75f);

            float octave1 = PerlinSimplexNoise.noise(worldX * 0.0001f, worldY * 0.0001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise(worldX * 0.0005f, worldY * 0.0005f) * 0.25f;
            float octave3 = PerlinSimplexNoise.noise(worldX * 0.005f, worldY * 0.005f) * 0.12f;
            float octave4 = PerlinSimplexNoise.noise(worldX * 0.01f, worldY * 0.01f) * 0.12f;
            float octave5 = PerlinSimplexNoise.noise(worldX * 0.03f, worldY * 0.03f) * octave4;
            float lowerGroundHeight = octave1 + octave2 + octave3 + octave4 + octave5;

            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

            bool sunlit = true;

            BlockType blockType = BlockType.None;

            for (int y = Chunk.MAX.Y; y >= 0; y--)
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
                
                
                chunk.setBlock(  blockXInChunk, (byte)y, blockZInChunk,new Block(blockType));
                
                
                //  Debug.WriteLine(string.Format("chunk {0} : ({1},{2},{3})={4}", chunk.Position, blockXInChunk, y, blockZInChunk, blockType));
            }
        }
        #endregion

        #region BuildTree
        public virtual void BuildTree(Chunk chunk, byte tx, byte ty, byte tz)
        {

            // Trunk
            byte height = (byte)(4 + (byte)r.Next(3));
            if ((ty + height) < Chunk.MAX.Y)
            {
                for (byte y = ty; y < ty + height; y++)
                {
                    chunk.setBlock(tx, y, tz, new Block(BlockType.Tree));
                }
            }

            // Foliage
            int radius = 3 + r.Next(2);
            int ny = ty + height;
            for (int i = 0; i < 40 + r.Next(4); i++)
            {
                int lx = tx + r.Next(radius) - r.Next(radius);
                int ly = ny + r.Next(radius) - r.Next(radius);
                int lz = tz + r.Next(radius) - r.Next(radius);
                unchecked //TODO foliage out of bound => new chunk.blockat or needs a chunk.setat
                {
                    if (chunk.outOfBounds((byte)lx, (byte)ly, (byte)lz) == false)
                    {
                        //if (chunk.Blocks[lx, ly, lz].Type == BlockType.None)
                        if (chunk.Blocks[lx * Chunk.FlattenOffset + lz * Chunk.SIZE.Y + ly].Type == BlockType.None)
                            chunk.setBlock((byte)lx, (byte)ly, (byte)lz, new Block(BlockType.Leaves));
                    }
                }
            }

        }
        #endregion

        #region MakeTreeTrunk
        private void MakeTreeTrunk(Chunk chunk, byte tx, byte ty, byte tz, int height)
        {
            Debug.WriteLine("New tree    at {0},{1},{2}={3}", tx, ty, tz, height);
            for (byte y = ty; y < ty + height; y++)
            {
                chunk.setBlock(tx, y, tz, new Block(BlockType.Tree));
            }
        }
        #endregion

        #region MakeTreeFoliage
        private void MakeTreeFoliage(Chunk chunk, int tx, int ty, int tz, int height)
        {
            Debug.WriteLine("New foliage at {0},{1},{2}={3}", tx, ty, tz, height);
            int start = ty + height - 4;
            int end = ty + height + 3;

            int rad;
            int radiusEnd = 2;
            int radiusMiddle = radiusEnd + 1;

            for (int y = start; y < end; y++)
            {
                if ((y > start) && (y < end - 1))
                {
                    rad = radiusMiddle;
                }
                else
                {
                    rad = radiusEnd;
                }

                for (int xoff = -rad; xoff < rad + 1; xoff++)
                {
                    for (int zoff = -rad; zoff < rad + 1; zoff++)
                    {
                        if (chunk.outOfBounds((byte)(tx + xoff), (byte)y, (byte)(tz + zoff)) == false)
                        {
                            chunk.setBlock((byte)(tx + xoff), (byte)y, (byte)(tz + zoff), new Block(BlockType.Leaves));
                            //Debug.WriteLine("rad={0},xoff={1},zoff={2},y={3},start={4},end={5}", rad, xoff, zoff, y, start, end);
                        }
                    }
                }
            }
        }
        #endregion


    }
}
