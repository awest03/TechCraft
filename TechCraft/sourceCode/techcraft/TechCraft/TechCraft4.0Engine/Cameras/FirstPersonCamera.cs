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
    public class FirstPersonCamera : Camera
    {
        private const float _rotationSpeed = 0.05f;
        private float _leftRightRotation = 0f;
        private float _upDownRotation = 0f;
        private Vector3 _cameraFinalTarget;

        public FirstPersonCamera(TechCraftGame game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            _upDownRotation = 0;
            _leftRightRotation = 0;

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            CalculateView();
            base.Update(gameTime);
        }

        public Vector3 Target
        {
            get { return _cameraFinalTarget; }
        }

        public float LeftRightRotation
        {
            get { return _leftRightRotation; }
            set
            {
                _leftRightRotation = value;
                CalculateView();
            }
        }

        public float UpDownRotation
        {
            get { return _upDownRotation; }
            set
            {
                _upDownRotation = value;
                CalculateView();
            }
        }

        protected override void CalculateView()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(_upDownRotation) * Matrix.CreateRotationY(_leftRightRotation);
            Vector3 cameraRotatedTarget = Vector3.Transform(Vector3.Forward, rotationMatrix);
            _cameraFinalTarget = Position + cameraRotatedTarget;
            Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, rotationMatrix);
            View = Matrix.CreateLookAt(Position, _cameraFinalTarget, cameraRotatedUpVector);

            base.CalculateView();
        }

        public void LookAt(Vector3 target)
        {
            // Doesn't take into account the rotated UP vector
            // Should calculate rotations here!
            View = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }
    }
}
