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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NewTake.view;
using NewTake.view.blocks;
#endregion

namespace NewTake.model
{

    public class Chunk
    {
        private const byte MAX_SUN_VALUE = 16;

        #region Fields

        private Chunk _N, _S, _E, _W, _NE, _NW, _SE, _SW; //TODO infinite y would require Top , Bottom, maybe vertical diagonals

        public static Vector3b SIZE = new Vector3b(16, 128, 16);
        public static Vector3b MAX = new Vector3b(15, 127, 15);

        public VertexBuffer VertexBuffer;
        public VertexBuffer waterVertexBuffer;

        public IndexBuffer IndexBuffer;
        public IndexBuffer waterIndexBuffer;

        public List<short> indexList;
        public List<short> waterindexList;

        public List<VertexPositionTextureLight> vertexList;
        public List<VertexPositionTextureLight> watervertexList;

        public short VertexCount;
        public short waterVertexCount;

        /// <summary>
        /// Contains blocks as a flattened array.
        /// </summary>
        public Block[] Blocks;

        /* 
        For accessing array for x,z,y coordianate use the pattern: Blocks[x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y]
        For allowing sequental access on blocks using iterations, the blocks are stored as [x,z,y]. So basically iterate x first, z then and y last.
        Consider the following pattern;
        for (int x = 0; x < Chunk.WidthInBlocks; x++)
        {
            for (int z = 0; z < Chunk.LenghtInBlocks; z++)
            {
                int offset = x * Chunk.FlattenOffset + z * Chunk.HeightInBlocks; // we don't want this x-z value to be calculated each in in y-loop!
                for (int y = 0; y < Chunk.HeightInBlocks; y++)
                {
                    var block=Blocks[offset + y].Type 
        */

        /// <summary>
        /// Used when accessing flatten blocks array.
        /// </summary>
        public static int FlattenOffset = SIZE.Z * SIZE.Y;

        public Vector3i Position;
        public Vector3i Index;

        public bool dirty;
        //public bool visible;
        //public bool generated;
        //public bool built;

        public bool broken;

        public readonly World world;

        private BoundingBox _boundingBox;

        public Vector3b highestSolidBlock = new Vector3b(0, 0, 0);
        //highestNoneBlock starts at 0 so it will be adjusted. if you start at highest it will never be adjusted ! 

        public Vector3b lowestNoneBlock = new Vector3b(0, SIZE.Y, 0);
        #endregion

        public ChunkState State { get; set; }

        public Chunk(World world, Vector3i index)
        {
            this.world = world;
            this.Blocks = new Block[SIZE.X * SIZE.Z * SIZE.Y];
            vertexList = new List<VertexPositionTextureLight>();
            watervertexList = new List<VertexPositionTextureLight>();
            indexList = new List<short>();
            waterindexList = new List<short>();

            Assign(index);
            /* world.viewableChunks[index.X, index.Z] = this;
             dirty = true;
             this.Index = index;
             this.Position = new Vector3i(index.X * SIZE.X, index.Y * SIZE.Y, index.Z * SIZE.Z);
             this._boundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + SIZE.X, Position.Y + SIZE.Y, Position.Z + SIZE.Z));
             */
        }

        public void Assign(Vector3i index)
        {
            //ensure world is set directly in here to have access to N S E W as soon as possible

            world.Chunks.Remove(this.Index.X, this.Index.Z);
            world.Chunks[index.X, index.Z] = this;

            dirty = true;
            //Array.Clear(Blocks, 0, Blocks.Length);
            this.Index = index;
            this.Position = new Vector3i(index.X * SIZE.X, index.Y * SIZE.Y, index.Z * SIZE.Z);
            this._boundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + SIZE.X, Position.Y + SIZE.Y, Position.Z + SIZE.Z));

