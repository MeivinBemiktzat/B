using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Frequency Tuner.
    /// The player drags a slider to tune a comms dish to the exact target
    /// frequency (within a small tolerance) to send a distress signal.
    /// A live signal-strength readout gives feedback as the player gets closer.
    /// </summary>
    public class FrequencyTunerPuzzle : Puzzle
    {
        private const float TargetFrequency = 0.732f; // normalized 0..1 position
        private const float Tolerance = 0.02f;

        private float _sliderValue = 0.1f;
        private bool _dragging;
        private Rectangle _panelBounds;
        private Rectangle _trackRect;

        public FrequencyTunerPuzzle(GameServices services) : base("frequency_tuner", "כיוונון תדר שידור", services) { }

        public override void Open()
        {
            base.Open();
            _sliderValue = 0.1f;
            _dragging = false;
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            bool pressed = mouse.LeftButton == ButtonState.Pressed;
            bool justPressed = prevMouse.LeftButton == ButtonState.Released && pressed;
            var handleRect = HandleRect();

            if (justPressed && (handleRect.Contains(mouse.Position) || _trackRect.Contains(mouse.Position)))
                _dragging = true;
            if (!pressed) _dragging = false;

            if (_dragging)
            {
                float rel = (mouse.Position.X - _trackRect.X) / (float)_trackRect.Width;
                _sliderValue = MathHelper.Clamp(rel, 0f, 1f);

                if (System.Math.Abs(_sliderValue - TargetFrequency) <= Tolerance)
                {
                    MarkSolved();
                }
            }
        }

        private Rectangle HandleRect()
        {
            int handleX = _trackRect.X + (int)(_sliderValue * _trackRect.Width) - 12;
            return new Rectangle(handleX, _trackRect.Y - 15, 24, _trackRect.Height + 30);
        }

        private float SignalStrength()
        {
            float diff = System.Math.Abs(_sliderValue - TargetFrequency);
            return MathHelper.Clamp(1f - diff * 4f, 0f, 1f);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 380, screenBounds.Center.Y - 200, 760, 400);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(80, 255, 220));

            spriteBatch.DrawString(Services.Font, "כוון את התדר למקסימום עוצמת אות ושדר קריאת מצוקה",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            _trackRect = new Rectangle(_panelBounds.X + 60, _panelBounds.Y + 150, _panelBounds.Width - 120, 8);
            spriteBatch.Draw(Services.Assets.Pixel, _trackRect, new Color(50, 50, 60));

            var handle = HandleRect();
            spriteBatch.Draw(Services.Assets.Pixel, handle, new Color(80, 255, 220));

            // Signal strength bar
            float strength = SignalStrength();
            var barBg = new Rectangle(_panelBounds.X + 60, _panelBounds.Y + 220, _panelBounds.Width - 120, 30);
            spriteBatch.Draw(Services.Assets.Pixel, barBg, new Color(20, 25, 30));
            var barFill = new Rectangle(barBg.X, barBg.Y, (int)(barBg.Width * strength), barBg.Height);
            Color fillColor = strength > 0.85f ? new Color(80, 255, 140) : new Color(255, 180, 60);
            spriteBatch.Draw(Services.Assets.Pixel, barFill, fillColor);

            spriteBatch.DrawString(Services.Font, $"עוצמת אות: {(int)(strength * 100)}%",
                new Vector2(_panelBounds.X + 60, _panelBounds.Y + 260), Color.White);

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "אות מצוקה נשלח בהצלחה!", new Vector2(_panelBounds.X + 60, _panelBounds.Bottom - 60), new Color(80, 255, 140));
        }
    }
}
