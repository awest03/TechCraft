using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace TechCraftEngine.WorldEngine
{
    public class RegionCache
    {
        private const int CACHE_SIZE = 25;

        private Region[] _regions;
        private bool[] _availability;
        private Queue<Region> _available;
        private World _world;
     
        private Queue<Vector3i> _toLoad;
        private Thread _loadingThread;

        private Queue<Region> _toBuild;
        private Thread _buildingThread;
        //private ThreadManager _threadManager;

        private bool _running = true;
        private Vector3 _playerPosition;


        public RegionCache(World world)
        {
            _world = world;
            _regions = new Region[CACHE_SIZE];
            _availability = new bool[CACHE_SIZE];
            _available = new Queue<Region>(CACHE_SIZE);

            _toLoad = new Queue<Vector3i>();
            _toBuild = new Queue<Region>();

            Clear();

            //_threadManager = (ThreadManager)Game.Services.GetService(typeof(ThreadManager));

            _loadingThread = new Thread(new ThreadStart(LoadingThread));
            //_threadManager.Add(_loadingThread);
            _loadingThread.Start();

            _buildingThread = new Thread(new ThreadStart(BuildingThread));
            //_threadManager.Add(_buildingThread);
            _buildingThread.Start();
        }

        public Vector3 PlayerPosition
        {
            get { return _playerPosition; }
            set { _playerPosition = value; }
        }

        public void Clear()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                _availability[i] = true;
                _regions[i] = new Region(_world,i);
                
                _available.Enqueue(_regions[i]);
              
            }
        }

        public void Flush(Vector3 playerPosition,float radius)
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i])
                {

                    float distance = (_regions[i].Center - playerPosition).Length();                    
                    if (distance < 0) distance = 0-distance;
                    if (distance > radius)
                    {
                        //Debug.WriteLine(string.Format("Flushing {0} : {1},{2},{3}", _regions[i].GetFilename(), _regions[i].Center, position, distance)); 
                        _regions[i].RegionManager.Flush(true);
                        _availability[i] = true;
                        _available.Enqueue(_regions[i]);
                    }
                }
            }
        }

        public void Save()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i])
                {
                    _regions[i].RegionManager.Flush(false);
                }
            }
        }

        public Region FindRegion(Vector3i regionPosition)
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i] && _regions[i].Position == regionPosition)
                {
                    return _regions[i];
                }
            }
            return null;
        }

       
        public void SubmitModifiedRegionsForBuild()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i] && _regions[i].Modified)
                {
                    QueueBuild(_regions[i]);
                    _regions[i].Modified = false;
                }
            }
        }

        public void QueueBuild(Region region)
        {
            Debug.WriteLine(string.Format("Queue Build {0}-{1}-{2}", (int) region.Position.x, (int) region.Position.y, (int) region.Position.z));
            lock (_toBuild)
            {
                _toBuild.Enqueue(region);
            }
        }

        public void QueueLoad(Vector3i position)
        {
            Debug.WriteLine(string.Format("Queue Load {0}-{1}-{2}", (int) position.x, (int) position.y, (int) position.z));
            lock (_toLoad)
            {
                foreach (Vector3i check in _toLoad)
                {
                    if (position==check) 
                    {
                        //Debug.WriteLine("Already queued");
                        return;
                    }
                }
                if (IsLoaded(position)) return;
                _toLoad.Enqueue(position);
            }
        }

        public bool IsLoaded(Vector3i position)
        {
            if (FindRegion(position) == null) return false;
            Debug.WriteLine("Already loaded");
            return true;
        }

        public void BuildingThread()
        {
#if XBOX
            _buildingThread.SetProcessorAffinity(4);
#endif
            while (_running)
            {
                Region buildRegion = null;
                bool doBuild = false;
                lock (_toBuild)
                {
                    if (_toBuild.Count > 0)
                    {
                        buildRegion = _toBuild.Dequeue();
                        doBuild = true;
                    }
                }
                if (doBuild)
                {
                    DoBuild(buildRegion);
                }
                Thread.Sleep(1);
            }
        }

        public void DoBuild(Region region)
        {
            region.BuildVertexBuffers();
        }

        public void LoadingThread()
        {
#if XBOX
            _loadingThread.SetProcessorAffinity(4);
#endif
            while (_running)
            {
                Vector3i loadPosition = new Vector3i(0, 0, 0);
                bool doLoad = false;
                lock (_toLoad)
                {
                    if (_toLoad.Count > 0)
                    {
                        loadPosition = _toLoad.Dequeue();
                        doLoad = true;
                    }
                }
                if (doLoad)
                {
                    DoLoad(loadPosition);
                }
                Thread.Sleep(1);
            }
        }

        public void DoLoad(Vector3i regionPosition)
        {
            if (FindRegion(regionPosition) == null)
            {
                QueueBuild(LoadRegion(regionPosition));
            }
        }

        private Region LoadRegion(Vector3i position)
        {
            Flush(_playerPosition, 250);
            if (_available.Count > 0)
            {
                Region region = _available.Dequeue();
                _availability[region.NodeIndex] = false;

                region.RegionManager.Load(position);

                return region;
            }
            else
            {
                throw new Exception("No available regions");
            }
        }

        public Region GetRegion(Vector3i position)
        {
            Region region = FindRegion(position);
            if (region != null)
            {
                Debug.WriteLine(string.Format("Found : {0},{0},{0}", position.x, position.y, position.z));
                return region;
            }
            else
            {
                //return LoadRegion(position);
                //StartLoading(position);
                return null;
            }
        }
    }
}
