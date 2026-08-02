using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.Puzzles;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Scenes
{
    /// <summary>
    /// The main gameplay loop: shows the current room's background, lets the
    /// player click puzzle hotspots to open puzzle overlays, click doorway
    /// hotspots (in the corridor hub) to travel between rooms, tracks the
    /// countdown timer, draws the inventory/HUD bar, and handles save/load
    /// and pause/exit-to-menu.
    /// </summary>
    public class GameplayScene : Scene
    {
        private readonly GameState _gameState;
        private readonly SaveManager _saveManager;
        private readonly System.Action _onWin;
        private readonly System.Action _onLose;
        private readonly System.Action _onExitToMenu;

        private Puzzle _openPuzzle;
        private string _openPuzzleRoomHint;
        private double _toastTimer;
        private string _toastText;
        private double _glitchTimer;

        private UiButton _saveBtn;
        private UiButton _menuBtn;
        private UiButton _closePuzzleBtn;

        // Fixed hotspot layout (normalized 0..1) for each puzzle within its room,
        // and for room-to-room doorways within the corridor hub.
        private static readonly Dictionary<string, Rectangle> PuzzleHotspotsNormalizedTenths = new Dictionary<string, Rectangle>();

        public GameplayScene(GameServices services, SceneManager manager, GameState gameState, SaveManager saveManager,
            System.Action onWin, System.Action onLose, System.Action onExitToMenu)
            : base(services, manager)
        {
            _gameState = gameState;
            _saveManager = saveManager;
            _onWin = onWin;
            _onLose = onLose;
            _onExitToMenu = onExitToMenu;
        }

        public override void OnEnter()
        {
            _saveBtn = new UiButton(new Rectangle(30, 30, 160, 50), "שמור משחק");
            _menuBtn = new UiButton(new Rectangle(210, 30, 160, 50), "תפריט ראשי");
            _closePuzzleBtn = new UiButton(new Rectangle(0, 0, 44, 44), "X");
            Services.Audio.PlayMusic("ambient_station.ogg");
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            _gameState.PlayTimeSeconds += dt;
            if (_toastTimer > 0) _toastTimer -= dt;
            if (_glitchTimer > 0) _glitchTimer -= dt;

            if (!_gameState.HasWon)
            {
                _gameState.TimeRemainingSeconds -= dt;
                if (_gameState.TimeRemainingSeconds <= 0)
                {
                    _gameState.HasLost = true;
                    _onLose?.Invoke();
                    return;
                }
            }

            if (_openPuzzle != null)
            {
                UpdateOpenPuzzle(gameTime, mouse, prevMouse, keys, prevKeys);
                return; // modal: no room interaction while a puzzle overlay is open
            }

            if (_saveBtn.WasClicked(mouse, prevMouse))
            {
                _saveManager.Save(_gameState.ToSaveData());
                ShowToast("המשחק נשמר");
                Services.Audio.PlaySfx("button_click.wav");
                return;
            }
            if (_menuBtn.WasClicked(mouse, prevMouse))
            {
                _saveManager.Save(_gameState.ToSaveData());
                Services.Audio.PlaySfx("button_click.wav");
                _onExitToMenu?.Invoke();
                return;
            }

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            var room = _gameState.Rooms[_gameState.CurrentRoomId];
            var roomBounds = RoomDrawBounds();

            // Hidden-item puzzles render directly on the room and need raw click testing first.
            foreach (var puzzleId in room.PuzzleIds)
            {
                if (_gameState.Puzzles[puzzleId] is HiddenItemPuzzle hp && !hp.IsSolved)
                {
                    if (hp.TryClick(mouse.Position, roomBounds, out var foundItem))
                    {
                        _gameState.CollectItem(foundItem);
                        if (hp.IsSolved)
                        {
                            _gameState.RefreshUnlocks();
                            CheckWin();
                        }
                        return;
                    }
                }
            }

            // Puzzle console hotspots (rectangles drawn as glowing panels on the room image).
            int consoleIndex = 0;
            foreach (var puzzleId in room.PuzzleIds)
            {
                var puzzle = _gameState.Puzzles[puzzleId];
                if (puzzle is HiddenItemPuzzle) { consoleIndex++; continue; }

                var hotspot = ConsoleHotspotRect(consoleIndex, roomBounds);
                if (hotspot.Contains(mouse.Position))
                {
                    OpenPuzzle(puzzle);
                    return;
                }
                consoleIndex++;
            }

            // Corridor hub: click a door to travel to an unlocked room; back arrow returns from other rooms.
            if (_gameState.CurrentRoomId == "corridor_hub")
            {
                int doorIndex = 0;
                foreach (var targetRoom in _gameState.Rooms.Values.Where(r => r.Id != "corridor_hub"))
                {
                    var doorRect = DoorHotspotRect(doorIndex, roomBounds);
                    if (doorRect.Contains(mouse.Position) && _gameState.UnlockedRoomIds.Contains(targetRoom.Id))
                    {
                        _gameState.CurrentRoomId = targetRoom.Id;
                        Services.Audio.PlaySfx("door_unlock.wav");
                        return;
                    }
                    doorIndex++;
                }
            }
            else
            {
                var backRect = new Rectangle(roomBounds.X + 20, roomBounds.Bottom - 80, 160, 56);
                if (backRect.Contains(mouse.Position))
                {
                    _gameState.CurrentRoomId = "corridor_hub";
                    Services.Audio.PlaySfx("button_click.wav");
                }
            }
        }

        private void UpdateOpenPuzzle(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            bool wasSolved = _openPuzzle.IsSolved;
            _openPuzzle.Update(gameTime, mouse, prevMouse, keys, prevKeys);

            if (!wasSolved && _openPuzzle.IsSolved)
            {
                _gameState.RefreshUnlocks();
                _glitchTimer = 0.4;
                CheckWin();
            }

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            var closeRect = new Rectangle(960 + 300, 200, 44, 44);
            if (clicked && closeRect.Contains(mouse.Position))
            {
                _openPuzzle.Close();
                _openPuzzle = null;
            }
        }

        private void CheckWin()
        {
            if (_gameState.IsPuzzleSolved("airlock_override"))
            {
                _gameState.HasWon = true;
                _onWin?.Invoke();
            }
        }

        private void OpenPuzzle(Puzzle puzzle)
        {
            if (puzzle.IsSolved) return;
            puzzle.Open();
            _openPuzzle = puzzle;
        }

        private void ShowToast(string text)
        {
            _toastText = text;
            _toastTimer = 2.0;
        }

        private Rectangle RoomDrawBounds() => new Rectangle(0, 100, 1920, 880);

        /// <summary>Layout for up to 3 non-hidden-item puzzle consoles within a room, left to right.</summary>
        private Rectangle ConsoleHotspotRect(int index, Rectangle roomBounds)
        {
            int w = 260, h = 180;
            int spacing = 60;
            int totalW = w * 2 + spacing;
            int startX = roomBounds.Center.X - totalW / 2;
            int y = roomBounds.Y + 550;
            return new Rectangle(startX + index * (w + spacing), y, w, h);
        }

        /// <summary>Layout for up to 5 doorway hotspots in the corridor hub, arranged in a row.</summary>
        private Rectangle DoorHotspotRect(int index, Rectangle roomBounds)
        {
            int w = 240, h = 400;
            int spacing = 40;
            int totalW = w * 5 + spacing * 4;
            int startX = roomBounds.Center.X - totalW / 2;
            int y = roomBounds.Y + 280;
            return new Rectangle(startX + index * (w + spacing), y, w, h);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            var room = _gameState.Rooms[_gameState.CurrentRoomId];
            var roomBounds = RoomDrawBounds();
            var bg = Services.Assets.GetTexture("rooms", room.BackgroundImageKey);
            spriteBatch.Draw(bg, roomBounds, Color.White);

            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(0, 0, screenBounds.Width, 100), new Color(4, 6, 10) * 0.92f);

            spriteBatch.DrawString(Services.FontLarge, room.DisplayName, new Vector2(400, 25), Color.White);

            DrawCountdown(spriteBatch);
            DrawInventoryBar(spriteBatch);

            _saveBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, Mouse.GetState().Position);
            _menuBtn.Draw(spriteBatch, Services.Assets.Pixel, Services.Font, Mouse.GetState().Position);

            // Hidden item hotspots drawn inline on the room image
            foreach (var puzzleId in room.PuzzleIds)
            {
                if (_gameState.Puzzles[puzzleId] is HiddenItemPuzzle hp)
                    DrawHiddenItemHotspots(spriteBatch, hp, roomBounds);
            }

            // Puzzle consoles
            int consoleIndex = 0;
            foreach (var puzzleId in room.PuzzleIds)
            {
                var puzzle = _gameState.Puzzles[puzzleId];
                if (puzzle is HiddenItemPuzzle) { consoleIndex++; continue; }
                DrawConsole(spriteBatch, puzzle, ConsoleHotspotRect(consoleIndex, roomBounds));
                consoleIndex++;
            }

            if (_gameState.CurrentRoomId == "corridor_hub")
            {
                int doorIndex = 0;
                foreach (var targetRoom in _gameState.Rooms.Values.Where(r => r.Id != "corridor_hub"))
                {
                    DrawDoor(spriteBatch, targetRoom, DoorHotspotRect(doorIndex, roomBounds));
                    doorIndex++;
                }
            }
            else
            {
                var backRect = new Rectangle(roomBounds.X + 20, roomBounds.Bottom - 80, 160, 56);
                UiPanel.DrawButtonBackground(spriteBatch, Services.Assets.Pixel, backRect, backRect.Contains(Mouse.GetState().Position));
                spriteBatch.DrawString(Services.Font, "< מסדרון", new Vector2(backRect.X + 20, backRect.Y + 15), Color.White);
            }

            if (_toastTimer > 0)
            {
                var toastRect = new Rectangle(screenBounds.Center.X - 200, 120, 400, 50);
                spriteBatch.Draw(Services.Assets.Pixel, toastRect, new Color(20, 60, 40) * 0.9f);
                var size = Services.Font.MeasureString(_toastText);
                spriteBatch.DrawString(Services.Font, _toastText, new Vector2(toastRect.Center.X - size.X / 2f, toastRect.Center.Y - size.Y / 2f), new Color(120, 255, 170));
            }

            if (_openPuzzle != null)
            {
                spriteBatch.Draw(Services.Assets.Pixel, screenBounds, Color.Black * 0.6f);
                _openPuzzle.Draw(spriteBatch, screenBounds);

                var closeRect = new Rectangle(960 + 300, 200, 44, 44);
                UiPanel.DrawButtonBackground(spriteBatch, Services.Assets.Pixel, closeRect, closeRect.Contains(Mouse.GetState().Position), new Color(220, 90, 90));
                spriteBatch.DrawString(Services.Font, "X", new Vector2(closeRect.X + 14, closeRect.Y + 8), Color.White);
            }

            if (_glitchTimer > 0)
                UiPanel.DrawGlitchFlicker(spriteBatch, Services.Assets.Pixel, screenBounds, new System.Random((int)(_glitchTimer * 1000)), (float)(_glitchTimer / 0.4));

            UiPanel.DrawScanlines(spriteBatch, Services.Assets.Pixel, screenBounds, 0.03f);
        }

        private void DrawConsole(SpriteBatch spriteBatch, Puzzle puzzle, Rectangle rect)
        {
            Color accent = puzzle.IsSolved ? new Color(60, 220, 120) : new Color(0, 190, 230);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, rect, accent);

            var titleSize = Services.Font.MeasureString(puzzle.Title);
            spriteBatch.DrawString(Services.Font, puzzle.Title,
                new Vector2(rect.Center.X - titleSize.X / 2f, rect.Y + 20), Color.White);

            string status = puzzle.IsSolved ? "פתור" : "לחץ לפתיחה";
            var statusSize = Services.Font.MeasureString(status);
            spriteBatch.DrawString(Services.Font, status,
                new Vector2(rect.Center.X - statusSize.X / 2f, rect.Bottom - 40), accent);
        }

        private void DrawDoor(SpriteBatch spriteBatch, Room room, Rectangle rect)
        {
            bool unlocked = _gameState.UnlockedRoomIds.Contains(room.Id);
            Color accent = unlocked ? new Color(60, 220, 120) : new Color(140, 40, 40);

            spriteBatch.Draw(Services.Assets.Pixel, rect, unlocked ? new Color(15, 30, 22) : new Color(30, 15, 15));
            int t = 4;
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, t), accent);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), accent);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.X, rect.Y, t, rect.Height), accent);
            spriteBatch.Draw(Services.Assets.Pixel, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), accent);

            var nameSize = Services.Font.MeasureString(room.DisplayName);
            spriteBatch.DrawString(Services.Font, room.DisplayName, new Vector2(rect.Center.X - nameSize.X / 2f, rect.Bottom - 60), Color.White);

            string status = unlocked ? "פתוח" : "נעול";
            var statusSize = Services.Font.MeasureString(status);
            spriteBatch.DrawString(Services.Font, status, new Vector2(rect.Center.X - statusSize.X / 2f, rect.Center.Y), accent);
        }

        private void DrawHiddenItemHotspots(SpriteBatch spriteBatch, HiddenItemPuzzle puzzle, Rectangle roomBounds)
        {
            foreach (var hotspot in puzzle.Hotspots)
            {
                if (hotspot.Found) continue;
                var screenPos = new Vector2(
                    roomBounds.X + hotspot.PositionNormalized.X * roomBounds.Width,
                    roomBounds.Y + hotspot.PositionNormalized.Y * roomBounds.Height);

                float pulse = 0.5f + 0.5f * (float)System.Math.Sin(_gameState.PlayTimeSeconds * 4.0);
                var glowRect = new Rectangle((int)(screenPos.X - 18), (int)(screenPos.Y - 18), 36, 36);
                spriteBatch.Draw(Services.Assets.Pixel, glowRect, new Color(255, 220, 100) * (0.3f + 0.3f * pulse));

                var dotRect = new Rectangle((int)(screenPos.X - 6), (int)(screenPos.Y - 6), 12, 12);
                spriteBatch.Draw(Services.Assets.Pixel, dotRect, new Color(255, 220, 100));
            }
        }

        private void DrawCountdown(SpriteBatch spriteBatch)
        {
            int minutes = (int)(_gameState.TimeRemainingSeconds / 60);
            int seconds = (int)(_gameState.TimeRemainingSeconds % 60);
            string text = $"זמן עד לקריסת תחנה: {minutes:00}:{seconds:00}";
            Color color = _gameState.TimeRemainingSeconds < 300 ? new Color(255, 90, 90) : new Color(200, 210, 220);
            var size = Services.Font.MeasureString(text);
            spriteBatch.DrawString(Services.Font, text, new Vector2(1920 - size.X - 40, 35), color);
        }

        private void DrawInventoryBar(SpriteBatch spriteBatch)
        {
            int x = 1920 - 40;
            int y = 70;
            foreach (var itemId in _gameState.CollectedItemIds)
            {
                var item = _gameState.ItemCatalog[itemId];
                var size = Services.Font.MeasureString(item.DisplayName);
                x -= (int)size.X + 30;
            }
        }
    }
}
