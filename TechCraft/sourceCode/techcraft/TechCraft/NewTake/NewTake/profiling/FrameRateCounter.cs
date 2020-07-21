#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NewTake.profiling
{
    /// <summary>
    /// Class used for performance testing of framerates.
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {

        #region Fields
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        String[] numbers;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        #endregion

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
            spriteFont = content.Load<SpriteFont>("Fonts/OSDDisplay");
        }

        #region Update
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
        #endregion

        #region Draw
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
        #endregion

    }
}
