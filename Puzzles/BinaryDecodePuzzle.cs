using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Binary Decoding.
    /// A corrupted terminal displays a binary string. The player must decode
    /// it (each 8-bit byte -> ASCII character) and type the resulting word
    /// into an input field to unlock the terminal.
    /// </summary>
    public class BinaryDecodePuzzle : Puzzle
    {
        private const string Answer = "REBOOT";
        private readonly string _binaryDisplay;
        private string _playerInput = "";
        private double _shakeTimer;

        public BinaryDecodePuzzle(GameServices services) : base("binary_decode", "פענוח קוד בינארי", services)
        {
            _binaryDisplay = string.Join(" ", ToBinaryBytes(Answer));
        }

        private static string[] ToBinaryBytes(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            var result = new string[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                result[i] = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
            return result;
        }

        public override void Open()
        {
            base.Open();
            _playerInput = "";
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;
            if (_shakeTimer > 0) _shakeTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var key in keys.GetPressedKeys())
            {
                if (prevKeys.IsKeyDown(key)) continue; // only fire on the frame the key goes down

                if (key >= Keys.A && key <= Keys.Z)
                {
                    if (_playerInput.Length < 12) _playerInput += key.ToString();
                }
                else if (key == Keys.Back && _playerInput.Length > 0)
                {
                    _playerInput = _playerInput[..^1];
                }
                else if (key == Keys.Enter)
                {
                    if (_playerInput.Equals(Answer, StringComparison.OrdinalIgnoreCase))
                    {
                        MarkSolved();
                    }
                    else
                    {
                        _shakeTimer = 0.3;
                        Services.Audio.PlaySfx("puzzle_fail.wav");
                        _playerInput = "";
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;

            var bounds = new Rectangle(screenBounds.Center.X - 380, screenBounds.Center.Y - 260, 760, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, bounds, new Color(60, 255, 120));

            spriteBatch.DrawString(Services.Font, "פענח את הקוד הבינארי והקלד את המילה:", new Vector2(bounds.X + 40, bounds.Y + 30), Color.White);

            // Wrap the binary display across a couple lines for readability
            spriteBatch.DrawString(Services.Font, _binaryDisplay, new Vector2(bounds.X + 40, bounds.Y + 90), new Color(80, 255, 140));

            var inputBoxColor = _shakeTimer > 0 ? Color.Red : new Color(60, 255, 120);
            var inputBox = new Rectangle(bounds.X + 40, bounds.Y + 200, bounds.Width - 80, 60);
            spriteBatch.Draw(Services.Assets.Pixel, inputBox, new Color(10, 15, 12));
            int t = 2;
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Y, inputBox.Width, t), inputBoxColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Bottom - t, inputBox.Width, t), inputBoxColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Y, t, inputBox.Height), inputBoxColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.Right - t, inputBox.Y, t, inputBox.Height), inputBoxColor);

            spriteBatch.DrawString(Services.Font, _playerInput + "_", new Vector2(inputBox.X + 15, inputBox.Y + 15), Color.White);

            spriteBatch.DrawString(Services.Font, "רמז: כל 8 ביטים = תו אחד באסקי (ASCII). לחץ Enter לאישור.",
                new Vector2(bounds.X + 40, bounds.Bottom - 60), new Color(150, 150, 160));

            if (IsSolved)
            {
                spriteBatch.DrawString(Services.Font, "קוד פוענח בהצלחה: " + Answer, new Vector2(bounds.X + 40, bounds.Bottom - 100), new Color(80, 255, 140));
            }
        }
    }
}
