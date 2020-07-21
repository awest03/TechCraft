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
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;
#endregion

namespace NewTake.view
{
    class MultiThreadLightingWorldRenderer : WorldRenderer
    {
        #region Fields

        private BasicEffect _debugEffect;

        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;
        private LightingChunkProcessor _lightingChunkProcessor;

        private readonly BlockingCollection<Vector3i> _generationQueue = new BlockingCollection<Vector3i>(); // uses concurrent queues by default.
        private readonly BlockingCollection<Vector3i> _buildingQueue = new BlockingCollection<Vector3i>();

        #endregion

        public MultiThreadLightingWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) :
            base(graphicsDevice, camera, world) { }

        #region Initialize
        public override void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(GraphicsDevice);
            _lightingChunkProcessor = new LightingChunkProcessor();

            base.Initialize();
        }
        #endregion

        public override void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\LightingAOBlockEffect");
            _debugEffect = new BasicEffect(GraphicsDevice);

            #region SkyDome and Clouds
            // SkyDome
            skyDome = content.Load<Model>("Models\\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = content.Load<Effect>("Effects\\SkyDome");
            cloudMap = content.Load<Texture2D>("Textures\\cloudMap");
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);

            // GPU Generated Clouds
            if (cloudsEnabled)
            {
                _perlinNoiseEffect = content.Load<Effect>("Effects\\PerlinNoise");
                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                //the mipmap does not work on some pc ( i5 laptops at least), with mipmap false it s fine 
                cloudsRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                cloudStaticMap = CreateStaticMap(32);
                fullScreenVertices = SetUpFullscreenVertices();
            }
            #endregion

            Task.Factory.StartNew(() => chunkReGenBuildTask(), TaskCreationOptions.LongRunning); // Starts the task that checks chunks for remove/generate/build
            Task.Factory.StartNew(() => WorkerTask(), TaskCreationOptions.LongRunning); // Starts the task that generates or builds chunks
        }

        #region WorkerTask
        private void WorkerTask()
        {
            while (_running)
            {
                Vector3i newIndex;
                BlockingCollection<Vector3i>.TakeFromAny(new[] { _generationQueue, _buildingQueue }, out newIndex);

                try
                {
                    //Debug.WriteLine("genQ = {0}", _generationQueue.Count);
                    //Debug.WriteLine("genQ = {0}, buildQ = {1}", _generationQueue.Count, _buildingQueue.Count);
                    if (world.viewableChunks[newIndex.X, newIndex.Z] == null)
                    {
                        //Debug.WriteLine("Worker Generate {0},{1}", newIndex.X, newIndex.Z);
                        //var DoGenerateWatch = new Stopwatch();
                        //DoGenerateWatch.Start();
                        DoGenerate(newIndex);
                        //DoGenerateWatch.Stop();
                        //Debug.WriteLine(DoGenerateWatch.Elapsed);
                    }
                    else
                    {
                        Chunk chunk = world.viewableChunks[newIndex.X, newIndex.Z];
                        //if ((!chunk.built) && (chunk.generated))//TODO why can it be null now
                        //Debug.WriteLine("buildQ = {0}", _buildingQueue.Count);
                        //Debug.WriteLine("Worker Build {0},{1} - State = {2}", newIndex.X, newIndex.Z, chunk.State);
                        switch (chunk.State)
                        {
                            case ChunkState.AwaitingBuild:
                                //var DoBuildWatch = new Stopwatch();
                                //DoBuildWatch.Start();
                                DoBuild(newIndex);
                                //DoBuildWatch.Stop();
                                //Debug.WriteLine(DoBuildWatch.Elapsed);
                                break;
                            case ChunkState.Generating:
                                Debug.WriteLine("Worker Attempted Unsuccessful Build {0},{1} - Chunk was found in State Generating", newIndex.X, newIndex.Z);
                                break;
                            case ChunkState.Building:
                                Debug.WriteLine("Worker Attempted Unsuccessful Build {0},{1} - Chunk was found in State Building", newIndex.X, newIndex.Z);
                                break;
                            case ChunkState.Lighting:
                                Debug.WriteLine("Worker Attempted Unsuccessful Build {0},{1} - Chunk was found in State Lighting", newIndex.X, newIndex.Z);
                                break;
                            case ChunkState.Ready:
                                //Debug.WriteLine("Worker Attempted Unsuccessful Build {0},{1} - Chunk was found in State Ready", newIndex.X, newIndex.Z);
                                break;
                            default:
                                Debug.WriteLine("Worker Attempted Unsuccessful Build {0},{1} - Chunk is now in State = {2}", newIndex.X, newIndex.Z, chunk.State);
                                break;
                        }
                    }
                }
                catch (AggregateException ae)
                {
                    Debug.WriteLine("Worker Exception {0}", ae);
                }
            }
        }
        #endregion

        #region Chunk Remove/Generate/Build Task
        private void chunkReGenBuildTask()
        {
            while (_running)
            {
                uint x = (uint)camera.Position.X;
                uint z = (uint)camera.Position.Z;

                uint cx = x / Chunk.SIZE.X;
                uint cz = z / Chunk.SIZE.Z;

                Vector3i currentChunkIndex = new Vector3i(cx, 0, cz);
               
                if (currentChunkIndex != previousChunkIndex) // Check so that we don't process if the player hasn't moved to a new chunk
                {
                    //Debug.WriteLine("previous = {0}, current = {1}:", previousChunkIndex, currentChunkIndex);
                    //if (previousChunkIndex.Z > currentChunkIndex.Z)
                    //{
                    //    Debug.WriteLine("Z has decreased");
                    //}
                    //if (previousChunkIndex.Z < currentChunkIndex.Z)
                    //{
                    //    Debug.WriteLine("Z has increased");
                    //}
                    //if (previousChunkIndex.X > currentChunkIndex.X)
                    //{
                    //    Debug.WriteLine("X has decreased");
                    //}
                    //if (previousChunkIndex.X < currentChunkIndex.X)
                    //{
                    //    Debug.WriteLine("X has increased");
                    //}

                    previousChunkIndex = currentChunkIndex; // Player has moved to a new chunk. Therefore update the previous index with the current one

                    // Loop through all possible chunks around the camera in both X and Z directions
                    for (uint j = cx - (World.VIEW_DISTANCE_FAR_X + 1); j < cx + (World.VIEW_DISTANCE_FAR_X + 1); j++)
                    {
                        for (uint l = cz - (World.VIEW_DISTANCE_FAR_Z + 1); l < cz + (World.VIEW_DISTANCE_FAR_Z + 1); l++)
                        {
                            int distancecx = (int)(j - cx);        // The distance from the camera to the chunk in the X direction
                            int distancecz = (int)(l - cz);        // The distance from the camera to the chunk in the Z direction

                            if (distancecx < 0) distancecx = 0 - distancecx; // If the distance is negative make it positive
                            if (distancecz < 0) distancecz = 0 - distancecz; // If the distance is negative make it positive

                            #region Remove Chunks
                            // Remove Chunks
                            if ((distancecx > World.VIEW_DISTANCE_NEAR_X) || (distancecz > World.VIEW_DISTANCE_NEAR_Z))
                            {
                                if ((world.viewableChunks[j, l] != null)) // Chunk is created, therefore remove
                                {
                                    //Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz)); // This is the chunk in the loop, offset from the camera
                                    Chunk chunk = world.viewableChunks[j, l];
                                    try
                                    {
                                        //chunk.visible = false;
                                        world.viewableChunks.Remove(j, l);
                                    }
                                    catch (AggregateException ae)
                                    {
                                        Debug.WriteLine("RemoveChunk Exception {0}", ae);
                                    }
                                }
                                //else
                                //{
                                //    //Debug.WriteLine("[Remove] chunk not found at at {0},0,{1}", j, l);
                                //}
                            }
                            #endregion
                            #region Generate Chunks
                            // Generate Chunks
                            else if ((distancecx > World.VIEW_CHUNKS_X) || (distancecz > World.VIEW_CHUNKS_Z))
                            {
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                try
                                {
                                    // TODO: Add code to only add chunks to queue based on current movement direction

                                    //Chunk chunk = world.viewableChunks[newIndex.X, newIndex.Z];
                                    //if (chunk != null) Debug.WriteLine("State = {0}", chunk.State);
                                    // A new chunk is coming into view - we need to generate or load it
                                    //if(world.viewableChunks[j, l].BoundingBox.Intersects(viewFrustum))
                                    //{
                                    //    Debug.WriteLine("New chunk found in view {0}", world.viewableChunks[j, l]);
                                    //}

                                    if (world.viewableChunks[j, l] == null && !_generationQueue.Contains(newIndex)) // Chunk is not created or loaded, therefore create - 
                                    {
                                        //Debug.WriteLine("Adding new chunk to queue, chunk = {0},{1}", newIndex.X, newIndex.Z);
                                        this._generationQueue.Add(newIndex); // adds the chunk index to the generation queue 
                                    }   
                                    else if (_generationQueue.Contains(newIndex))
                                    {
                                        //Debug.WriteLine("Chunk found on genQ, chunk = {0},{1} genQ Count = {2}", newIndex.X, newIndex.Z, _generationQueue.Count);
                                    }
                                }
                                catch (AggregateException ae)
                                {
                                    Debug.WriteLine("GenerateChunk Exception {0}", ae);
                                }
                            }
                            #endregion
                            #region Build Chunks
                            //Build Chunks
                            else
                            {
                                Chunk chunk = world.viewableChunks[j, l];
                                //if (!chunk.built && chunk.generated && (chunk.State == ChunkState.AwaitingBuild))//TODO why can it be null now 
                                if (chunk.State == ChunkState.AwaitingBuild)//TODO why can it be null now 
                                {
                                    try
                                    {
                                        // We have a chunk in view - it has been generated but we haven't built a vertex buffer for it
                                        Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz)); // This is the chunk in the loop, offset from the camera

                                        // TODO: Add code to only add chunks to queue based on current movement direction

                                        this._buildingQueue.Add(newIndex); // adds the chunk index to the build queue
                                        //this._buildingQueue.Add(chunk.Index); // adds the chunk index to the build queue
                                        //DoBuild(newIndex);
                                        //Debug.WriteLine("Vertices built at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                                        //Debug.WriteLine("Queue build {0},{1},{2} : {3},{4}", chunk.Index.X, chunk.Index.Y, chunk.Index.Z, distancecx, distancecz);
                                    }
                                    catch (AggregateException ae)
                                    {
                                        Debug.WriteLine("BuildChunk Exception {0}", ae);
                                    }
                                }
                            }
                            #endregion

                        } // end for loop cz
                    } // end for loop cx
                } // end if
            }
        }
        #endregion

        #region DoGenerate
        public override Chunk DoGenerate(Vector3i index)
        {
            Chunk chunk = world.viewableChunks.load(index);
            try
            {
                if (chunk == null)
                {
                    // Create a new chunk
                    chunk = new Chunk(world, index);

                    if (chunk.State == ChunkState.AwaitingGenerate)
                    {
                        // Generate the chunk with the current generator
                        chunk.State = ChunkState.Generating;
                        world.Generator.Generate(chunk);
                        _lightingChunkProcessor.ProcessChunk(chunk);
                        chunk.State = ChunkState.AwaitingLighting;
                    }
                }
                // Assign a renderer
                // ChunkRenderer cRenderer = new MultiThreadLightingChunkRenderer(GraphicsDevice, world, chunk);
                // this.ChunkRenderers.TryAdd(chunk.Index,cRenderer);           
            }
            catch (Exception e)
            {
                //Debug.WriteLine("DoGenerate Exception {0}", e);
                Debug.WriteLine("DoGenerate Exception Chunk = {0},{1} - State = {2}", chunk.Position.X, chunk.Position.Z, chunk.State);
                throw e;
            }
            return chunk;
        }
        #endregion

        #region DoBuild
        public override Chunk DoBuild(Chunk chunk)
        {            
            try
            {
                if ((chunk != null) && (chunk.State == ChunkState.AwaitingBuild))
                {
                    // Propogate the chunk lighting
                    chunk.State = ChunkState.Lighting;
                    _lightingChunkProcessor.ProcessChunk(chunk);

                    chunk.State = ChunkState.Building;
                    _vertexBuildChunkProcessor.ProcessChunk(chunk);

                    chunk.State = ChunkState.Ready;
                    //chunk.built = true;
                   
                }
                return chunk;
            }
            catch (Exception e)
            {
                //Debug.WriteLine("DoBuild Exception {0}", e);
                Debug.WriteLine("DoBuild Exception Chunk = {0},{1} - State = {2}", chunk.Position.X, chunk.Position.Z, chunk.State);
                throw e;
            }
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);
            if (cloudsEnabled)
            {
                // Generate the clouds
                float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                base.GeneratePerlinNoise(time);
            }

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            // Draw the skyDome
            base.DrawSkyDome(camera.View);

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

                foreach(Chunk chunk in world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum) && (chunk.State==ChunkState.Ready) && !chunk.dirty)
                    //if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.generated && !chunk.dirty)
                    {
                        base.DrawChunk(chunk);
                    }
                }
            }

            // Diagnostic rendering
            if (diagnosticsMode)
            {
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
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime) {
            //update of chunks is handled in chunkReGenBuildTask for this class
            base.UpdateTOD(gameTime);
        }
        #endregion

    }
}
