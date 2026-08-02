using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Scenes
{
    /// <summary>
    /// Shown if the station's self-destruct countdown reaches zero before the
    /// player solves the airlock override puzzle. Offers a retry (returns to
    /// main menu, where Continue/New Game are available).
    /// </summary>
    public class DefeatScene : Scene
    {
        private readonly System.Action _onMainMenu;
        private UiButton _menuBtn;
        private double _flickerTimer;

        public DefeatScene(GameServices services, SceneManager manager, System.Action onMainMenu) : base(services, manager)
        {
            _onMainMenu = onMainMenu;
        }

        public override void OnEnter()
        {
            _menuBtn = new UiButton(new Rectangle(960 - 175, 820, 350, 64), "חזרה לתפריט הראשי");
            Services.Audio.PlayMusic("tension_theme.ogg", loop: true);
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            _flickerTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_menuBtn.WasClicked(mouse, prevMouse))
            {
                Services.Audio.PlaySfx("button_click.wav");
                _onMainMenu?.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            var bg = Services.Assets.GetTexture("ui", "defeat_bg");
            spriteBatch.Draw(bg, screenBounds, Color.White);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(screenBounds.X, screenBounds.Y + 150, screenBounds.Width, 400), Color.Black * 0.5f);

            string title = "תחנת החלל אבודה...";
            var titleSize = Services.FontLarge.MeasureString(title);
            float flash = 0.6f + 0.4f * (float)System.Math.Sin(_flickerTimer * 6.0);
            spriteBatch.DrawString(Services.FontLarge, title, new Vector2(screenBounds.Center.X - titleSize.X / 2f, 260), Color.Lerp(new Color(150, 20, 20), new Color(255, 80, 80), flash));

            string sub = "הזמן אזל לפני שהצלחת להימלט. נסה שוב.";
            var subSize = Services.Font.MeasureString(sub);
            spriteBatch.DrawString(Services.Font, sub, new Vector2(screenBounds.Center.X - subSize.X / 2f, 380), new Color(220, 200, 200));

            _menuBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, Mouse.GetState().Position, new Color(220, 90, 90));

            UiPanel.DrawGlitchFlicker(spriteBatch, Services.Assets.Pixel, screenBounds, new System.Random((int)(_flickerTimer * 1000) % 10007), 0.6f);
        }
    }
}
