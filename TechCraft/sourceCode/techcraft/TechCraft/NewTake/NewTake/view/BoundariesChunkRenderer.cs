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

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.model;
using NewTake.view.blocks;
#endregion

namespace NewTake.view
{
    class BoundariesChunkRenderer : ChunkRenderer
    {

        #region inits

        public Queue<Chunk> _toBuildVertices;

        public bool _vertexbuildrunning = true;
        public Thread _buildingVerticesThread;

        #endregion

        public BoundariesChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk) : base(graphicsDevice, world, chunk)
        {
            _toBuildVertices = new Queue<Chunk>();
            _buildingVerticesThread = new Thread(new ThreadStart(BuildingVerticesThread));
            _buildingVerticesThread.Start();
        }

        public void QueueBuild()
        {
            //Debug.WriteLine(string.Format("Queue Vertex Build at Chunk {0}-{1}-{2}", (int)chunk.Position.X, (int)chunk.Position.Y, (int)chunk.Position.Z));
            lock (_toBuildVertices)
            {
                _toBuildVertices.Enqueue(chunk);
            }
        }

        public void BuildingVerticesThread()
        {
            while (_vertexbuildrunning)
            {
                Chunk buildChunk = null;
                bool doBuild = false;
                lock (_toBuildVertices)
                {
                    if (_toBuildVertices.Count > 0)
                    {
                        buildChunk = _toBuildVertices.Dequeue();
                        doBuild = true;
                    }
                }
                if (doBuild)
                {
                    DoBuild();
                }
                Thread.Sleep(50);
            }
            //there are cleaner way but all this will be rewritten
            //_toBuildVertices.Abort();
        }

        public void DoBuild()
        {
            BuildVertexList();
        }

