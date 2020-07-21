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
using System.Collections.Concurrent;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
#endregion

namespace NewTake.view
{
    public abstract class WorldRenderer
    {
        #region Fields

        #region Atmospheric settings
        public const float FARPLANE = 220 * 4;
        public const int FOGNEAR = 200 * 4;
        public const int FOGFAR = 220 * 4;

        protected Vector3 SUNCOLOR = Color.White.ToVector3();

        protected Vector4 OVERHEADSUNCOLOR = Color.DarkBlue.ToVector4();
        protected Vector4 NIGHTCOLOR = Color.Black.ToVector4();

        protected Vector4 FOGCOLOR = Color.White.ToVector4();
        protected Vector4 HORIZONCOLOR = Color.White.ToVector4();

        protected Vector4 EVENINGTINT = Color.Red.ToVector4();
        protected Vector4 MORNINGTINT = Color.Gold.ToVector4();

        protected float CLOUDOVERCAST = 0.8f;

        protected const bool cloudsEnabled = true;

        #region SkyDome and Clouds
        // SkyDome
        protected Model skyDome;
        protected Matrix projectionMatrix;
        protected Texture2D cloudMap;
        protected float rotation;

        // GPU generated clouds
        protected Texture2D cloudStaticMap;
        protected RenderTarget2D cloudsRenderTarget;
        protected Effect _perlinNoiseEffect;
        protected VertexPositionTexture[] fullScreenVertices;

        // Day/Night
        public float tod = 12; // Midday
        public Vector3 SunPos = new Vector3(0, 1, 0); // Directly overhead
        public bool RealTime = false;
        #endregion
        #endregion

        protected World world;
        protected readonly GraphicsDevice GraphicsDevice;
        public readonly FirstPersonCamera camera;

        protected Effect _solidBlockEffect;
        protected BasicEffect _debugEffect;
        protected Texture2D _textureAtlas;
        protected Vector3i previousChunkIndex;

        protected readonly RasterizerState _wireframedRaster = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        protected readonly RasterizerState _normalRaster = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, FillMode = FillMode.Solid };

        protected bool _wireframed = false;
        public bool dayNightMode = true;
        public bool diagnosticsMode = false;
        public bool _running = true;
        #endregion

        public void ToggleRasterMode()
        {
            this._wireframed = !this._wireframed;
        }

