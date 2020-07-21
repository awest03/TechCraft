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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.view.blocks;
using NewTake.model;
using NewTake;
#endregion

namespace NewTake.view
{

    public class ChunkRenderer
    {
        #region inits

        public List<VertexPositionTextureShade> _vertexList;

        public VertexBuffer vertexBuffer;

        public Chunk chunk;
        public readonly World world;
        protected readonly VertexBlockRenderer blocksRenderer;
        public readonly GraphicsDevice graphicsDevice;

        #endregion

        protected ChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk)
        {
            this.chunk = chunk;
            this.world = world;
            this.graphicsDevice = graphicsDevice;
            _vertexList = new List<VertexPositionTextureShade>();

            blocksRenderer = new VertexBlockRenderer(world);
        }

        public virtual bool isInView(BoundingFrustum viewFrustum)
        {
            return chunk.BoundingBox.Intersects(viewFrustum);
        }

        #region BuildVertexList
        public virtual void BuildVertexList()
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

                            blocksRenderer.BuildBlockVertices(ref _vertexList, block, chunk, new Vector3i(x, y, z));
                        }
                    }
                }
            }
            VertexPositionTextureShade[] a = _vertexList.ToArray();

            if (a.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureShade), a.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(a);
            }
        }
        #endregion

        public virtual void DoLighting()
        {
        }

        #region Update
        public virtual void Update(GameTime gameTime)
        {
            if (chunk.dirty)
            {
                //_buildingThread = new Thread(new ThreadStart(BuildVertexList));
                ////_threadManager.Add(_buildingThread);
                //_buildingThread.Start();
                BuildVertexList();
            }
        }
        #endregion

        #region Draw
        public virtual void Draw(GameTime gameTime)
        {

            if (!chunk.generated) return;
            if (chunk.dirty)
            {
                BuildVertexList();
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
