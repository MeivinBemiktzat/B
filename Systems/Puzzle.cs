using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// Base class for every interactive puzzle in the game. A puzzle has a
    /// stable Id (used for save/load), a Hebrew display title, and tracks
    /// whether it is currently open (shown as a modal overlay) and solved.
    /// </summary>
    public abstract class Puzzle
    {
        public string Id { get; }
        public string Title { get; }
        public bool IsOpen { get; set; }
        public bool IsSolved { get; private set; }

        protected readonly GameServices Services;

        protected Puzzle(string id, string title, GameServices services)
        {
            Id = id;
            Title = title;
            Services = services;
        }

        /// <summary>Called when the player opens this puzzle's overlay (resets attempt state).</summary>
        public virtual void Open()
        {
            IsOpen = true;
        }

        /// <summary>Called when the player closes/cancels the puzzle overlay without solving it.</summary>
        public virtual void Close()
        {
            IsOpen = false;
        }

        /// <summary>Marks the puzzle solved, plays the standard success sound, and closes it shortly after.</summary>
        protected void MarkSolved()
        {
            IsSolved = true;
            Services.Audio.PlaySfx("puzzle_solve.wav");
        }

        /// <summary>
        /// Restores a puzzle to the solved state when loading a save game,
        /// without re-triggering the solve sound effect.
        /// </summary>
        public void MarkSolvedSilently()
        {
            IsSolved = true;
        }

        public abstract void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys);

        public abstract void Draw(SpriteBatch spriteBatch, Rectangle screenBounds);
    }
}
