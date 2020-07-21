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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NewTake.model;
using NewTake.view.blocks;
using NewTake.profiling;
using NewTake;
#endregion

namespace NewTake.view.blocks
{
    public class VertexBuildChunkProcessor : IChunkProcessor
    {
        private GraphicsDevice _graphicsDevice;
        private const int MAX_SUN_VALUE = 16;

        public VertexBuildChunkProcessor(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        #region BuildVertexList
        private void BuildVertexList(Chunk chunk)
        {
            //lowestNoneBlock and highestNoneBlock come from the terrain gen (Eventually, if the terraingen did not set them you gain nothing)
            //and digging is handled correctly too 
            //TODO generalize highest/lowest None to non-solid

            byte yLow = (byte)(chunk.lowestNoneBlock.Y == 0 ? 0 : chunk.lowestNoneBlock.Y - 1);
            byte yHigh = (byte)(chunk.highestSolidBlock.Y == Chunk.SIZE.Y ? Chunk.SIZE.Y : chunk.highestSolidBlock.Y + 1);

            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!

                    #region ylow and yhigh on chunk borders
                    if (x == 0)
                    {
                        if (chunk.E == null)
                        {
                            yHigh = Chunk.SIZE.Y;
                            yLow = 0;
                        }
                        else
                        {
                            yHigh = Math.Max(yHigh, chunk.E.highestSolidBlock.Y);
                            yLow = Math.Min(yLow, chunk.E.lowestNoneBlock.Y);
                        }
                    }
                    else if (x == Chunk.MAX.X)
                    {
                        if (chunk.W == null)
                        {
                            yHigh = Chunk.SIZE.Y;
                            yLow = 0;
                        }
                        else
                        {
                            yHigh = Math.Max(yHigh, chunk.W.highestSolidBlock.Y);
                            yLow = Math.Min(yLow, chunk.W.lowestNoneBlock.Y);
                        }
                    }

                    if (z == 0)
                    {
                        if (chunk.S == null)
                        {
                            yHigh = Chunk.SIZE.Y;
                            yLow = 0;
                        }
                        else
                        {
                            yHigh = Math.Max(yHigh, chunk.S.highestSolidBlock.Y);
                            yLow = Math.Min(yLow, chunk.S.lowestNoneBlock.Y);
                        }
                    }
                    else if (z == Chunk.MAX.Z)
                    {
                        if (chunk.N == null)
                        {
                            yHigh = Chunk.SIZE.Y;
                            yLow = 0;
                        }
                        else
                        {
                            yHigh = Math.Max(yHigh, chunk.N.highestSolidBlock.Y);
                            yLow = Math.Min(yLow, chunk.N.lowestNoneBlock.Y);
                        }
                    }
                    #endregion

                    for (byte y = yLow; y < yHigh; y++)
                    {
                        if (chunk.Blocks[offset + y].Type != BlockType.None)
                        {
                            if (BlockInformation.IsPlantBlock(chunk.Blocks[offset + y].Type))
                            {
                                BuildPlantVertexList(chunk.Blocks[offset + y], chunk, new Vector3i(x, y, z));
                            }
                            else if (BlockInformation.IsGrassBlock(chunk.Blocks[offset + y].Type))
                            {
                                BuildGrassVertexList(chunk.Blocks[offset + y], chunk, new Vector3i(x, y, z));
                            }
                            else
                            {
                                BuildBlockVertexList(chunk.Blocks[offset + y], chunk, new Vector3i(x, y, z));
                            }
                        }
                    }
                }
            }

            VertexPositionTextureLight[] v = chunk.vertexList.ToArray();
            short[] i = chunk.indexList.ToArray();

            VertexPositionTextureLight[] water = chunk.watervertexList.ToArray();
            short[] iWater = chunk.waterindexList.ToArray();

