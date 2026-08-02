using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Scenes
{
    /// <summary>Volume settings screen: drag sliders for music/SFX, back button returns to caller scene.</summary>
    public class SettingsScene : Scene
    {
        private readonly System.Action _onBack;
        private UiButton _backBtn;

        private Rectangle _musicTrack;
        private Rectangle _sfxTrack;
        private bool _draggingMusic;
        private bool _draggingSfx;

        public SettingsScene(GameServices services, SceneManager manager, System.Action onBack) : base(services, manager)
        {
            _onBack = onBack;
        }

        public override void OnEnter()
        {
            _backBtn = new UiButton(new Rectangle(960 - 150, 780, 300, 60), "חזרה");
            _musicTrack = new Rectangle(960 - 250, 340, 500, 12);
            _sfxTrack = new Rectangle(960 - 250, 460, 500, 12);
            _draggingMusic = false;
            _draggingSfx = false;
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            bool leftDown = mouse.LeftButton == ButtonState.Pressed;
            bool justPressed = mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;

            var musicHandle = HandleRect(_musicTrack, Services.Settings.Current.MusicVolume);
            var sfxHandle = HandleRect(_sfxTrack, Services.Settings.Current.SfxVolume);

            if (justPressed && (musicHandle.Contains(mouse.Position) || _musicTrack.Contains(mouse.Position)))
                _draggingMusic = true;
            if (justPressed && (sfxHandle.Contains(mouse.Position) || _sfxTrack.Contains(mouse.Position)))
                _draggingSfx = true;

            if (!leftDown) { _draggingMusic = false; _draggingSfx = false; }

            if (_draggingMusic)
            {
                float t = MathHelper.Clamp((mouse.X - _musicTrack.X) / (float)_musicTrack.Width, 0f, 1f);
                Services.Settings.Current.MusicVolume = t;
                Services.Audio.UpdateMusicVolume();
            }
            if (_draggingSfx)
            {
                float t = MathHelper.Clamp((mouse.X - _sfxTrack.X) / (float)_sfxTrack.Width, 0f, 1f);
                Services.Settings.Current.SfxVolume = t;
            }

            if (_backBtn.WasClicked(mouse, prevMouse))
            {
                Services.Settings.Save();
                Services.Audio.PlaySfx("button_click.wav");
                _onBack?.Invoke();
            }
        }

        private Rectangle HandleRect(Rectangle track, float value)
        {
            int x = track.X + (int)(value * track.Width) - 10;
            return new Rectangle(x, track.Y - 10, 20, 32);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            spriteBatch.Draw(Services.Assets.Pixel, screenBounds, new Color(6, 9, 14));
            UiPanel.DrawScanlines(spriteBatch, Services.Assets.Pixel, screenBounds, 0.04f);

            string title = "הגדרות קול";
            var titleSize = Services.FontLarge.MeasureString(title);
            spriteBatch.DrawString(Services.FontLarge, title, new Vector2(screenBounds.Center.X - titleSize.X / 2f, 180), Color.White);

            DrawSlider(spriteBatch, "עוצמת מוזיקה", _musicTrack, Services.Settings.Current.MusicVolume);
            DrawSlider(spriteBatch, "עוצמת אפקטים", _sfxTrack, Services.Settings.Current.SfxVolume);

            _backBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, Mouse.GetState().Position);
        }

        private void DrawSlider(SpriteBatch spriteBatch, string label, Rectangle track, float value)
        {
            spriteBatch.DrawString(Services.Font, label, new Vector2(track.X, track.Y - 40), Color.White);
            spriteBatch.Draw(Services.Assets.Pixel, track, new Color(40, 45, 55));

            var fill = new Rectangle(track.X, track.Y, (int)(track.Width * value), track.Height);
            spriteBatch.Draw(Services.Assets.Pixel, fill, new Color(0, 210, 255));

            var handle = HandleRect(track, value);
            spriteBatch.Draw(Services.Assets.Pixel, handle, Color.White);

            spriteBatch.DrawString(Services.Font, $"{(int)(value * 100)}%", new Vector2(track.Right + 20, track.Y - 6), Color.White);
        }
    }
}
