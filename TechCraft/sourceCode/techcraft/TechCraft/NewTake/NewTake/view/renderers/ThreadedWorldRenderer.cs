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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake;
using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;
#endregion

namespace NewTake.view.renderers
{
    public class ThreadedWorldRenderer : IRenderer
    {

        #region Fields
        protected Effect _solidBlockEffect;
        protected Effect _waterBlockEffect;

        protected Texture2D _textureAtlas;
        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;
        private LightingChunkProcessor _lightingChunkProcessor;

        #region Thread fields
        private Queue<Vector3i> _generateQueue = new Queue<Vector3i>();
        private Queue<Vector3i> _buildQueue = new Queue<Vector3i>();
        private Queue<Vector3i> _lightingQueue = new Queue<Vector3i>();
        
        private Thread _workerQueueThread;
        private Thread _workerCheckThread;

        private Thread _workerRemoveThread;

        //private Thread _workerGenerateQueueThread;
        //private Thread _workerLightingQueueThread;
        //private Thread _workerBuildQueueThread;

        private bool _running = true;
        #endregion

        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        private Vector3i _previousChunkIndex;

        #region Atmospheric settings
        protected Vector4 NIGHTCOLOR = Color.Black.ToVector4();
        public Vector4 SUNCOLOR = Color.White.ToVector4();
        protected Vector4 HORIZONCOLOR = Color.White.ToVector4();

        protected Vector4 EVENINGTINT = Color.Red.ToVector4();
        protected Vector4 MORNINGTINT = Color.Gold.ToVector4();

        private float _tod;
        public bool dayMode = false;
        public bool nightMode = false;
        public const int FOGNEAR = 14 * 16;//(BUILD_RANGE - 1) * 16;
        public const float FOGFAR = 16 * 16;//(BUILD_RANGE + 1) * 16;
        #endregion

        #region Range fields
        private const byte BUILD_RANGE = 10;
        private const byte LIGHT_RANGE = BUILD_RANGE + 1;
        private const byte GENERATE_RANGE = LIGHT_RANGE + 1;
        private const byte REMOVE_RANGE = GENERATE_RANGE + 1;
        #endregion

        #region debugFont
        SpriteBatch debugSpriteBatch;
        SpriteFont debugFont;
        Texture2D debugRectTexture;
        bool debugRectangle = true;
        Vector2 genQVector2;
        Vector2 lightQVector2;
        Vector2 buildQVector2;
        Vector2 memVector2;
        Rectangle backgroundRectangle;
        #endregion

        #endregion

        public ThreadedWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        public void Stop()
        {
            _running = false;
        }

        #region Initialize
        public void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(_graphicsDevice);
            _lightingChunkProcessor = new LightingChunkProcessor();

            Debug.WriteLine("Generate initial chunks");
            _world.visitChunks(DoInitialGenerate, GENERATE_RANGE);
            Debug.WriteLine("Light initial chunks");
            _world.visitChunks(DoLighting, LIGHT_RANGE);
            Debug.WriteLine("Build initial chunks");
            _world.visitChunks(DoBuild, BUILD_RANGE);

            #region debugFont Rectangle
            debugRectTexture = new Texture2D(_graphicsDevice, 1, 1);
            Color[] texcol = new Color[1];
            debugRectTexture.GetData(texcol);
            texcol[0] = Color.Black;
            debugRectTexture.SetData(texcol);

            genQVector2 = new Vector2(580, 0);
            lightQVector2 = new Vector2(580, 16);
            buildQVector2 = new Vector2(580, 32);
            memVector2 = new Vector2(580, 48);
            backgroundRectangle = new Rectangle(580, 0, 100, 144);
            #endregion

            #region Thread creation
            _workerQueueThread = new Thread(new ThreadStart(WorkerThread));
            _workerQueueThread.Priority = ThreadPriority.Highest;
            _workerQueueThread.IsBackground = true;
            _workerQueueThread.Start();

            _workerRemoveThread = new Thread(new ThreadStart(WorkerRemoveThread));
            _workerRemoveThread.Priority = ThreadPriority.Highest;
            _workerRemoveThread.IsBackground = true;
            _workerRemoveThread.Start();