        #region BuildVertexList
        public override void BuildVertexList()
        {
            //Debug.WriteLine("building vertexlist ...");
            _vertexList.Clear();
            for (uint x = 0; x < Chunk.SIZE.X; x++)
            {
                for (uint z = 0; z < Chunk.SIZE.Z; z++)
                {
                    uint offset = x * (uint)Chunk.FlattenOffset + z * Chunk.SIZE.Y;
                    for (uint y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        //Block block = chunk.Blocks[x, y, z];
                        Block block = chunk.Blocks[offset + y];
                        if (block.Type != BlockType.None)
                        {
                            // Vector3i blockPosition = chunk.Position + new Vector3i(x, y, z);

                            BuildBlockVertices(ref _vertexList, block, chunk, new Vector3i(x, y, z));
                        }
                        else
                        {
                            //If we're on a boundary
                            if (x == 0)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X - 1, chunk.Index.Z];
                                if (neighbouringChunk != null)
                                {
                                    //Block neighbouringBlock = neighbouringChunk.Blocks[Chunk.MAX.X, y, z];
                                    Block neighbouringBlock = chunk.Blocks[(Chunk.MAX.X) * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(Chunk.MAX.X, y, z), BlockFaceDirection.XIncreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            else if (x == Chunk.MAX.X)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X + 1, chunk.Index.Z];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    //Block neighbouringBlock = neighbouringChunk.Blocks[0, y, z];
                                    Block neighbouringBlock = chunk.Blocks[z * Chunk.SIZE.Y + y];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(0, y, z), BlockFaceDirection.XDecreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            if (y == 0)
                            {
                            }
                            else if (y == Chunk.MAX.Y)
                            {
                            }
                            if (z == 0)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X, chunk.Index.Z - 1];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    //Block neighbouringBlock = neighbouringChunk.Blocks[x, y, Chunk.MAX.Z];
                                    Block neighbouringBlock = neighbouringChunk.Blocks[x * Chunk.FlattenOffset + (Chunk.MAX.Z) * Chunk.SIZE.Y + y];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(x, y, Chunk.MAX.Z), BlockFaceDirection.ZIncreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            else if (z == Chunk.MAX.Z)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X, chunk.Index.Z + 1];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    //Block neighbouringBlock = neighbouringChunk.Blocks[x, y, 0];
                                    Block neighbouringBlock = neighbouringChunk.Blocks[x * Chunk.FlattenOffset + y];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(x, y, 0), BlockFaceDirection.ZDecreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            VertexPositionTextureShade[] a = _vertexList.ToArray();

            if (a.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureShade), a.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(a);

                chunk.dirty = false;
                //Debug.WriteLine("............building Vertexlist done");
            }
        }
        #endregion

        #region BuildBlockVertices
        public void BuildBlockVertices(ref List<VertexPositionTextureShade> vertexList, Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {
            //optimized by using chunk.Blocks[][][] except for "out of current chunk" blocks

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;
            Block blockXDecreasing, blockXIncreasing, blockYDecreasing, blockYIncreasing, blockZDecreasing, blockZIncreasing;
            Block solidBlock = new Block(BlockType.Rock);

            // X Boundary
            if (chunkRelativePosition.X == 0)
            {
                blockXDecreasing = solidBlock; //world.BlockAt(blockPosition.X - 1, blockPosition.Y, blockPosition.Z);
            }
            else
            {
                //blockXDecreasing = chunk.Blocks[chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
                blockXDecreasing = chunk.Blocks[(chunkRelativePosition.X - 1) * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.SIZE.Y + chunkRelativePosition.Y];
            }
            if (chunkRelativePosition.X == Chunk.MAX.X)
            {
                blockXIncreasing = solidBlock; //world.BlockAt(blockPosition.X + 1, blockPosition.Y, blockPosition.Z);
            }
            else
            {
                //blockXIncreasing = chunk.Blocks[chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
                blockXIncreasing = chunk.Blocks[(chunkRelativePosition.X + 1) * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.SIZE.Y + chunkRelativePosition.Y];
            }

            // Y Boundary
            if (chunkRelativePosition.Y == 0)
            {
                blockYDecreasing = solidBlock; //world.BlockAt(blockPosition.X, blockPosition.Y - 1, blockPosition.Z);
            }
            else
            {
                //blockYDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z];
                blockYDecreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.SIZE.Y + (chunkRelativePosition.Y - 1)];
            }
            if (chunkRelativePosition.Y == Chunk.MAX.Y)
            {
                blockYIncreasing = solidBlock; // world.BlockAt(blockPosition.X, blockPosition.Y + 1, blockPosition.Z);
            }
            else
            {
                //blockYIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z];
                blockYIncreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.SIZE.Y + (chunkRelativePosition.Y + 1)];
            }

            // Z Boundary
            if (chunkRelativePosition.Z == 0)
            {
                blockZDecreasing = solidBlock; //world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z - 1);
            }
            else
            {
                //blockZDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1];
                blockZDecreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + (chunkRelativePosition.Z - 1) * Chunk.SIZE.Y + chunkRelativePosition.Y];
            }
            if (chunkRelativePosition.Z == Chunk.MAX.Z)
            {
                blockZIncreasing = solidBlock; // world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z + 1);
            }
            else
            {
                //blockZIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1];
                blockZIncreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + (chunkRelativePosition.Z + 1) * Chunk.SIZE.Y + chunkRelativePosition.Y];
            }

            if (!blockXDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XDecreasing, block.Type);
            if (!blockXIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XIncreasing, block.Type);

            if (!blockYDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YDecreasing, block.Type);
            if (!blockYIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YIncreasing, block.Type);

            if (!blockZDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZDecreasing, block.Type);
            if (!blockZIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZIncreasing, block.Type);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {

            if (!chunk.generated) return;
            if (chunk.dirty)
            {
                QueueBuild();
            }

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
                    // graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexList.ToArray(), 0, _vertexList.Count / 3);
                }
                else
                {
                    Debug.WriteLine("no vertices");
                }
            }
        }
        #endregion

    }
}
