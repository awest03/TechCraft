using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TechCraftEngine;
using TechCraftEngine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using TechCraftEngine.WorldEngine;
using TechCraft.ParticleSystems;

namespace TechCraft
{
    public enum Weapon
    {
        Bazooka=0
    }

    public class WeaponManager
    {
        private TechCraftGame _game;
        private Player _player;
        private World _world;
        private ProjectileManager _projectileManager;


        private Model _bazookaModel;
        private Effect _weaponEffect;
        private Texture2D _weaponTexture;
        private SoundEffect[] _weaponSounds;
        private Weapon _activeWeapon;

        float[] weaponScale = { 0.7f };
        float[] weaponRotationX = { -0.3f };
        float[] weaponOffsetX = { 0.3f };
        Vector3[] weaponOffsetY = {new Vector3(0, -0.25f, 0)};
        Vector3[] weaponOffsetZ = {new Vector3(0, 0, -0.2f)};                       
        float[] weaponRotationOffsetY = {0.1f};
        float[] weaponRotationOffsetZ = {-0.1f};


        private SpriteBatch debugBatch;
        private SpriteFont debugFont;

        public WeaponManager(TechCraftGame game, World world, Player player)
        {
            _game = game;
            _player = player;
            _activeWeapon = Weapon.Bazooka;
            _world = world;
        }

        public void Initialize()
        {
            debugBatch = new SpriteBatch(_game.GraphicsDevice);
            _projectileManager = new ProjectileManager(_game, _player, _world);
            _projectileManager.Initialize();

        }

        public static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        public void LoadContent()
        {
            _bazookaModel = _game.Content.Load<Model>("Models\\Weapons\\bazooka");
            _weaponEffect = _game.Content.Load<Effect>("Effects\\WeaponEffect");
            _weaponTexture = _game.Content.Load<Texture2D>("Models\\Weapons\\bazooka_auv");
            RemapModel(_bazookaModel, _weaponEffect);
            _projectileManager.LoadContent();
            debugFont = _game.Content.Load<SpriteFont>("Fonts\\console");
            _weaponSounds = new SoundEffect[1];
            _weaponSounds[0] = _game.Content.Load<SoundEffect>("Sounds\\bazooka");
        }

        public void Update(GameTime gameTime)
        {
            PlayerIndex controlIndex;
            // ROTATION Y
            if (_game.InputState.IsKeyPressed(Keys.N, null, out controlIndex))
            {
                weaponRotationOffsetY[(int)_activeWeapon] += 0.1f;
            }
            if (_game.InputState.IsKeyPressed(Keys.M, null, out controlIndex))
            {
                weaponRotationOffsetY[(int)_activeWeapon] -= 0.1f;
            }
            // ROTATION Z
            if (_game.InputState.IsKeyPressed(Keys.V, null, out controlIndex))
            {
                weaponRotationOffsetZ[(int)_activeWeapon] += 0.1f;
            }
            if (_game.InputState.IsKeyPressed(Keys.B, null, out controlIndex))
            {
                weaponRotationOffsetZ[(int)_activeWeapon] -= 0.1f;
            }
            // OFFSET X
            if (_game.InputState.IsKeyPressed(Keys.T, null, out controlIndex))
            {
                weaponOffsetX[(int)_activeWeapon] += 0.1f;
            }
            if (_game.InputState.IsKeyPressed(Keys.G, null, out controlIndex))
            {
                weaponOffsetX[(int)_activeWeapon] -= 0.1f;
            }
            // ROTATION X
            if (_game.InputState.IsKeyPressed(Keys.Y, null, out controlIndex))
            {
                weaponRotationX[(int)_activeWeapon] += 0.1f;
            }
            if (_game.InputState.IsKeyPressed(Keys.H, null, out controlIndex))
            {
                weaponRotationX[(int)_activeWeapon] -= 0.1f;
            }
            // SCALE
            if (_game.InputState.IsKeyPressed(Keys.U, null, out controlIndex))
            {
                weaponScale[(int)_activeWeapon] += 0.1f;
            }
            if (_game.InputState.IsKeyPressed(Keys.J, null, out controlIndex))
            {
                weaponScale[(int)_activeWeapon] -= 0.1f;
            }
            //OFFSET Y
            if (_game.InputState.IsKeyPressed(Keys.I, null, out controlIndex))
            {
                weaponOffsetY[(int)_activeWeapon] += new Vector3(0, 0.05f, 0);
            }
            if (_game.InputState.IsKeyPressed(Keys.K, null, out controlIndex))
            {
                weaponOffsetY[(int)_activeWeapon] -= new Vector3(0, 0.05f, 0);
            }
            //OFFSET Z
            if (_game.InputState.IsKeyPressed(Keys.O, null, out controlIndex))
            {
                weaponOffsetZ[(int)_activeWeapon] += new Vector3(0, 0, 0.05f);
            }
            if (_game.InputState.IsKeyPressed(Keys.L, null, out controlIndex))
            {
                weaponOffsetZ[(int)_activeWeapon] -= new Vector3(0, 0, 0.05f);
            }


            _projectileManager.Update(gameTime);

        }


