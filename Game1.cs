using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.Scenes;

namespace EscapeSpaceStation
{
    /// <summary>
    /// MonoGame entry point. Sets up the graphics device, loads shared
    /// services (assets, audio, settings, fonts) and boots the main menu.
    /// Scene switching and all gameplay logic live in Systems/Scenes -
    /// this class stays a thin composition root.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private GameServices _services;
        private SceneManager _sceneManager;
        private SaveManager _saveManager;
        private GameState _gameState;

        private MouseState _prevMouse;
        private KeyboardState _prevKeys;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080,
                SynchronizeWithVerticalRetrace = true
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Escape Space Station - תחנת החלל";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var settingsManager = new SettingsManager();
            var assetManager = new AssetManager(GraphicsDevice, AppContext.BaseDirectory);
            var audioManager = new AudioManager(assetManager, settingsManager);

            _services = new GameServices(assetManager, audioManager, settingsManager)
            {
                Font = Content.Load<SpriteFont>("Fonts/GameFont"),
                FontLarge = Content.Load<SpriteFont>("Fonts/GameFontLarge")
            };

            _saveManager = new SaveManager();
            _sceneManager = new SceneManager();

            _sceneManager.ChangeScene(BuildMainMenuScene());
        }

        private MainMenuScene BuildMainMenuScene()
        {
            return new MainMenuScene(_services, _sceneManager, _saveManager,
                onNewGame: StartNewGame,
                onContinue: ContinueGame,
                onSettings: () => _sceneManager.ChangeScene(BuildSettingsScene(BuildMainMenuScene())));
        }

        private SettingsScene BuildSettingsScene(Scene returnTo)
        {
            return new SettingsScene(_services, _sceneManager, () => _sceneManager.ChangeScene(returnTo));
        }

        private void StartNewGame()
        {
            _gameState = new GameState(_services);
            _sceneManager.ChangeScene(BuildGameplayScene());
        }

        private void ContinueGame()
        {
            _gameState = new GameState(_services);
            var saveData = _saveManager.Load();
            if (saveData != null)
                _gameState.ApplySaveData(saveData);
            _sceneManager.ChangeScene(BuildGameplayScene());
        }

        private GameplayScene BuildGameplayScene()
        {
            return new GameplayScene(_services, _sceneManager, _gameState, _saveManager,
                onWin: () => _sceneManager.ChangeScene(new VictoryScene(_services, _sceneManager, _gameState.PlayTimeSeconds, ReturnToMainMenu)),
                onLose: () => _sceneManager.ChangeScene(new DefeatScene(_services, _sceneManager, ReturnToMainMenu)),
                onExitToMenu: ReturnToMainMenu);
        }

        private void ReturnToMainMenu()
        {
            _sceneManager.ChangeScene(BuildMainMenuScene());
        }

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.Escape) && !_prevKeys.IsKeyDown(Keys.Escape))
            {
                // Escape is handled per-scene where relevant (e.g. closing a puzzle);
                // no global quit-on-escape to avoid accidental progress loss.
            }

            _sceneManager.CurrentScene?.Update(gameTime, mouse, _prevMouse, keys, _prevKeys);

            _prevMouse = mouse;
            _prevKeys = keys;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(4, 6, 10));

            _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            var screenBounds = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _sceneManager.CurrentScene?.Draw(_spriteBatch, screenBounds);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
