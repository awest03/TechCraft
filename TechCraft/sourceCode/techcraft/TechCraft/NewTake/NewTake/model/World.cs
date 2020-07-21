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

using NewTake.model.terrain;
using NewTake.model.terrain.biome;
using NewTake.model.types;
#endregion

namespace NewTake.model
{
    public class World
    {

        #region Fields

        public ChunkManager Chunks;

        //public const byte VIEW_CHUNKS_X = 8;
        //public const byte VIEW_CHUNKS_Y = 1;
        //public const byte VIEW_CHUNKS_Z = 8;

        public static int SEED = 54321;

        public static uint origin = 1000;
        //TODO UInt32 requires decoupling rendering coordinates to avoid float problems

        //public const byte VIEW_DISTANCE_NEAR_X = VIEW_CHUNKS_X * 2;
       // public const byte VIEW_DISTANCE_NEAR_Z = VIEW_CHUNKS_Z * 2;

        //public const byte VIEW_DISTANCE_FAR_X = VIEW_CHUNKS_X * 4;
        //public const byte VIEW_DISTANCE_FAR_Z = VIEW_CHUNKS_Z * 4;

        public readonly RasterizerState _wireframedRaster = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        public readonly RasterizerState _normalRaster = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, FillMode = FillMode.Solid };
        public bool _wireframed = false;

        // Day/Night
        public float tod = 12; // Midday
        public Vector3 SunPos = new Vector3(0, 1, 0); // Directly overhead
        public bool RealTime = false;
        public bool dayMode = false;
        public bool nightMode = false;

        #region Atmospheric settings
        public Vector4 NIGHTCOLOR = Color.Black.ToVector4();
        public Vector4 SUNCOLOR = Color.White.ToVector4();
        public Vector4 HORIZONCOLOR = Color.White.ToVector4();

        public Vector4 EVENINGTINT = Color.Red.ToVector4();
        public Vector4 MORNINGTINT = Color.Gold.ToVector4();

        //private float _tod;
        //public bool dayMode = false;
        //public bool nightMode = false;
        public int FOGNEAR = 14 * 16;//(BUILD_RANGE - 1) * 16;
        public float FOGFAR = 16 * 16;//(BUILD_RANGE + 1) * 16;
        #endregion

        #endregion

        #region choose terrain generation
        //public IChunkGenerator Generator = new SimpleTerrain();
        //public IChunkGenerator Generator = new FlatReferenceTerrain();
        //public IChunkGenerator Generator = new TerrainWithCaves();
        public IChunkGenerator Generator = new DualLayerTerrainWithMediumValleysForRivers();

        // Biomes
        //public IChunkGenerator Generator = new Tundra_Alpine();
        //public IChunkGenerator Generator = new Desert_Subtropical();
        //public IChunkGenerator Generator = new Grassland_Temperate();
        #endregion

        public void ToggleRasterMode()
        {
            this._wireframed = !this._wireframed;
        }

        public World()
        {
            //Chunks = new Dictionary2<Chunk>();//
            Chunks = new ChunkManager(new MockChunkPersistence(this));
        }

        #region visitChunks
        public void visitChunks(Func<Vector3i,Chunk> visitor,byte radius)
        {
            //+1 is for having the player on a center chunk
            for (uint x = origin - radius; x < origin + radius+1; x++)
            {
                for (uint z = origin - radius; z < origin + radius+1; z++)
                {
                    visitor(new Vector3i(x, 0, z));
                }
            }
        }
        #endregion

        #region BlockAt
        public Block BlockAt(Vector3 position)
        {

            return BlockAt((uint)position.X, (uint)position.Y, (uint)position.Z);

        }

        public Chunk ChunkAt(Vector3 position)
        {
            uint x = (uint) position.X;
            uint z = (uint) position.Z;

            uint cx = x / Chunk.SIZE.X;
            uint cz = z / Chunk.SIZE.Z;

            Chunk at = Chunks[cx,cz];

            return at;
        }

        public Block BlockAt(uint x, uint y, uint z)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = Chunks[x / Chunk.SIZE.X, z / Chunk.SIZE.Z];
                return chunk.Blocks[(x % Chunk.SIZE.X) * Chunk.FlattenOffset + (z % Chunk.SIZE.Z) * Chunk.SIZE.Y + (y % Chunk.SIZE.Y)];
            }
            else
            {
                //Debug.WriteLine("no block at  ({0},{1},{2}) ", x, y, z);
                return new Block(BlockType.None); //TODO blocktype.unknown ( with matrix films green symbols texture ? ) 
            }
        }
        #endregion

        #region setBlock
        public Block setBlock(Vector3i pos, Block b)
        {
            return setBlock(pos.X, pos.Y, pos.Z, b);
        }

        public Block setBlock(uint x, uint y, uint z, Block newType)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = Chunks[x / Chunk.SIZE.X, z / Chunk.SIZE.Z];

                byte localX = (byte)(x % Chunk.SIZE.X);
                byte localY = (byte)(y % Chunk.SIZE.Y);
                byte localZ = (byte)(z % Chunk.SIZE.Z);

                Block old = chunk.Blocks[localX * Chunk.FlattenOffset + localZ * Chunk.SIZE.Y + localY];

                //chunk.setBlock is also called by terrain generators for Y loops min max optimisation
                chunk.setBlock(localX, localY, localZ, new Block(newType.Type));
             
                //Chunk should be responsible for maintaining this
                chunk.State = ChunkState.AwaitingRelighting;

                // use Chunk accessors
                if (localX == 0)
                {
                    if (chunk.E != null) chunk.E.State = ChunkState.AwaitingRelighting;
                }
                if (localX == Chunk.MAX.X)
                {
                    //viewableChunks[(x / Chunk.SIZE.X) + 1, z / Chunk.SIZE.Z].dirty = true;
                    if (chunk.W != null) chunk.W.State = ChunkState.AwaitingRelighting;
                }
                if (localZ == 0)
                {
                    //viewableChunks[x / Chunk.SIZE.X, (z / Chunk.SIZE.Z) - 1].dirty = true;
                    if (chunk.S != null) chunk.S.State = ChunkState.AwaitingRelighting;
                }
                if (localZ == Chunk.MAX.Z)
                {
                    //viewableChunks[x / Chunk.SIZE.X, (z / Chunk.SIZE.Z) + 1].dirty = true;
                    if (chunk.N != null) chunk.N.State = ChunkState.AwaitingRelighting;
                }

                return old;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region InView
        public bool InView(uint x, uint y, uint z)
        {
            if (Chunks[x / Chunk.SIZE.X, z / Chunk.SIZE.Z] == null)
                return false;

            uint lx = x % Chunk.SIZE.X;
            uint ly = y % Chunk.SIZE.Y;
            uint lz = z % Chunk.SIZE.Z;

            if (lx < 0 || ly < 0 || lz < 0
                || lx >= Chunk.SIZE.X
                || ly >= Chunk.SIZE.Y
                || lz >= Chunk.SIZE.Z)
            {

                //  Debug.WriteLine("no block at  ({0},{1},{2}) ", x, y, z);
                return false;
            }
            return true;
        }
        #endregion

    }
}