            //TODO next optimization step would be reusing the vertexbuffer
            //vertexList.Clear(); 
            //indexList.Clear();

        }

        public void Clear()
        {
            vertexList.Clear();
            indexList.Clear();

            watervertexList.Clear();
            waterindexList.Clear();

            VertexCount = 0;
            waterVertexCount = 0;
        }

        #region setBlock
        public void setBlock(byte x, byte y, byte z, Block b)
        {
            if (b.Type == BlockType.Water)
            {

                if (lowestNoneBlock.Y > y)
                {
                    lowestNoneBlock = new Vector3b(x, y, z);
                }
            }

            if (b.Type == BlockType.None)
            {
                if (lowestNoneBlock.Y > y)
                {
                    lowestNoneBlock = new Vector3b(x, y, z);
                }
            }
            else if (highestSolidBlock.Y < y)
            {
                highestSolidBlock = new Vector3b(x, y, z);
            }

            //comment this line : you should have nothing on screen, else you ve been setting blocks directly in array !
            Blocks[x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y] = b;
            dirty = true;
        }
        #endregion

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }


        public bool outOfBounds(byte x, byte y, byte z)
        {
            return (x < 0 || x >= SIZE.X || y < 0 || y >= SIZE.Y || z < 0 || z >= SIZE.Z);
        }

        #region BlockAt

        public Block BlockAt(int relx, int rely, int relz)
        {

            if (rely < 0 || rely > Chunk.MAX.Y)
            {
                //infinite Y : y bounds currently set as rock for never rendering those y bounds
                return new Block(BlockType.Rock);
            }

            //handle the normal simple case
            if (relx >= 0 && relz >= 0 && relx < Chunk.SIZE.X && relz < Chunk.SIZE.Z)
            {
                Block block = Blocks[relx * Chunk.FlattenOffset + relz * Chunk.SIZE.Y + rely];
                return block;
            }

            //handle all special cases

            int x = relx, z = relz;
            Chunk nChunk = null;

            //TODO chunk relative BlockAt could even handle more tha just -1 but -2 -3 ... -15 

            if (relx < 0) x = Chunk.MAX.X;
            if (relz < 0) z = Chunk.MAX.Z;
            if (relx > 15) x = 0;
            if (relz > 15) z = 0;


            if (x != relx && x == 0)
                if (z != relz && z == 0)
                    nChunk = NW;
                else if (z != relz && z == 15)
                    nChunk = SW;
                else
                    nChunk = W;
            else if (x != relx && x == 15)
                if (z != relz && z == 0)
                    nChunk = NE;
                else if (z != relz && z == 15)
                    nChunk = SE;
                else
                    nChunk = E;
            else
                if (z != relz && z == 0)
                    nChunk = N;
                else if (z != relz && z == 15)
                    nChunk = S;

            if (nChunk == null)
            {
                //happens at current world bounds
                return new Block(BlockType.Rock);
            }
            else
            {
                Block block = nChunk.Blocks[x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + rely];
                return block;
            }

        }
        #endregion

        #region N S E W NE NW SE SW Neighbours accessors
        //this neighbours check can not be done in constructor, there would be some holes => it has to be done at access time 
        //seems there is no mem leak so no need for weakreferences
        public Chunk N
        {
            get
            {
                if (_N == null) _N = world.Chunks[Index.X, Index.Z + 1];
                if (_N != null) _N._S = this;
                return _N;
            }
        }
        public Chunk S
        {
            get
            {
                if (_S == null) _S = world.Chunks[Index.X, Index.Z - 1];
                if (_S != null) _S._N = this;
                return _S;
            }
        }
        public Chunk E
        {
            get
            {
                if (_E == null) _E = world.Chunks[Index.X - 1, Index.Z];
                if (_E != null) _E._W = this;
                return _E;
            }
        }

        public Chunk W
        {
            get
            {
                if (_W == null) _W = world.Chunks[Index.X + 1, Index.Z];
                if (_W != null) _W._E = this;
                return _W;
            }
        }

        public Chunk NW { get { return _NW != null ? _NW : _NW = world.Chunks[Index.X + 1, Index.Z + 1]; } }
        public Chunk NE { get { return _NE != null ? _NE : _NE = world.Chunks[Index.X - 1, Index.Z + 1]; } }
        public Chunk SW { get { return _SW != null ? _SW : _SW = world.Chunks[Index.X + 1, Index.Z - 1]; } }
        public Chunk SE { get { return _SE != null ? _SE : _SE = world.Chunks[Index.X - 1, Index.Z - 1]; } }

        public Chunk GetNeighbour(Cardinal c)
        {
            switch (c)
            {
                case Cardinal.N:
                    return N;
                case Cardinal.S:
                    return S;
                case Cardinal.E:
                    return E;
                case Cardinal.W:
                    return W;
                case Cardinal.SE:
                    return SE;
                case Cardinal.SW:
                    return SW;
                case Cardinal.NE:
                    return NE;
                case Cardinal.NW:
                    return NW;

            }
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString()
        {
            return ("chunk at index " + Index);
        }

        #region main as unit test for neighbours
        static void Main(string[] args)
        {
            World world = new World();

            uint n = 4, s = 6, w = 4, e = 6;

            Chunk cw = new Chunk(world, new Vector3i(w, 5, 5));
            Chunk c = new Chunk(world, new Vector3i(5, 5, 5));
            Chunk ce = new Chunk(world, new Vector3i(e, 5, 5));

            Chunk cn = new Chunk(world, new Vector3i(5, 5, n));
            Chunk cs = new Chunk(world, new Vector3i(5, 5, s));
            Chunk cne = new Chunk(world, new Vector3i(e, 5, n));
            Chunk cnw = new Chunk(world, new Vector3i(w, 5, n));
            Chunk cse = new Chunk(world, new Vector3i(e, 5, s));
            Chunk csw = new Chunk(world, new Vector3i(w, 5, s));


            c.setBlock(0, 0, 0, new Block(BlockType.Dirt));
            cw.setBlock(15, 0, 0, new Block(BlockType.Grass));

            Block w15 = c.BlockAt(-1, 0, 0);
            Debug.Assert(w15.Type == BlockType.Grass);

            ce.setBlock(0, 0, 0, new Block(BlockType.Tree));
            Block e0 = c.BlockAt(16, 0, 0);
            Debug.Assert(e0.Type == BlockType.Tree);

            csw.setBlock(15, 0, 0, new Block(BlockType.Lava));
            Block swcorner = c.BlockAt(-1, 0, 16);
            Debug.Assert(swcorner.Type == BlockType.Lava);

            cne.setBlock(0, 0, 15, new Block(BlockType.Leaves));
            Block necorner = c.BlockAt(16, 0, -1);
            Debug.Assert(necorner.Type == BlockType.Leaves);
        }
        #endregion

    }
}
