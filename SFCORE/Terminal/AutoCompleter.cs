using System;
using System.Collections.Generic;
using System.Linq;
using SFCORE.Terminal;
using SFML.Graphics;
using SFML.System;

namespace SFCORE.Terminal
{
    public class AutoCompleter : IAutoCompleter
    {
        private readonly Func<string, List<string>> _getSuggestions;
        private readonly Func<Vector2f> _getOrigin;
        private readonly RectangleShape _displayRegion;
        private List<Text> _suggestionTexts;
        private readonly View _renderView = new View();
        public AutoCompleter(Func<string, List<string>> getSuggestions, Func<Vector2f> getOrigin, Font font, uint characterSize)
        {
            _charSize = characterSize;
            _getSuggestions = getSuggestions;
            _getOrigin = getOrigin;
            _font = font;
            _displayRegion = new RectangleShape { FillColor = new Color(200, 200, 200, 50) };
        }

        public void UpdateInputString(string str)
        {
            var suggestions = _getSuggestions(str).Take(10).ToList();

            if (!suggestions.Any())
            {
                IsActive = false;
                return;
            }
            _suggestionTexts = suggestions.Select(a => new Text
            {
                Font = _font,
                CharacterSize = _charSize,
                DisplayedString = a,
                Color = Color.White
            }).Take(10).ToList();
            var height = 0;
            foreach (var suggestionText in _suggestionTexts)
            {
                suggestionText.Position = _displayRegion.Position + new Vector2f(3, height);
                height += (int)_charSize;
            }
            _displayRegion.Size = new Vector2f(_suggestionTexts.Max(a => a.GetLocalBounds().Width + 6), height + _charSize * .5f);
            _renderView.Reset(_displayRegion.GetGlobalBounds());
            IsActive = true;
            _selectionIndex = 0;
            ApplySelectionRect();
        }

        public void Escape()
        {
            IsActive = false;
        }

        public bool IsActive { get; set; }
        private int _selectionIndex;
        private readonly Font _font;
        private readonly uint _charSize;

        public void IncrementSelection()
        {
            _selectionIndex--;
            if (_selectionIndex < 0)
                _selectionIndex = _suggestionTexts.Count - 1;
            ApplySelectionRect();
        }

        private readonly RectangleShape _selectionRect = new RectangleShape { FillColor = new Color(155, 255, 255, 50) };
        private void ApplySelectionRect()
        {
            var selectedTextBounds = _suggestionTexts[_selectionIndex].GetGlobalBounds();
            _selectionRect.Size = new Vector2f(selectedTextBounds.Width, _charSize);
            _selectionRect.Position = new Vector2f(selectedTextBounds.Left, (_selectionIndex + .25f) * _charSize);
        }

        public void DecrementSelection()
        {
            _selectionIndex++;
            if (_selectionIndex >= _suggestionTexts.Count)
                _selectionIndex = 0;
            ApplySelectionRect();
        }

        public string ChooseSelectedItem()
        {
            return _suggestionTexts[_selectionIndex].DisplayedString;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (!IsActive)
                return;
            _renderView.Viewport = Core.WindowUtil.GetFractionalRect(new FloatRect(_getOrigin(), _displayRegion.Size));
            target.SetView(_renderView);
            _displayRegion.Draw(target, states);
            _suggestionTexts.ForEach(a => a.Draw(target, states));
            _selectionRect.Draw(target, states);
            target.SetView(target.DefaultView);
        }
    }
}