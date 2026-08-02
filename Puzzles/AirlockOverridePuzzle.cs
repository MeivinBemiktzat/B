using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Airlock Override (final puzzle).
    /// Requires the player to have collected the red keycard, blue keycard,
    /// and power cell from earlier rooms. Combines a keycard-insertion step
    /// with a final confirmation button -- the culmination of the whole game.
    /// </summary>
    public class AirlockOverridePuzzle : Puzzle
    {
        private bool _redInserted, _blueInserted, _powerInserted;
        private Rectangle _panelBounds;
        private readonly GameState _gameState;

        public AirlockOverridePuzzle(GameServices services, GameState gameState) : base("airlock_override", "עקיפת מנעול מפוצץ אוויר", services)
        {
            _gameState = gameState;
        }

        public override void Open()
        {
            base.Open();
            _redInserted = _blueInserted = _powerInserted = false;
        }

        private Rectangle SlotRect(int index, Rectangle bounds)
        {
            return new Rectangle(bounds.X + 60 + index * 220, bounds.Y + 130, 180, 100);
        }

        private Rectangle ConfirmButtonRect(Rectangle bounds)
        {
            return new Rectangle(bounds.Center.X - 100, bounds.Bottom - 100, 200, 60);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            if (!_redInserted && SlotRect(0, _panelBounds).Contains(mouse.Position) && _gameState.HasItem("keycard_red"))
            {
                _redInserted = true;
                Services.Audio.PlaySfx("button_click.wav");
            }
            else if (!_blueInserted && SlotRect(1, _panelBounds).Contains(mouse.Position) && _gameState.HasItem("keycard_blue"))
            {
                _blueInserted = true;
                Services.Audio.PlaySfx("button_click.wav");
            }
            else if (!_powerInserted && SlotRect(2, _panelBounds).Contains(mouse.Position) && _gameState.HasItem("power_cell"))
            {
                _powerInserted = true;
                Services.Audio.PlaySfx("button_click.wav");
            }
            else if (_redInserted && _blueInserted && _powerInserted && ConfirmButtonRect(_panelBounds).Contains(mouse.Position))
            {
                MarkSolved();
                Services.Audio.PlaySfx("door_unlock.wav");
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 380, screenBounds.Center.Y - 260, 760, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(255, 60, 60));

            spriteBatch.DrawString(Services.Font, "הכנס את כל שלושת הפריטים כדי לעקוף את מנעול פתח האוויר",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            DrawSlot(spriteBatch, 0, "כרטיס אדום", _redInserted, _gameState.HasItem("keycard_red"));
            DrawSlot(spriteBatch, 1, "כרטיס כחול", _blueInserted, _gameState.HasItem("keycard_blue"));
            DrawSlot(spriteBatch, 2, "תא כוח", _powerInserted, _gameState.HasItem("power_cell"));

            bool canConfirm = _redInserted && _blueInserted && _powerInserted;
            var confirmRect = ConfirmButtonRect(_panelBounds);
            spriteBatch.Draw(Services.Assets.Pixel, confirmRect, canConfirm ? new Color(60, 200, 100) : new Color(50, 50, 55));
            spriteBatch.DrawString(Services.Font, "פתח פתח אוויר", new Vector2(confirmRect.X + 20, confirmRect.Y + 18), Color.White);

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "פתח האוויר נפתח - אתה חופשי!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 40), new Color(80, 255, 140));
        }

        private void DrawSlot(SpriteBatch spriteBatch, int index, string label, bool inserted, bool hasItem)
        {
            var rect = SlotRect(index, _panelBounds);
            Color color = inserted ? new Color(60, 200, 100) : (hasItem ? new Color(60, 80, 100) : new Color(30, 30, 35));
            spriteBatch.Draw(Services.Assets.Pixel, rect, color);
            spriteBatch.DrawString(Services.Font, label, new Vector2(rect.X + 10, rect.Y + 10), Color.White);
            if (!hasItem && !inserted)
                spriteBatch.DrawString(Services.Font, "(חסר)", new Vector2(rect.X + 10, rect.Bottom - 30), new Color(200, 80, 80));
        }
    }
}
