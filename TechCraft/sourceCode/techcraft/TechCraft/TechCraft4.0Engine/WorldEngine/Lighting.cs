using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Threading;

namespace TechCraftEngine.WorldEngine
{
    public class Lighting
    {
        private struct Light
        {
            public int X;
            public int Y;
            public int Z;
            public byte Intensity;

            public Light(int x, int y, int z, byte intensity)
            {
                X = x;
                Y = y;
                Z = z;
                Intensity = intensity;
            }
        }

        private TechCraftGame _game;
        private World _world;
        private byte[, ,] _lighting;
        private int[,] _lightHeight;

        private Queue<Light> toLight = new Queue<Light>();
        private Queue<Light> toDark = new Queue<Light>();
        private Dictionary<Vector3i,bool> toSun = new Dictionary<Vector3i,bool>();


        public Lighting(TechCraftGame game, World world)
        {
            _lighting = new byte[WorldSettings.MAPWIDTH, WorldSettings.MAPHEIGHT, WorldSettings.MAPLENGTH];
            _game = game;
            _world = world;
        }

        public void Initialize()
        {
            InitLighting();
        }

        private void InitLighting()
        {
            toLight.Clear();
            toDark.Clear();
            toSun.Clear();
            //XXX moved _lighting instanciation to constructor
            _lightHeight = new int[WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH];
            LightRegion(0, WorldSettings.MAPWIDTH, 0, WorldSettings.MAPLENGTH);
            FillLighting();

            //Thread lightingThread = new Thread(new ThreadStart(ThreadedUpdate));            
            //lightingThread.Start();
            //_game.Threads.Add(lightingThread);
  
        }

        private void LightRegion(int sx, int ex, int sz, int ez)
        {
            if (sx < 0) sx = 0;
            if (sz < 0) sz = 0;
            if (ex > WorldSettings.MAPWIDTH) ex = WorldSettings.MAPWIDTH;
            if (ez > WorldSettings.MAPLENGTH) ex = WorldSettings.MAPLENGTH;

            for (int x = sx; x < ex; x++)
            {
                for (int z = sz; z < ez; z++)
                {
                    bool inShadow = false;
                    for (int y = WorldSettings.MAPHEIGHT - 1; y > 0 ; y--)
                    {
                        BlockType blockType = _world.BlockAt(x, y, z);
                        if (!BlockInformation.IsLightTransparentBlock(blockType) && !inShadow)
                        {
                            inShadow = true;
                            _lightHeight[x, z] = y+1;
                        }
                        if (!inShadow)
                        {
                            _lighting[x, y, z] = WorldSettings.MAXLIGHT;
                            toLight.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT));
                        }
                        else
                        {
                            _lighting[x, y, z] = WorldSettings.MINLIGHT;
                            if (BlockInformation.IsLightEmittingBlock(blockType))
                            {
                                toLight.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT));
                            }
                        }
                        _world.MakeDirty(x, y, z);
                    }
                }
            }
        }

        public void BlockAdded(BlockType blockType, int x, int y, int z)
        {
            // Don't want to call this during initial map loading
            if (_lighting != null)
            {
                if (BlockInformation.IsLightEmittingBlock(blockType))
                {
                    toLight.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT));
                }
                if (!BlockInformation.IsLightTransparentBlock(blockType))
                {
                    toDark.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT * 2));
                }
            }
        }

        public void BlockRemoved(BlockType blockType, int x, int y, int z)
        {
            if (!BlockInformation.IsLightTransparentBlock(blockType) && !BlockInformation.IsLightEmittingBlock(blockType))
            {
                toLight.Enqueue(new Light(x, y, z, _lighting[x, y, z]));
            }
            if (BlockInformation.IsLightEmittingBlock(blockType))
            {
                LightRemoved(x, y, z);
            }
        }

        private void LightAdded(int x, int y, int z, byte intensity)
        {
            _lighting[x, y, z] = intensity;
            toLight.Enqueue(new Light(x,y,z,intensity));
        }

        private void LightRemoved(int x, int y, int z)
        {
            _lighting[x, y, z] = WorldSettings.MINLIGHT;
            toDark.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT * 2));
        }

        public void Update()
        {
            DeFillLighting();
            FillLighting();
        }

        private void ThreadedUpdate()
        {
//#if XBOX
            //Thread.CurrentThread.SetProcessorAffinity(5);
//#endif
            while (true)
            {
                DeFillLighting();
                FillLighting();
                Thread.Sleep(1);
            }
        }

        private void FillLighting()
        {
            while (toLight.Count > 0)
            {
                Light light = toLight.Dequeue();
                if (light.Intensity >= WorldSettings.MINLIGHT)
                {
                    CheckLight((int)light.X + 1, (int)light.Y, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X - 1, (int)light.Y, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y + 1, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y - 1, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y, (int)light.Z + 1, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y, (int)light.Z - 1, light.Intensity);
                }
            }
        }

        private void CheckLight(int x, int y, int z, byte intensity)
        {
            intensity = (byte)(intensity - 1);
            if (_world.InWorldBounds(x, y, z))
            {
                if (_lighting[x, y, z] < intensity)
                {
                    _lighting[x, y, z] = intensity;
                    _world.MakeDirty(x, y, z);
                    if (BlockInformation.IsLightTransparentBlock(_world.BlockAt(x, y, z)))
                    {
                        toLight.Enqueue(new Light(x, y, z, intensity));
                    }
                }
            }
        }

        private void DeFillLighting()
        {
            while (toDark.Count > 0)
            {
                Light dark = toDark.Dequeue();              
                if (dark.Intensity > WorldSettings.MINLIGHT)
                {
                    CheckDark((int)dark.X + 1, (int)dark.Y, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X - 1, (int)dark.Y, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y + 1, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y - 1, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y, (int)dark.Z + 1, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y, (int)dark.Z - 1, dark.Intensity);
                }
            }
        }

        private void CheckDark(int x, int y, int z, byte intensity)
        {
            intensity = (byte)(intensity - 1);
            if (_world.InWorldBounds(x, y, z))
            {
                if (intensity > WorldSettings.MINLIGHT && _lighting[x,y,z]!=WorldSettings.MINLIGHT)
                {
                    _lighting[x, y, z] = WorldSettings.MINLIGHT;
                    // If we're in sunlight on a light emitter we need to requeue
                    if (y >= _lightHeight[x, z] || BlockInformation.IsLightEmittingBlock(_world.BlockAt(x, y, z)))
                    {
                        // We darked a light so schedule it to be relit
                        toLight.Enqueue(new Light(x, y, z, WorldSettings.MAXLIGHT+1));
                    }
                    _world.MakeDirty(x, y, z);
                    toDark.Enqueue(new Light(x, y, z, intensity));
                }
            }
        }

        public float GetLight(int x, int y, int z)
        {
            if (_world.InWorldBounds(x, y, z))
            {
                float result = ((float)_lighting[x, y, z]) / (float)WorldSettings.MAXLIGHT;
                return result * 1.5f;
            }
            else
            {
                return 0.01f;
            }
        }
    }
}