            //_workerGenerateQueueThread = new Thread(new ThreadStart(WorkerGenerateQueueThread));
            //_workerGenerateQueueThread.Priority = ThreadPriority.AboveNormal;
            //_workerGenerateQueueThread.IsBackground = true;
            //_workerGenerateQueueThread.Name = "WorkerGenerateQueueThread";
            //_workerGenerateQueueThread.Start();

            //_workerLightingQueueThread = new Thread(new ThreadStart(WorkerLightingQueueThread));
            //_workerLightingQueueThread.Priority = ThreadPriority.AboveNormal;
            //_workerLightingQueueThread.IsBackground = true;
            //_workerLightingQueueThread.Name = "WorkerLightingQueueThread";
            //_workerLightingQueueThread.Start();

            //_workerBuildQueueThread = new Thread(new ThreadStart(WorkerBuildQueueThread));
            //_workerBuildQueueThread.Priority = ThreadPriority.AboveNormal;
            //_workerBuildQueueThread.IsBackground = true;
            //_workerBuildQueueThread.Name = "WorkerBuildQueueThread";
            //_workerBuildQueueThread.Start();

            _workerCheckThread = new Thread(new ThreadStart(WorkerCheckThread));
            _workerCheckThread.Priority = ThreadPriority.Highest;
            _workerCheckThread.IsBackground = true;
            _workerCheckThread.Name = "WorkerCheckThread";
            _workerCheckThread.Start();
            #endregion
        }
        #endregion

        public void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks_APR28_3");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
            _waterBlockEffect = content.Load<Effect>("Effects\\WaterBlockEffect");

