using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Scenes
{
    /// <summary>
    /// The professional main menu: game title over a cinematic station backdrop,
    /// with New Game / Continue / Settings / Quit. "Continue" is disabled when
    /// no save file exists.
    /// </summary>
    public class MainMenuScene : Scene
    {
        private readonly SaveManager _saveManager;
        private readonly System.Action _onNewGame;
        private readonly System.Action _onContinue;
        private readonly System.Action _onSettings;

        private UiButton _newGameBtn;
        private UiButton _continueBtn;
        private UiButton _settingsBtn;
        private UiButton _quitBtn;

        private double _flickerTimer;

        public MainMenuScene(GameServices services, SceneManager manager, SaveManager saveManager,
            System.Action onNewGame, System.Action onContinue, System.Action onSettings)
            : base(services, manager)
        {
            _saveManager = saveManager;
            _onNewGame = onNewGame;
            _onContinue = onContinue;
            _onSettings = onSettings;
        }

        public override void OnEnter()
        {
            Services.Audio.PlayMusic("ambient_station.ogg");

            int btnW = 340, btnH = 64, spacing = 20;
            int startY = 560;
            int centerX = 960 - btnW / 2;

            _newGameBtn = new UiButton(new Rectangle(centerX, startY, btnW, btnH), "משחק חדש");
            _continueBtn = new UiButton(new Rectangle(centerX, startY + (btnH + spacing), btnW, btnH), "טעינת משחק")
            {
                Enabled = _saveManager.SaveExists()
            };
            _settingsBtn = new UiButton(new Rectangle(centerX, startY + 2 * (btnH + spacing), btnW, btnH), "הגדרות");
            _quitBtn = new UiButton(new Rectangle(centerX, startY + 3 * (btnH + spacing), btnW, btnH), "יציאה");
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            _flickerTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_newGameBtn.WasClicked(mouse, prevMouse))
            {
                Services.Audio.PlaySfx("button_click.wav");
                _onNewGame?.Invoke();
            }
            else if (_continueBtn.WasClicked(mouse, prevMouse))
            {
                Services.Audio.PlaySfx("button_click.wav");
                _onContinue?.Invoke();
            }
            else if (_settingsBtn.WasClicked(mouse, prevMouse))
            {
                Services.Audio.PlaySfx("button_click.wav");
                _onSettings?.Invoke();
            }
            else if (_quitBtn.WasClicked(mouse, prevMouse))
            {
                System.Environment.Exit(0);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            var bg = Services.Assets.GetTexture("ui", "main_menu_bg");
            spriteBatch.Draw(bg, screenBounds, Color.White);

            // Dark vignette gradient band behind title for legibility
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(screenBounds.X, screenBounds.Y, screenBounds.Width, 260), Color.Black * 0.45f);

            string title = "תחנת החלל: מבצע בריחה";
            Vector2 titleSize = Services.FontLarge.MeasureString(title);
            var titlePos = new Vector2(screenBounds.Center.X - titleSize.X / 2f, 90);

            // Subtle glitch-flicker on the title glow to sell the "damaged station" mood
            float glow = 0.5f + 0.5f * (float)System.Math.Sin(_flickerTimer * 3.0);
            spriteBatch.DrawString(Services.FontLarge, title, titlePos + new Vector2(2, 2), Color.Black * 0.6f);
            spriteBatch.DrawString(Services.FontLarge, title, titlePos, Color.Lerp(new Color(120, 220, 255), Color.White, glow));

            string subtitle = "Escape Space Station";
            Vector2 subSize = Services.Font.MeasureString(subtitle);
            spriteBatch.DrawString(Services.Font, subtitle, new Vector2(screenBounds.Center.X - subSize.X / 2f, 170), new Color(150, 170, 190));

            var mousePos = Mouse.GetState().Position;
            _newGameBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, mousePos);
            _continueBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, mousePos);
            _settingsBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, mousePos);
            _quitBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, mousePos, new Color(220, 90, 90));

            UiPanel.DrawScanlines(spriteBatch, Services.Assets.Pixel, screenBounds, 0.05f);
        }
    }
}
