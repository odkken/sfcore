using SFML.System;
using SFML.Window;
using System;
using System.Linq;
using System.Numerics;

namespace SFCORE
{
    public class BlockableInput : IInput
    {
        private readonly IInput _input;
        private bool _blocked;

        private readonly Func<bool> _isControlDown;
        public bool IsControlDown => _isControlDown();

        private readonly Func<bool> _isShiftDown;
        private Keyboard.Key[] _blockedKeys;
        public bool IsShiftDown => _isShiftDown();
        public event Action<MouseMoveEventArgs> MouseMoved;
        public event Action<MouseMoveEventArgs> BlockableMouseMoved;
        public event Action<MouseButtonEventArgs> MouseButtonDown;
        public event Action<MouseButtonEventArgs> BlockableMouseButtonDown;
        public event Action<MouseButtonEventArgs> MouseButtonUp;
        public event Action<MouseWheelScrollEventArgs> MouseWheelScrolled;
        public event Action<MouseWheelScrollEventArgs> BlockableMouseWheelScrolled;
        public event Action<MouseButtonEventArgs> BlockableMouseButtonUp;
        private event Action<KeyEventArgs> BlockableKeyPressed;
        public event Action<KeyEventArgs> KeyPressed;
        public bool IsKeyDown(Keyboard.Key key)
        {
            return !_blocked && _input.IsKeyDown(key);
        }

        public Vector2 GetMousePos()
        {
            return _input.GetMousePos();
        }

        public bool WasKeyPressed(Keyboard.Key key)
        {
            return _input.WasKeyPressed(key);
        }

        public Vector2 GetMousePosAbsolute()
        {
            return _input.GetMousePosAbsolute();
        }

        public event Action<TextEventArgs> TextEntered;
        public event Action<TextEventArgs> BlockableTextEntered;

        public BlockableInput(IInput input)
        {
            _input = input;
            input.TextEntered += OnTextEntered;
            input.KeyPressed += OnKeyPressed;
            input.MouseButtonDown += OnMouseDown;
            input.MouseButtonUp += OnMouseUp;
            input.MouseMoved += OnMouseMoved;
            input.MouseWheelScrolled += OnMouseScrolled;
            _isControlDown = () => input.IsControlDown;
            _isShiftDown = () => input.IsShiftDown;
        }

        private void OnMouseScrolled(MouseWheelScrollEventArgs args)
        {

            MouseWheelScrolled?.Invoke(args);
            if (!_blocked)
                BlockableMouseWheelScrolled?.Invoke(args);
        }

        private void OnMouseMoved(MouseMoveEventArgs args)
        {
            MouseMoved?.Invoke(args);
            if (!_blocked)
                BlockableMouseMoved?.Invoke(args);
        }

        public BlockableInput(BlockableInput blockableInput)
        {
            _input = blockableInput;
            blockableInput.BlockableTextEntered += OnTextEntered;
            blockableInput.BlockableKeyPressed += OnKeyPressed;
            blockableInput.BlockableMouseButtonDown += OnMouseDown;
            blockableInput.BlockableMouseButtonUp += OnMouseUp;
            blockableInput.BlockableMouseMoved += OnMouseMoved;
            blockableInput.BlockableMouseWheelScrolled += OnMouseScrolled;
            _isControlDown = () => blockableInput.BlockedIsControlDown;
            _isShiftDown = () => blockableInput.BlockedIsShiftDown;
        }

        public bool BlockedIsShiftDown => !_blocked && _isShiftDown();

        public bool BlockedIsControlDown => !_blocked && _isControlDown();

        private void OnTextEntered(TextEventArgs args)
        {
            var initialBlocked = _blocked;
            TextEntered?.Invoke(args);

            if (!_blocked && initialBlocked == _blocked)
                BlockableTextEntered?.Invoke(args);
        }

        private void OnKeyPressed(KeyEventArgs args)
        {
            var initialBlocked = _blocked;
            KeyPressed?.Invoke(args);

            var thisKeyAllowed = !_blocked || _blockedKeys != null && !_blockedKeys.Contains(args.Code);
            if (thisKeyAllowed && initialBlocked == _blocked)
                BlockableKeyPressed?.Invoke(args);
        }

        private void OnMouseDown(MouseButtonEventArgs args)
        {
            MouseButtonDown?.Invoke(args);
            if (!_blocked)
                BlockableMouseButtonDown?.Invoke(args);
        }

        private void OnMouseUp(MouseButtonEventArgs args)
        {
            MouseButtonUp?.Invoke(args);
            if (!_blocked)
                BlockableMouseButtonUp?.Invoke(args);
        }

        public void Consume()
        {
            _blocked = true;
            _blockedKeys = null;
        }

        public void Consume(params Keyboard.Key[] keys)
        {
            _blockedKeys = keys;
        }

        public void Release()
        {
            _blocked = false;
        }
    }
}