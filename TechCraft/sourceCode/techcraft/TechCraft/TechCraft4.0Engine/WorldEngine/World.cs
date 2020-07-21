using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using TechCraftEngine.WorldEngine.Generators;

namespace TechCraftEngine.WorldEngine
{
    public class World
    {
        public const int REGIONRATIOWIDTH = WorldSettings.MAPWIDTH / WorldSettings.REGIONWIDTH;
        public const int REGIONRATIOLENGTH = WorldSettings.MAPLENGTH / WorldSettings.REGIONLENGTH;
        public const int REGIONRATIOHEIGHT = WorldSettings.MAPHEIGHT / WorldSettings.REGIONHEIGHT;
        public const int REGIONCOUNT = REGIONRATIOWIDTH * REGIONRATIOLENGTH * REGIONRATIOHEIGHT;

        private Region[,,] _regions;
        private TechCraftGame _game;
        private Lighting _lighting;

        // Rendering statistics
        private int _regionsDrawn;
        //private int _regionsBuilt;
        //private int _polysDrawn;

        private VertexDeclaration _vertexDeclaration;
        private Effect _solidBlockEffect;
        private Effect _waterBlockEffect;
        private Texture2D _textureAtlas;

        private Thread _waterThread;
        private Queue<Flow> _waterQueue;
        private Queue<Flow> _lavaQueue;

       // private Block _blockNone;

        public World(TechCraftGame game)
        {
            _game = game;
            //_blockNone = new Block(BlockType.None);
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return _vertexDeclaration; }
        }

        public void Initialize()
        {
            InitializeRegions();
            _vertexDeclaration = new VertexDeclaration(VertexPositionTextureShade.VertexElements);
            _solidBlockEffect = Game.Content.Load<Effect>("Effects\\SolidBlockEffect");
            _waterBlockEffect = Game.Content.Load<Effect>("Effects\\WaterBlockEffect");
            _textureAtlas = _game.Content.Load<Texture2D>("Textures\\blocks");
            _waterQueue = new Queue<Flow>();
            _lavaQueue = new Queue<Flow>();
            _lighting = new Lighting(_game,this);
            _waterThread = new Thread(new ThreadStart(DoFlow));
            _waterThread.Start();
            _game.Threads.Add(_waterThread);
        }

        private void InitializeRegions()
        {
            _regions = new Region[REGIONRATIOWIDTH, REGIONRATIOHEIGHT, REGIONRATIOLENGTH];
            for (int x = 0; x < REGIONRATIOWIDTH; x++)
            {
                for (int y = 0; y < REGIONRATIOHEIGHT; y++)
                {
                    for (int z = 0; z < REGIONRATIOLENGTH; z++)
                    {
                        _regions[x, y, z] = new Region(this, new Vector3i(x * WorldSettings.REGIONWIDTH, y * WorldSettings.REGIONHEIGHT, z * WorldSettings.REGIONLENGTH));
                    }
                }
            }
        }

        public void BuildRegions(IRegionBuilder builder)
        {
             for (int x = 0; x < REGIONRATIOWIDTH; x++)
            {
                for (int y = 0; y < REGIONRATIOHEIGHT; y++)
                {
                    for (int z = 0; z < REGIONRATIOLENGTH; z++)
                    {
                        builder.build(_regions[x, y, z]);
                        _regions[x, y, z].Build();
                    }
                }
            }
        }

