namespace MiniEngine
{
    public enum MouseButton : byte
    {
        Left = 0,
        Right = 1,
        Middle = 2,
        M1 = 0,
        M2 = 1,
        M3 = 2,
        M4 = 3,
        M5 = 4,
    }

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

    public static class InputManager
    {
        private static MouseState _previousMouseState = new();
        private static MouseState _mouseState = new();
        public static MouseState MouseState => _mouseState;

        public static void UpdateState()
        {
            UpdateMouseState();
        }

        public static bool IsMouseButtonPressed(MouseButton mouseButton)
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

        public static bool IsMouseButtonReleased(MouseButton mouseButton)
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


        public static bool IsMouseButtonDown(MouseButton mouseButton)
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

        public static bool IsMouseButtonUp(MouseButton mouseButton)
            => !IsMouseButtonDown(mouseButton);

        public static Vector2 GetMousePosition()
            => (_mouseState.X, _mouseState.Y);

        private static bool MaskCheck(uint mask, uint value)
            => (mask & value) != 0;

        private static void CopyPreviousMouseState()
        {
            _previousMouseState.X = _mouseState.X;
            _previousMouseState.Y = _mouseState.Y;
            _previousMouseState.MouseButton1 = _mouseState.MouseButton1;
            _previousMouseState.MouseButton2 = _mouseState.MouseButton2;
            _previousMouseState.MouseButton3 = _mouseState.MouseButton3;
            _previousMouseState.MouseButton4 = _mouseState.MouseButton4;
            _previousMouseState.MouseButton5 = _mouseState.MouseButton5;
        }

        private static void UpdateMouseState()
        {
            //CopyPreviousMouseState();
            //var buttonBitMask = SDL_GetMouseState(out var mouseX, out var mouseY);
            //_mouseState.X = mouseX;
            //_mouseState.Y = mouseY;
            //_mouseState.MouseButton1 = MaskCheck(SDL_BUTTON_LMASK, buttonBitMask);
            //_mouseState.MouseButton2 = MaskCheck(SDL_BUTTON_RMASK, buttonBitMask);
            //_mouseState.MouseButton3 = MaskCheck(SDL_BUTTON_MMASK, buttonBitMask);
            //_mouseState.MouseButton4 = MaskCheck(SDL_BUTTON_X1MASK, buttonBitMask);
            //_mouseState.MouseButton5 = MaskCheck(SDL_BUTTON_X2MASK, buttonBitMask);
        }
    }
}
