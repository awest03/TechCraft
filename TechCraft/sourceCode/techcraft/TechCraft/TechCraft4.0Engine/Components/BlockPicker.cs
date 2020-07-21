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

using TechCraftEngine.Common;
using TechCraftEngine.WorldEngine;
using System.Diagnostics;

namespace TechCraftEngine.Components
{
    public class BlockPicker : TechEngineComponent
    {
        private Texture2D _tileSelectorTexture;
        private Texture2D _tileStripTexture;
        private BlockType _blockType = BlockType.Brick;
        private SpriteBatch _spiteBatch;
        private float _drawX;
        private float _drawY;
        private MouseState _previousMouseState;

        public BlockPicker(TechCraftGame game, SpriteBatch spriteBatch)
            : base(game)
        {
            _spiteBatch = spriteBatch;
        }

        public BlockType SelectedBlockType
        {
            get { return _blockType; }
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            _tileSelectorTexture = Game.Content.Load<Texture2D>("Textures\\tileSelector");
            _tileStripTexture = Game.Content.Load<Texture2D>("Textures\\tileStrip");

            _drawX = (Game.GraphicsDevice.Viewport.Width / 2) - (_tileStripTexture.Width / 2);
            _drawY = Game.GraphicsDevice.Viewport.Height - 70;
        }

        public override void Update(GameTime gameTime)
        {
            PlayerIndex controlIndex;
            MouseState mouseState = Mouse.GetState();
            int scrollWheelDelta = _previousMouseState.ScrollWheelValue - mouseState.ScrollWheelValue;
            //Debug.WriteLine(scrollWheelDelta);
            if (Game.InputState.IsKeyPressed(Keys.Z, Game.ActivePlayerIndex, out controlIndex) ||
                Game.InputState.IsButtonPressed(Buttons.DPadLeft, Game.ActivePlayerIndex, out controlIndex)
                || scrollWheelDelta >= 120
                )
            {
                _blockType--;
                if (_blockType == BlockType.None) _blockType = BlockType.Water;
            }
            if (Game.InputState.IsKeyPressed(Keys.C, Game.ActivePlayerIndex, out controlIndex) ||
                Game.InputState.IsButtonPressed(Buttons.DPadRight, Game.ActivePlayerIndex, out controlIndex)
                || scrollWheelDelta <= -120
                )
            {
                _blockType++;
                if (_blockType == BlockType.MAXIMUM) _blockType = BlockType.Brick;
            }
            _previousMouseState = mouseState;
        }

        public override void Draw(GameTime gameTime)
        {
            _spiteBatch.Draw(_tileStripTexture, new Vector2(_drawX, _drawY), Color.White);
            float selectorPos = ((int) _blockType - 1) * 32;
            _spiteBatch.Draw(_tileSelectorTexture, new Vector2(_drawX + selectorPos, _drawY), Color.White);
        }
    }
}