            debugSpriteBatch = new SpriteBatch(_graphicsDevice);
            debugFont = content.Load<SpriteFont>("Fonts\\OSDdisplay");
        }

        #region DoInitialGenerate
        private Chunk DoInitialGenerate(Vector3i chunkIndex)
        {
            //Debug.WriteLine("DoGenerate " + chunkIndex);
            Chunk chunk = new Chunk(_world, chunkIndex);
            _world.Chunks[chunkIndex.X, chunkIndex.Z] = chunk;
            if (chunk.State == ChunkState.AwaitingGenerate)
            {
                chunk.State = ChunkState.Generating;
                _world.Generator.Generate(chunk);
                chunk.State = ChunkState.AwaitingLighting;
            }
            return chunk;
        }
        #endregion

        public void QueueGenerate(Vector3i chunkIndex)
        {
            lock (_generateQueue)
            {
                _generateQueue.Enqueue(chunkIndex);
            }
        }
        #region DoGenerate
        
        //TODO try to avoid using this method in favor of the method taking a chunk in param
        private Chunk DoGenerate(Vector3i target) { 
           return DoGenerate(_world.Chunks.get(target)); 
        }
        
        private Chunk DoGenerate(Chunk chunk)
        {
            lock (this)
            {
                //Debug.WriteLine("DoGenerate " + chunk);
                
                if (chunk == null)
                {
                    // Thread sync issue - requeue
                    //QueueGenerate(chunkIndex);
                    return null;
                }
                if (chunk.State == ChunkState.AwaitingGenerate)
                {
                    chunk.State = ChunkState.Generating;
                    _world.Generator.Generate(chunk);
                    chunk.State = ChunkState.AwaitingLighting;
                }
                return chunk;
            }
        }
        #endregion

        public void QueueLighting(Vector3i chunkIndex)
        {
            if (_world.Chunks[chunkIndex.X, chunkIndex.Z] == null)
            {
                throw new ArgumentNullException("queuing lighting for a null chunk");
            }
            lock (_lightingQueue)
            {
                _lightingQueue.Enqueue(chunkIndex);
            }
        }
        #region DoLighting

        //TODO try to avoid using this method in favor of the method taking a chunk in param
        private Chunk DoLighting(Vector3i target)
        {
            return DoLighting(_world.Chunks.get(target));
        }

        private Chunk DoLighting(Chunk chunk)
        {
            lock (this)
            {
                //Debug.WriteLine("DoLighting " + chunk);
                
                //TODO chunk happens to be null here sometime : it was not null when enqueued , it became null after
                // => cancel this lighting
                if (chunk == null) return null;

                if (chunk.State == ChunkState.AwaitingLighting)
                {
                    chunk.State = ChunkState.Lighting;
                    _lightingChunkProcessor.ProcessChunk(chunk);
                    chunk.State = ChunkState.AwaitingBuild;
                }
                else if (chunk.State == ChunkState.AwaitingRelighting)
                {
                    chunk.State = ChunkState.Lighting;
                    _lightingChunkProcessor.ProcessChunk(chunk);
                    chunk.State = ChunkState.AwaitingBuild;
                    QueueBuild(chunk.Index);
                }
                return chunk;
            }
        }
        #endregion

        public void QueueBuild(Vector3i chunkIndex)
        {
            lock (_buildQueue)
            {
                _buildQueue.Enqueue(chunkIndex);
            }
        }
        #region DoBuild
        //TODO try to avoid using this method in favor of the method taking a chunk in param
        private Chunk DoBuild(Vector3i target)
        {
            return DoBuild(_world.Chunks.get(target));
        }
        private Chunk DoBuild(Chunk chunk)
        {
            lock (this)
            {
                //Debug.WriteLine("DoBuild " + chunk);              
                if (chunk == null) return null;
                if (chunk.State == ChunkState.AwaitingBuild || chunk.State == ChunkState.AwaitingRebuild)
                {
                    chunk.State = ChunkState.Building;
                    _vertexBuildChunkProcessor.ProcessChunk(chunk);
                    chunk.State = ChunkState.Ready;
                }
                return chunk;
            }
        }
        #endregion

        #region WorkerCheckThread
        public void WorkerCheckThread()
        {
            while (_running)
            {
                uint cameraX = (uint)(_camera.Position.X / Chunk.SIZE.X);
                uint cameraZ = (uint)(_camera.Position.Z / Chunk.SIZE.Z);

                //Vector3i currentChunkIndex = new Vector3i(cameraX, 0, cameraZ); // GC.GetGeneration(0)

                //if (_previousChunkIndex != currentChunkIndex)
                //{
                //    _previousChunkIndex = currentChunkIndex;

                    for (uint ix = cameraX - REMOVE_RANGE; ix < cameraX + REMOVE_RANGE; ix++)
                    {
                        for (uint iz = cameraZ - REMOVE_RANGE; iz < cameraZ + REMOVE_RANGE; iz++)
                        {
                            int distX = (int)(ix - cameraX);
                            int distZ = (int)(iz - cameraZ);
                            int xdir = 1, zdir = 1;
                            if (distX < 0)
                            {
                                distX = 0 - distX;
                                xdir = -1;
                            }
                            if (distZ < 0)
                            {
                                distZ = 0 - distZ;
                                zdir = -1;
                            }

                            Vector3i chunkIndex = new Vector3i(ix, 0, iz); // GC.GetGeneration(0)

                            //Debug.WriteLine("currentChunkIndex = {0}, chunkIndex = {1}, distX = {2}, distZ = {3}", currentChunkIndex, chunkIndex, distX, distZ);

                            #region Remove
                            if (distX > GENERATE_RANGE || distZ > GENERATE_RANGE)
                            {
                                if (_world.Chunks[ix, iz] != null)
                                {
                                    //Debug.WriteLine("Remove({0},{1}) ChunkCount = {2}", ix, iz, _world.viewableChunks.Count);
                                    _world.Chunks.Remove(ix, iz);
                                }
                                continue;
                            }
                            #endregion
                            #region Generate
                            if ((distX > LIGHT_RANGE || distZ > LIGHT_RANGE) && (distX < REMOVE_RANGE || distZ < REMOVE_RANGE)) 
                            {
                                if (_world.Chunks[ix, iz] == null)
                                {
                                    uint removeX = ix, removeZ = iz;

                                    if (distX > LIGHT_RANGE) removeX = (uint)(ix - distX * xdir * 2);
                                    if (distZ > LIGHT_RANGE) removeZ = (uint)(iz - distZ * zdir * 2);

                                    Chunk toReAssign = _world.Chunks[removeX, removeZ];
                                    if (toReAssign != null)
                                    {
                                        switch (toReAssign.State)
                                        {
                                            case ChunkState.Ready:
                                                lock (this)
                                                {
                                                    Chunk chunkGenerate = new Chunk(_world, chunkIndex);
                                                    chunkGenerate.State = ChunkState.AwaitingGenerate;
                                                    _world.Chunks[ix, iz] = chunkGenerate;
                                                    _world.Chunks.Remove(removeX, removeZ);                                                   
                                                    //reassign is not ready, make the rest work first
                                                    //toReAssign.Assign(chunkIndex);
                                                    //toReAssign.State = ChunkState.AwaitingGenerate;
                                                    
                                                    QueueGenerate(chunkIndex);
                                                }
                                                break;
                                            case ChunkState.AwaitingGenerate:
                                                lock (this)
                                                {
                                                    QueueGenerate(chunkIndex);
                                                }
                                                break;
                                            case ChunkState.AwaitingLighting:
                                                break;
                                            case ChunkState.AwaitingBuild:
                                                lock (this)
                                                {
                                                    DoBuild(toReAssign);
                                                }
                                                break;
                                            case ChunkState.AwaitingRebuild:
                                                lock (this)
                                                {
                                                    DoBuild(toReAssign);
                                                }
                                                break;
                                            default:
                                                //Debug.WriteLine("Generate: State = {0}", toReAssign.State);
                                                break;
                                        }
                                    }
                                    //else
                                    //{
                                    //    // for some reason we have identified a null chunk, therefore create one temporarily
                                    //    lock (this)
                                    //    {
                                    //        Chunk chunkGenerate = new Chunk(_world, chunkIndex);
                                    //        chunkGenerate.State = ChunkState.AwaitingGenerate;
                                    //        _world.Chunks[ix, iz] = chunkGenerate;
                                    //        QueueGenerate(chunkIndex);
                                    //    }
                                    //}
                                }
                                continue;
                            }
                            #endregion
                            #region Light
                            if (distX >= LIGHT_RANGE || distZ >= LIGHT_RANGE)
                            {
                                Chunk chunk = _world.Chunks[ix, iz];
                                if (chunk != null && chunk.State == ChunkState.AwaitingLighting)
                                {
                                    QueueLighting(chunkIndex);
                                }
                                continue;
                            }
                            #endregion
                            #region Rebuild
                            Chunk rebuildChunk = _world.Chunks[ix, iz];
                            if (rebuildChunk != null)
                            {
                                if (rebuildChunk.State == ChunkState.AwaitingRelighting)
                                {
                                    QueueLighting(chunkIndex);
                                }
                                if (rebuildChunk.State == ChunkState.AwaitingRebuild || rebuildChunk.State == ChunkState.AwaitingBuild)
                                {
                                    QueueBuild(chunkIndex);
                                }
                            }
                            #endregion
                        }
                    }

                //}
                Thread.Sleep(1);
            }
        }
        #endregion
        #region WorkerThread
        private void WorkerThread()
        {

            bool foundGenerate, foundLighting, foundBuild;
            Vector3i target = new Vector3i(0, 0, 0);
            while (_running)
            {
                foundGenerate = false;
                foundLighting = false;
                foundBuild = false;

                #region Generate
                // LOOK FOR CHUNKS REQUIRING GENERATION
                lock (_generateQueue)
                {
                    if (_generateQueue.Count > 0)
                    {
                        target = _generateQueue.Dequeue();
                        foundGenerate = true;
                    }
                }
                if (foundGenerate)
                {
                    try
                    {
                        Chunk chunkGenerate = _world.Chunks[target.X, target.Z];
                        if (chunkGenerate != null && chunkGenerate.State == ChunkState.AwaitingGenerate)
                        {
                            //Debug.WriteLine("DoGenerate target = {0}, state = {1}", target, chunkGenerate.State);
                            DoGenerate(chunkGenerate);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("NullReferenceException DoGenerate target = {0}", target);
                        if (Game1.throwExceptions) throw e; 
                        DoGenerate(target);
                    }
                    continue;
                }
                #endregion
                #region Light
                // LOOK FOR CHUNKS REQUIRING LIGHTING
                lock (_lightingQueue)
                {
                    if (_lightingQueue.Count > 0)
                    {
                        target = _lightingQueue.Dequeue();
                        foundLighting = true;
                    }
                }
                if (foundLighting)
                {
                    try
                    {
                        Chunk chunkLighting = _world.Chunks[target.X, target.Z];
                        if (chunkLighting.State == ChunkState.AwaitingLighting || chunkLighting.State == ChunkState.AwaitingRelighting)
                        {
                            //Debug.WriteLine("DoLighting target = {0}, state = {1}", target, chunkLighting.State);
                            DoLighting(chunkLighting);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("NullReferenceException DoLighting target = {0}", target);
                        if (Game1.throwExceptions) throw e;

                        DoGenerate(target);
                    }
                    continue;
                }
                #endregion
                #region Build
                // LOOK FOR CHUNKS REQUIRING BUILD
                lock (_buildQueue)
                {
                    if (_buildQueue.Count > 0)
                    {
                        target = _buildQueue.Dequeue();
                        foundBuild = true;
                    }
                }
                if (foundBuild)
                {
                    try
                    {
                        Chunk chunkBuild = _world.Chunks[target.X, target.Z];
                        if (chunkBuild.State == ChunkState.AwaitingBuild || chunkBuild.State == ChunkState.AwaitingRebuild)
                        {
                            //Debug.WriteLine("DoBuild target = {0}, state = {1}", target, chunkBuild.State);
                            DoBuild(chunkBuild);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("NullReferenceException DoBuild target = {0}", target);
                        if (Game1.throwExceptions) throw e; 
                        DoGenerate(target);
                    }
                    continue;
                }
                #endregion

                Thread.Sleep(10);
            }
        }
        #endregion
        #region WorkerGenerateQueueThread
        private void WorkerGenerateQueueThread()
        {
            Vector3i target = new Vector3i(0, 0, 0);
            bool foundGenerate;

            while (_running)
            {
                foundGenerate = false;

                //if (_generateQueue.Count != 0 || _lightingQueue.Count != 0 || _buildQueue.Count != 0)
                //    Debug.WriteLine("_gQ = {0}, _lQ = {1}, _bQ = {2}", _generateQueue.Count, _lightingQueue.Count, _buildQueue.Count);

                // LOOK FOR CHUNKS REQUIRING GENERATION
                lock (_generateQueue)
                {
                    if (_generateQueue.Count > 0)
                    {
                        target = _generateQueue.Dequeue();
                        foundGenerate = true;
                    }
                }
                if (foundGenerate)
                {
                    try
                    {
                        Chunk chunkGenerate = _world.Chunks[target.X, target.Z];
                        if (chunkGenerate != null && chunkGenerate.State == ChunkState.AwaitingGenerate)
                        {
                            //Debug.WriteLine("DoGenerate target = {0}, state = {1}", target, chunkGenerate.State);
                            DoGenerate(target);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("NullReferenceException DoGenerate target = {0}", target);
                        if (Game1.throwExceptions) throw e; 
                        DoGenerate(target);
                        
                    }
                    continue;
                }
                Thread.Sleep(1);
            }
        }
        #endregion
        #region WorkerLightingQueueThread
        private void WorkerLightingQueueThread()
        {
            Vector3i target = new Vector3i(0, 0, 0);
            bool foundLighting;

            while (_running)
            {
                foundLighting = false;

                //if (_generateQueue.Count != 0 || _lightingQueue.Count != 0 || _buildQueue.Count != 0)
                //    Debug.WriteLine("_gQ = {0}, _lQ = {1}, _bQ = {2}", _generateQueue.Count, _lightingQueue.Count, _buildQueue.Count);

                // LOOK FOR CHUNKS REQUIRING LIGHTING
                lock (_lightingQueue)
                {
                    if (_lightingQueue.Count > 0)
                    {
                        target = _lightingQueue.Dequeue();
                        foundLighting = true;
                    }
                }
                if (foundLighting)
                {
                    try
                    {
                        Chunk chunkLighting = _world.Chunks[target.X, target.Z];
                        if (chunkLighting.State == ChunkState.AwaitingLighting || chunkLighting.State == ChunkState.AwaitingRelighting)
                        {
                            //Debug.WriteLine("DoLighting target = {0}, state = {1}", target, chunkLighting.State);
                            DoLighting(target);
                        }
                    }
                    catch (NullReferenceException e)
                    {                        
                        Debug.WriteLine("NullReferenceException DoLighting target = {0}", target);
                        if (Game1.throwExceptions) throw e; 
                        DoLighting(target);

                    }
                    continue;
                }
                Thread.Sleep(1);
            }
        }
        #endregion
        #region WorkerBuildQueueThread
        private void WorkerBuildQueueThread()
        {
            Vector3i target = new Vector3i(0, 0, 0);
            bool foundBuild;

            while (_running)
            {
                foundBuild = false;

                //if (_generateQueue.Count != 0 || _lightingQueue.Count != 0 || _buildQueue.Count != 0)
                //    Debug.WriteLine("_gQ = {0}, _lQ = {1}, _bQ = {2}", _generateQueue.Count, _lightingQueue.Count, _buildQueue.Count);

                // LOOK FOR CHUNKS REQUIRING BUILD
                lock (_buildQueue)
                {
                    if (_buildQueue.Count > 0)
                    {
                        target = _buildQueue.Dequeue();
                        foundBuild = true;
                    }
                }

                if (foundBuild)
                {
                    try
                    {
                        Chunk chunkBuild = _world.Chunks[target.X, target.Z];
                        if (chunkBuild.State == ChunkState.AwaitingBuild || chunkBuild.State == ChunkState.AwaitingRebuild)
                        {
                            //Debug.WriteLine("DoBuild target = {0}, state = {1}", target, chunkBuild.State);
                            DoBuild(target);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("NullReferenceException DoBuild target = {0}", target);
                        if (Game1.throwExceptions) throw e; 
                        DoBuild(target);
                    }
                    continue;
                }
                Thread.Sleep(1);
            }
        }
        #endregion

        #region WorkerRemoveThread
        private void WorkerRemoveThread()
        {
            while (_running)
            {
                uint cameraX = (uint)(_camera.Position.X / Chunk.SIZE.X);
                uint cameraZ = (uint)(_camera.Position.Z / Chunk.SIZE.Z);

                for (uint ix = cameraX - REMOVE_RANGE*4; ix < cameraX + REMOVE_RANGE*4; ix++)
                {
                    for (uint iz = cameraZ - REMOVE_RANGE*4; iz < cameraZ + REMOVE_RANGE*4; iz++)
                    {
                        int distX = (int)(ix - cameraX);
                        int distZ = (int)(iz - cameraZ);

                        if (distX < 0)
                        {
                            distX = 0 - distX;
                        }
                        if (distZ < 0)
                        {
                            distZ = 0 - distZ;
                        }

                        Vector3i chunkIndex = new Vector3i(ix, 0, iz); // GC.GetGeneration(0)
                        #region Remove
                        if (distX > GENERATE_RANGE || distZ > GENERATE_RANGE)
                        {
                            if (_world.Chunks[ix, iz] != null)
                            {
                                //Debug.WriteLine("Remove({0},{1}) ChunkCount = {2}", ix, iz, _world.Chunks.Count);
                                _world.Chunks.Remove(ix, iz);
                            }
                            continue;
                        }
                        #endregion
                    }
                }
                GC.Collect(9);
                Thread.Sleep(20);
            }
        }
        #endregion


        #region DrawSolid
        private void DrawSolid(GameTime gameTime)
        {

            _tod = _world.tod;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(_camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

            _solidBlockEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);
            _solidBlockEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);

            _solidBlockEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
            _solidBlockEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);

            _solidBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);
            _solidBlockEffect.Parameters["timeOfDay"].SetValue(_tod);

            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.Chunks.Values)
                {
                    if (chunk == null) continue;

                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.IndexBuffer != null)
                    {
                        lock (chunk)
                        {
                            if (chunk.IndexBuffer.IndexCount > 0)
                            {
                                _graphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                                _graphicsDevice.Indices = chunk.IndexBuffer;
                                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.VertexBuffer.VertexCount, 0, chunk.IndexBuffer.IndexCount / 3);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region DrawWater
        float rippleTime = 0;
        private void DrawWater(GameTime gameTime)
        {
            rippleTime += 0.1f;

            _tod = _world.tod;

            _waterBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _waterBlockEffect.Parameters["View"].SetValue(_camera.View);
            _waterBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _waterBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _waterBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _waterBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _waterBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);
            _waterBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);

            _waterBlockEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);
            _waterBlockEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);

            _waterBlockEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
            _waterBlockEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);

            _waterBlockEffect.Parameters["timeOfDay"].SetValue(_tod);
            _waterBlockEffect.Parameters["RippleTime"].SetValue(rippleTime);

            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in _waterBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.Chunks.Values)
                {
                    if (chunk == null) continue;

                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.waterVertexBuffer != null)
                    {
                        lock (chunk)
                        {
                            if (chunk.waterIndexBuffer.IndexCount > 0)
                            {
                                _graphicsDevice.SetVertexBuffer(chunk.waterVertexBuffer);
                                _graphicsDevice.Indices = chunk.waterIndexBuffer;
                                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.waterVertexBuffer.VertexCount, 0, chunk.waterIndexBuffer.IndexCount / 3);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            ////Debug.WriteLine("M:" + GC.GetTotalMemory(false));

            //uint cameraX = (uint)(_camera.Position.X / Chunk.SIZE.X);
            //uint cameraZ = (uint)(_camera.Position.Z / Chunk.SIZE.Z);

            //Vector3i currentChunkIndex = new Vector3i(cameraX, 0, cameraZ);

            ////if (_previousChunkIndex != currentChunkIndex)
            ////{
            ////_previousChunkIndex = currentChunkIndex;

            //for (uint ix = cameraX - REMOVE_RANGE; ix < cameraX + REMOVE_RANGE; ix++)
            //{
            //    for (uint iz = cameraZ - REMOVE_RANGE; iz < cameraZ + REMOVE_RANGE; iz++)
            //    {
            //        int distX = (int)(ix - cameraX);
            //        int distZ = (int)(iz - cameraZ);
            //        int xdir = 1, zdir = 1;
            //        if (distX < 0)
            //        {
            //            distX = 0 - distX;
            //            xdir = -1;
            //        }
            //        if (distZ < 0)
            //        {
            //            distZ = 0 - distZ;
            //            zdir = -1;
            //        }
            //        Vector3i chunkIndex = new Vector3i(ix, 0, iz);

            //        #region Remove
            //        if (distX > GENERATE_RANGE || distZ > GENERATE_RANGE)
            //        {
            //            if (_world.viewableChunks[ix, iz] != null)
            //            {
            //                Debug.WriteLine("Remove({0},{1}) ChunkCount = {2}", ix, iz, _world.viewableChunks.Count);
            //                _world.viewableChunks.Remove(ix, iz);
            //            }
            //            continue;
            //        }
            //        #endregion
            //        #region Generate
            //        if (distX > LIGHT_RANGE || distZ > LIGHT_RANGE)
            //        {
            //            if (_world.viewableChunks[ix, iz] == null)
            //            {
            //                uint removeX = ix, removeZ = iz;

            //                // find the opposite chunk
            //                if (distX > LIGHT_RANGE) removeX = (uint)(ix - distX * xdir * 2);
            //                if (distZ > LIGHT_RANGE) removeZ = (uint)(iz - distZ * zdir * 2);

            //                // now that we have the opposite chunk, we can check if it is in a Ready state.
            //                // any previously showing chunks, would have that state. If so, we only attempt to regenerate showing chunks
            //                Chunk chunkRemove = _world.viewableChunks[removeX, removeZ];

            //                if (chunkRemove != null)
            //                {
            //                    if (chunkRemove.State == ChunkState.Ready)
            //                    {
            //                        Debug.WriteLine("Remove({0},{1}), Assign ({2},{3}), Dist ({4},{5}), Dir ({6},{7}) ChunkCount = {8}", removeX, removeZ, ix, iz, distX, distZ, xdir, zdir, _world.viewableChunks.Count);

            //                        // remove chunk is in a ready state, so we can remove it
            //                        _world.viewableChunks.Remove(removeX, removeZ);

            //                        // now we can add the front facing chunk to the generate queue, once only.
            //                        Chunk chunkGenerate = new Chunk(_world, chunkIndex);
            //                        chunkGenerate.State = ChunkState.AwaitingGenerate;
            //                        _world.viewableChunks[ix, iz] = chunkGenerate;

            //                        //Debug.WriteLine("chunkGenerate at {0}", chunkIndex);
            //                        QueueGenerate(chunkIndex);

            //                        /*when it works, replace by toReAssign next commented block*/
            //                        /* Chunk toReAssign = _world.viewableChunks[removeX, removeZ];
            //                        if(toReAssign!=null) toReAssign.Assign(chunkIndex);
            //                        toReAssign.State = ChunkState.AwaitingGenerate; 
            //                        */
            //                    }
            //                    else if (chunkRemove.State != ChunkState.AwaitingLighting)
            //                    {
            //                        Debug.WriteLine("chunkGenerate at {0}, state = {1}", chunkIndex, chunkRemove.State);
            //                    }
            //                }
            //                //else if (chunkRemove == null)
            //                //{
            //                //    Debug.WriteLine("NULL Remove found at ({0},{1}), ChunkCount = {2}", removeX, removeZ, _world.viewableChunks.Count);
            //                //}

            //            }
            //            continue;
            //        }
            //        /*
            //        if (distX >= GENERATE_RANGE || distZ >= GENERATE_RANGE)
            //        {
            //            if (_world.viewableChunks[ix, iz] == null)
            //            {
            //                Debug.WriteLine("Add({0},{1})", ix, iz);

            //                Chunk chunk = new Chunk(_world, chunkIndex);
            //                chunk.State = ChunkState.AwaitingGenerate;
            //                _world.viewableChunks[ix, iz] = chunk;
            //                QueueGenerate(chunkIndex);
            //            }
            //            continue;
            //        }*/
            //        #endregion
            //        #region Light
            //        if (distX <= LIGHT_RANGE || distZ <= LIGHT_RANGE)
            //        {
            //            Chunk chunk = _world.viewableChunks[ix, iz];
            //            if (chunk != null && chunk.State == ChunkState.AwaitingLighting)
            //            {
            //                QueueLighting(chunkIndex);
            //            }

            //            if (chunk != null && chunk.State == ChunkState.AwaitingRelighting)
            //            //if (rebuildChunk != null && rebuildChunk.State == ChunkState.AwaitingRelighting)
            //            {
            //                QueueLighting(chunkIndex);
            //            }

            //            if (chunk != null && (chunk.State == ChunkState.AwaitingRebuild || chunk.State == ChunkState.AwaitingBuild))
            //            //if (rebuildChunk != null && (rebuildChunk.State == ChunkState.AwaitingRebuild || rebuildChunk.State == ChunkState.AwaitingBuild))
            //            {
            //                QueueBuild(chunkIndex);
            //            }

            //            continue;
            //        }
            //        #endregion
            //        //#region Rebuild
            //        //Chunk rebuildChunk = _world.viewableChunks[ix, iz];

            //        //if (rebuildChunk.State == ChunkState.AwaitingRelighting)
            //        ////if (rebuildChunk != null && rebuildChunk.State == ChunkState.AwaitingRelighting)
            //        //{
            //        //    QueueLighting(chunkIndex);
            //        //}

            //        //if (rebuildChunk.State == ChunkState.AwaitingRebuild || rebuildChunk.State == ChunkState.AwaitingBuild)
            //        ////if (rebuildChunk != null && (rebuildChunk.State == ChunkState.AwaitingRebuild || rebuildChunk.State == ChunkState.AwaitingBuild))
            //        //{
            //        //    QueueBuild(chunkIndex);
            //        //}
            //        //#endregion
            //    }
            //}
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime)
        {
            DrawSolid(gameTime);
            DrawWater(gameTime);

            #region OSD debug texts
            debugSpriteBatch.Begin();

            if (debugRectangle)
            {
                debugSpriteBatch.Draw(debugRectTexture, backgroundRectangle, Color.Black);
            }
            //long workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            debugSpriteBatch.DrawString(debugFont, "GenQ: " + _generateQueue.Count.ToString(), genQVector2, Color.White);
            debugSpriteBatch.DrawString(debugFont, "LightQ: " + _lightingQueue.Count.ToString(), lightQVector2, Color.White);
            debugSpriteBatch.DrawString(debugFont, "BuildQ: " + _buildQueue.Count.ToString(), buildQVector2, Color.White);
            //debugSpriteBatch.DrawString(debugFont, (GC.GetTotalMemory(false) / (1024 * 1024)).ToString() + "MB/" + workingSet / (1024 * 1024) + "MB", memVector2, Color.White);
            debugSpriteBatch.End();
            #endregion
        }
        #endregion

    }
}
