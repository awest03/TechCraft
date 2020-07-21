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
    public class SimpleCamera : Camera
    {

        private Vector3 _target;

        public SimpleCamera(TechCraftGame game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            _target = Vector3.Zero;

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        public Vector3 Target
        {
            get { return _target; }
            set { 
                _target = value; 
                CalculateView();
            }
        }


        protected override void CalculateView()
        {

            View = Matrix.CreateLookAt(Position, _target, Vector3.Up);

            base.CalculateView();
        }

    }
}
