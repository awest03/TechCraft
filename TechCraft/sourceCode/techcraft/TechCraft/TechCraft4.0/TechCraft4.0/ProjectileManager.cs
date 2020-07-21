using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using TechCraftEngine;
using TechCraftEngine.Common;
using TechCraftEngine.WorldEngine;
using TechCraft.ParticleSystems;

namespace TechCraft
{
    public enum ProjectileType {
        Bullet,
        Missile,
        Grenade
    }

    public struct Projectile
    {
        public Vector3 Position;        
        public Vector3 Velocity;
        public float Size;
        public Color Color;
        public ProjectileType Type;
        public byte Payload;
        public byte PlayerId;           // Id of player who launched the projectile
    }

    public class ProjectileManager
    {

        private ExplosionParticleSystem _explosionParticleSystem;
        private Player _player;

        private Color[] _projectileColors = { Color.Gray, Color.Gray, Color.DarkGreen };
        private float[] _projectileSizes = { 0.005f, 0.2f, 0.05f };
        private float[] _projectileSpeeds = { 40f, 20f, 10f };

        private Vector3[] _cubeNormals = {
                                             new Vector3(0,0,1),
                                             new Vector3(0,0,-1),
                                             new Vector3(1,0,0),
                                             new Vector3(-1,0,0),
                                             new Vector3(0,1,0),
                                             new Vector3(0,-1,0)
                                         };

        private TechCraftGame _game;
        private World _world;
        private Projectile[] _projectilePool;
        private bool[] _activeProjectiles;
        private Queue<int> _availableProjectiles;
        private List<VertexPositionColor> _projectileVertexList;
        private VertexBuffer _projectileVertexBuffer;
        private Effect _projectileEffect;
       // private VertexDeclaration _vertexDeclaration;
        private BazookaSmokeParticleSystem _bazookaSmokeParticleSystem;
        private ExplosionSmokeParticleSystem _explosionSmokeParticleSystem;

        private SoundEffect[] _explosionSounds;

        public ProjectileManager(TechCraftGame game, Player player, World world) {
            _game = game;
            _world = world;
            _player = player;
        }

        public void Initialize()
        {
            _projectilePool = new Projectile[WorldSettings.MAXPROJECTILES];
            _activeProjectiles = new bool[WorldSettings.MAXPROJECTILES];
            _availableProjectiles = new Queue<int>(WorldSettings.MAXPROJECTILES);
            _projectileVertexList = new List<VertexPositionColor>(WorldSettings.MAXPROJECTILES * 6);
            //_vertexDeclaration = new VertexDeclaration(_game.GraphicsDevice, VertexPositionColor.VertexElements);
            _bazookaSmokeParticleSystem = new BazookaSmokeParticleSystem(_game, _game.Content);
            _bazookaSmokeParticleSystem.Initialize();
            _explosionParticleSystem = new ExplosionParticleSystem(_game, _game.Content);
            _explosionParticleSystem.Initialize();
            _explosionSmokeParticleSystem = new ExplosionSmokeParticleSystem(_game, _game.Content);
            _explosionSmokeParticleSystem.Initialize();
            Clear();
        }

        public void Clear()
        {
            _availableProjectiles.Clear();
            for (int i = 0; i < WorldSettings.MAXPROJECTILES; i++)
            {
                _activeProjectiles[i] = false;
                _availableProjectiles.Enqueue(i);
            }
        }

        public void AddProjectile(Vector3 position, Vector3 direction, ProjectileType type)
        {
            int index = _availableProjectiles.Dequeue();
            _activeProjectiles[index] = true;

            _projectilePool[index].Position = position;
            _projectilePool[index].Velocity = direction * _projectileSpeeds[(int)type];
            _projectilePool[index].Color = _projectileColors[(int)type];
            _projectilePool[index].Size = _projectileSizes[(int)type];
            _projectilePool[index].Type = type;
            _projectilePool[index].Payload = 0;
            _projectilePool[index].PlayerId = 0;
        }

        public void ReleaseProjectile(int index)
        {
            _activeProjectiles[index] = false;
            _availableProjectiles.Enqueue(index);
        }

        public void LoadContent()
        { 
            _projectileEffect = _game.Content.Load<Effect>("Effects\\CubeEffect");
            _explosionSounds = new SoundEffect[4];
            _explosionSounds[0] = _game.Content.Load<SoundEffect>("Sounds\\explosion-01");
            _explosionSounds[1] = _game.Content.Load<SoundEffect>("Sounds\\explosion-02");
            _explosionSounds[2] = _game.Content.Load<SoundEffect>("Sounds\\explosion-03");
            _explosionSounds[3] = _game.Content.Load<SoundEffect>("Sounds\\explosion-04");
        }

