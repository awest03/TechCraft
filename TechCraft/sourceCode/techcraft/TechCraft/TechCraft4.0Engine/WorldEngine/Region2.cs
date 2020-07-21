using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace TechCraftEngine.WorldEngine
{
   /* public class Region2
    {
        // MOVE THIS OUTSIDE OF REGION
        private Color[] _colours;

        private Vector3 _regionPosition;
        private Vector3 _size;  
        private Vector3 _worldPosition;
        private Vector3 _worldCenter;
        public Vector3 Center
        {
            get { return _worldCenter; }
        }
        private byte[, ,] _blockType;

        private World _world;

        private List<VertexPositionTextureShade> _vertexList;
        private VertexBuffer _vertexBuffer;
     
        private BoundingBox _boundingBox;
        private BoundingSphere _boundingSphere;

        private bool _dirty;        
        private bool _modified;
        private int _nodeIndex;

        private Game _game;
       

        public Region2(Game game)
        {
            _game = game;
          
        }

        public void Initialize(World world, Vector3 size)
        {

            GetServices();
            _size = size;
            _blockType = new byte[(int)_size.X, (int)_size.Y, (int)_size.Z];
            _vertexList = new List<VertexPositionTextureShade>();

            _world = world;
            _boundingBox = new BoundingBox(Vector3.Zero, new Vector3(size.X, size.Y, size.Z));
            _boundingSphere = BoundingSphere.CreateFromBoundingBox(_boundingBox);


           
        }

        public bool Modified
        {
            get { return _modified; }
            set { _modified = value; }
        }

        public string GetFilename()
        {
            return string.Format("{0}{1}-{2}-{3}", _world.LevelFolder, (int)_regionPosition.X, (int)_regionPosition.Y, (int)_regionPosition.Z);
        }

        public void Flush(bool clear)
        {
#if !XBOX
            //Debug.WriteLine("Flushing " + GetFilename());
            FileStream fs = File.Open(GetFilename(), FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            Save(writer);
            writer.Flush();
            fs.Close();
#endif
            if (clear)
            {
                Clear();
            }
        }

        public void Load(Vector3 position)
        {
            _regionPosition = position;
            _worldPosition = new Vector3(_regionPosition.X * _size.X, _regionPosition.Y * _size.Y, _regionPosition.Z * _size.Z);
            _worldCenter = _worldPosition + new Vector3(_size.X / 2, _size.Y / 2, _size.Z / 2);
          
            string filename = GetFilename();


                //Debug.WriteLine("Loading " + filename);
                
#if XBOX
            if (_graphicsManager.ContentAvailable(filename))
            {
                Stream fs = TitleContainer.OpenStream("Content\\" + filename);
#else
            if (File.Exists(filename))
            {
                FileStream fs = File.Open(filename, FileMode.Open);
#endif
                BinaryReader reader = new BinaryReader(fs);
                Load(reader);
                reader.Close();
                fs.Close();               
            }
            else
            {
                //Debug.WriteLine("New " + filename);
                Clear();

            }
        }

        public void Save(BinaryWriter writer)
        {
            //writer.Write(_regionPosition.X);
            //writer.Write(_regionPosition.Y);
            //writer.Write(_regionPosition.Z);
            //writer.Write(_size.X);
            //writer.Write(_size.Y);
            //writer.Write(_size.Z);

            for (int x = 0; x < _size.X; x++)
            {
                for (int y = 0; y < _size.Y; y++)
                {
                    for (int z = 0; z < _size.Z; z++)
                    {
                        writer.Write(_blockType[x, y, z]);
                    }
                }
            }
        }

        public void Load(BinaryReader reader)
        {
            //_regionPosition = new Vector3((int)reader.ReadDouble(), (int)reader.ReadDouble(), (int)reader.ReadDouble());
            //_size = new Vector3((int)reader.ReadDouble(), (int)reader.ReadDouble(), (int)reader.ReadDouble());
            for (int x = 0; x < _size.X; x++)
            {
                for (int y = 0; y < _size.Y; y++)
                {
                    for (int z = 0; z < _size.Z; z++)
                    {
                        _blockType[x, y, z] = reader.ReadByte();
                    }
                }
            }
            //_dirty = true;


            //Debug.WriteLine(string.Format("Loaded {0}-{1}-{2}", (int) _regionPosition.X, (int) _regionPosition.Y, (int) _regionPosition.Z));
        }

        public Game Game
        {
            get { return _game; }
        }



      

        public int NodeIndex
        {
            get { return _nodeIndex; }
            set { _nodeIndex = value; }
        }

        private void Clear()
        {
            for (int x = 0; x < _size.X; x++)
            {
                for (int y = 0; y < _size.Y; y++)
                {
                    for (int z = 0; z < _size.Z; z++)
                    {
                        _blockType[x, y, z] = 0;
                    }
                }
            }
            _vertexList.Clear();
            _dirty = false;
            _modified = false;
        }

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public Vector3 RegionPosition
        {
            get { return _regionPosition; }
        }

        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

       

        public void UpdateSceneObject()
        {
            //Debug.WriteLine("Updating Scene Object " + GetFilename());
            BuildVertexBuffer();
            BuildSceneObject();
            _dirty = false;
        }

        public void BuildVertexList()
        {
            _vertexList.Clear();

            for (int x = 0; x < _size.X; x++)
            {
                for (int y = 0; y < _size.Y; y++)
                {
                    for (int z = 0; z < _size.Z; z++)
                    {
                        byte blockType = _blockType[x, y, z];
                        if (blockType != 0)
                        {
                            Vector3i position = new Vector3i(x, y, z);
                            BuildBlockVertices(blockType, position);
                        }
                    }
                }
            }            
            _dirty = true;
            //Debug.WriteLine("Vertex List Built " + GetFilename());
        }

        private void BuildVertexBuffer()
        {           
            if (_vertexList.Count > 0)
            {
                _vertexBuffer = new Microsoft.Xna.Framework.Graphics.VertexBuffer(_world.Game.GraphicsDevice, typeof(VertexPositionTextureShade), _vertexList.Count, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(_vertexList.ToArray());
            }
        }

        private void BuildSceneObject()
        {
            _sceneObject.Clear(); // Clear any existing meshes
            if (_vertexBuffer != null)
            {
                RenderableMesh mesh = new RenderableMesh();
                mesh.Build(_sceneObject, _graphicsManager.BlockEffect, Matrix.Identity, _boundingSphere, _boundingBox, null, _vertexBuffer, 0, PrimitiveType.TriangleList, _vertexList.Count / 3, 0, _vertexList.Count, 0);
                _sceneObject.Add(mesh);
                _sceneObject.World = Matrix.CreateTranslation(_worldPosition);
            }
        }

        public void LoadFromHeightMap(int x, int z, Color[] _data)
        {
            for (int ix = x; ix < x + 96; ix++)
            {
                for (int iz = z; iz < z + 96; iz++)
                {
                    Color color = _data[iz * 2048 + ix];
                    int height = color.R / 8;
                    for (int y = 0; y < height; y++)
                    {
                        _blockType[ix-x, y, iz-z] = 1;
                    }
                }
            }
        }

        private void BuildBlockVertices(byte blockType, Vector3i blockPosition)
        {
            byte blockXDecreasing = _world.BlockAt((int) _worldPosition.X + blockPosition.x - 1, (int) _worldPosition.Y + blockPosition.y, (int) _worldPosition.Z + blockPosition.z);
            byte blockXIncreasing = _world.BlockAt((int)_worldPosition.X + blockPosition.x + 1, (int)_worldPosition.Y + blockPosition.y, (int)_worldPosition.Z + blockPosition.z);

            byte blockYDecreasing = _world.BlockAt((int)_worldPosition.X + blockPosition.x, (int)_worldPosition.Y + blockPosition.y - 1, (int)_worldPosition.Z + blockPosition.z);
            byte blockYIncreasing = _world.BlockAt((int)_worldPosition.X + blockPosition.x, (int)_worldPosition.Y + blockPosition.y + 1, (int)_worldPosition.Z + blockPosition.z);

            byte blockZDecreasing = _world.BlockAt((int)_worldPosition.X + blockPosition.x, (int)_worldPosition.Y + blockPosition.y, (int)_worldPosition.Z + blockPosition.z - 1);
            byte blockZIncreasing = _world.BlockAt((int)_worldPosition.X + blockPosition.x, (int)_worldPosition.Y + blockPosition.y, (int)_worldPosition.Z + blockPosition.z + 1);

            if (blockXDecreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.XDecreasing, _colours[blockType]);
            if (blockXIncreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.XIncreasing, _colours[blockType]);

            if (blockYDecreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.YDecreasing, _colours[blockType]);
            if (blockYIncreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.YIncreasing, _colours[blockType]);

            if (blockZDecreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.ZDecreasing, _colours[blockType]);
            if (blockZIncreasing == 0) BuildFaceVertices(blockPosition.x, blockPosition.y, blockPosition.z, BlockFaceDirection.ZIncreasing, _colours[blockType]);
        }


        private void BuildFaceVertices(int x, int y, int z, BlockFaceDirection faceDir, Color color)
        {
            Vector2[] UVList = GetUVMapping(faceDir);

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), new Vector3(1, 0, 0), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), new Vector3(1, 0, 0), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector3(1, 0, 0), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector3(1, 0, 0), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), new Vector3(1, 0, 0), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector3(1, 0, 0), color, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(-1, 0, 0), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), new Vector3(-1, 0, 0), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector3(-1, 0, 0), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(-1, 0, 0), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector3(-1, 0, 0), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z), new Vector3(-1, 0, 0), color, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(0, 1, 0), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), new Vector3(0, 1, 0), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), new Vector3(0, 1, 0), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(0, 1, 0), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), new Vector3(0, 1, 0), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), new Vector3(0, 1, 0), color, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector3(0, -1, 0), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector3(0, -1, 0), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector3(0, -1, 0), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector3(0, -1, 0), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector3(0, -1, 0), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z), new Vector3(0, -1, 0), color, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), new Vector3(0, 0, 1), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z + 1), new Vector3(0, 0, 1), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector3(0, 0, 1), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z + 1), new Vector3(0, 0, 1), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector3(0, 0, 1), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector3(0, 0, 1), color, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y + 1, z), new Vector3(0, 0, -1), color, UVList[0]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(0, 0, -1), color, UVList[1]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector3(0, 0, -1), color, UVList[2]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector3(0, 0, -1), color, UVList[3]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y + 1, z), new Vector3(0, 0, -1), color, UVList[4]));
                        _vertexList.Add(new VertexPositionTextureShade(new Vector3(x, y, z), new Vector3(0, 0, -1), color, UVList[5]));
                    }
                    break;
            }
        }

        public byte BlockAt(int x, int y, int z)
        {
            return _blockType[x, y, z];
        }


        public void AddBlock(int x, int y, int z, byte blockType)
        {
            if (_world.InWorldBounds(x, y, z))
            {
                _blockType[x, y, z] = (byte) blockType;
                _modified = true;  
            }
        }

        public byte RemoveBlock(int x, int y, int z)
        {
            if (_world.InWorldBounds(x, y, z))
            {
                byte blockType = (byte)_blockType[x, y, z];
                _blockType[x, y, z] = 0;
                _modified = true;
                return blockType;
            }
            else
            {
                return 0;
            }
        }

        private Vector2[] GetUVMapping(BlockFaceDirection faceDir)
        {
            Vector2[] UVList = new Vector2[6];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    UVList[0] = new Vector2(0, 0);
                    UVList[1] = new Vector2(1, 0);
                    UVList[2] = new Vector2(0, 1);
                    UVList[3] = new Vector2(0, 1);
                    UVList[4] = new Vector2(1, 0);
                    UVList[5] = new Vector2(1, 1);
                    break;
                case BlockFaceDirection.XDecreasing:
                    UVList[0] = new Vector2(0, 0);
                    UVList[1] = new Vector2(1, 0);
                    UVList[2] = new Vector2(1, 1);
                    UVList[3] = new Vector2(0, 0);
                    UVList[4] = new Vector2(1, 1);
                    UVList[5] = new Vector2(0, 1);
                    break;
                case BlockFaceDirection.YIncreasing:
                    UVList[0] = new Vector2(0, 1);
                    UVList[1] = new Vector2(0, 0);
                    UVList[2] = new Vector2(1, 0);
                    UVList[3] = new Vector2(0, 1);
                    UVList[4] = new Vector2(1, 0);
                    UVList[5] = new Vector2(1, 1);
                    break;
                case BlockFaceDirection.YDecreasing:
                    UVList[0] = new Vector2(0, 0);
                    UVList[1] = new Vector2(1, 0);
                    UVList[2] = new Vector2(0, 1);
                    UVList[3] = new Vector2(0, 1);
                    UVList[4] = new Vector2(1, 0);
                    UVList[5] = new Vector2(1, 1);
                    break;
                case BlockFaceDirection.ZIncreasing:
                    UVList[0] = new Vector2(0, 0);
                    UVList[1] = new Vector2(1, 0);
                    UVList[2] = new Vector2(1, 1);
                    UVList[3] = new Vector2(0, 0);
                    UVList[4] = new Vector2(1, 1);
                    UVList[5] = new Vector2(0, 1);
                    break;
                case BlockFaceDirection.ZDecreasing:
                    UVList[0] = new Vector2(0, 0);
                    UVList[1] = new Vector2(1, 0);
                    UVList[2] = new Vector2(0, 1);
                    UVList[3] = new Vector2(0, 1);
                    UVList[4] = new Vector2(1, 0);
                    UVList[5] = new Vector2(1, 1);
                    break;
            }
            return UVList;
        }

        public Vector3 WorldCenter
        {
            get { return _worldCenter; }
        }
    }*/
}
