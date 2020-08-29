using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SFCORE.Terminal;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SFCORE.Terminal
{
    public enum OpenState
    {
        Closed,
        Open
    }

    public class Terminal : Drawable
    {
        private static readonly uint CharacterSize = 16;
        private readonly float _closedYPos;
        private readonly RectangleShape _inputBackground;

        private readonly Color _inputBackgroundColor = new Color(30, 30, 30, 220);

        private readonly float _openingRate = 7f;

        private readonly RectangleShape _reportBackground;
        private readonly Color _reportBackgroundColor = new Color(10, 10, 10, 250);
        private readonly List<string> _inputHistory = new List<string>();
        private float _currentOpenness;
        private static readonly Color InputTextColor = new Color(155, 255, 255);
        private static readonly Color ResponseTextColor = new Color(255, 155, 255);
        private float _lastInputTime;
        private OpenState _state = OpenState.Closed;
        private float _targetOpenness;
        private readonly IAutoCompleter _completer;

        public float XOffset = CharacterSize * .2f;
        private readonly ICursorizedText _inputText;

        public void SetHighlightColor(Color color)
        {
            _inputText.SetHighlightColor(color);
        }

        private readonly IWrappedTextRenderer _reportText;
        public Terminal(RenderWindow window, Font font, IInput input, Func<ICommandRunner> getCommandRunner, Func<string, List<string>> getSuggestions)
        {
            _closedYPos = window.Size.Y / 2f;
            _reportBackground = new RectangleShape(new Vector2f(window.Size.X, window.Size.Y / 2f))
            {
                FillColor = _reportBackgroundColor,

            };
            _inputBackground =
                new RectangleShape(new Vector2f(window.Size.X, font.GetLineSpacing(CharacterSize) * 1.1f))
                {
                    FillColor = _inputBackgroundColor
                };

            _reportText = new WrappedTextRenderer(() => _reportBackground.GetGlobalBounds(), font, CharacterSize, new Dictionary<Tag, Color>
            {
                { Tag.Input, InputTextColor},
                { Tag.Response, ResponseTextColor},
                { Tag.Error, new Color(255, 100, 100)},
                { Tag.Warning, new Color(255,155,55)},
                { Tag.Debug, Color.Yellow},
                { Tag.SuperLowDebug, Color.Green}
            });
            _inputHistory = new List<string>();

            _inputText = new CursorizedText(new Text("", font, CharacterSize) { Color = InputTextColor },
                _inputBackground.GetGlobalBounds, CharacterSize,
                () =>
                {
                    var fraction = MathF.Sin(3 * (Core.TimeInfo.CurrentTime - _lastInputTime));
                    fraction *= fraction;
                    return Color.White.Lerp(new Color(255, 255, 255, 0), fraction);
                });

            _completer = new AutoCompleter(getSuggestions, () => _inputBackground.Position + new Vector2f(5, CharacterSize * 1.5f), font, CharacterSize);

            _inputText.OnTextChanged += str => _completer.UpdateInputString(str);
            var inputHistoryIndex = 0;
            var toggled = false;
            input.TextEntered += args =>
            {
                if (args.Unicode == "`")
                    switch (_state)
                    {
                        case OpenState.Closed:
                            _state = OpenState.Open;
                            input.Consume();
                            toggled = true;
                            break;
                        case OpenState.Open:
                            _state = OpenState.Closed;
                            input.Release();
                            toggled = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                if (_state == OpenState.Open && !input.IsControlDown)
                {
                    _lastInputTime = Core.TimeInfo.CurrentTime;
                    if (!toggled)
                        switch (args.Unicode)
                        {
                            case "\b":
                                {
                                    _inputText.Backspace();
                                }
                                break;
                            case "\r":
                                {
                                    if (false)//_completer.IsActive)
                                    {
                                        _inputText.SetString(_completer.ChooseSelectedItem());
                                        _completer.Escape();
                                    }
                                    else
                                    {
                                        var inputString = _inputText.ToString();
                                        if (string.IsNullOrWhiteSpace(inputString))
                                            break;
                                        _inputText.SetString("");
                                        _inputHistory.Add(inputString);
                                        inputHistoryIndex = _inputHistory.Count;
                                        _reportText.AddLine(inputString, Tag.Input);
                                        getCommandRunner().RunCommand(inputString)
                                            .ForEach(a => _reportText.AddLine(a, Tag.Response));
                                    }
                                }
                                break;
                            case @"\u001b":
                                break;
                            default:
                                var text = Regex.Replace(args.Unicode, @"[^\u0020-\u007E]", string.Empty);
                                if (string.IsNullOrEmpty(text))
                                    break;
                                _inputText.AddString(text);
                                break;
                        }
                    else toggled = false;
                }
                if (toggled)
                    UpdateOpenness();
            };
            input.KeyPressed += args =>
            {
                if (_state != OpenState.Open) return;

                _lastInputTime = Core.TimeInfo.CurrentTime;
                switch (args.Code)
                {
                    case Keyboard.Key.Up:
                        if (_completer.IsActive)
                            _completer.IncrementSelection();
                        else if (_inputHistory.Any())
                        {
                            inputHistoryIndex--;
                            if (inputHistoryIndex < 0)
                                inputHistoryIndex = 0;
                            _inputText.SetString(_inputHistory[inputHistoryIndex]);
                        }
                        break;
                    case Keyboard.Key.Down:
                        if (_completer.IsActive)
                            _completer.DecrementSelection();
                        else if (_inputHistory.Any())
                        {
                            inputHistoryIndex++;
                            if (inputHistoryIndex >= _inputHistory.Count)
                                inputHistoryIndex = _inputHistory.Count - 1;
                            _inputText.SetString(_inputHistory[inputHistoryIndex]);
                        }
                        break;
                    case Keyboard.Key.Left:
                        _inputText.RecedeCursor(args.Control, args.Shift);
                        break;
                    case Keyboard.Key.Right:
                        _inputText.AdvanceCursor(args.Control, args.Shift);
                        break;
                    case Keyboard.Key.Delete:
                        _inputText.Delete();
                        break;
                    case Keyboard.Key.Home:
                        _inputText.Home(args.Shift);
                        break;
                    case Keyboard.Key.End:
                        _inputText.End(args.Shift);
                        break;
                    case Keyboard.Key.Z:
                        if (args.Control)
                            _inputText.Undo();
                        if (args.Control && args.Shift)
                            _inputText.Redo();
                        break;
                    case Keyboard.Key.A:
                        if (args.Control)
                            _inputText.SelectAll();
                        break;
                    case Keyboard.Key.C:
                        if (args.Control)
                        {
                            var selected = _inputText.SelectedText;
                            if (string.IsNullOrWhiteSpace(selected))
                                break;
                            Clippy.PushStringToClipboard(selected);
                        }
                        break;
                    case Keyboard.Key.X:
                        if (args.Control)
                        {
                            var selected = _inputText.SelectedText;
                            if (string.IsNullOrWhiteSpace(selected))
                                break;
                            Clippy.PushStringToClipboard(selected);
                            _inputText.Delete();
                        }
                        break;
                    case Keyboard.Key.V:
                        if (args.Control)
                            _inputText.AddString(Clippy.GetText());
                        break;
                    case Keyboard.Key.Escape:
                        if (_completer.IsActive)
                            _completer.Escape();
                        else
                        {
                            _state = OpenState.Closed;
                            input.Release();
                            toggled = true;
                        }
                        break;
                    case Keyboard.Key.Tab:
                        if (_completer.IsActive)
                        {
                            _inputText.SetString(_completer.ChooseSelectedItem());
                            _completer.Escape();
                        }
                        break;
                    case Keyboard.Key.Space:
                        if (input.IsControlDown && !_completer.IsActive)
                            _completer.UpdateInputString(_inputText.ToString());
                        break;
                }
                if (toggled)
                    UpdateOpenness();
            };

            input.MouseButtonDown += args => _inputText.HandleMouseDown(new Vector2f(args.X, args.Y), input.IsShiftDown);
            input.MouseButtonUp += args => _inputText.HandleMouseUp(new Vector2f(args.X, args.Y));
            input.MouseMoved += args => _inputText.HandleMouseMoved(new Vector2f(args.X, args.Y));
            input.MouseWheelScrolled += args =>
            {
                if (args.Delta > 0)
                    _reportText.ScrollUp();
                else
                    _reportText.ScrollDown();
            };

        }

        public bool IsOpen => _currentOpenness > 0;


        public void Draw(RenderTarget target, RenderStates states)
        {
            UpdateOpenness();
            if (IsOpen)
            {
                _reportBackground.Position = new Vector2f(0, _closedYPos * (_currentOpenness - 1));
                var bounds = _reportBackground.GetGlobalBounds();
                _inputBackground.Position = new Vector2f(0, (int)(bounds.Top + bounds.Height));
                _reportBackground.Draw(target, states);

                _reportText.Draw(target, states);
                _inputBackground.Draw(target, states);
                _inputText.Draw(target, states);
                _completer.Draw(target, states);
            }
        }

        private void UpdateOpenness()
        {
            _targetOpenness = _state == OpenState.Closed ? 0 : 1;
            var dt = Core.TimeInfo.CurrentDt;
            var dOpen = dt * _openingRate;

            if (_currentOpenness < _targetOpenness)
            {
                _currentOpenness += dOpen;
                if (_currentOpenness > _targetOpenness)
                    _currentOpenness = _targetOpenness;
            }
            else if (_currentOpenness > _targetOpenness)
            {
                _currentOpenness -= dOpen;
                if (_currentOpenness < _targetOpenness)
                    _currentOpenness = _targetOpenness;
            }
        }

        public void LogMessage(string arg1, Category arg2)
        {
            Tag tag;
            switch (arg2)
            {
                case Category.Debug:
                    tag = Tag.Debug;
                    break;
                case Category.Warning:
                    tag = Tag.Warning;
                    break;
                case Category.Error:
                    tag = Tag.Error;
                    break;
                case Category.SuperLowDebug:
                    tag = Tag.SuperLowDebug;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg2), arg2, null);
            }
            _reportText.AddLine(arg1, tag);
        }
    }
}