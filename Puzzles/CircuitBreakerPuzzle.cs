using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;
using EscapeSpaceStation.UI;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Circuit Breaker Reroute.
    /// A 3x3 grid of toggleable breaker nodes. The player must switch ON
    /// exactly the nodes forming a valid diagonal power path (a fixed
    /// solution pattern) while all other nodes remain OFF, rerouting power
    /// around the damaged main line to the laboratory.
    /// </summary>
    public class CircuitBreakerPuzzle : Puzzle
    {
        private const int GridSize = 3;
        private bool[,] _nodeOn = new bool[GridSize, GridSize];

        // Solution: the diagonal + center-cross pattern must be ON, all else OFF.
        private readonly bool[,] _solution =
        {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };

        private Rectangle _panelBounds;

        public CircuitBreakerPuzzle(GameServices services) : base("circuit_breaker", "ניתוב מעגל חשמלי", services) { }

        public override void Open()
        {
            base.Open();
            _nodeOn = new bool[GridSize, GridSize];
        }

        private Rectangle NodeRect(int row, int col, Rectangle bounds)
        {
            int cellSize = 120;
            int startX = bounds.Center.X - (cellSize * GridSize) / 2;
            int startY = bounds.Y + 110;
            return new Rectangle(startX + col * cellSize, startY + row * cellSize, cellSize - 16, cellSize - 16);
        }

        private bool CheckSolved()
        {
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (_nodeOn[r, c] != _solution[r, c]) return false;
            return true;
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!IsOpen || IsSolved) return;

            bool clicked = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            if (!clicked) return;

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    if (NodeRect(r, c, _panelBounds).Contains(mouse.Position))
                    {
                        _nodeOn[r, c] = !_nodeOn[r, c];
                        Services.Audio.PlaySfx("button_click.wav");
                        if (CheckSolved()) MarkSolved();
                        return;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (!IsOpen) return;
            _panelBounds = new Rectangle(screenBounds.Center.X - 300, screenBounds.Center.Y - 260, 600, 520);
            UiPanel.DrawFramedPanel(spriteBatch, Services.Assets.Pixel, _panelBounds, new Color(150, 100, 255));

            spriteBatch.DrawString(Services.Font, "הפעל אך ורק את הצמתים הדרושים לניתוב תקין",
                new Vector2(_panelBounds.X + 40, _panelBounds.Y + 30), Color.White);

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    var rect = NodeRect(r, c, _panelBounds);
                    Color color = _nodeOn[r, c] ? new Color(150, 100, 255) : new Color(30, 30, 40);
                    spriteBatch.Draw(Services.Assets.Pixel, rect, color);
                }
            }

            if (IsSolved)
                spriteBatch.DrawString(Services.Font, "הניתוב הושלם - המעבדה מחוברת לחשמל!", new Vector2(_panelBounds.X + 40, _panelBounds.Bottom - 60), new Color(80, 255, 140));
        }
    }
}
