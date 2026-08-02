using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Main Control Panel.
    /// Three power-distribution levers must all be set to the "UP" (routed to
    /// life-support) position simultaneously to restore main power. Each lever
    /// toggles independently when clicked -- the challenge is a red herring
    /// warning light that turns on if only some are active, teaching the
    /// player to check all three instead of assuming success too early.
    /// </summary>
    public class ControlPanelPuzzle : Puzzle
    {
        private bool[] _leverUp;
        private Rectangle _panelBounds;

        public ControlPanelPuzzle(GameServices services) : base("control_panel", "לוח בקרה ראשי", services)
        {
            _leverUp = new bool[3];
        }

        public override void Open()
        {
            base.Open();
            for (int i = 0; i < _leverUp.Length; i++) _leverUp[i] = false;
        }

        private Rectangle LeverRect(int index, Rectangle bounds)
        {
            int x = bounds.X + 80 + index * 180;
            return new Rectangle(x, bounds.Y + 120, 100, 220);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            for (int i = 0; i < _leverUp.Length; i++)
            {
                if (LeverRect(i, _panelBounds).Contains(mouse.Position))
                {
                    _leverUp[i] = !_leverUp[i];
                    Services.Audio.PlaySfx("button_click.wav");

                    bool allUp = true;
                    foreach (var l in _leverUp) if (!l) allUp = false;
                    if (allUp) MarkSolved();
                    return;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 350, screenBounds.Center.Y - 260, 700, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(0, 210, 255));

            spriteBatch.DrawString(Services.Font, "העלה את כל שלושת המתגים למצב פעיל (למעלה)",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            bool allUp = true;
            for (int i = 0; i < _leverUp.Length; i++)
            {
                if (!_leverUp[i]) allUp = false;
                var rect = LeverRect(i, _panelBounds);

                // Track background
                spriteBatch.Draw(Services.Assets.Pixel, rect, new Color(20, 24, 30));

                // Lever handle
                int handleH = 60;
                int handleY = _leverUp[i] ? rect.Y : rect.Bottom - handleH;
                var handleRect = new Rectangle(rect.X, handleY, rect.Width, handleH);
                spriteBatch.Draw(Services.Assets.Pixel, handleRect, _leverUp[i] ? new Color(60, 220, 120) : new Color(200, 60, 60));

                spriteBatch.DrawString(Services.Font, $"מעגל {i + 1}", new Vector2(rect.X + 10, rect.Bottom + 10), Color.White);
            }

            // Warning light: on unless all levers are up
            var warningRect = new Rectangle(_panelBounds.Center.X - 20, _panelBounds.Bottom - 90, 40, 40);
            spriteBatch.Draw(Services.Assets.Pixel, warningRect, allUp ? new Color(40, 60, 40) : new Color(220, 40, 40));

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "החשמל הראשי שוחזר!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(80, 255, 140));
        }
    }
}
