using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Symbol Matching.
    /// The player must click symbols in the correct sequence shown briefly
    /// at the start (a "Simon says" style memory challenge representing
    /// an alien/ancient docking authorization glyph sequence).
    /// </summary>
    public class SymbolMatchPuzzle : Puzzle
    {
        private readonly Color[] _symbolColors =
        {
            new Color(255, 100, 100),
            new Color(100, 200, 255),
            new Color(255, 220, 100),
            new Color(150, 255, 150),
            new Color(200, 130, 255)
        };

        private List<int> _sequence;
        private int _playerStep;
        private double _showTimer;
        private bool _showingSequence;
        private int _showIndex;
        private double _errorTimer;
        private Rectangle _panelBounds;

        public SymbolMatchPuzzle(GameServices services) : base("symbol_match", "התאמת סמלים עתיקים", services)
        {
            _sequence = new List<int> { 2, 0, 4, 1, 3 };
        }

        public override void Open()
        {
            base.Open();
            _playerStep = 0;
            _showingSequence = true;
            _showIndex = 0;
            _showTimer = 0.7;
        }

        private Rectangle SymbolRect(int index, Rectangle bounds)
        {
            int spacing = 120;
            int startX = bounds.Center.X - (spacing * (_symbolColors.Length - 1)) / 2;
            return new Rectangle(startX + index * spacing - 40, bounds.Center.Y - 40, 80, 80);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            if (_errorTimer > 0) _errorTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            if (_showingSequence)
            {
                _showTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_showTimer <= 0)
                {
                    _showIndex++;
                    _showTimer = 0.7;
                    if (_showIndex >= _sequence.Count)
                    {
                        _showingSequence = false;
                    }
                }
                return;
            }

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            for (int i = 0; i < _symbolColors.Length; i++)
            {
                if (SymbolRect(i, _panelBounds).Contains(mouse.Position))
                {
                    if (i == _sequence[_playerStep])
                    {
                        _playerStep++;
                        Services.Audio.PlaySfx("button_click.wav");
                        if (_playerStep >= _sequence.Count)
                        {
                            MarkSolved();
                        }
                    }
                    else
                    {
                        _errorTimer = 0.4;
                        _playerStep = 0;
                        Services.Audio.PlaySfx("puzzle_fail.wav");
                    }
                    return;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 400, screenBounds.Center.Y - 220, 800, 440);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(200, 130, 255));

            string instructions = _showingSequence
                ? "שנן את רצף הסמלים המוצג..."
                : "לחץ על הסמלים לפי הסדר שראית";
            spriteBatch.DrawString(Services.Font, instructions, new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            for (int i = 0; i < _symbolColors.Length; i++)
            {
                var rect = SymbolRect(i, _panelBounds);
                bool highlight = _showingSequence && _showIndex < _sequence.Count && _sequence[_showIndex] == i;
                Color color = _symbolColors[i];
                if (highlight) color = Color.Lerp(color, Color.White, 0.6f);
                spriteBatch.Draw(Services.Assets.Pixel, rect, color);
            }

            if (_errorTimer > 0)
                spriteBatch.Draw(Services.Assets.Pixel, _panelBounds, Color.Red * 0.2f);

            if (!_showingSequence)
            {
                spriteBatch.DrawString(Services.Font, $"התקדמות: {_playerStep}/{_sequence.Count}",
                    new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(180, 180, 190));
            }

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "רצף אושר בהצלחה!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(80, 255, 140));
        }
    }
}
