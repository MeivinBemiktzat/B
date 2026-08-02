using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Document Code Search.
    /// Several scattered "documents" (data-pad logs) can be opened by clicking
    /// them; one of them contains the access code buried in flavor text. The
    /// player then must enter that code into a keypad.
    /// </summary>
    public class DocumentCodePuzzle : Puzzle
    {
        private const string CorrectCode = "4471";

        private class Document
        {
            public Rectangle Rect;
            public string Title;
            public string Body;
            public bool Open;
        }

        private List<Document> _documents;
        private string _playerInput = "";
        private double _errorTimer;
        private Rectangle _panelBounds;

        public DocumentCodePuzzle(GameServices services) : base("document_code", "חיפוש קוד במסמכים", services)
        {
            _documents = new List<Document>
            {
                new Document
                {
                    Title = "יומן קברניט - יום 112",
                    Body = "המערכות התייצבו זמנית. עלינו לזכור שקוד הגישה\nלמעבדה שונה החודש. אני תמיד משתמש בשילוב של\nמספר הצוות שלי (17) וקוד התחנה (54) יחד: 4471."
                },
                new Document
                {
                    Title = "דוח תחזוקה",
                    Body = "בדיקת מערכת קירור בוצעה. אין חריגות.\nיש להחליף מסנן אוויר בחודש הבא."
                },
                new Document
                {
                    Title = "הודעת צוות",
                    Body = "תזכורת: פגישת צוות הבוקר בוטלה עקב\nהתקלה במערכת החשמל הראשית."
                }
            };
        }

        public override void Open()
        {
            base.Open();
            _playerInput = "";
            foreach (var doc in _documents) doc.Open = false;
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;
            if (_errorTimer > 0) _errorTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (clicked)
            {
                for (int i = 0; i < _documents.Count; i++)
                {
                    var rect = new Rectangle(_panelBounds.X + 40 + i * 230, _panelBounds.Y + 90, 200, 120);
                    _documents[i].Rect = rect;
                    if (rect.Contains(mouse.Position))
                    {
                        _documents[i].Open = !_documents[i].Open;
                        Services.Audio.PlaySfx("button_click.wav");
                    }
                }
            }

            foreach (var key in keys.GetPressedKeys())
            {
                if (prevKeys.IsKeyDown(key)) continue;
                if (key >= Keys.D0 && key <= Keys.D9 && _playerInput.Length < 4)
                {
                    _playerInput += (key - Keys.D0).ToString();
                }
                else if (key == Keys.Back && _playerInput.Length > 0)
                {
                    _playerInput = _playerInput[..^1];
                }
                else if (key == Keys.Enter)
                {
                    if (_playerInput == CorrectCode) MarkSolved();
                    else
                    {
                        _errorTimer = 0.3;
                        _playerInput = "";
                        Services.Audio.PlaySfx("puzzle_fail.wav");
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 400, screenBounds.Center.Y - 260, 800, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(255, 200, 80));

            spriteBatch.DrawString(Services.Font, "פתח את המסמכים ומצא את קוד הגישה בן 4 הספרות",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            for (int i = 0; i < _documents.Count; i++)
            {
                var doc = _documents[i];
                var rect = new Rectangle(_panelBounds.X + 40 + i * 230, _panelBounds.Y + 90, 200, 120);
                doc.Rect = rect;
                spriteBatch.Draw(Services.Assets.Pixel, rect, new Color(40, 35, 20));
                spriteBatch.DrawString(Services.Font, doc.Title, new Vector2(rect.X + 8, rect.Y + 8), new Color(255, 220, 140));

                if (doc.Open)
                {
                    var popup = new Rectangle(_panelBounds.X + 40, _panelBounds.Y + 230, _panelBounds.Width - 80, 140);
                    spriteBatch.Draw(Services.Assets.Pixel, popup, new Color(25, 22, 15, 240));
                    spriteBatch.DrawString(Services.Font, doc.Body, new Vector2(popup.X + 15, popup.Y + 15), Color.White);
                }
            }

            var inputBox = new Rectangle(_panelBounds.X + 40, _panelBounds.Bottom - 90, 200, 50);
            var inputColor = _errorTimer > 0 ? Color.Red : new Color(255, 200, 80);
            spriteBatch.Draw(Services.Assets.Pixel, inputBox, new Color(10, 10, 12));
            int t = 2;
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Y, inputBox.Width, t), inputColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Bottom - t, inputBox.Width, t), inputColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.X, inputBox.Y, t, inputBox.Height), inputColor);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(inputBox.Right - t, inputBox.Y, t, inputBox.Height), inputColor);
            spriteBatch.DrawString(Services.Font, _playerInput + "_", new Vector2(inputBox.X + 15, inputBox.Y + 12), Color.White);

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "קוד אומת בהצלחה!", new Vector2(_panelBounds.X + 280, _panelBounds.Bottom - 78), new Color(80, 255, 140));
        }
    }
}
