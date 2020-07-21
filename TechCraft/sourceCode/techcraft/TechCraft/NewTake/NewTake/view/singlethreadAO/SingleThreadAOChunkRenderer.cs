#region license

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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.model;
using NewTake.view.blocks;
using System.Diagnostics;

namespace NewTake.view
{

    //this is the same code as in boundariesChunkRenderer. 
   
    class SingleThreadAOChunkRenderer : ChunkRenderer
    {

        private AOVertexBlockRenderer _blockRenderer;
        public new List<VertexPositionDualTexture> _vertexList;

        public SingleThreadAOChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk)
            : base(graphicsDevice, world, chunk) 
        { 
             _vertexList = new List<VertexPositionDualTexture>();
             _blockRenderer = new AOVertexBlockRenderer(world);
        }

        public override void draw(GameTime gameTime)
        {
            if (!chunk.generated) return;
            if (!chunk.visible)
            {
                _vertexList.Clear();
                vertexBuffer.Dispose();
                chunk.dirty = false;
            }

            if (vertexBuffer != null)
            {
                if (vertexBuffer.IsDisposed)
                {
                    return;
                }

                if (vertexBuffer.VertexCount > 0)
                {
                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                }

                else
                {
                    Debug.WriteLine("no vertices");
                }
            }
        }

        #region BuildVertexList
         public override void BuildVertexList()
         {
            _blockRenderer.Clear();

            //lowestNoneBlock and highestNoneBlock come from the terrain gen (Eventually, if the terraingen did not set them you gain nothing)
            //and digging is handled correctly too 
            //TODO generalize highest/lowest None to non-solid
            byte yLow = (byte)(chunk.lowestNoneBlock.Y == 0 ? 0 : chunk.lowestNoneBlock.Y - 1);
            byte yHigh = (byte)(chunk.highestSolidBlock.Y == Chunk.CHUNK_YMAX - 1 ? Chunk.CHUNK_YMAX - 1 : chunk.highestSolidBlock.Y + 1);

            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX;
                    for (byte y = yLow; y < yHigh; y++)
                    {
                        //Block block = chunk.Blocks[x, y, z];
                        Block block = chunk.Blocks[offset + y];
                        if (block.Type != BlockType.None)
                        {
                            BuildBlockVertices(block, chunk, new Vector3i(x, y, z));
                        }
                    }
                }
            }

            VertexPositionDualTexture[] v = _blockRenderer.vertexList.ToArray();

            if (v.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionDualTexture), v.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(v);
            }

            chunk.dirty = false;
        }

        #endregion

        #region BuildBlockVertices
         public void BuildBlockVertices(Block block, Chunk chunk, Vector3i chunkRelativePosition)
         {
             //optimized by using chunk.Blocks[][][] except for "out of current chunk" blocks

             Vector3i blockPosition = chunk.Position + chunkRelativePosition;

             Block blockTopNW, blockTopN, blockTopNE, blockTopW, blockTopM, blockTopE, blockTopSW, blockTopS, blockTopSE;
             Block blockMidNW, blockMidN, blockMidNE, blockMidW, blockMidM, blockMidE, blockMidSW, blockMidS, blockMidSE;
             Block blockBotNW, blockBotN, blockBotNE, blockBotW, blockBotM, blockBotE, blockBotSW, blockBotS, blockBotSE;

             Block solidBlock = new Block(BlockType.Rock);

             blockTopNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
             blockTopN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
             blockTopNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
             blockTopW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
             blockTopM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
             blockTopE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
             blockTopSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
             blockTopS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
             blockTopSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));

             blockMidNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
             blockMidN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
             blockMidNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
             blockMidW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
             blockMidM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z));
             blockMidE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
             blockMidSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
             blockMidS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
             blockMidSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));

             blockBotNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
             blockBotN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
             blockBotNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
             blockBotW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
             blockBotM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
             blockBotE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
             blockBotSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
             blockBotS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
             blockBotSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));

             float aoTL, aoTR, aoBL, aoBR;

             float aoVertexWeight = 0.05f;

             if (!blockMidW.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                 if (blockTopNW.Solid) { aoTL -= aoVertexWeight; }
                 if (blockTopW.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockTopSW.Solid) { aoTR -= aoVertexWeight; }

                 if (blockMidNW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                 if (blockMidSW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }

                 if (blockBotNW.Solid) { aoBL -= aoVertexWeight; }
                 if (blockBotW.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotSW.Solid) { aoBR -= aoVertexWeight; }

                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, aoTL, aoTR, aoBL, aoBR);
             }
             if (!blockMidE.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                 if (blockTopNE.Solid) { aoTR -= aoVertexWeight; }
                 if (blockTopE.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockTopSE.Solid) { aoTL -= aoVertexWeight; }

                 if (blockMidNE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockMidSE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                 if (blockBotNE.Solid) { aoBR -= aoVertexWeight; }
                 if (blockBotE.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotSE.Solid) { aoBL -= aoVertexWeight; }

                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XIncreasing, block.Type, aoTL, aoTR, aoBL, aoBR);
             }
             if (!blockBotM.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;
                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YDecreasing, block.Type, aoTL, aoTR, aoBL, aoBR);

                 if (blockBotNW.Solid) { aoTR -= aoVertexWeight; }
                 if (blockBotN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockBotNE.Solid) { aoTL -= aoVertexWeight; }

                 if (blockBotW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                 if (blockBotSW.Solid) { aoBR -= aoVertexWeight; }
                 if (blockBotS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotSE.Solid) { aoBL -= aoVertexWeight; }

             }
             if (!blockTopM.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                 if (blockTopNW.Solid) { aoTL -= aoVertexWeight; }
                 if (blockTopN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockTopNE.Solid) { aoTR -= aoVertexWeight; }

                 if (blockTopW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                 if (blockTopE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }

                 if (blockTopSW.Solid) { aoBL -= aoVertexWeight; }
                 if (blockTopS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockTopSE.Solid) { aoBR -= aoVertexWeight; }

                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YIncreasing, block.Type, aoTL, aoTR, aoBL, aoBR);
             }
             if (!blockMidS.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                 if (blockTopSW.Solid) { aoTL -= aoVertexWeight; }
                 if (blockTopS.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockTopSE.Solid) { aoTR -= aoVertexWeight; }

                 if (blockMidSW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                 if (blockMidSE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }

                 if (blockBotSW.Solid) { aoBL -= aoVertexWeight; }
                 if (blockBotS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotSE.Solid) { aoBR -= aoVertexWeight; }


                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZDecreasing, block.Type, aoTL, aoTR, aoBL, aoBR);

             }
             if (!blockMidN.Solid)
             {
                 aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                 if (blockTopNW.Solid) { aoTR -= aoVertexWeight; }
                 if (blockTopN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                 if (blockTopNE.Solid) { aoTL -= aoVertexWeight; }

                 if (blockMidNW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockMidNE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                 if (blockBotNW.Solid) { aoBR -= aoVertexWeight; }
                 if (blockBotN.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                 if (blockBotNE.Solid) { aoBL -= aoVertexWeight; }

                 _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZIncreasing, block.Type, aoTL, aoTR, aoBL, aoBR);
             }

         }
        #endregion

    }
}
