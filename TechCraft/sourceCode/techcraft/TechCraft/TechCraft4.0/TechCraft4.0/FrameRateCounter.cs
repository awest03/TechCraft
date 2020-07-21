using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TechCraft
{
    /// <summary>
    /// Class used for performance testing of framerates.
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        String[] numbers;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Constructor initializes the numbers array for garbage free strings later.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public FrameRateCounter(Game game)
            : base(game)
        {
            content = game.Content;
            numbers = new String[10];
            for (int j = 0; j < 10; j++)
            {
                numbers[j] = j.ToString();
            }
        }

        /// <summary>
        /// Loads the spritebatch and font needed to draw the framerate to screen.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("Fonts/console");
        }

        /// <summary>
        /// The framerate is calculated in this method.  It actually calculates
        /// the update rate, but when fixed time step and syncronize with retrace 
        /// are turned off, it is the same value.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        /// <summary>
        /// Draws the framerate to screen with a shadow outline to make it easy
        /// to see in any game.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            //Framerates over 1000 aren't important as we have lots of room for features.
            if (frameRate >= 1000)
            {
                frameRate = 999;
            }

            //Break the framerate down to single digit components so we can use
            //the number lookup to draw them.
            int fps1 = frameRate / 100;
            int fps2 = (frameRate - fps1 * 100) / 10;
            int fps3 = frameRate - fps1 * 100 - fps2 * 10;

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, numbers[fps1], new Vector2(33, 33), Color.Black);
            spriteBatch.DrawString(spriteFont, numbers[fps1], new Vector2(32, 32), Color.White);

            spriteBatch.DrawString(spriteFont, numbers[fps2], new Vector2(33 + spriteFont.MeasureString(numbers[fps1]).X, 33), Color.Black);
            spriteBatch.DrawString(spriteFont, numbers[fps2], new Vector2(32 + spriteFont.MeasureString(numbers[fps1]).X, 32), Color.White);

            spriteBatch.DrawString(spriteFont, numbers[fps3], new Vector2(33 + spriteFont.MeasureString(numbers[fps1]).X + spriteFont.MeasureString(numbers[fps2]).X, 33), Color.Black);
            spriteBatch.DrawString(spriteFont, numbers[fps3], new Vector2(32 + spriteFont.MeasureString(numbers[fps1]).X + spriteFont.MeasureString(numbers[fps2]).X, 32), Color.White);

            spriteBatch.End();
        }
    }
}