        public void BuildRegions()
        {
            for (int x = 0; x < REGIONRATIOWIDTH; x++)
            {
                for (int y = 0; y < REGIONRATIOHEIGHT; y++)
                {
                    for (int z = 0; z < REGIONRATIOLENGTH; z++)
                    {
                        if (_regions[x, y, z].Dirty) _regions[x, y, z].Build();
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _lighting.Update();
        }

        float rippleTime = 0;
        public void Draw(GameTime gameTime, bool underWater)
        {
            BoundingFrustum viewFrustrum = new BoundingFrustum(Game.Camera.View * Game.Camera.Projection);

           // _game.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
           // _game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            
            /*_game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            */

            _game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise; 

            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            _game.GraphicsDevice.BlendState = BlendState.Opaque; 



            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(Game.Camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(Game.Camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(Game.Camera.Position);
            _solidBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["AmbientIntensity"].SetValue(0.8f);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(WorldSettings.FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(WorldSettings.FOGFAR);
            _solidBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);

            //_solidBlockEffect.Parameters["RippleTime"].SetValue(rippleTime*2);

            //if (underWater)
            //{
            //    _solidBlockEffect.Parameters["RippleAmount"].SetValue(0.2f);
            //}
            //else
            //{
            //    _solidBlockEffect.Parameters["RippleAmount"].SetValue(0f);
            //}

            _regionsDrawn = 0;
            //_solidBlockEffect.Begin();
            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                for (int x=0;x<REGIONRATIOWIDTH;x++) {
                    for (int y=0;y<REGIONRATIOHEIGHT;y++) {
                        for (int z=0;z<REGIONRATIOLENGTH;z++) {
                            Region region = _regions[x,y,z];
                            if (region.BoundingBox.Intersects(viewFrustrum)) {
                                if (region.Dirty) region.Build();
                                if (region.SolidVertexBuffer != null)
                                {
                                    _regionsDrawn++;
                                    
                                    //_game.GraphicsDevice.Vertices[0].SetSource(region.SolidVertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                                    _game.GraphicsDevice.SetVertexBuffer(region.SolidVertexBuffer); 
                                    //_game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (region.SolidVertexBuffer.SizeInBytes / VertexPositionTextureShade.SizeInBytes) / 3);
                                    _game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, region.SolidVertexBuffer.VertexCount  / 3);
                                
                                }
                            }
                        }
                    }
                }
                //pass.End();
            }
            //_solidBlockEffect.End();

            if (!underWater)
            {

                rippleTime += 0.1f;

               /* _game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                _game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                _game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                */
                _game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                _waterBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
                _waterBlockEffect.Parameters["View"].SetValue(Game.Camera.View);
                _waterBlockEffect.Parameters["Projection"].SetValue(Game.Camera.Projection);
                _waterBlockEffect.Parameters["CameraPosition"].SetValue(Game.Camera.Position);
                _waterBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
                _waterBlockEffect.Parameters["AmbientIntensity"].SetValue(0.8f);
                _waterBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
                _waterBlockEffect.Parameters["RippleTime"].SetValue(rippleTime);
                _waterBlockEffect.Parameters["FogNear"].SetValue(WorldSettings.FOGNEAR);
                _waterBlockEffect.Parameters["FogFar"].SetValue(WorldSettings.FOGFAR);
                _waterBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);

                //_waterBlockEffect.Begin();
                foreach (EffectPass pass in _waterBlockEffect.CurrentTechnique.Passes)
                {
                    //pass.Begin();
                    pass.Apply();
                    for (int x = 0; x < REGIONRATIOWIDTH; x++)
                    {
                        for (int y = 0; y < REGIONRATIOHEIGHT; y++)
                        {
                            for (int z = 0; z < REGIONRATIOLENGTH; z++)
                            {
                                Region region = _regions[x, y, z];
                                if (region.BoundingBox.Intersects(viewFrustrum))
                                {
                                    if (region.Dirty) region.Build();
                                }
                                if (region.WaterVertexBuffer != null)
                                {
                                    _regionsDrawn++;
                                   // _game.GraphicsDevice.Vertices[0].SetSource(region.WaterVertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                                    _game.GraphicsDevice.SetVertexBuffer(region.WaterVertexBuffer);
                                    //_game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (region.WaterVertexBuffer.SizeInBytes / VertexPositionTextureShade.SizeInBytes) / 3);
                                    _game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (region.WaterVertexBuffer.VertexCount) / 3);
                                
                                }
                            }
                        }
                    }
                   // pass.End();
                }
               // _waterBlockEffect.End();
            }

            //_game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            _game.GraphicsDevice.BlendState = BlendState.Opaque; 
        }

        public int RegionsDrawn
        {
            get { return _regionsDrawn; }
        }


        public TechCraftGame Game
        {
            get { return _game; }
        }

        public Lighting Lighting
        {
            get { return _lighting; }
        }

        public void RemoveBlock(int x, int y, int z) {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            BlockType blockType = region.RemoveBlock(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH);
            _waterQueue.Enqueue(new Flow(x, y, z,WorldSettings.WATERFLOWDISTANCE));
            //_lavaQueue.Enqueue(new Flow(x, y, z, WorldSettings.LAVAFLOWDISTANCE));
            _lighting.BlockRemoved(blockType, x, y, z);
        }

        private void DoFlow()
        {
            while (true)
            {
                int toProcessWater = _waterQueue.Count;
                for (int x = 0; x < toProcessWater; x++)
                {
                    Flow water = _waterQueue.Dequeue();
                    if (water.Count > 0)
                    {
                        DoWaterFlow(water);
                    }
                }
                int toProcessLava = _lavaQueue.Count;
                for (int y = 0; y < toProcessLava; y++)
                {
                    Flow lava = _lavaQueue.Dequeue();
                    if (lava.Count > 0)
                    {
                        DoLavaFlow(lava);
                    }
                }
                Thread.Sleep(100);//XXX  was 1000, fast water flow
            }
        }

        private void DoWaterFlow(Flow water)
        {
            BlockType xDecreasing = BlockAt(water.X - 1, water.Y, water.Z);
            BlockType xIncreasing = BlockAt(water.X + 1, water.Y, water.Z);
            BlockType yDecreasing = BlockAt(water.X, water.Y - 1, water.Z);
            BlockType yIncreasing = BlockAt(water.X, water.Y + 1, water.Z);
            BlockType zDecreasing = BlockAt(water.X, water.Y, water.Z - 1);
            BlockType zIncreasing = BlockAt(water.X, water.Y, water.Z + 1);

            // Don't flow water onto water
            // if (yDecreasing == BlockType.Water) return;

            bool flowed = false;

            if (yIncreasing == BlockType.Water) flowed = true;
            if (xIncreasing == BlockType.Water) flowed = true;
            if (xDecreasing == BlockType.Water) flowed = true;
            if (zIncreasing == BlockType.Water) flowed = true;
            if (zDecreasing == BlockType.Water) flowed = true;

            if (flowed)
            {
                AddBlock(water.X, water.Y, water.Z, BlockType.Water, false,false);

                if (yDecreasing == BlockType.None)
                {
                    // Don't flow out if we can flow down
                    _waterQueue.Enqueue(new Flow(water.X, water.Y-1, water.Z,water.Count));
                }
                else
                {
                    if (xDecreasing == BlockType.None) _waterQueue.Enqueue(new Flow(water.X - 1, water.Y, water.Z, water.Count - 1));
                    if (xIncreasing == BlockType.None) _waterQueue.Enqueue(new Flow(water.X + 1, water.Y, water.Z, water.Count - 1));
                    if (zDecreasing == BlockType.None) _waterQueue.Enqueue(new Flow(water.X, water.Y, water.Z - 1, water.Count - 1));
                    if (zIncreasing == BlockType.None) _waterQueue.Enqueue(new Flow(water.X, water.Y, water.Z + 1, water.Count - 1));
                }
            }
        }

        private void DoLavaFlow(Flow lava)
        {

            BlockType xDecreasing = BlockAt(lava.X - 1, lava.Y, lava.Z);
            BlockType xIncreasing = BlockAt(lava.X + 1, lava.Y, lava.Z);
            BlockType yDecreasing = BlockAt(lava.X, lava.Y - 1, lava.Z);
            BlockType yIncreasing = BlockAt(lava.X, lava.Y + 1, lava.Z);
            BlockType zDecreasing = BlockAt(lava.X, lava.Y, lava.Z - 1);
            BlockType zIncreasing = BlockAt(lava.X, lava.Y, lava.Z + 1);

            // Don't lava onto water or lava
            if (yDecreasing == BlockType.Water || yDecreasing == BlockType.Lava) return;

            bool flowed = false;

            if (yIncreasing == BlockType.Lava) flowed = true;
            if (xIncreasing == BlockType.Lava) flowed = true;
            if (xDecreasing == BlockType.Lava) flowed = true;
            if (zIncreasing == BlockType.Lava) flowed = true;
            if (zDecreasing == BlockType.Lava) flowed = true;

            if (flowed)
            {
                AddBlock(lava.X, lava.Y, lava.Z, BlockType.Lava, false,false);
                if (yDecreasing == BlockType.None)
                {
                    // Don't flow out if we can flow down
                    _lavaQueue.Enqueue(new Flow(lava.X, lava.Y - 1, lava.Z, lava.Count));
                }
                else
                {
                    if (xDecreasing == BlockType.None) _lavaQueue.Enqueue(new Flow(lava.X - 1, lava.Y, lava.Z, lava.Count - 1));
                    if (xIncreasing == BlockType.None) _lavaQueue.Enqueue(new Flow(lava.X + 1, lava.Y, lava.Z, lava.Count - 1));
                    if (zDecreasing == BlockType.None) _lavaQueue.Enqueue(new Flow(lava.X, lava.Y, lava.Z - 1, lava.Count - 1));
                    if (zIncreasing == BlockType.None) _lavaQueue.Enqueue(new Flow(lava.X, lava.Y, lava.Z + 1, lava.Count - 1));
                }
            }
        }


        public void AddBlock(int x, int y, int z, BlockType blockType)
        {
            AddBlock(x, y, z, blockType, true,false);
        }

        public void AddBlock(int x, int y, int z, BlockType blockType,bool calcShadows,bool flow)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.AddBlock(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, blockType);
            
            //XXX ADDED FLOW 
            if (flow && blockType== BlockType.Water ){
                if ( MapTools.WithinMapBounds(x, y, z))
                    _waterQueue.Enqueue(new Flow(x, y, z, WorldSettings.WATERFLOWDISTANCE));
            }
            
            if (calcShadows)
            {
                _lighting.BlockAdded(blockType, x, y, z);
            }
        }

