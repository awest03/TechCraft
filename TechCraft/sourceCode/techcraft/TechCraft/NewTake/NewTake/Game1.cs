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
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using fbDeprofiler;

using NewTake.model;
using NewTake.view;
using NewTake.controllers;
using NewTake.view.blocks;
using NewTake.profiling;
using NewTake.view.renderers;

#endregion

namespace NewTake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        private GraphicsDeviceManager graphics;
        private World world;
        private IRenderer renderer;

        private KeyboardState _oldKeyboardState;

        private bool releaseMouse = false;

        private int preferredBackBufferHeight, preferredBackBufferWidth;

        private HudRenderer hud;

        private Player player1; //wont add a player2 for some time, but naming like this helps designing  
        private PlayerRenderer player1Renderer;

        private DiagnosticWorldRenderer diagnosticWorldRenderer;
        bool _diagnosticMode = false;

        private SkyDomeRenderer skyDomeRenderer;

        public static bool throwExceptions = true;

        #endregion

        public Game1()
        {
            DeProfiler.Run();
            graphics = new GraphicsDeviceManager(this);

            preferredBackBufferHeight = graphics.PreferredBackBufferHeight;
            preferredBackBufferWidth = graphics.PreferredBackBufferWidth;

            //enter stealth mode at start
            //graphics.PreferredBackBufferHeight = 100;
            //graphics.PreferredBackBufferWidth = 160;

            FrameRateCounter frameRate = new FrameRateCounter(this);
            frameRate.DrawOrder = 1;
            Components.Add(frameRate);

            Content.RootDirectory = "Content";
            graphics.SynchronizeWithVerticalRetrace = true; // press f3 to set it to false at runtime 

            showDebugKeysHelp();
        }

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            world = new World();

            player1 = new Player(world);

            player1Renderer = new PlayerRenderer(GraphicsDevice, player1);
            player1Renderer.Initialize();

            hud = new HudRenderer(GraphicsDevice, world, player1Renderer.camera);
            hud.Initialize();

            #region choose renderer

            //renderer = new ThreadedWorldRenderer(GraphicsDevice, player1Renderer.camera, world);
            renderer = new SimpleRenderer(GraphicsDevice, player1Renderer.camera, world);
            
            diagnosticWorldRenderer = new DiagnosticWorldRenderer(GraphicsDevice, player1Renderer.camera, world);
            skyDomeRenderer = new SkyDomeRenderer(GraphicsDevice, player1Renderer.camera, world);
            renderer.Initialize();
            diagnosticWorldRenderer.Initialize();
            skyDomeRenderer.Initialize();
            #endregion

            //TODO refactor WorldRenderer needs player position + view frustum 

            base.Initialize();
        }
        #endregion

        protected override void OnExiting(Object sender, EventArgs args)
        {
            renderer.Stop();
            base.OnExiting(sender, args);
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            renderer.LoadContent(Content);
            diagnosticWorldRenderer.LoadContent(Content);
            skyDomeRenderer.LoadContent(Content);
            player1Renderer.LoadContent(Content);
            hud.LoadContent(Content);
        }
        #endregion

        #region UnloadContent
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region DebugKeys

        private void showDebugKeysHelp()
        {
            Console.WriteLine("Debug keys");
            Console.WriteLine("F1  = toggle freelook(fly) / player physics");
            Console.WriteLine("F3  = toggle vsync + fixedtimestep updates ");
            Console.WriteLine("F4  = toggle 100*160 window size");
            Console.WriteLine("F7  = toggle wireframe");
            Console.WriteLine("F8  = toggle chunk diagnostics");
            Console.WriteLine("F9  = toggle day cycle / day mode");
            Console.WriteLine("F10 = toggle day cycle / night mode");
            Console.WriteLine("F11 = toggle fullscreen");
            Console.WriteLine("F   = release / regain focus");
            Console.WriteLine("Esc = exit");
        }

        private void ProcessDebugKeys()
        {
            KeyboardState keyState = Keyboard.GetState();

            //toggle fullscreen
            if (_oldKeyboardState.IsKeyUp(Keys.F11) && keyState.IsKeyDown(Keys.F11))
            {
                graphics.ToggleFullScreen();
            }

            //freelook mode
            if (_oldKeyboardState.IsKeyUp(Keys.F1) && keyState.IsKeyDown(Keys.F1))
            {
                player1Renderer.freeCam = !player1Renderer.freeCam;
            }

            //minimap mode
            if (_oldKeyboardState.IsKeyUp(Keys.M) && keyState.IsKeyDown(Keys.M))
            {
                hud.showMinimap = !hud.showMinimap;
            }

            //wireframe mode
            if (_oldKeyboardState.IsKeyUp(Keys.F7) && keyState.IsKeyDown(Keys.F7))
            {
                world.ToggleRasterMode();
            }

            //diagnose mode
            if (_oldKeyboardState.IsKeyUp(Keys.F8) && keyState.IsKeyDown(Keys.F8))
            {
                _diagnosticMode = !_diagnosticMode;
            }

            //day cycle/dayMode
            if (_oldKeyboardState.IsKeyUp(Keys.F9) && keyState.IsKeyDown(Keys.F9))
            {
                world.dayMode = !world.dayMode;
                //Debug.WriteLine("Day Mode is " + world.dayMode);
            }

            //day cycle/nightMode
            if (_oldKeyboardState.IsKeyUp(Keys.F10) && keyState.IsKeyDown(Keys.F10))
            {
                world.nightMode = !world.nightMode;
                //Debug.WriteLine("Day/Night Mode is " + world.nightMode);
            }

            // Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Release the mouse pointer
            if (_oldKeyboardState.IsKeyUp(Keys.F) && keyState.IsKeyDown(Keys.F))
            {
                this.releaseMouse = !this.releaseMouse;
                this.IsMouseVisible = !this.IsMouseVisible;
            }

            // fixed time step
            if (_oldKeyboardState.IsKeyUp(Keys.F3) && keyState.IsKeyDown(Keys.F3))
            {
                graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                this.IsFixedTimeStep = !this.IsFixedTimeStep;
                Debug.WriteLine("FixedTimeStep and vsync are " + this.IsFixedTimeStep);
                graphics.ApplyChanges();
            }

            // stealth mode / keep screen space for profilers
            if (_oldKeyboardState.IsKeyUp(Keys.F4) && keyState.IsKeyDown(Keys.F4))
            {
                if (graphics.PreferredBackBufferHeight == preferredBackBufferHeight)
                {
                    graphics.PreferredBackBufferHeight = 100;
                    graphics.PreferredBackBufferWidth = 160;
                }
                else
                {
                    graphics.PreferredBackBufferHeight = preferredBackBufferHeight;
                    graphics.PreferredBackBufferWidth = preferredBackBufferWidth;
                }
                graphics.ApplyChanges();
            }

            this._oldKeyboardState = keyState;
        }
        #endregion

        #region UpdateTOD
        public virtual Vector3 UpdateTOD(GameTime gameTime)
        {
            long div = 20000;

            if (!world.RealTime)
                world.tod += ((float)gameTime.ElapsedGameTime.Milliseconds / div);
            else
                world.tod = ((float)DateTime.Now.Hour) + ((float)DateTime.Now.Minute) / 60 + (((float)DateTime.Now.Second) / 60) / 60;

            if (world.tod >= 24)
                world.tod = 0;

            if (world.dayMode)
            {
                world.tod = 12;
                world.nightMode = false;
            }
            else if (world.nightMode)
            {
                world.tod = 0;
                world.dayMode = false;
            }

            // Calculate the position of the sun based on the time of day.
            float x = 0;
            float y = 0;
            float z = 0;

            if (world.tod <= 12)
            {
                y = world.tod / 12;
                x = 12 - world.tod;
            }
            else
            {
                y = (24 - world.tod) / 12;
                x = 12 - world.tod;
            }

            x /= 10;

            world.SunPos = new Vector3(-x, y, z);

            return world.SunPos;
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ProcessDebugKeys();

            if (this.IsActive)
            {
                if (!releaseMouse)
                {
                    player1Renderer.Update(gameTime);
                }

                skyDomeRenderer.Update(gameTime);
                renderer.Update(gameTime);
                if (_diagnosticMode)
                {
                    diagnosticWorldRenderer.Update(gameTime);
                }
                base.Update(gameTime);
            }
            UpdateTOD(gameTime);
        }
        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            skyDomeRenderer.Draw(gameTime);
            renderer.Draw(gameTime);
            if (_diagnosticMode)
            {
                diagnosticWorldRenderer.Draw(gameTime);
            }
            player1Renderer.Draw(gameTime);
            hud.Draw(gameTime);
            base.Draw(gameTime);
        }
        #endregion

    }
}
