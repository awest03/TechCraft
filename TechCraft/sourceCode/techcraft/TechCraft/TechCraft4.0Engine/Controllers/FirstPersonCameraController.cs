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
    public class FirstPersonCameraController : Controller
    {
        private const float MOVEMENTSPEED = 0.25f;
        private const float ROTATIONSPEED = 0.1f;

        private MouseState _mouseMoveState;
        private MouseState _mouseState;

        public FirstPersonCameraController(TechCraftGame game) 
            : base(game)
        {

        }

        public FirstPersonCamera Camera
        {
            get { return (FirstPersonCamera)Game.Camera; }
        }

        public override void Initialize()
        {
            _mouseState = Mouse.GetState();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            ProcessInput(gameTime);
            base.Update(gameTime);
        }

        public void ProcessInput(GameTime gameTime)
        {
            //PlayerIndex activeIndex;

            //Vector3 moveVector = new Vector3(GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Left.X,0,-GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Left.Y);

            //if (Game.InputState.IsKeyDown(Keys.W, PlayerIndex.One, out activeIndex))
            //{
            //    moveVector += Vector3.Forward;
            //}
            //if (Game.InputState.IsKeyDown(Keys.S, PlayerIndex.One, out activeIndex))
            //{
            //    moveVector += Vector3.Backward;
            //}
            //if (Game.InputState.IsKeyDown(Keys.A, PlayerIndex.One, out activeIndex))
            //{
            //    moveVector += Vector3.Left;
            //}
            //if (Game.InputState.IsKeyDown(Keys.D, PlayerIndex.One, out activeIndex))
            //{
            //    moveVector += Vector3.Right;
            //}
            //if (moveVector != Vector3.Zero)
            //{
            //    Matrix rotationMatrix = Matrix.CreateRotationX(Camera.UpDownRotation) * Matrix.CreateRotationY(Camera.LeftRightRotation);
            //    Vector3 rotatedVector = Vector3.Transform(moveVector, rotationMatrix);
            //    Camera.Position += rotatedVector * MOVEMENTSPEED;
            //}

            if (this.Game.IsActive)
            {

                MouseState currentMouseState = Mouse.GetState();
                float mouseDX = currentMouseState.X - _mouseMoveState.X;
                float mouseDY = currentMouseState.Y - _mouseMoveState.Y;
                if (mouseDX != 0)
                {
                    Camera.LeftRightRotation -= ROTATIONSPEED * (mouseDX / 50);
                }
                if (mouseDY != 0)
                {
                    Camera.UpDownRotation -= ROTATIONSPEED * (mouseDY / 50);
                }
                Camera.LeftRightRotation -= GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Right.X / 20;
                Camera.UpDownRotation += GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Right.Y / 20;
                //Mouse.SetPosition(Game.GraphicsDevice.DisplayMode.Width / 2, Game.GraphicsDevice.DisplayMode.Height / 2);
                _mouseMoveState = new MouseState(Game.GraphicsDevice.DisplayMode.Width / 2,
                    Game.GraphicsDevice.DisplayMode.Height / 2,
                    0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                Mouse.SetPosition((int)_mouseMoveState.X, (int)_mouseMoveState.Y);
                _mouseState = Mouse.GetState();
            }
        }
    }
}
