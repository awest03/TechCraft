using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using TechCraftEngine;

namespace TechCraftEngine.Common
{
    public class InputState
    {
        private const int MAX_INPUTS = 4;

        private KeyboardState[] _currentKeyboardStates;
        private KeyboardState[] _previousKeyboardStates;

        private GamePadState[] _currentGamepadStates;
        private GamePadState[] _previousGamepadStates;

        private bool[] _gamepadConnected;

        public InputState()
        {
            _currentKeyboardStates = new KeyboardState[MAX_INPUTS];
            _previousKeyboardStates = new KeyboardState[MAX_INPUTS];
            _currentGamepadStates = new GamePadState[MAX_INPUTS];
            _previousGamepadStates = new GamePadState[MAX_INPUTS];
            _gamepadConnected = new bool[MAX_INPUTS];
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < MAX_INPUTS; i++)
            {   
                _previousKeyboardStates[i] = _currentKeyboardStates[i];
                _previousGamepadStates[i] = _currentGamepadStates[i];

                _currentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                _currentGamepadStates[i] = GamePad.GetState((PlayerIndex)i);
                if (_currentGamepadStates[i].IsConnected)
                {
                    _gamepadConnected[i] = true;
                }
            }
        }
        
        public bool IsKeyPressed(Keys key, PlayerIndex? controlIndex, out PlayerIndex activeIndex) {
            if (controlIndex.HasValue)
            {
                activeIndex = controlIndex.Value;
                int i = (int)controlIndex;
                return (_currentKeyboardStates[i].IsKeyDown(key) && _previousKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                return (IsKeyPressed(key, PlayerIndex.One, out activeIndex) ||
                    IsKeyPressed(key, PlayerIndex.Two, out activeIndex) ||
                    IsKeyPressed(key, PlayerIndex.Three, out activeIndex) ||
                    IsKeyPressed(key, PlayerIndex.Four, out activeIndex));
            }
        }

        public bool IsKeyDown(Keys key, PlayerIndex? controlIndex, out PlayerIndex activeIndex)
        {
            if (controlIndex.HasValue)
            {
                activeIndex = controlIndex.Value;
                int i = (int)controlIndex;
                return _currentKeyboardStates[i].IsKeyDown(key);
            }
            else
            {
                return (IsKeyDown(key, PlayerIndex.One, out activeIndex) ||
                    IsKeyDown(key, PlayerIndex.Two, out activeIndex) ||
                    IsKeyDown(key, PlayerIndex.Three, out activeIndex) ||
                    IsKeyDown(key, PlayerIndex.Four, out activeIndex));
            }
        }

        public bool IsButtonPressed(Buttons button, PlayerIndex? controlIndex, out PlayerIndex activeIndex)
        {
            if (controlIndex.HasValue)
            {
                activeIndex = controlIndex.Value;
                int i = (int)controlIndex;
                return (_currentGamepadStates[i].IsButtonDown(button) && _previousGamepadStates[i].IsButtonUp(button));
            }
            else
            {
                return (IsButtonPressed(button, PlayerIndex.One, out activeIndex) ||
                    IsButtonPressed(button, PlayerIndex.Two, out activeIndex) ||
                    IsButtonPressed(button, PlayerIndex.Three, out activeIndex) ||
                    IsButtonPressed(button, PlayerIndex.Four, out activeIndex));
            }
        }

        public bool IsButtonDown(Buttons button, PlayerIndex? controlIndex, out PlayerIndex activeIndex)
        {
            if (controlIndex.HasValue)
            {
                activeIndex = controlIndex.Value;
                int i = (int)controlIndex;
                return _currentGamepadStates[i].IsButtonDown(button);
            }
            else
            {
                return (IsButtonDown(button, PlayerIndex.One, out activeIndex) ||
                    IsButtonDown(button, PlayerIndex.Two, out activeIndex) ||
                    IsButtonDown(button, PlayerIndex.Three, out activeIndex) ||
                    IsButtonDown(button, PlayerIndex.Four, out activeIndex));
            }
        }
    }
}
