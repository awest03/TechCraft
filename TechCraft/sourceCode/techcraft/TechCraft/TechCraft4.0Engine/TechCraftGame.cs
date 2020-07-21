using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using TechCraftEngine.Cameras;
using TechCraftEngine.Common;
using TechCraftEngine.Managers;
using TechCraftEngine.Network;

using System.Threading;

namespace TechCraftEngine
{
    public class TechCraftGame : Game
    {
        private StateManager _stateManager;
        private InputState _inputState;
        private Camera _camera;
        private GraphicsDeviceManager _graphics;
        private PlayerIndex _activePlayerIndex;

        private StorageDevice _storageDevice;
        private GamerServicesComponent _gamerServices;
        private GameClient _gameClient;

        private List<Thread> _threads;

        public bool ShowDebugInfo { get; set; }

        public TechCraftGame()
            : base()
        {
            ShowDebugInfo = false;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            //   _graphics.PreferredBackBufferWidth = 160;
            // _graphics.PreferredBackBufferHeight = 100;
            _graphics.PreparingDeviceSettings += PrepareDeviceSettings;

            //those two will be changed when pressing F3 for debug / profiling
            _graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = true;

            _graphics.PreferMultiSampling = false;

            _gamerServices = new GamerServicesComponent(this);
            this.Components.Add(_gamerServices);

            Content.RootDirectory = "Content";

            _threads = new List<Thread>();
            _stateManager = new StateManager(this);
            _inputState = new InputState();
        }

        public GameClient GameClient
        {
            get { return _gameClient; }
            set { _gameClient = value; }
        }

        private void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PlatformContents;
        }

        public StorageDevice StorageDevice
        {
            get { return _storageDevice; }
            set { _storageDevice = value; }
        }

        public PlayerIndex ActivePlayerIndex
        {
            get { return _activePlayerIndex; }
            set { _activePlayerIndex = value; }
        }

        public GraphicsDeviceManager Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        public StateManager StateManager
        {
            get { return _stateManager; }
        }

        public InputState InputState
        {
            get { return _inputState; }
        }

        public Camera Camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        public List<Thread> Threads
        {
            get { return _threads; }
        }

        protected override void Initialize()
        {
            _stateManager.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _stateManager.LoadContent();
            base.LoadContent();
            //Initialize and add the profiler to the component list.
            Profiler.profiler = new Profiler(this);
            Components.Add(Profiler.profiler);
        }


        protected override void Update(GameTime gameTime)
        {

            Profiler.profiler.Start("Update");

            PlayerIndex controlIndex;
            if (_inputState.IsKeyPressed(Keys.Escape, null, out controlIndex) ||
                _inputState.IsButtonPressed(Buttons.Back, null, out controlIndex))
            {
                foreach (Thread thread in _threads)
                {
                    thread.Abort();
                }
                Exit();
            }

            if (_inputState.IsKeyPressed(Keys.F3, null, out controlIndex))
            {
                ShowDebugInfo = !ShowDebugInfo;
            }

            if (ShowDebugInfo && _graphics.SynchronizeWithVerticalRetrace)
            {
                // we do not want to applychanges each Update, so we check current SynchronizeWithVerticalRetrace value
                _graphics.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
                _graphics.ApplyChanges();
            }


            _inputState.Update(gameTime);
            _stateManager.ProcessInput(gameTime);
            _stateManager.Update(gameTime);
            base.Update(gameTime);

            Profiler.profiler.Stop("Update");
        }

        protected override void Draw(GameTime gameTime)
        {
            Profiler.profiler.Start("Draw");
            _stateManager.Draw(gameTime);
            base.Draw(gameTime);
            Profiler.profiler.Stop("Draw");
        }
    }
}
