using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
using TechCraftEngine.Network;

namespace TechCraft.States
{
    public class LoadingState : State
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Vector2 _progressTextPosition;
        private Vector2 _spinnerPosition;
        private float _spinnerRotation;

        private string _progressText;
        private bool _loaded = false;
        private bool _loading = false;

        private Texture2D _spinner;
        private Texture2D _loadingBackground;

        public LoadingState()
        {
            
        }

        public override void Initialize()
        {            
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void LoadContent()
        {
            _spriteFont = Game.Content.Load<SpriteFont>("Fonts\\console");

            _progressText = "LOADING";
            Vector2 textSize = _spriteFont.MeasureString(_progressText);
            _progressTextPosition = new Vector2((Game.GraphicsDevice.Viewport.Width / 2.0f) - (textSize.X / 2), 40);

            _spinner = Game.Content.Load<Texture2D>("Textures\\spinner");
            _spinnerPosition = new Vector2((Game.GraphicsDevice.Viewport.Width / 2.0f), 60);

            _loadingBackground = Game.Content.Load<Texture2D>("Textures\\loading_background");

            _game.GameClient = new GameClient(_game);
        }

        public override void Update(GameTime gameTime)
        {
            _progressText = Game.GameClient.StatusText;
            if (!_loading)
            {
                _loading = true;
                Thread loadThread = new Thread(new ThreadStart(DoLoad));
                loadThread.Start();
            }
            if (_loaded) StartGame();

            _spinnerRotation += 0.05f;
        }

        public override void ProcessInput(GameTime gameTime)
        {

        }

        private void DoLoad()
        {
            
            _game.GameClient.LoadMap();
            _loaded = true;
        }

        public void StartGame()
        {
            Game.StateManager.ActiveState = Game.StateManager.GetState(typeof(PlayingState));
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            Rectangle screenRect = new Rectangle(0, 0, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_loadingBackground, screenRect, Color.White);
            _spriteBatch.Draw(_spinner, _spinnerPosition, null, Color.White, _spinnerRotation, new Vector2(32, 32), 1f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, _progressText, _progressTextPosition, Color.Black);

            _spriteBatch.End();
        }
    }
}
