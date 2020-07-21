#define XBOX

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

using TechCraftEngine;

namespace TechCraftEngine.Cameras
{
    public class ArcBallCamera : Camera
    {
        private double _rotateSpeed = 1f;
        private float _zoom;
        private float _verticalRotation = -0.6f;
        private float _horizontalRotation = 0f;
        private Vector3 _target;

        private int _wobble = 0;
        private int _initWobble = 0;
        private Random _rnd;

        public ArcBallCamera(TechCraftGame game)
            : base(game)
        {
            _rnd = new Random();
        }

        public override void Initialize()
        {
            CalculateView();
            CalculateProjection();

            // Zoom = dist from target!!
            _zoom = 6f;
        }

        public float HorizontalRotation
        {
            get { return _horizontalRotation; }
            set { _horizontalRotation = value; }
        }

        public float VerticalRotation
        {
            get { return _verticalRotation; }
            set { _verticalRotation = value; }
        }

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }

        public override void Update(GameTime gameTime)
        {
            

            ProcessKeyboardInput(gameTime);

            if (_wobble > 0)
            {
                Vector3 offset = new Vector3(((float)_rnd.NextDouble() - 0.5f) / (_initWobble - _wobble), 0, ((float)_rnd.NextDouble() - 0.5f) / (_initWobble - _wobble));
                //_position += offset;
                _target += offset;
                _wobble--;
            }

            CalculateView();

            base.Update(gameTime);
        }

        public void Wobble(int count)
        {
            _initWobble = count+1;
            _wobble = count;
        }

        private void ProcessKeyboardInput(GameTime gameTime)
        {
            PlayerIndex activeIndex;

            float rot = (float)(gameTime.ElapsedGameTime.TotalSeconds * _rotateSpeed);

            if (Game.InputState.IsKeyDown(Keys.A, PlayerIndex.One, out activeIndex) ||
                Game.InputState.IsButtonDown(Buttons.LeftShoulder, PlayerIndex.One, out activeIndex))
            {
                _horizontalRotation += rot;
            }
            if (Game.InputState.IsKeyDown(Keys.D, PlayerIndex.One, out activeIndex) ||
                Game.InputState.IsButtonDown(Buttons.RightShoulder, PlayerIndex.One, out activeIndex))
            {
                _horizontalRotation -= rot;
            }
            if (Game.InputState.IsKeyDown(Keys.W, PlayerIndex.One, out activeIndex))
            {
                _zoom -= 0.1f;
            }
            if (Game.InputState.IsKeyDown(Keys.S, PlayerIndex.One, out activeIndex))
            {
                _zoom += 0.1f;
            }
            if (Game.InputState.IsKeyDown(Keys.Z, PlayerIndex.One, out activeIndex))
            {
                _verticalRotation += 0.01f;
            }
            if (Game.InputState.IsKeyDown(Keys.C, PlayerIndex.One, out activeIndex))
            {
                _verticalRotation -= 0.01f;
            }
        }

        protected override void CalculateView()
        {
            // Keep angles within PI
            if (_horizontalRotation > MathHelper.TwoPi) _horizontalRotation -= MathHelper.TwoPi;
            if (_horizontalRotation < 0.0f) _horizontalRotation += MathHelper.TwoPi;
            if (_verticalRotation > MathHelper.TwoPi) _verticalRotation -= MathHelper.TwoPi;
            if (_verticalRotation < 0.0f) _verticalRotation += MathHelper.TwoPi;

            // Calculate rotations around the origin
            Vector3 offset = new Vector3(0, _zoom, 0);
            offset = Vector3.Transform(offset, Matrix.CreateRotationX(_verticalRotation));
            offset = Vector3.Transform(offset, Matrix.CreateRotationY(_horizontalRotation));

            Vector3 offsetPosition = Position + offset;
            View = Matrix.CreateLookAt(offsetPosition, _target, Vector3.Up);
        }

        public Vector3 Target
        {
            get { return _target; }
        }

        public void LookAt(Vector3 target)
        {
            _target = target;
        }

    }
}
