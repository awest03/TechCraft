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

namespace TechCraft.States
{
    public class TitleState : State
    {
#if XBOX
        private const string STARTTEXT = "Press the start button";
#else
        private const string STARTTEXT = "Press Space";
#endif

        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Vector2 _startTextPosition;       
        //public TitleState(TechCraftGame game)
        //    : base(game)
        //{
        //}

        public TitleState()
        {
        }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void LoadContent()
        {
            _spriteFont = Game.Content.Load<SpriteFont>("Fonts\\console");
            Vector2 textSize = _spriteFont.MeasureString(STARTTEXT);
            _startTextPosition = new Vector2((Game.GraphicsDevice.Viewport.Width / 2.0f) - (textSize.X / 2), 20);
        }
        
        public override void Update(GameTime gameTime)
        {
        }

        PlayerIndex _controllerIndex;
        public override void ProcessInput(GameTime gameTime)
        {
            if (Game.InputState.IsButtonPressed(Buttons.Start,null,out _controllerIndex) ||
                Game.InputState.IsKeyPressed(Keys.Space,null,out _controllerIndex)) {
                    Game.ActivePlayerIndex = _controllerIndex;
                    //Guide.BeginShowStorageDeviceSelector(new AsyncCallback(StorageDeviceSelected), null);

                    if (!Guide.IsVisible) {
                        StorageDevice.BeginShowSelector(new AsyncCallback(StorageDeviceSelected), null);
                    } 

                }
        }

        public void StorageDeviceSelected(IAsyncResult result)
        {
            _game.StorageDevice = StorageDevice.EndShowSelector(result); 

            //_game.StorageDevice = Guide.EndShowStorageDeviceSelector(result);
            StartGame();
        }

        public void StartGame()
        {
            Game.StateManager.ActiveState = Game.StateManager.GetState(typeof(LoadingState));
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, STARTTEXT, _startTextPosition, Color.White);
            _spriteBatch.End();
        }
    }
}
