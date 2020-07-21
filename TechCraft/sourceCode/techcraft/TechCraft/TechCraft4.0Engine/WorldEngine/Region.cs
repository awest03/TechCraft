using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace TechCraftEngine.WorldEngine
{
    public enum Buffer
    {
        Water,
        Model,
        Solid
    }

    public class Region
    {
        private Vector3i _position;

        public BlockType[, ,] Blocks { get; set; }
        private byte[, ,] _faceInfo;
        private World _world;
        private VertexBuffer _solidVertexBuffer;
        private VertexBuffer _modelVertexBuffer;
        private VertexBuffer _waterVertexBuffer;
        private BoundingBox _boundingBox;
        private bool _dirty;
        private int _solidFaceCount;
        private int _modelFaceCount;
        private int _waterFaceCount;
        private int _solidIndex;
        private int _modelIndex;
        private int _waterIndex;
        //private List<VertexPositionTextureShade> _solidVertexList;
        //private List<VertexPositionTextureShade> _modelVertexList;
        //private List<VertexPositionTextureShade> _waterVertexList;
        private VertexPositionTextureShade[] _solidVertexArray;
        private VertexPositionTextureShade[] _modelVertexArray;
        private VertexPositionTextureShade[] _waterVertexArray;

        private Vector3 _worldCenter;
        public Vector3 Center
        {
            get { return _worldCenter; }
        }


        private RegionManager _regionMngr;

        public RegionManager RegionManager { 

            get { return _regionMngr; } 
        }

        public bool Modified { get; set; }

        public int NodeIndex { get; set; }

        public Region(World world, Vector3i position)
        {
            _position = position;
            _regionMngr = new RegionManager(this);

            Blocks = new BlockType[WorldSettings.REGIONWIDTH, WorldSettings.REGIONHEIGHT, WorldSettings.REGIONLENGTH];
            _faceInfo = new byte[WorldSettings.REGIONWIDTH, WorldSettings.REGIONHEIGHT, WorldSettings.REGIONLENGTH];
            _world = world;

            _dirty = true;
            _boundingBox = new BoundingBox(new Vector3(_position.x, _position.y, _position.z), new Vector3(_position.x + WorldSettings.REGIONWIDTH, _position.y + WorldSettings.REGIONHEIGHT, _position.z + WorldSettings.REGIONLENGTH));
            //_solidVertexList = new List<VertexPositionTextureShade>();
            //_modelVertexList = new List<VertexPositionTextureShade>();
            //_waterVertexList = new List<VertexPositionTextureShade>();
            BuildUVMappings();
            InitRegion();

        }

        public Region(World _world, int nodeIndex)
        {
           
            this._world = _world;
            this.NodeIndex = nodeIndex;
        }

        public void InitRegion()
        {
            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.REGIONHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                    {
                        Blocks[x, y, z] = BlockType.None;
                        _faceInfo[x, y, z] = 0;
                    }
                }
            }
        }

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public Vector3i Position
        {
            get { return _position; }
        }

        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        public void Build()
        {
            BuildVertexBuffers();
            _dirty = false;
        }

        public VertexBuffer SolidVertexBuffer
        {
            get { return _solidVertexBuffer; }
        }

        public VertexBuffer ModelVertexBuffer
        {
            get { return _modelVertexBuffer; }
        }

        public VertexBuffer WaterVertexBuffer
        {
            get { return _waterVertexBuffer; }
        }

        public void BuildVertexBuffers()
        {
            //_solidVertexList.Clear();
            //_modelVertexList.Clear();
            //_waterVertexList.Clear();
            _solidVertexArray = new VertexPositionTextureShade[_solidFaceCount * 6];
            _modelVertexArray = new VertexPositionTextureShade[_modelFaceCount * 6];
            _waterVertexArray = new VertexPositionTextureShade[_waterFaceCount * 6];
            _solidIndex = 0;
            _waterIndex = 0;
            _modelIndex = 0;
            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.REGIONHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                    {
                        BlockType blockType = Blocks[x, y, z];
                        if (blockType != BlockType.None)
                        {
                            byte faceInfo = _faceInfo[x, y, z];
                            Vector3i position = new Vector3i(_position.x + x, _position.y + y, _position.z + z);
                            if (blockType == BlockType.Water)
                            {
                                BuildBlockVertices(_waterVertexArray, Buffer.Water, blockType, faceInfo, position);
                            }
                            else if (BlockInformation.IsModelBlock(blockType))
                            {
                                BuildBlockVertices(_modelVertexArray, Buffer.Model, blockType, faceInfo, position);
                            }
                            else
                            {
                                BuildBlockVertices(_solidVertexArray, Buffer.Solid, blockType, faceInfo, position);
                            }
                        }
                    }
                }
            }
            if (_waterVertexArray.Length > 0)
            {

                _waterVertexBuffer = new VertexBuffer(_world.Game.GraphicsDevice, typeof(VertexPositionTextureShade), _waterVertexArray.Length, BufferUsage.WriteOnly);

                _waterVertexBuffer.SetData(_waterVertexArray);
            }
            else
            {
                _waterVertexBuffer = null;
            }
            if (_modelVertexArray.Length > 0)
            {
                _modelVertexBuffer = new VertexBuffer(_world.Game.GraphicsDevice, typeof(VertexPositionTextureShade), _modelVertexArray.Length, BufferUsage.WriteOnly);

                _modelVertexBuffer.SetData(_modelVertexArray);
            }
            else
            {
                _modelVertexBuffer = null;

            }
            if (_solidVertexArray.Length > 0)
            {
                _solidVertexBuffer = new VertexBuffer(
                     _world.Game.GraphicsDevice, typeof(VertexPositionTextureShade), _solidVertexArray.Length, BufferUsage.WriteOnly);

                _solidVertexBuffer.SetData(_solidVertexArray);
            }
            else
            {
                _solidVertexBuffer = null;
            }
        }

        private void BuildBlockVertices(VertexPositionTextureShade[] vertexArray, Buffer buffer, BlockType blockType, byte faceInfo, Vector3i position)
        {
            if ((faceInfo & (byte)BlockFaceDirection.XDecreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.XDecreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.XDecreasing));
            if ((faceInfo & (byte)BlockFaceDirection.XIncreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.XIncreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.XIncreasing));
            if ((faceInfo & (byte)BlockFaceDirection.YDecreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.YDecreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.YDecreasing));
            if ((faceInfo & (byte)BlockFaceDirection.YIncreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.YIncreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.YIncreasing));
            if ((faceInfo & (byte)BlockFaceDirection.ZDecreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.ZDecreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.ZDecreasing));
            if ((faceInfo & (byte)BlockFaceDirection.ZIncreasing) > 0) BuildFaceVertices(position.x, position.y, position.z, buffer, ref vertexArray, BlockFaceDirection.ZIncreasing, BlockInformation.GetTexture(blockType, BlockFaceDirection.ZIncreasing));
        }


        private void BuildFaceVertices(int x, int y, int z, Buffer buffer, ref VertexPositionTextureShade[] vertexArray, BlockFaceDirection faceDir, BlockTexture texture)
        {
            int faceIndex = 0;
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    faceIndex = 0;
                    break;
                case BlockFaceDirection.XDecreasing:
                    faceIndex = 1;
                    break;
                case BlockFaceDirection.YIncreasing:
                    faceIndex = 2;
                    break;
                case BlockFaceDirection.YDecreasing:
                    faceIndex = 3;
                    break;
                case BlockFaceDirection.ZIncreasing:
                    faceIndex = 4;
                    break;
                case BlockFaceDirection.ZDecreasing:
                    faceIndex = 5;
                    break;
            }
            Vector2[] UVList = UVMappings[(int)texture * 6 + faceIndex];
            int index = 0;
            switch (buffer)
            {
                case Buffer.Model:
                    index = _modelIndex;
                    break;
                case Buffer.Solid:
                    index = _solidIndex;
                    break;
                case Buffer.Water:
                    index = _waterIndex;
                    break;
            }

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {

                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), UVList[0], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), UVList[1], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), UVList[2], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), UVList[3], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), UVList[4], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), UVList[5], _world.Lighting.GetLight(x + 1, y, z) + WorldSettings.SIDESHADOWS);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[0], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), UVList[1], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), UVList[2], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[3], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), UVList[4], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), UVList[5], _world.Lighting.GetLight(x - 1, y, z) + WorldSettings.SIDESHADOWS);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[0], _world.Lighting.GetLight(x, y + 1, z));
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), UVList[1], _world.Lighting.GetLight(x, y + 1, z));
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), UVList[2], _world.Lighting.GetLight(x, y + 1, z));
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[3], _world.Lighting.GetLight(x, y + 1, z));
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), UVList[4], _world.Lighting.GetLight(x, y + 1, z));
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), UVList[5], _world.Lighting.GetLight(x, y + 1, z));
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), UVList[0], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), UVList[1], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), UVList[2], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), UVList[3], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), UVList[4], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), UVList[5], _world.Lighting.GetLight(x, y - 1, z) + WorldSettings.SIDESHADOWS);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), UVList[0], _world.Lighting.GetLight(x, y, z + 1));
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), UVList[1], _world.Lighting.GetLight(x, y, z + 1));
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), UVList[2], _world.Lighting.GetLight(x, y, z + 1));
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), UVList[3], _world.Lighting.GetLight(x, y, z + 1));
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), UVList[4], _world.Lighting.GetLight(x, y, z + 1));
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), UVList[5], _world.Lighting.GetLight(x, y, z + 1));
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        vertexArray[index] = new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), UVList[0], _world.Lighting.GetLight(x, y, z - 1));
                        vertexArray[index + 1] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[1], _world.Lighting.GetLight(x, y, z - 1));
                        vertexArray[index + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), UVList[2], _world.Lighting.GetLight(x, y, z - 1));
                        vertexArray[index + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), UVList[3], _world.Lighting.GetLight(x, y, z - 1));
                        vertexArray[index + 4] = new VertexPositionTextureShade(new Vector3(x, y + 1, z), UVList[4], _world.Lighting.GetLight(x, y, z - 1));
                        vertexArray[index + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), UVList[5], _world.Lighting.GetLight(x, y, z - 1));
                    }
                    break;
            }
            index += 6;
            switch (buffer)
            {
                case Buffer.Model:
                    _modelIndex = index;
                    break;
                case Buffer.Solid:
                    _solidIndex = index;
                    break;
                case Buffer.Water:
                    _waterIndex = index;
                    break;
            }
        }

        private VertexPositionTextureShade BuildVertex(Vector3 position, Vector2 textureCoordinate, float shade)
        {
            VertexPositionTextureShade vertex = new VertexPositionTextureShade();
            vertex.Position = position;
            vertex.TextureCoordinate = textureCoordinate;
            vertex.Shade = shade;
            return vertex;
        }

        public BlockType BlockAt(int x, int y, int z)
        {
            return Blocks[x, y, z];
        }

        public byte FacesAt(int x, int y, int z)
        {
            return _faceInfo[x, y, z];
        }

        public void AddBlock(int x, int y, int z, BlockType blockType)
        {
            if (_world.InWorldBounds(x, y, z))
            {
                Blocks[x, y, z] = blockType;
                _faceInfo[x, y, z] = 0;
                Vector3i position = new Vector3i(_position.x + x, _position.y + y, _position.z + z);

                BlockType xDecreasing = _world.BlockAt(position.x - 1, position.y, position.z);
                BlockType xIncreasing = _world.BlockAt(position.x + 1, position.y, position.z);
                BlockType yDecreasing = _world.BlockAt(position.x, position.y - 1, position.z);
                BlockType yIncreasing = _world.BlockAt(position.x, position.y + 1, position.z);
                BlockType zDecreasing = _world.BlockAt(position.x, position.y, position.z - 1);
                BlockType zIncreasing = _world.BlockAt(position.x, position.y, position.z + 1);

                SetBlockFaces(blockType, position.x, position.y, position.z, xDecreasing, position.x - 1, position.y, position.z, BlockFaceDirection.XDecreasing, BlockFaceDirection.XIncreasing);
                SetBlockFaces(blockType, position.x, position.y, position.z, xIncreasing, position.x + 1, position.y, position.z, BlockFaceDirection.XIncreasing, BlockFaceDirection.XDecreasing);
                SetBlockFaces(blockType, position.x, position.y, position.z, yDecreasing, position.x, position.y - 1, position.z, BlockFaceDirection.YDecreasing, BlockFaceDirection.YIncreasing);
                SetBlockFaces(blockType, position.x, position.y, position.z, yIncreasing, position.x, position.y + 1, position.z, BlockFaceDirection.YIncreasing, BlockFaceDirection.YDecreasing);
                SetBlockFaces(blockType, position.x, position.y, position.z, zDecreasing, position.x, position.y, position.z - 1, BlockFaceDirection.ZDecreasing, BlockFaceDirection.ZIncreasing);
                SetBlockFaces(blockType, position.x, position.y, position.z, zIncreasing, position.x, position.y, position.z + 1, BlockFaceDirection.ZIncreasing, BlockFaceDirection.ZDecreasing);
                _dirty = true;
            }
        }

        public BlockType RemoveBlock(int x, int y, int z)
        {
            if (_world.InWorldBounds(x, y, z))
            {
                BlockType blockType = Blocks[x, y, z];
                Blocks[x, y, z] = BlockType.None;
                Vector3i position = new Vector3i(_position.x + x, _position.y + y, _position.z + z);
                BlockType xDecreasing = _world.BlockAt(position.x - 1, position.y, position.z);
                BlockType xIncreasing = _world.BlockAt(position.x + 1, position.y, position.z);
                BlockType yDecreasing = _world.BlockAt(position.x, position.y - 1, position.z);
                BlockType yIncreasing = _world.BlockAt(position.x, position.y + 1, position.z);
                BlockType zDecreasing = _world.BlockAt(position.x, position.y, position.z - 1);
                BlockType zIncreasing = _world.BlockAt(position.x, position.y, position.z + 1);
                SetBlockFaces(xDecreasing, position.x - 1, position.y, position.z, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.XIncreasing, BlockFaceDirection.XDecreasing);
                SetBlockFaces(xIncreasing, position.x + 1, position.y, position.z, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.XDecreasing, BlockFaceDirection.XIncreasing);
                SetBlockFaces(yDecreasing, position.x, position.y - 1, position.z, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.YIncreasing, BlockFaceDirection.YDecreasing);
                SetBlockFaces(yIncreasing, position.x, position.y + 1, position.z, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.YDecreasing, BlockFaceDirection.YIncreasing);
                SetBlockFaces(zDecreasing, position.x, position.y, position.z - 1, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.ZIncreasing, BlockFaceDirection.ZDecreasing);
                SetBlockFaces(zIncreasing, position.x, position.y, position.z + 1, BlockType.None, position.x, position.y, position.z, BlockFaceDirection.ZDecreasing, BlockFaceDirection.ZIncreasing);
                _dirty = true;
                return blockType;
            }
            else
            {
                return BlockType.None;
            }
        }

        public void AddWaterFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) == 0)
            {
                _waterFaceCount++;
                _faceInfo[x, y, z] |= (byte)face;
                _dirty = true;
            }
        }

        public void RemoveWaterFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) > 0)
            {
                _waterFaceCount--;
                _faceInfo[x, y, z] &= (byte)(255 - (byte)face);
                _dirty = true;
            }
        }

        public void AddModelFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) == 0)
            {
                _modelFaceCount++;
                _faceInfo[x, y, z] |= (byte)face;
                _dirty = true;
            }
        }

        public void RemoveModelFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) > 0)
            {
                _modelFaceCount--;
                _faceInfo[x, y, z] &= (byte)(255 - (byte)face);
                _dirty = true;
            }
        }

        public void AddSolidFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) == 0)
            {
                _solidFaceCount++;
                _faceInfo[x, y, z] |= (byte)face;
                _dirty = true;
            }
        }

        public void RemoveSolidFace(int x, int y, int z, BlockFaceDirection face)
        {
            if ((_faceInfo[x, y, z] & (byte)face) > 0)
            {
                _solidFaceCount--;
                _faceInfo[x, y, z] &= (byte)(255 - (byte)face);
                _dirty = true;
            }
        }
        private void SetBlockFaces(BlockType blockType, int bx, int by, int bz, BlockType neighbourType, int nx, int ny, int nz, BlockFaceDirection bFace, BlockFaceDirection nFace)
        {
            if (neighbourType == BlockType.None)
            {
                // Bordering sky make face visible
                if (blockType != BlockType.None)
                {
                    if (blockType == BlockType.Water)
                    {
                        _world.AddWaterFace(bx, by, bz, bFace);
                    }
                    else
                    {
                        _world.AddSolidFace(bx, by, bz, bFace);
                    }
                }
            }
            else
            {
                if (neighbourType == BlockType.Water)
                {
                    if (blockType == BlockType.None)
                    {
                        if (blockType == BlockType.Water)
                        {
                            _world.AddWaterFace(bx, by, bz, bFace);
                        }
                        else
                        {
                            _world.AddSolidFace(bx, by, bz, bFace);
                        }
                    }
                    else if (blockType == BlockType.Water)
                    {
                        // Removed ajoining faces if both blocks are water
                        _world.RemoveWaterFace(bx, by, bz, bFace);
                        _world.RemoveWaterFace(nx, ny, nz, nFace);

                    }
                    else if (BlockInformation.IsModelBlock(blockType))
                    {
                        _world.AddModelFace(bx, by, bz, bFace);
                        _world.AddWaterFace(nx, ny, nz, nFace);
                    }
                    else
                    {
                        // Can always see face through water
                        if (blockType == BlockType.Water)
                        {
                            _world.AddWaterFace(bx, by, bz, bFace);
                        }
                        else
                        {
                            _world.AddSolidFace(bx, by, bz, bFace);
                        }
                        _world.RemoveWaterFace(nx, ny, nz, nFace);
                    }
                }
                else if (BlockInformation.IsModelBlock(neighbourType))
                {
                    if (blockType == BlockType.None)
                    {
                        _world.AddModelFace(nx, ny, nz, nFace);
                    }
                    else if (blockType == BlockType.Water)
                    {
                        // Can see through water
                        _world.AddWaterFace(bx, by, bz, bFace);
                        _world.AddModelFace(nx, ny, nz, nFace);
                    }
                    else if (BlockInformation.IsModelBlock(blockType))
                    {
                        // Can see through transparent
                        _world.AddModelFace(bx, by, bz, bFace);
                        _world.AddModelFace(nx, ny, nz, nFace);
                    }
                    else
                    {
                        // Can see  through transparent and note that neightbour face is occluded
                        _world.AddSolidFace(bx, by, bz, bFace);
                        _world.RemoveModelFace(nx, ny, nz, nFace);
                    }
                }
                else
                {
                    if (blockType == BlockType.None)
                    {
                        _world.AddSolidFace(nx, ny, nz, nFace);
                    }
                    else if (blockType == BlockType.Water)
                    {
                        // We can see it through water
                        if (bFace == BlockFaceDirection.YIncreasing)
                        {
                            _world.AddWaterFace(bx, by, bz, bFace);
                        }
                        else
                        {
                            _world.RemoveWaterFace(bx, by, bz, bFace);
                        }
                        _world.AddSolidFace(nx, ny, nz, nFace);
                    }
                    else if (BlockInformation.IsModelBlock(blockType))
                    {
                        // We can see neighbour face and note that block face is occluded
                        _world.RemoveModelFace(bx, by, bz, bFace);
                        _world.AddSolidFace(nx, ny, nz, nFace);
                    }
                    else
                    {
                        // Both faces hidden

                        _world.RemoveSolidFace(bx, by, bz, bFace);
                        _world.RemoveSolidFace(nx, ny, nz, nFace);
                    }
                }
            }
        }

        private Dictionary<int, Vector2[]> UVMappings;
      

        private void BuildUVMappings()
        {
            UVMappings = new Dictionary<int, Vector2[]>();
            for (int i = 0; i < (int)BlockTexture.MAXIMUM; i++)
            {
                UVMappings.Add((i * 6), GetUVMapping(i, BlockFaceDirection.XIncreasing));
                UVMappings.Add((i * 6) + 1, GetUVMapping(i, BlockFaceDirection.XDecreasing));
                UVMappings.Add((i * 6) + 2, GetUVMapping(i, BlockFaceDirection.YIncreasing));
                UVMappings.Add((i * 6) + 3, GetUVMapping(i, BlockFaceDirection.YDecreasing));
                UVMappings.Add((i * 6) + 4, GetUVMapping(i, BlockFaceDirection.ZIncreasing));
                UVMappings.Add((i * 6) + 5, GetUVMapping(i, BlockFaceDirection.ZDecreasing));
            }
        }

        private Vector2[] GetUVMapping(int texture, BlockFaceDirection faceDir)
        {
            int textureIndex = texture;
            // Assumes a texture atlas of 8x8 textures

            int y = textureIndex / WorldSettings.TEXTUREATLASSIZE;
            int x = textureIndex % WorldSettings.TEXTUREATLASSIZE;

            float ofs = 1f / 8f;

            float yOfs = y * ofs;
            float xOfs = x * ofs;

            //ofs -= 0.01f;

            Vector2[] UVList = new Vector2[6];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.XDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.YIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[1] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.YDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.ZIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.ZDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
            }
            return UVList;
        }


        public void Clear()
        {
            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.REGIONHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                    {
                        {
                            Blocks[x, y, z] = BlockType.None;
                        }
                    }
                }
                //TODO _vertexList.Clear();
                _dirty = false;
                //TODO _modified = false;
            }

        }

       
    }
}
