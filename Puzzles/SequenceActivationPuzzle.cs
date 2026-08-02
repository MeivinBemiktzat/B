using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Sequence Activation.
    /// Reactor startup requires activating four subsystems (coolant, plasma
    /// injectors, containment field, ignition) strictly in the correct order.
    /// Activating out of order resets progress -- simulating a real safety interlock.
    /// </summary>
    public class SequenceActivationPuzzle : Puzzle
    {
        private readonly string[] _labels = { "קירור", "מזרקי פלזמה", "שדה בלימה", "הצתה" };
        private readonly int[] _correctOrder = { 2, 0, 1, 3 }; // shuffled correct order by label index
        private int _progress;
        private bool[] _activated;
        private double _errorTimer;
        private Rectangle _panelBounds;

        public SequenceActivationPuzzle(GameServices services) : base("sequence_activation", "הפעלת רצף מנוע", services)
        {
            _activated = new bool[_labels.Length];
        }

        public override void Open()
        {
            base.Open();
            _progress = 0;
            for (int i = 0; i < _activated.Length; i++) _activated[i] = false;
        }

        private Rectangle ButtonRect(int index, Rectangle bounds)
        {
            int y = bounds.Y + 110 + index * 90;
            return new Rectangle(bounds.X + 60, y, bounds.Width - 120, 60);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;
            if (_errorTimer > 0) _errorTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            for (int i = 0; i < _labels.Length; i++)
            {
                if (ButtonRect(i, _panelBounds).Contains(mouse.Position) && !_activated[i])
                {
                    if (_correctOrder[_progress] == i)
                    {
                        _activated[i] = true;
                        _progress++;
                        Services.Audio.PlaySfx("button_click.wav");
                        if (_progress >= _correctOrder.Length) MarkSolved();
                    }
                    else
                    {
                        _errorTimer = 0.4;
                        _progress = 0;
                        for (int j = 0; j < _activated.Length; j++) _activated[j] = false;
                        Services.Audio.PlaySfx("puzzle_fail.wav");
                    }
                    return;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 350, screenBounds.Center.Y - 260, 700, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(255, 120, 60));

            spriteBatch.DrawString(Services.Font, "הפעל את המערכות בסדר הבטיחות הנכון",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            for (int i = 0; i < _labels.Length; i++)
            {
                var rect = ButtonRect(i, _panelBounds);
                Color color = _activated[i] ? new Color(60, 200, 100) : new Color(40, 40, 50);
                spriteBatch.Draw(Services.Assets.Pixel, rect, color);
                spriteBatch.DrawString(Services.Font, _labels[i], new Vector2(rect.X + 15, rect.Y + 15), Color.White);
            }

            if (_errorTimer > 0)
                spriteBatch.Draw(Services.Assets.Pixel, _panelBounds, Color.Red * 0.2f);

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "רצף ההפעלה הושלם - המנוע פעיל!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(80, 255, 140));
        }
    }
}