        private void AddCube(Vector3 position, float size, Color color)
        {
            for (int x= 0;x<_cubeNormals.Length;x++)
            {
                Vector3 normal = _cubeNormals[x];

                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

               _projectileVertexList.Add(new VertexPositionColor(position + ((normal - side1 - side2) * size / 2), color));
                _projectileVertexList.Add(new VertexPositionColor(position + ((normal - side1 + side2) * size / 2), color));
                _projectileVertexList.Add(new VertexPositionColor(position + ((normal + side1 + side2) * size / 2), color));
                _projectileVertexList.Add(new VertexPositionColor(position + ((normal - side1 - side2) * size / 2), color));
                _projectileVertexList.Add(new VertexPositionColor(position + ((normal + side1 + side2) * size / 2), color));
                _projectileVertexList.Add(new VertexPositionColor(position + ((normal + side1 - side2) * size / 2), color));
            }
        }

        private void Explode(int eX, int eY, int eZ)
        {
            Random r = new Random();
            for (int x = eX - WorldSettings.MISSILEEXPLOSIONRADIUS; x < eX + WorldSettings.MISSILEEXPLOSIONRADIUS; x++)
            {
                for (int y = eY - WorldSettings.MISSILEEXPLOSIONRADIUS; y < eY + WorldSettings.MISSILEEXPLOSIONRADIUS; y++)
                {
                    for (int z = eZ - WorldSettings.MISSILEEXPLOSIONRADIUS; z < eZ + WorldSettings.MISSILEEXPLOSIONRADIUS; z++)
                    {
                        
                        if (r.Next(5) != 1)
                        {
                            BlockType type = _world.BlockAt(x, y, z);
                            if (type != BlockType.None && type != BlockType.Water)
                            {
                                _world.RemoveBlock(x, y, z);
                                for (int i = 1; i < 5; i++)
                                {
                                    _explosionParticleSystem.AddParticle(new Vector3(eX+1, eY+1, eZ+1), Vector3.Zero);
                                }
                                for (int i = 1; i < 2; i++)
                                {
                                    _explosionSmokeParticleSystem.AddParticle(new Vector3(x+1, y+1, z+1), Vector3.Zero);
                                }
                                int sound = r.Next(4);
                                Vector3 explosionPosition = new Vector3(eX, eY, eZ);
                                Vector3 dist = explosionPosition - _player.Position;
                                if (dist.Length() > 0)
                                {
                                    float volume = 1/(dist.Length()*2);
                                    if (volume > 1) volume = 1;
                                    _explosionSounds[sound].Play(volume, 0, 0);
                                }
                                else
                                {
                                    _explosionSounds[sound].Play();
                                }
                            }
                        }
                    }
                }
            }
            _world.RemoveBlock(eX, eY, eZ);
        }

        private bool CheckCollision(int projectile)
        {
            Vector3 position = _projectilePool[projectile].Position;
            BlockType block = _world.BlockAtPoint(position);
            if (block != BlockType.None && block!=BlockType.Water)
            {
                Explode((int)position.X, (int)position.Y, (int)position.Z);

                ReleaseProjectile(projectile);

                return true;
            }
            return false;
        }

        public void Update(GameTime gameTime)
        {
            _projectileVertexList.Clear();
            for (int x = 0; x < WorldSettings.MAXPROJECTILES; x++)
            {
                if (_activeProjectiles[x])
                {                    
                    // Add smoke at old position
                    _bazookaSmokeParticleSystem.AddParticle(_projectilePool[x].Position, Vector3.Zero);

                    // Update positions
                    _projectilePool[x].Position += (_projectilePool[x].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (!CheckCollision(x))
                    {
                        AddCube(_projectilePool[x].Position, _projectilePool[x].Size, _projectilePool[x].Color);
                    }
                }
            }
            _bazookaSmokeParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
            _bazookaSmokeParticleSystem.Update(gameTime);
            _explosionParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
            _explosionParticleSystem.Update(gameTime);
            _explosionSmokeParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
            _explosionSmokeParticleSystem.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            if (_projectileVertexList.Count > 0)
            {
                _projectileVertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor) , _projectileVertexList.Count , BufferUsage.WriteOnly);
                _projectileVertexBuffer.SetData<VertexPositionColor>(_projectileVertexList.ToArray());

                //_game.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
                //_game.GraphicsDevice.RenderState.CullMode = CullMode.None;
//                _game.GraphicsDevice.RenderState.DepthBufferEnable = true;
  //              _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
    //            _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                _game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                _game.GraphicsDevice.BlendState = BlendState.Opaque;

                _projectileEffect.Parameters["World"].SetValue(Matrix.Identity);
                _projectileEffect.Parameters["View"].SetValue(_game.Camera.View);
                _projectileEffect.Parameters["Projection"].SetValue(_game.Camera.Projection);

                //_projectileEffect.Begin();
                _projectileEffect.CurrentTechnique.Passes[0].Apply();// Begin();

                //_game.GraphicsDevice.Vertices[0].SetSource(_projectileVertexBuffer, 0, VertexPositionColor.SizeInBytes);
                _game.GraphicsDevice.SetVertexBuffer(_projectileVertexBuffer);
                
                _game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _projectileVertexBuffer.VertexCount / 3);

                //_projectileEffect.CurrentTechnique.Passes[0].End();
                //_projectileEffect.End();
            }
            _bazookaSmokeParticleSystem.Draw(gameTime);
            _explosionParticleSystem.Draw(gameTime);
            _explosionSmokeParticleSystem.Draw(gameTime);
        }       
    }
}