        public BlockType BlockAtPoint(Vector3 position) {
            return BlockAt((int)position.X, (int)position.Y, (int)position.Z);
        }

        public BlockType BlockAt(int x, int y, int z)
        {
            if (InWorldBounds(x, y, z))
            {
                Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
                return region.BlockAt(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH);
            }
            else
            {
                return BlockType.None;
            }
        }

        public void AddWaterFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.AddWaterFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public void RemoveWaterFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.RemoveWaterFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public void AddSolidFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.AddSolidFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public void RemoveSolidFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.RemoveSolidFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public void AddModelFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.AddModelFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public void RemoveModelFace(int x, int y, int z, BlockFaceDirection face)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.RemoveModelFace(x % WorldSettings.REGIONWIDTH, y % WorldSettings.REGIONHEIGHT, z % WorldSettings.REGIONLENGTH, face);
        }

        public bool SolidAtPoint(Vector3 position)
        {
            return SolidAt((int)position.X, (int)position.Y, (int)position.Z);
        }

        public bool SolidAt(int x, int y, int z)
        {
            BlockType blockType = BlockAt(x, y, z);
            return BlockInformation.IsSolidBlock(blockType);
        }

        public void MakeDirty(int x, int y, int z)
        {
            Region region = _regions[x / WorldSettings.REGIONWIDTH, y / WorldSettings.REGIONHEIGHT, z / WorldSettings.REGIONLENGTH];
            region.Dirty = true;
        }

        public bool InWorldBounds(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= WorldSettings.MAPWIDTH || y >= WorldSettings.MAPHEIGHT || z >= WorldSettings.MAPLENGTH) return false;
            return true;
        }
    }
}
