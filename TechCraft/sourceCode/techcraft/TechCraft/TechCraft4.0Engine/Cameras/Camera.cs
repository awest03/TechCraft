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

using TechCraftEngine.WorldEngine;

namespace TechCraftEngine.Cameras
{
    public abstract class Camera
    {
        private TechCraftGame _game;

        private Matrix _view;
        private Matrix _projection;
        protected Vector3 _position = Vector3.Zero;

        private float _viewAngle = MathHelper.PiOver4;
        private float _nearPlane = 0.01f;
        private float _farPlane = WorldSettings.FARPLANE;

        public Camera(TechCraftGame game)
        {
            _game = game;            
        }

        protected TechCraftGame Game
        {
            get { return _game; }
        }

        public Matrix View
        {
            get { return _view; }
            protected set { _view = value; }
        }

        public Matrix Projection
        {
            get { return _projection; }
            protected set { _projection = value; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                CalculateView();
            }
        }

        protected virtual void CalculateProjection()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(_viewAngle, _game.GraphicsDevice.Viewport.AspectRatio, _nearPlane, _farPlane);
        }

        protected virtual void CalculateView()
        {
        }

        public virtual void Initialize()
        {
            CalculateView();
            CalculateProjection();
        }

        public virtual void Update(GameTime gameTime)
        {
        }


    }
}