        public WorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            this.world = world;
            this.GraphicsDevice = graphicsDevice;
            this.camera = camera;
        }

        public virtual void Initialize()
        {
            #region Generate the initial chunks
            // Generate the initial chunks
            var generatingWatch = new Stopwatch();
            generatingWatch.Start();
            Debug.Write("Generating initial chunks.. ");
            world.visitChunks(DoGenerate,World.VIEW_DISTANCE_FAR_X);
            generatingWatch.Stop();
            Debug.WriteLine(generatingWatch.Elapsed);
            #endregion

            #region Build the initial chunks
            // Build the initial chunks
            var buildWatch = new Stopwatch();
            buildWatch.Start();
            Debug.Write("Building initial chunks.. ");
            world.visitChunks(DoBuild, World.VIEW_DISTANCE_FAR_X);
            buildWatch.Stop();
            Debug.WriteLine(buildWatch.Elapsed);
            #endregion

            this.previousChunkIndex = new Vector3i(World.origin , 0, World.origin);
        }

        public virtual void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks)");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
            _debugEffect = new BasicEffect(GraphicsDevice);
        }

        #region DoGenerate
        public Chunk DoGenerate(int xIndex, int zIndex)
        {
            Vector3i temp = new Vector3i((uint)xIndex, 0, (uint)zIndex);
            return DoGenerate(temp);
        }

        public abstract Chunk DoGenerate(Vector3i vector);
        #endregion

        #region DoBuild
        public Chunk DoBuild(int xIndex, int zIndex)
        {
            Vector3i temp = new Vector3i((uint)xIndex, 0, (uint)zIndex);
            return DoBuild(temp);
        }

        public Chunk DoBuild(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            return DoBuild(chunk);
        }

        public abstract Chunk DoBuild(Chunk chunk);
        #endregion

        #region Generate Clouds
        public virtual Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(GraphicsDevice, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        public virtual VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        public virtual void GeneratePerlinNoise(float time)
        {
            GraphicsDevice.SetRenderTarget(cloudsRenderTarget);
            GraphicsDevice.Clear(Color.White);

            _perlinNoiseEffect.CurrentTechnique = _perlinNoiseEffect.Techniques["PerlinNoise"];
            _perlinNoiseEffect.Parameters["xTexture"].SetValue(cloudStaticMap);
            _perlinNoiseEffect.Parameters["xOvercast"].SetValue(CLOUDOVERCAST);
            _perlinNoiseEffect.Parameters["xTime"].SetValue(time / 1000.0f);

            foreach (EffectPass pass in _perlinNoiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }

            GraphicsDevice.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;
        }
        #endregion

        #region DrawSkyDome
        public virtual void DrawSkyDome(Matrix currentViewMatrix)
        {

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            //rotation += 0.0005f;
            rotation = 0;

            if (!dayNightMode) tod = 12;

            Matrix wMatrix = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0, -0.1f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);

                    currentEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);
                    currentEffect.Parameters["SunColor"].SetValue(OVERHEADSUNCOLOR);
                    currentEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);

                    currentEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
                    currentEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);
                    currentEffect.Parameters["timeOfDay"].SetValue(tod);
                }
                mesh.Draw();
            }
        }
        #endregion

        public virtual void DrawChunk(Chunk chunk)
        {
            //if (chunk.built)
            if (chunk.State == ChunkState.Ready)
            {
                GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                GraphicsDevice.Indices = chunk.IndexBuffer;
                //graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.VertexBuffer.VertexCount, 0, chunk.IndexBuffer.IndexCount / 3);
            }
        }

        #region DrawChunks
        public virtual void DrawChunks()
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["FogColor"].SetValue(FOGCOLOR);
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

            _solidBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);
            _solidBlockEffect.Parameters["timeOfDay"].SetValue(tod);

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum) && (chunk.State == ChunkState.Ready) && !chunk.dirty)
                    //if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.generated && !chunk.dirty)
                    {
                        //if (chunk.built)
                        if (chunk.State == ChunkState.Ready)
                        {
                            GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                            GraphicsDevice.Indices = chunk.IndexBuffer;
                            //graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.VertexBuffer.VertexCount, 0, chunk.IndexBuffer.IndexCount / 3);
                        }
                    }
                }
            }
        }
        #endregion

        #region DrawChunkDiagnostics
        public virtual void DrawChunkDiagnostics()
        {
            // Diagnostic rendering

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (Chunk chunk in world.viewableChunks.Values)
            {
                if (chunk.BoundingBox.Intersects(viewFrustum))
                {
                    if (chunk.broken)
                    {
                        Utility.DrawBoundingBox(chunk.BoundingBox, GraphicsDevice, _debugEffect, Matrix.Identity, camera.View, camera.Projection, Color.Red);
                    }
                    else
                    {
                        //if (!chunk.built)
                        if (chunk.State != ChunkState.Ready)
                        {
                            // Draw the bounding box for the chunk so we can see them
                            Utility.DrawBoundingBox(chunk.BoundingBox, GraphicsDevice, _debugEffect, Matrix.Identity, camera.View, camera.Projection, Color.Green);
                        }
                    }
                }
            }
        }
        #endregion

        #region UpdateTOD
        public virtual Vector3 UpdateTOD(GameTime gameTime)
        {
            long div = 10000;

            if (!RealTime)
                tod += ((float)gameTime.ElapsedGameTime.Milliseconds / div);
            else
                tod = ((float)DateTime.Now.Hour) + ((float)DateTime.Now.Minute) / 60 + (((float)DateTime.Now.Second) / 60) / 60;

            if (tod >= 24)
                tod = 0;

            // Calculate the position of the sun based on the time of day.
            float x = 0;
            float y = 0;
            float z = 0;

            if (tod <= 12)
            {
                y = tod / 12;
                x = 12 - tod;
            }
            else
            {
                y = (24 - tod) / 12;
                x = 12 - tod;
            }

            x /= 10;

            SunPos = new Vector3(-x, y, z);

            return SunPos;
        }
        #endregion

        public abstract void Update(GameTime gameTime);

        #region Draw
        public virtual void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            DrawSkyDome(camera.View);

            DrawChunks();

            if (diagnosticsMode)
            {
                DrawChunkDiagnostics();
            }
        }
        #endregion
    }
}
