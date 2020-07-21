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

namespace TechCraftEngine.Managers
{
    public class StateManager : Manager
    {
        private Dictionary<Type, State> _stateCache;
        private State _activeState;

        public StateManager(TechCraftGame game)
            : base(game)
        {          
        }

        public State ActiveState
        {
            get { return _activeState; }
            set { _activeState = value; }
        }

        public override void Initialize()
        {
            _stateCache = new Dictionary<Type, State>();
        }

        public State GetState(Type type)
        {
            if (_stateCache.ContainsKey(type))
            {
                return _stateCache[type];
            }
            else
            {
                //State state = (State) Activator.CreateInstance(type,Game);
                State state = (State)Activator.CreateInstance(type);
                state.Game = Game;
                state.Initialize();
                state.LoadContent();
                _stateCache[type] = state;
                return state;
            }
        }

        public override void LoadContent()
        {
        }

        public void ProcessInput(GameTime gameTime)
        {
            if (_activeState != null)
            {
                _activeState.ProcessInput(gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_activeState != null)
            {
                _activeState.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (_activeState != null)
            {
                _activeState.Draw(gameTime);
            }
        }
    }
}
