using SFML.Window;
using System;
using System.Numerics;

namespace SFCORE
{
    public interface IInput
    {
        event Action<TextEventArgs> TextEntered;
        bool IsControlDown { get; }
        bool IsShiftDown { get; }
        event Action<MouseMoveEventArgs> MouseMoved;
        event Action<MouseButtonEventArgs> MouseButtonDown;
        event Action<MouseButtonEventArgs> MouseButtonUp;
        event Action<MouseWheelScrollEventArgs> MouseWheelScrolled;
        void Consume();
        void Consume(params Keyboard.Key[] keys);
        void Release();
        event Action<KeyEventArgs> KeyPressed;
        bool IsKeyDown(Keyboard.Key key);
        Vector2 GetMousePos();
        bool WasKeyPressed(Keyboard.Key key);
        Vector2 GetMousePosAbsolute();
    }
}