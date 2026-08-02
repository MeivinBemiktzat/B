using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Cable Wiring.
    /// The player must connect each colored cable stub on the left to its
    /// matching colored socket on the right, in the correct order, to
    /// restore power routing to a subsystem. Clicking a cable then clicking
    /// its target socket makes the connection; wrong socket briefly flashes red.
    /// </summary>
    public class CableWiringPuzzle : Puzzle
    {
        private readonly Color[] _colors =
        {
            new Color(220, 60, 60),   // red
            new Color(60, 140, 220),  // blue
            new Color(60, 200, 100),  // green
            new Color(230, 200, 60)   // yellow
        };

        private int[] _socketOrder; // which cable color index sits at each socket slot (shuffled target)
        private bool[] _connected;
        private int _selectedCable = -1;
        private double _errorFlashTimer;

        private Rectangle _panelBounds;

        public CableWiringPuzzle(GameServices services) : base("cable_wiring", "תיקון חיווט חשמלי", services)
        {
            _connected = new bool[_colors.Length];
            // Shuffle target sockets so the correct match isn't index==index.
            _socketOrder = new[] { 2, 0, 3, 1 };
        }

        public override void Open()
        {
            base.Open();
            _selectedCable = -1;
            for (int i = 0; i < _connected.Length; i++) _connected[i] = false;
        }

        private Rectangle CableRect(int index, Rectangle bounds)
        {
            int y = bounds.Y + 100 + index * 90;
            return new Rectangle(bounds.X + 60, y, 160, 50);
        }

        private Rectangle SocketRect(int slot, Rectangle bounds)
        {
            int y = bounds.Y + 100 + slot * 90;
            return new Rectangle(bounds.Right - 220, y, 160, 50);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            if (_errorFlashTimer > 0) _errorFlashTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            // Check cable clicks (left column)
            for (int i = 0; i < _colors.Length; i++)
            {
                if (_connected[i]) continue;
                if (CableRect(i, _panelBounds).Contains(mouse.Position))
                {
                    _selectedCable = i;
                    return;
                }
            }

            // Check socket clicks (right column)
            if (_selectedCable != -1)
            {
                for (int slot = 0; slot < _socketOrder.Length; slot++)
                {
                    if (SocketRect(slot, _panelBounds).Contains(mouse.Position))
                    {
                        if (_socketOrder[slot] == _selectedCable)
                        {
                            _connected[_selectedCable] = true;
                            Services.Audio.PlaySfx("spark_zap.wav");
                            _selectedCable = -1;

                            bool allDone = true;
                            foreach (var c in _connected) if (!c) allDone = false;
                            if (allDone) MarkSolved();
                        }
                        else
                        {
                            _errorFlashTimer = 0.3;
                            Services.Audio.PlaySfx("puzzle_fail.wav");
                            _selectedCable = -1;
                        }
                        return;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;

            _panelBounds = new Rectangle(screenBounds.Center.X - 350, screenBounds.Center.Y - 280, 700, 560);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds);

            spriteBatch.DrawString(Services.Font, "חבר כל כבל לשקע התואם בצבעו", new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            for (int i = 0; i < _colors.Length; i++)
            {
                var rect = CableRect(i, _panelBounds);
                if (!_connected[i])
                {
                    Color c = _colors[i];
                    if (_selectedCable == i) c = Color.Lerp(c, Color.White, 0.4f);
                    spriteBatch.Draw(Services.Assets.Pixel, rect, c);
                }
            }

            for (int slot = 0; slot < _socketOrder.Length; slot++)
            {
                var rect = SocketRect(slot, _panelBounds);
                int cableIdx = _socketOrder[slot];
                Color borderColor = _connected[cableIdx] ? _colors[cableIdx] : new Color(80, 80, 90);
                spriteBatch.Draw(Services.Assets.Pixel, rect, new Color(15, 18, 25));
                int t = 3;
                spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, t), borderColor);
                spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), borderColor);
                spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Y, t, rect.Height), borderColor);
                spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), borderColor);
            }

            if (_errorFlashTimer > 0)
            {
                spriteBatch.Draw(Services.Assets.Pixel, _panelBounds, Color.Red * 0.15f);
            }

            if (IsSolved)
            {
                spriteBatch.DrawString(Services.Font, "המערכת תוקנה בהצלחה!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(80, 255, 140));
            }
        }
    }
}
