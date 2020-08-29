using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;

namespace SFCORE.Terminal
{

    public class WrappedTextRenderer : IWrappedTextRenderer
    {
        private readonly Func<FloatRect> _getBoundsForText;
        private readonly Font _font;
        private readonly uint _charSize;

        public WrappedTextRenderer(Func<FloatRect> getBounds, Font font, uint charSize, Dictionary<Tag, Color> colorLookup)
        {
            _getFullBounds = getBounds;
            
            _getBoundsForText = () =>
            {
                var allBounds = getBounds();
                return new FloatRect(allBounds.Left + 2, allBounds.Top, allBounds.Width - 4, allBounds.Height);
            };
            _font = font;
            _charSize = charSize;
            _colorLookup = colorLookup;
            _textOrigin = new Vector2f(200000, 200000);
            _textViewport = new View(new FloatRect(_textOrigin, new Vector2f(getBounds().Width, getBounds().Height)));
        }

        private readonly View _textViewport;

        public void Draw(RenderTarget target, RenderStates states)
        {
            var boundsForText = _getBoundsForText();
            _textViewport.Viewport = Core.WindowUtil.GetFractionalRect(boundsForText);
            target.SetView(_textViewport);
            _textsToDraw.ForEach(a => a.Text.Draw(target, states));
            target.SetView(target.DefaultView);
        }

        class WrappedTextItem
        {
            public Text Text { get; set; }
            public Tag Tag { get; set; }
            public int NumLines { get; set; }
        }

        private readonly Vector2f _textOrigin;
        private readonly List<Tuple<string, Tag>> _receivedLines = new List<Tuple<string, Tag>>();
        private readonly List<WrappedTextItem> _texts = new List<WrappedTextItem>();

        public void AddLine(string line, Tag tag)
        {
            _receivedLines.Add(Tuple.Create(line, tag));

            var shadowString = line ?? "";
            var newText = new Text(shadowString, _font, _charSize) { Color = _colorLookup[tag] };
            var lastSplitIndex = 0;

            var bounds = _getBoundsForText();
            while (newText.GetLocalBounds().Width > bounds.Width)
            {
                for (; lastSplitIndex < shadowString.Length; lastSplitIndex++)
                {
                    if (newText.FindCharacterPos((uint)lastSplitIndex).X - newText.Position.X > bounds.Width)
                    {
                        lastSplitIndex--;
                        shadowString = shadowString.Insert(lastSplitIndex, "\n ");
                        newText.DisplayedString = shadowString;
                        break;
                    }
                }
            }

            var spacing = _font.GetLineSpacing(_charSize);
            if (_texts.Any())
            {
                var lastText = _texts.Last();
                newText.Position = lastText.Text.Position + new Vector2f(0, lastText.NumLines * spacing);
            }
            else
                newText.Position = _textOrigin;

            _texts.Add(new WrappedTextItem
            {
                NumLines = shadowString.Count(a => a == '\n') + 1,
                Tag = tag,
                Text = newText
            });
            MoveViewportToBottom();
        }

        private readonly int scrollAmount = 50;
        public void ScrollUp()
        {
            MoveViewport(new Vector2f(0, -scrollAmount));
            ClampViewportPos();
        }

        FloatRect CreateViewRect(Vector2f origin)
        {
            var bounds = _getBoundsForText();
            return new FloatRect(origin, new Vector2f(bounds.Width, bounds.Height));
        }

        public void ScrollDown()
        {
            MoveViewport(new Vector2f(0, scrollAmount));
            ClampViewportPos();
        }

        void MoveViewportToBottom()
        {
            var bottomOfText = 0;
            if (_texts.Any())
            {
                var lowestText = _texts.Last().Text.GetGlobalBounds();
                bottomOfText = (int)(lowestText.Top + lowestText.Height);
            }
            var bounds = _getBoundsForText();
            var bottom = new Vector2f(_textOrigin.X, bottomOfText);
            MoveViewportTo(bottom - new Vector2f(0, bounds.Height));
        }

        void MoveViewport(Vector2f delta)
        {
            _textViewport.Move(delta);
            CalculateDrawnText();
        }

        void MoveViewportTo(Vector2f position)
        {
            _textViewport.Reset(CreateViewRect(position));
            CalculateDrawnText();
        }

        void CalculateDrawnText()
        {
            var vtr = _textViewport.GetTargetRegion();
            _textsToDraw = _texts.Where(a => a.Text.GetGlobalBounds().Intersects(vtr)).ToList();
        }

        void ClampViewportPos()
        {
            if (!_texts.Any())
            {
                MoveViewportToBottom();
            }
            else
            {
                var firstTextBounds = _texts.First().Text.GetGlobalBounds();
                var viewportRegion = _textViewport.GetTargetRegion();
                if (viewportRegion.Top < firstTextBounds.Top)
                {
                    _textViewport.Move(new Vector2f(0, firstTextBounds.Top - viewportRegion.Top));
                }
                viewportRegion = _textViewport.GetTargetRegion();
                var lastTextBounds = _texts.Last().Text.GetGlobalBounds();
                if (viewportRegion.Bottom() > lastTextBounds.Bottom())
                {
                    _textViewport.Move(new Vector2f(0, lastTextBounds.Bottom() - viewportRegion.Bottom()));
                }
            }
            CalculateDrawnText();
        }


        private readonly Dictionary<Tag, Color> _colorLookup;
        private List<WrappedTextItem> _textsToDraw = new List<WrappedTextItem>();
        private Func<FloatRect> _getFullBounds;
    }
}