using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Scenes
{
    /// <summary>Shown when the player solves the final airlock puzzle and escapes the station.</summary>
    public class VictoryScene : Scene
    {
        private readonly System.Action _onMainMenu;
        private readonly double _finalTimeSeconds;
        private UiButton _menuBtn;
        private bool _musicStarted;

        public VictoryScene(GameServices services, SceneManager manager, double finalTimeSeconds, System.Action onMainMenu) : base(services, manager)
        {
            _finalTimeSeconds = finalTimeSeconds;
            _onMainMenu = onMainMenu;
        }

        public override void OnEnter()
        {
            _menuBtn = new UiButton(new Rectangle(960 - 175, 820, 350, 64), "חזרה לתפריט הראשי");
            Services.Audio.PlayMusic("victory_theme.ogg", loop: false);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (_menuBtn.WasClicked(mouse, prevMouse))
            {
                Services.Audio.PlaySfx("button_click.wav");
                _onMainMenu?.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            var bg = Services.Assets.GetTexture("ui", "victory_bg");
            spriteBatch.Draw(bg, screenBounds, Color.White);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(screenBounds.X, screenBounds.Y + 150, screenBounds.Width, 400), Color.Black * 0.4f);

            string title = "ברחת בהצלחה מתחנת החלל!";
            var titleSize = Services.FontLarge.MeasureString(title);
            spriteBatch.DrawString(Services.FontLarge, title, new Vector2(screenBounds.Center.X - titleSize.X / 2f, 260), new Color(255, 220, 120));

            int minutes = (int)(_finalTimeSeconds / 60);
            int seconds = (int)(_finalTimeSeconds % 60);
            string timeStr = $"זמן משחק כולל: {minutes:00}:{seconds:00}";
            var timeSize = Services.Font.MeasureString(timeStr);
            spriteBatch.DrawString(Services.Font, timeStr, new Vector2(screenBounds.Center.X - timeSize.X / 2f, 380), Color.White);

            _menuBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, Mouse.GetState().Position, new Color(255, 210, 100));
        }
    }
}