        public FirstPersonCamera Camera
        {
            get
            {
                return (FirstPersonCamera)_game.Camera;
            }
        }

        public void Fire()
        {
            float cameraRotationY = Camera.LeftRightRotation - MathHelper.PiOver2;
            float cameraRotationZ = -Camera.UpDownRotation;

            // Pick a target vector in the distance ahead.
            Vector3 target = Camera.Position + (_player.LookVector * 1000);

            Vector3 start = Vector3.Zero;
            Matrix rot = Matrix.CreateTranslation(weaponOffsetY[(int)_activeWeapon] + weaponOffsetZ[(int)_activeWeapon])
                * Matrix.CreateRotationZ(cameraRotationZ)
                * Matrix.CreateRotationY(cameraRotationY);

            Vector3 ofsStart = Vector3.Transform(start, rot) + Camera.Position;


            Vector3 direction = target - ofsStart;
            direction.Normalize();

            _projectileManager.AddProjectile(ofsStart, direction, ProjectileType.Missile);
            _weaponSounds[0].Play(0.4f,0,0);

        }

        public void Draw(GameTime gameTime)
        {

            _projectileManager.Draw(gameTime);


            _game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0);

            // Rotate weapon orientation
            // Translate to position
            // Rotate to player orientation

            //_game.GraphicsDevice.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            _game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            //_game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
           // _game.GraphicsDevice.RenderState.DepthBufferEnable = true;
          //  _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
           // _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
           // _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            _game.GraphicsDevice.BlendState = BlendState.Opaque;

            float cameraRotationY = Camera.LeftRightRotation - MathHelper.PiOver2;
            float cameraRotationZ = -Camera.UpDownRotation;

            foreach (ModelMesh mesh in _bazookaModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(weaponScale[(int)_activeWeapon]) *
                        Matrix.CreateRotationX(weaponRotationX[(int)_activeWeapon]) *
                        Matrix.CreateRotationY(weaponRotationOffsetY[(int)_activeWeapon]) *
                        Matrix.CreateRotationZ(weaponRotationOffsetZ[(int)_activeWeapon]) *
                        Matrix.CreateTranslation(weaponOffsetY[(int)_activeWeapon] + weaponOffsetZ[(int)_activeWeapon]) *
                        Matrix.CreateRotationZ(cameraRotationZ) *
                        Matrix.CreateRotationY(cameraRotationY) *
                        Matrix.CreateTranslation(Camera.Position + (_player.LookVector * weaponOffsetX[(int)_activeWeapon])));
                    effect.Parameters["View"].SetValue(_game.Camera.View);
                    effect.Parameters["Projection"].SetValue(_game.Camera.Projection);
                    effect.Parameters["BlockTexture"].SetValue(_weaponTexture);

                    mesh.Draw();
                }
            }

          /*  debugBatch.Begin();

            debugBatch.DrawString(debugFont, string.Format("Scale {0}", weaponScale[(int)_activeWeapon]), new Vector2(10, 10), Color.White);
            debugBatch.DrawString(debugFont, string.Format("RotationX {0}", weaponRotationX[(int)_activeWeapon]), new Vector2(10, 40), Color.White);
            debugBatch.DrawString(debugFont, string.Format("OffsetX {0}", weaponOffsetX[(int)_activeWeapon]), new Vector2(10, 70), Color.White);
            debugBatch.DrawString(debugFont, string.Format("OffsetY {0}", weaponOffsetY[(int)_activeWeapon]), new Vector2(10, 100), Color.White);
            debugBatch.DrawString(debugFont, string.Format("OffsetZ {0}", weaponOffsetZ[(int)_activeWeapon]), new Vector2(10, 130), Color.White);
            debugBatch.DrawString(debugFont, string.Format("RotationY {0}", weaponRotationOffsetY[(int)_activeWeapon]), new Vector2(10, 160), Color.White);
            debugBatch.DrawString(debugFont, string.Format("RotationZ {0}", weaponRotationOffsetZ[(int)_activeWeapon]), new Vector2(10, 190), Color.White);

            debugBatch.End();*/
        }
    }
}
