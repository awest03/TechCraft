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
using TechCraftEngine.Cameras;

namespace TechCraftEngine.Controllers
{
    public class ArcBallCameraController : Controller
    {
        private const double ROTATESPEED = 1f;

        public ArcBallCameraController(TechCraftGame game) :
            base(game)
        {
        }

        public ArcBallCamera Camera
        {
            get { return (ArcBallCamera)Game.Camera; }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        private void ProcessInput(GameTime gameTime)
        {
            PlayerIndex activeIndex;

            float rot = (float)(gameTime.ElapsedGameTime.TotalSeconds * ROTATESPEED);

            if (Game.InputState.IsKeyDown(Keys.A, PlayerIndex.One, out activeIndex) ||
                Game.InputState.IsButtonDown(Buttons.LeftShoulder, PlayerIndex.One, out activeIndex))
            {
                Camera.HorizontalRotation += rot;
            }
            if (Game.InputState.IsKeyDown(Keys.D, PlayerIndex.One, out activeIndex) ||
                Game.InputState.IsButtonDown(Buttons.RightShoulder, PlayerIndex.One, out activeIndex))
            {
                Camera.HorizontalRotation -= rot;
            }
            if (Game.InputState.IsKeyDown(Keys.W, PlayerIndex.One, out activeIndex))
            {
                Camera.Zoom -= 0.1f;
            }
            if (Game.InputState.IsKeyDown(Keys.S, PlayerIndex.One, out activeIndex))
            {
                Camera.Zoom += 0.1f;
            }
            if (Game.InputState.IsKeyDown(Keys.Z, PlayerIndex.One, out activeIndex))
            {
                Camera.VerticalRotation += 0.01f;
            }
            if (Game.InputState.IsKeyDown(Keys.C, PlayerIndex.One, out activeIndex))
            {
                Camera.VerticalRotation -= 0.01f;
            }
        }
    }
}
