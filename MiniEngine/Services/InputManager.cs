using System.Collections.Generic;
using MiniEngine.Utility;
using MiniEngine.Windowing;
using static SDL2.SDL;

namespace MiniEngine
{
    public struct MouseState
    {
        public int X = -1;
        public int Y = -1;
        public bool MouseButton1 = false;
        public bool MouseButton2 = false;
        public bool MouseButton3 = false;
        public bool MouseButton4 = false;
        public bool MouseButton5 = false;

        public MouseState() { }
    }

    public class InputManager(GameEngine gameEngine, SceneManager sceneManager)
    {
        private MouseState _previousMouseState = new();
        private MouseState _mouseState = new();
        private Vector2 _windowMousePosition;
        private Vector2F _sceneMousePosition;
        private readonly Dictionary<Key, bool> _keyStatesSnapshot = [];
        private readonly Dictionary<Key, bool> _keyStates = [];
        public MouseState MouseState => _mouseState;
        public Vector2 WindowMousePosition => _windowMousePosition;
        public Vector2F SceneMousePosition => _sceneMousePosition;

        private GameWindow? _gameWindow => gameEngine.WindowManager.GameWindow;

        private void AutoCorrectVector(ref Vector2 vector)
        {
            if (gameEngine.Setup.InvertYAxis != true)
                return;
            vector.Y = _gameWindow?.WindowSize.Height ?? 0 - vector.Y;
        }

        private void AutoCorrectVector(ref Vector2F vector)
        {
            if (gameEngine.Setup.InvertYAxis != true)
                return;
            vector.Y = _gameWindow?.WindowSize.Height ?? 0f - vector.Y;
        }

        internal void UpdateState()
        {
            UpdateMouseState();
            UpdateKeyStatesSnapshot();
        }

        internal void UpdateKeyState(Key key, bool pressed)
            => _keyStates[key] = pressed;

        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.M1 => !_previousMouseState.MouseButton1 && _mouseState.MouseButton1,
                MouseButton.M2 => !_previousMouseState.MouseButton2 && _mouseState.MouseButton2,
                MouseButton.M3 => !_previousMouseState.MouseButton3 && _mouseState.MouseButton3,
                MouseButton.M4 => !_previousMouseState.MouseButton4 && _mouseState.MouseButton4,
                MouseButton.M5 => !_previousMouseState.MouseButton5 && _mouseState.MouseButton5,
                _ => false
            };
        }

        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.M1 => _previousMouseState.MouseButton1 && !_mouseState.MouseButton1,
                MouseButton.M2 => _previousMouseState.MouseButton2 && !_mouseState.MouseButton2,
                MouseButton.M3 => _previousMouseState.MouseButton3 && !_mouseState.MouseButton3,
                MouseButton.M4 => _previousMouseState.MouseButton4 && !_mouseState.MouseButton4,
                MouseButton.M5 => _previousMouseState.MouseButton5 && !_mouseState.MouseButton5,
                _ => false
            };
        }

        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.M1 => _mouseState.MouseButton1,
                MouseButton.M2 => _mouseState.MouseButton2,
                MouseButton.M3 => _mouseState.MouseButton3,
                MouseButton.M4 => _mouseState.MouseButton4,
                MouseButton.M5 => _mouseState.MouseButton5,
                _ => false
            };
        }

        public bool IsMouseButtonUp(MouseButton mouseButton)
            => !IsMouseButtonDown(mouseButton);

        public bool IsKeyPressed(Key key)
        {
            _keyStatesSnapshot.TryGetValue(key, out var previousKeyState);
            _keyStates.TryGetValue(key, out var currentKeyState);
            return !previousKeyState && currentKeyState;
        }

        public bool IsKeyReleased(Key key)
        {
            _keyStatesSnapshot.TryGetValue(key, out var previousKeyState);
            _keyStates.TryGetValue(key, out var currentKeyState);
            return previousKeyState && !currentKeyState;
        }

        public bool IsKeyDown(Key key)
        {
            _keyStates.TryGetValue(key, out var currentKeyState);
            return currentKeyState;
        }

        public bool IsKeyUp(Key key)
            => !IsKeyDown(key);

        public Vector2 GetMousePosition()
            => (_mouseState.X, _mouseState.Y);

        public IEnumerable<Key> DetectPresses()
        {
            var diffList = new List<Key>();
            foreach (var (key, state) in _keyStates)
            {
                _keyStatesSnapshot.TryGetValue(key, out var previousState);
                if (state && !previousState)
                    diffList.Add(key);
            }

            return diffList;
        }

        private bool CheckMask(uint mask, uint value)
            => (mask & value) != 0;

        private void CopyPreviousMouseState()
        {
            _previousMouseState.X = _mouseState.X;
            _previousMouseState.Y = _mouseState.Y;
            _previousMouseState.MouseButton1 = _mouseState.MouseButton1;
            _previousMouseState.MouseButton2 = _mouseState.MouseButton2;
            _previousMouseState.MouseButton3 = _mouseState.MouseButton3;
            _previousMouseState.MouseButton4 = _mouseState.MouseButton4;
            _previousMouseState.MouseButton5 = _mouseState.MouseButton5;
        }

        private void UpdateMouseState()
        {
            CopyPreviousMouseState();
            var buttonBitMask = SDL_GetMouseState(out var mouseX, out var mouseY);

            _windowMousePosition.X = mouseX;
            _windowMousePosition.Y = gameEngine.Setup.InvertYAxis == true ? gameEngine.Setup.WindowSize!.Value.Height - mouseY : mouseY;
            _mouseState.X = _windowMousePosition.X;
            _mouseState.Y = _windowMousePosition.Y;
            var scenePosition = sceneManager.CurrentScene?.ViewPosition ?? (0, 0);
            var sceneRotation = sceneManager.CurrentScene?.ViewRotation ?? 0f;
            var centerOfWindow = (_gameWindow!.WindowSize.Width / 2,
                _gameWindow!.WindowSize.Height / 2);
            var centerToMouseAngle = Formulas.AngleBetween(centerOfWindow, _windowMousePosition);
            var centerToMouseDistance = Formulas.DistanceFrom(centerOfWindow, _windowMousePosition);

            _sceneMousePosition = scenePosition + Vector2F.From(centerToMouseAngle - sceneRotation, centerToMouseDistance);

            _mouseState.MouseButton1 = CheckMask(SDL_BUTTON_LMASK, buttonBitMask);
            _mouseState.MouseButton2 = CheckMask(SDL_BUTTON_RMASK, buttonBitMask);
            _mouseState.MouseButton3 = CheckMask(SDL_BUTTON_MMASK, buttonBitMask);
            _mouseState.MouseButton4 = CheckMask(SDL_BUTTON_X1MASK, buttonBitMask);
            _mouseState.MouseButton5 = CheckMask(SDL_BUTTON_X2MASK, buttonBitMask);
        }

        private void UpdateKeyStatesSnapshot()
        {
            _keyStatesSnapshot.Clear();
            foreach (var (key, state) in  _keyStates)
                _keyStatesSnapshot.Add(key, state);
        }
    }
}
