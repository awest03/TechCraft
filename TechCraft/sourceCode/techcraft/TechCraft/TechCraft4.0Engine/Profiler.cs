using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using Microsoft.Xna.Framework.Input;

namespace TechCraftEngine
{
    /// <summary>
    /// A simple comparative profiler.  The profiler will track the duration of
    /// named events in ticks and display them as bars.  Play with the 
    /// adjustment constant until the bars are big enough to see which is 
    /// largest without having them go offscreen.
    /// </summary>
    public class Profiler : DrawableGameComponent
    {   
        private const int X_START = 340;
        private const int Y_START = 120;
        private const float ADJUSTMENT = .001f;

        public static Profiler profiler;

        Dictionary<String, Stopwatch> timers;
        SpriteBatch spriteBatch;
        Texture2D blank;
        SpriteFont font;
    
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game"></param>
        public Profiler(TechCraftGame game)
            : base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            blank = game.Content.Load<Texture2D>("Textures\\blank");
            font = game.Content.Load<SpriteFont>("Fonts\\console");
            timers = new Dictionary<String, Stopwatch>();                        
        }

        /// <summary>
        /// Starts a timer.  If it doesn't exist, then the timer is created.
        /// The timers continue from their last point.  Timers are only reset
        /// during the draw phase or if explicitly reset with a call to
        /// Reset().  Every call to Start() must be accompanied by an 
        /// appropriate End() for accurate results.
        /// </summary>
        /// <param name="timerID">The name of the timer.  The name of the timer
        /// is what will be displayed next to the bar, when it is drawn to the
        /// screen.</param>
        public void Start(String timerID)
        {
            if (!timers.ContainsKey(timerID))
            {
                timers.Add(timerID, new Stopwatch());
            }
            timers[timerID].Start();
        }

        /// <summary>
        /// Resets a timer.  Can be used if a timer needs to be reset before
        /// the end the frame.
        /// </summary>
        /// <param name="timerID">The name of the timer.</param>
        public void Reset(String timerID)
        {
            timers[timerID].Reset();
        }

        /// <summary>
        /// Stops a timer.  You have to ensure the timer ID is identical to a previously
        /// called Start().
        /// </summary>
        /// <param name="timerID">The name of the timer.</param>
        public void Stop(String timerID)
        {
            timers[timerID].Stop();
        }

        /// <summary>
        /// Draws the timers in bar form.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            TechCraftGame techCraftGame = (TechCraftGame)Game;
           
            if (techCraftGame.ShowDebugInfo)
            {

                int currentY = Y_START;
                spriteBatch.Begin();
                foreach (String timerId in timers.Keys)
                {
                    spriteBatch.DrawString(font, timerId, new Vector2(X_START - 5 - font.MeasureString(timerId).X, currentY - 5), Color.Yellow);
                    spriteBatch.Draw(blank, new Rectangle(X_START, currentY, (int)(timers[timerId].ElapsedTicks * ADJUSTMENT), 18), Color.Yellow);
                    timers[timerId].Reset();
                    currentY += 20;
                }
                spriteBatch.End();
            }
        }
    }
}