            lock (chunk)
            {
                if (v.Length > 0)
                {
                    chunk.VertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTextureLight), v.Length, BufferUsage.WriteOnly);
                    chunk.VertexBuffer.SetData(v);
                    chunk.IndexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, i.Length, BufferUsage.WriteOnly);
                    chunk.IndexBuffer.SetData(i);
                }

                if (water.Length > 0)
                {
                    chunk.waterVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTextureLight), water.Length, BufferUsage.WriteOnly);
                    chunk.waterVertexBuffer.SetData(water);
                    chunk.waterIndexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, iWater.Length, BufferUsage.WriteOnly);
                    chunk.waterIndexBuffer.SetData(iWater);
                }
            }

            chunk.dirty = false;
        }
        #endregion

        #region BuildBlockVertexList
        private void BuildBlockVertexList(Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;

            //get signed bytes from these to be able to remove 1 without further casts
            sbyte X = (sbyte)chunkRelativePosition.X;
            sbyte Y = (sbyte)chunkRelativePosition.Y;
            sbyte Z = (sbyte)chunkRelativePosition.Z;


            Block blockTopNW, blockTopN, blockTopNE, blockTopW, blockTopM, blockTopE, blockTopSW, blockTopS, blockTopSE;
            Block blockMidNW, blockMidN, blockMidNE, blockMidW, blockMidM, blockMidE, blockMidSW, blockMidS, blockMidSE;
            Block blockBotNW, blockBotN, blockBotNE, blockBotW, blockBotM, blockBotE, blockBotSW, blockBotS, blockBotSE;

            Block solidBlock = new Block(BlockType.Rock);

            blockTopNW = chunk.BlockAt(X - 1, Y + 1, Z + 1);
            blockTopN = chunk.BlockAt(X, Y + 1, Z + 1);
            blockTopNE = chunk.BlockAt(X + 1, Y + 1, Z + 1);
            blockTopW = chunk.BlockAt(X - 1, Y + 1, Z);
            blockTopM = chunk.BlockAt(X, Y + 1, Z);
            blockTopE = chunk.BlockAt(X + 1, Y + 1, Z);
            blockTopSW = chunk.BlockAt(X - 1, Y + 1, Z - 1);
            blockTopS = chunk.BlockAt(X, Y + 1, Z - 1);
            blockTopSE = chunk.BlockAt(X + 1, Y + 1, Z - 1);

            blockMidNW = chunk.BlockAt(X - 1, Y, Z + 1);
            blockMidN = chunk.BlockAt(X, Y, Z + 1);
            blockMidNE = chunk.BlockAt(X + 1, Y, Z + 1);
            blockMidW = chunk.BlockAt(X - 1, Y, Z);
            blockMidM = chunk.BlockAt(X, Y, Z);
            blockMidE = chunk.BlockAt(X + 1, Y, Z);
            blockMidSW = chunk.BlockAt(X - 1, Y, Z - 1);
            blockMidS = chunk.BlockAt(X, Y, Z - 1);
            blockMidSE = chunk.BlockAt(X + 1, Y, Z - 1);

            blockBotNW = chunk.BlockAt(X - 1, Y - 1, Z + 1);
            blockBotN = chunk.BlockAt(X, Y - 1, Z + 1);
            blockBotNE = chunk.BlockAt(X + 1, Y - 1, Z + 1);
            blockBotW = chunk.BlockAt(X - 1, Y - 1, Z);
            blockBotM = chunk.BlockAt(X, Y - 1, Z);
            blockBotE = chunk.BlockAt(X + 1, Y - 1, Z);
            blockBotSW = chunk.BlockAt(X - 1, Y - 1, Z - 1);
            blockBotS = chunk.BlockAt(X, Y - 1, Z - 1);
            blockBotSE = chunk.BlockAt(X + 1, Y - 1, Z - 1);

            float sunTR, sunTL, sunBR, sunBL;
            float redTR, redTL, redBR, redBL;
            float grnTR, grnTL, grnBR, grnBL;
            float bluTR, bluTL, bluBR, bluBL;
            Color localTR, localTL, localBR, localBL;


            // XDecreasing
            if (BlockInformation.IsTransparentBlock(blockMidW.Type) && !(block.Type == blockMidW.Type))
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopW.R + blockMidNW.R + blockMidW.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopW.R + blockMidSW.R + blockMidW.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotW.R + blockMidNW.R + blockMidW.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotW.R + blockMidSW.R + blockMidW.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopW.G + blockMidNW.G + blockMidW.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopW.G + blockMidSW.G + blockMidW.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotW.G + blockMidNW.G + blockMidW.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotW.G + blockMidSW.G + blockMidW.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopW.B + blockMidNW.B + blockMidW.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopW.B + blockMidSW.B + blockMidW.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotW.B + blockMidNW.B + blockMidW.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotW.B + blockMidSW.B + blockMidW.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (BlockInformation.IsTransparentBlock(blockMidE.Type) && !(block.Type == blockMidE.Type))
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopE.Sun + blockMidSE.Sun + blockMidE.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopE.Sun + blockMidNE.Sun + blockMidE.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotE.Sun + blockMidSE.Sun + blockMidE.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotE.Sun + blockMidNE.Sun + blockMidE.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopE.R + blockMidSE.R + blockMidE.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopE.R + blockMidNE.R + blockMidE.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotE.R + blockMidSE.R + blockMidE.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotE.R + blockMidNE.R + blockMidE.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopE.G + blockMidSE.G + blockMidE.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopE.G + blockMidNE.G + blockMidE.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotE.G + blockMidSE.G + blockMidE.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotE.G + blockMidNE.G + blockMidE.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopE.B + blockMidSE.B + blockMidE.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopE.B + blockMidNE.B + blockMidE.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotE.B + blockMidSE.B + blockMidE.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotE.B + blockMidNE.B + blockMidE.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.XIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (BlockInformation.IsTransparentBlock(blockBotM.Type) && !(block.Type == blockBotM.Type))
            {
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotS.Sun + blockBotM.Sun + blockTopW.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotS.Sun + blockBotM.Sun + blockTopE.Sun) / 4);
                sunTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotN.Sun + blockBotM.Sun + blockTopW.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotN.Sun + blockBotM.Sun + blockTopE.Sun) / 4);

                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotS.R + blockBotM.R + blockTopW.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotS.R + blockBotM.R + blockTopE.R) / 4);
                redTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotN.R + blockBotM.R + blockTopW.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotN.R + blockBotM.R + blockTopE.R) / 4);

                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotS.G + blockBotM.G + blockTopW.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotS.G + blockBotM.G + blockTopE.G) / 4);
                grnTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotN.G + blockBotM.G + blockTopW.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotN.G + blockBotM.G + blockTopE.G) / 4);

                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotS.B + blockBotM.B + blockTopW.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotS.B + blockBotM.B + blockTopE.B) / 4);
                bluTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotN.B + blockBotM.B + blockTopW.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotN.B + blockBotM.B + blockTopE.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.YDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (BlockInformation.IsTransparentBlock(blockTopM.Type) && !(block.Type == blockTopM.Type))
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopN.Sun + blockTopW.Sun + blockTopM.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopN.Sun + blockTopE.Sun + blockTopM.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopS.Sun + blockTopW.Sun + blockTopM.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopS.Sun + blockTopE.Sun + blockTopM.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopN.R + blockTopW.R + blockTopM.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopN.R + blockTopE.R + blockTopM.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopS.R + blockTopW.R + blockTopM.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopS.R + blockTopE.R + blockTopM.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopN.G + blockTopW.G + blockTopM.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopN.G + blockTopE.G + blockTopM.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopS.G + blockTopW.G + blockTopM.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopS.G + blockTopE.G + blockTopM.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopN.B + blockTopW.B + blockTopM.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopN.B + blockTopE.B + blockTopM.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopS.B + blockTopW.B + blockTopM.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopS.B + blockTopE.B + blockTopM.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.YIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (BlockInformation.IsTransparentBlock(blockMidS.Type) && !(block.Type == blockMidS.Type))
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopS.Sun + blockMidSW.Sun + blockMidS.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopS.Sun + blockMidSE.Sun + blockMidS.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotS.Sun + blockMidSW.Sun + blockMidS.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotS.Sun + blockMidSE.Sun + blockMidS.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopS.R + blockMidSW.R + blockMidS.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopS.R + blockMidSE.R + blockMidS.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotS.R + blockMidSW.R + blockMidS.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotS.R + blockMidSE.R + blockMidS.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopS.G + blockMidSW.G + blockMidS.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopS.G + blockMidSE.G + blockMidS.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotS.G + blockMidSW.G + blockMidS.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotS.G + blockMidSE.G + blockMidS.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopS.B + blockMidSW.B + blockMidS.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopS.B + blockMidSE.B + blockMidS.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotS.B + blockMidSW.B + blockMidS.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotS.B + blockMidSE.B + blockMidS.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.ZDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (BlockInformation.IsTransparentBlock(blockMidN.Type) && !(block.Type == blockMidN.Type))
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopN.Sun + blockMidNE.Sun + blockMidN.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopN.Sun + blockMidNW.Sun + blockMidN.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotN.Sun + blockMidNE.Sun + blockMidN.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotN.Sun + blockMidNW.Sun + blockMidN.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopN.R + blockMidNE.R + blockMidN.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopN.R + blockMidNW.R + blockMidN.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotN.R + blockMidNE.R + blockMidN.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotN.R + blockMidNW.R + blockMidN.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopN.G + blockMidNE.G + blockMidN.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopN.G + blockMidNW.G + blockMidN.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotN.G + blockMidNE.G + blockMidN.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotN.G + blockMidNW.G + blockMidN.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopN.B + blockMidNE.B + blockMidN.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopN.B + blockMidNW.B + blockMidN.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotN.B + blockMidNE.B + blockMidN.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotN.B + blockMidNW.B + blockMidN.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                BuildFaceVertices(chunk, blockPosition, chunkRelativePosition, BlockFaceDirection.ZIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
        }
        #endregion

        #region BuildPlantVertexList
        private void BuildPlantVertexList(Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;

            //get signed bytes from these to be able to remove 1 without further casts
            sbyte X = (sbyte)chunkRelativePosition.X;
            sbyte Y = (sbyte)chunkRelativePosition.Y;
            sbyte Z = (sbyte)chunkRelativePosition.Z;


            //Block blockTopNW, blockTopN, blockTopNE, blockTopW, blockTopM, blockTopE, blockTopSW, blockTopS, blockTopSE;
            //Block blockMidNW, blockMidN, blockMidNE, blockMidW, blockMidM, blockMidE, blockMidSW, blockMidS, blockMidSE;
            //Block blockBotNW, blockBotN, blockBotNE, blockBotW, blockBotM, blockBotE, blockBotSW, blockBotS, blockBotSE;

            //Block solidBlock = new Block(BlockType.Rock);

            //blockTopNW = chunk.BlockAt(X - 1, Y + 1, Z + 1);
            //blockTopN = chunk.BlockAt(X, Y + 1, Z + 1);
            //blockTopNE = chunk.BlockAt(X + 1, Y + 1, Z + 1);
            //blockTopW = chunk.BlockAt(X - 1, Y + 1, Z);
            //blockTopM = chunk.BlockAt(X, Y + 1, Z);
            //blockTopE = chunk.BlockAt(X + 1, Y + 1, Z);
            //blockTopSW = chunk.BlockAt(X - 1, Y + 1, Z - 1);
            //blockTopS = chunk.BlockAt(X, Y + 1, Z - 1);
            //blockTopSE = chunk.BlockAt(X + 1, Y + 1, Z - 1);

            //blockMidNW = chunk.BlockAt(X - 1, Y, Z + 1);
            //blockMidN = chunk.BlockAt(X, Y, Z + 1);
            //blockMidNE = chunk.BlockAt(X + 1, Y, Z + 1);
            //blockMidW = chunk.BlockAt(X - 1, Y, Z);
            //blockMidM = chunk.BlockAt(X, Y, Z);
            //blockMidE = chunk.BlockAt(X + 1, Y, Z);
            //blockMidSW = chunk.BlockAt(X - 1, Y, Z - 1);
            //blockMidS = chunk.BlockAt(X, Y, Z - 1);
            //blockMidSE = chunk.BlockAt(X + 1, Y, Z - 1);

            //blockBotNW = chunk.BlockAt(X - 1, Y - 1, Z + 1);
            //blockBotN = chunk.BlockAt(X, Y - 1, Z + 1);
            //blockBotNE = chunk.BlockAt(X + 1, Y - 1, Z + 1);
            //blockBotW = chunk.BlockAt(X - 1, Y - 1, Z);
            //blockBotM = chunk.BlockAt(X, Y - 1, Z);
            //blockBotE = chunk.BlockAt(X + 1, Y - 1, Z);
            //blockBotSW = chunk.BlockAt(X - 1, Y - 1, Z - 1);
            //blockBotS = chunk.BlockAt(X, Y - 1, Z - 1);
            //blockBotSE = chunk.BlockAt(X + 1, Y - 1, Z - 1);

            //float sunTR, sunTL, sunBR, sunBL;
            //float redTR, redTL, redBR, redBL;
            //float grnTR, grnTL, grnBR, grnBL;
            //float bluTR, bluTL, bluBR, bluBL;
            //Color localTR, localTL, localBR, localBL;

            //localTR = Color.White; localTL = Color.White; localBR = Color.White; localBL = Color.White;

            //sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
            //sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);
            //sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
            //sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);

            //redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopW.R + blockMidNW.R + blockMidW.R) / 4);
            //redTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopW.R + blockMidSW.R + blockMidW.R) / 4);
            //redBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotW.R + blockMidNW.R + blockMidW.R) / 4);
            //redBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotW.R + blockMidSW.R + blockMidW.R) / 4);

            //grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopW.G + blockMidNW.G + blockMidW.G) / 4);
            //grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopW.G + blockMidSW.G + blockMidW.G) / 4);
            //grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotW.G + blockMidNW.G + blockMidW.G) / 4);
            //grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotW.G + blockMidSW.G + blockMidW.G) / 4);

            //bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopW.B + blockMidNW.B + blockMidW.B) / 4);
            //bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopW.B + blockMidSW.B + blockMidW.B) / 4);
            //bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotW.B + blockMidNW.B + blockMidW.B) / 4);
            //bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotW.B + blockMidSW.B + blockMidW.B) / 4);

            //localTL = new Color(redTL, grnTL, bluTL);
            //localTR = new Color(redTR, grnTR, bluTR);
            //localBL = new Color(redBL, grnBL, bluBL);
            //localBR = new Color(redBR, grnBR, bluBR);

            //_blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);

            BuildPlantVertices(chunk, blockPosition, chunkRelativePosition, block.Type, 0.6f, Color.LightGray);
        }
        #endregion

        #region BuildGrassVertexList
        private void BuildGrassVertexList(Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;

            //get signed bytes from these to be able to remove 1 without further casts
            sbyte X = (sbyte)chunkRelativePosition.X;
            sbyte Y = (sbyte)chunkRelativePosition.Y;
            sbyte Z = (sbyte)chunkRelativePosition.Z;


            //Block blockTopNW, blockTopN, blockTopNE, blockTopW, blockTopM, blockTopE, blockTopSW, blockTopS, blockTopSE;
            //Block blockMidNW, blockMidN, blockMidNE, blockMidW, blockMidM, blockMidE, blockMidSW, blockMidS, blockMidSE;
            //Block blockBotNW, blockBotN, blockBotNE, blockBotW, blockBotM, blockBotE, blockBotSW, blockBotS, blockBotSE;

            //Block solidBlock = new Block(BlockType.Rock);

            //blockTopNW = chunk.BlockAt(X - 1, Y + 1, Z + 1);
            //blockTopN = chunk.BlockAt(X, Y + 1, Z + 1);
            //blockTopNE = chunk.BlockAt(X + 1, Y + 1, Z + 1);
            //blockTopW = chunk.BlockAt(X - 1, Y + 1, Z);
            //blockTopM = chunk.BlockAt(X, Y + 1, Z);
            //blockTopE = chunk.BlockAt(X + 1, Y + 1, Z);
            //blockTopSW = chunk.BlockAt(X - 1, Y + 1, Z - 1);
            //blockTopS = chunk.BlockAt(X, Y + 1, Z - 1);
            //blockTopSE = chunk.BlockAt(X + 1, Y + 1, Z - 1);

            //blockMidNW = chunk.BlockAt(X - 1, Y, Z + 1);
            //blockMidN = chunk.BlockAt(X, Y, Z + 1);
            //blockMidNE = chunk.BlockAt(X + 1, Y, Z + 1);
            //blockMidW = chunk.BlockAt(X - 1, Y, Z);
            //blockMidM = chunk.BlockAt(X, Y, Z);
            //blockMidE = chunk.BlockAt(X + 1, Y, Z);
            //blockMidSW = chunk.BlockAt(X - 1, Y, Z - 1);
            //blockMidS = chunk.BlockAt(X, Y, Z - 1);
            //blockMidSE = chunk.BlockAt(X + 1, Y, Z - 1);

            //blockBotNW = chunk.BlockAt(X - 1, Y - 1, Z + 1);
            //blockBotN = chunk.BlockAt(X, Y - 1, Z + 1);
            //blockBotNE = chunk.BlockAt(X + 1, Y - 1, Z + 1);
            //blockBotW = chunk.BlockAt(X - 1, Y - 1, Z);
            //blockBotM = chunk.BlockAt(X, Y - 1, Z);
            //blockBotE = chunk.BlockAt(X + 1, Y - 1, Z);
            //blockBotSW = chunk.BlockAt(X - 1, Y - 1, Z - 1);
            //blockBotS = chunk.BlockAt(X, Y - 1, Z - 1);
            //blockBotSE = chunk.BlockAt(X + 1, Y - 1, Z - 1);

            //float sunTR, sunTL, sunBR, sunBL;
            //float redTR, redTL, redBR, redBL;
            //float grnTR, grnTL, grnBR, grnBL;
            //float bluTR, bluTL, bluBR, bluBL;
            //Color localTR, localTL, localBR, localBL;

            //localTR = Color.White; localTL = Color.White; localBR = Color.White; localBL = Color.White;

            //sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
            //sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);
            //sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
            //sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);

            //redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopW.R + blockMidNW.R + blockMidW.R) / 4);
            //redTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopW.R + blockMidSW.R + blockMidW.R) / 4);
            //redBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotW.R + blockMidNW.R + blockMidW.R) / 4);
            //redBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotW.R + blockMidSW.R + blockMidW.R) / 4);

            //grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopW.G + blockMidNW.G + blockMidW.G) / 4);
            //grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopW.G + blockMidSW.G + blockMidW.G) / 4);
            //grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotW.G + blockMidNW.G + blockMidW.G) / 4);
            //grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotW.G + blockMidSW.G + blockMidW.G) / 4);

            //bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopW.B + blockMidNW.B + blockMidW.B) / 4);
            //bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopW.B + blockMidSW.B + blockMidW.B) / 4);
            //bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotW.B + blockMidNW.B + blockMidW.B) / 4);
            //bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotW.B + blockMidSW.B + blockMidW.B) / 4);

            //localTL = new Color(redTL, grnTL, bluTL);
            //localTR = new Color(redTR, grnTR, bluTR);
            //localBL = new Color(redBL, grnBL, bluBL);
            //localBR = new Color(redBR, grnBR, bluBR);

            //_blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);

            BuildGrassVertices(chunk, blockPosition, chunkRelativePosition, block.Type, 0.6f, Color.LightGray);
        }
        #endregion

        public void BuildGrassVertices(Chunk chunk, Vector3i blockPosition, Vector3i chunkRelativePosition, BlockType blockType, float sunLight, Color localLight)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType);
            Vector2[] UVList;

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.3f, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.7f, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.3f), new Vector3(0, 0, 1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.3f), new Vector3(0, 0, 1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.3f), new Vector3(0, 0, 1), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.3f), new Vector3(0, 0, 1), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.3f), new Vector3(0, 0, -1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.3f), new Vector3(0, 0, -1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.3f), new Vector3(0, 0, -1), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.3f), new Vector3(0, 0, -1), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.7f), new Vector3(0, 0, 1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.7f), new Vector3(0, 0, 1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.7f), new Vector3(0, 0, 1), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.7f), new Vector3(0, 0, 1), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.7f), new Vector3(0, 0, -1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.7f), new Vector3(0, 0, -1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.7f), new Vector3(0, 0, -1), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.7f), new Vector3(0, 0, -1), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);
        }

        #region BuildPlantVertices
        public void BuildPlantVertices(Chunk chunk, Vector3i blockPosition, Vector3i chunkRelativePosition, BlockType blockType, float sunLight, Color localLight)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType);
            Vector2[] UVList;

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZIncreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.5f), new Vector3(0, 0, 1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.5f), new Vector3(0, 0, 1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.5f), new Vector3(0, 0, 1), UVList[5], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.5f), new Vector3(0, 0, 1), UVList[2], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZDecreasing];
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.5f), new Vector3(0, 0, -1), UVList[0], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.5f), new Vector3(0, 0, -1), UVList[1], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.5f), new Vector3(0, 0, -1), UVList[2], sunLight, localLight);
            AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.5f), new Vector3(0, 0, -1), UVList[5], sunLight, localLight);
            AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);
        }
        #endregion

        #region BuildFaceVertices
        public void BuildFaceVertices(Chunk chunk, Vector3i blockPosition, Vector3i chunkRelativePosition, BlockFaceDirection faceDir, BlockType blockType, float sunLightTL, float sunLightTR, float sunLightBL, float sunLightBR, Color localLightTL, Color localLightTR, Color localLightBL, Color localLightBR)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

            int faceIndex = (int)faceDir;

            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];

            float height = 1;
            if (BlockInformation.IsCapBlock(blockType)) height = 0.1f;
            if (BlockInformation.IsHalfBlock(blockType)) height = 0.5f;
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 1), new Vector3(1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 0), new Vector3(1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 0), new Vector3(-1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 1), new Vector3(-1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        //BL,BR,TR,BL,TR,TL
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 1), new Vector3(0, 1, 0), UVList[4], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 1), new Vector3(0, 1, 0), UVList[5], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 0), new Vector3(0, 1, 0), UVList[1], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 0), new Vector3(0, 1, 0), UVList[3], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 3, 2, 0, 3, 0, 1);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        //TR,BR,TL,TL,BR,BL
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, -1, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), UVList[2], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), UVList[4], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, -1, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 0, 2, 1, 1, 2, 3);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 1), new Vector3(0, 0, 1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 1), new Vector3(0, 0, 1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, 0, 1), UVList[5], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), UVList[2], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, height, 0), new Vector3(0, 0, -1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, height, 0), new Vector3(0, 0, -1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), UVList[2], sunLightBR, localLightBR);
                        AddVertex(chunk, blockType, blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, 0, -1), UVList[5], sunLightBL, localLightBL);
                        AddIndex(chunk, blockType, 0, 1, 2, 2, 1, 3);
                    }
                    break;
            }
        }
        #endregion

        private void AddVertex(Chunk chunk, BlockType blockType, Vector3i blockPosition, Vector3i chunkRelativePosition, Vector3 vertexAdd, Vector3 normal, Vector2 uv1, float sunLight, Color localLight)
        {
            if (blockType != BlockType.Water)
            {
                chunk.vertexList.Add(new VertexPositionTextureLight(blockPosition.asVector3() + vertexAdd, uv1, sunLight, localLight.ToVector3()));
            }
            else
            {
                chunk.watervertexList.Add(new VertexPositionTextureLight(blockPosition.asVector3() + vertexAdd, uv1, sunLight, localLight.ToVector3()));
            }
        }

        #region AddIndex
        private void AddIndex(Chunk chunk, BlockType blockType, short i1, short i2, short i3, short i4, short i5, short i6)
        {
            if (blockType != BlockType.Water)
            {
                chunk.indexList.Add((short)(chunk.VertexCount + i1));
                chunk.indexList.Add((short)(chunk.VertexCount + i2));
                chunk.indexList.Add((short)(chunk.VertexCount + i3));
                chunk.indexList.Add((short)(chunk.VertexCount + i4));
                chunk.indexList.Add((short)(chunk.VertexCount + i5));
                chunk.indexList.Add((short)(chunk.VertexCount + i6));
                chunk.VertexCount += 4;
            }
            else
            {
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i1));
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i2));
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i3));
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i4));
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i5));
                chunk.waterindexList.Add((short)(chunk.waterVertexCount + i6));
                chunk.waterVertexCount += 4;
            }
        }
        #endregion

        public void ProcessChunk(Chunk chunk)
        {
            chunk.Clear();
            BuildVertexList(chunk);
        }

    }
}
