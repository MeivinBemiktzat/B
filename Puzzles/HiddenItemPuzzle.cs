using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSpaceStation.Systems;

namespace EscapeSpaceStation.Puzzles
{
    /// <summary>
    /// Puzzle: Hidden Item Search.
    /// Not a modal overlay like the others -- this puzzle lives directly on
    /// the room background. Small interactive hotspots (rendered as subtle
    /// glowing dots) are scattered across the room image; clicking all of
    /// them "solves" the puzzle (found all hidden components).
    /// </summary>
    public class HiddenItemPuzzle : Puzzle
    {
        public class Hotspot
        {
            public Vector2 PositionNormalized; // 0..1 relative to room background size
            public bool Found;
            public string ItemId;
        }

        public List<Hotspot> Hotspots { get; }

        public HiddenItemPuzzle(GameServices services, List<Hotspot> hotspots) : base("hidden_items", "חיפוש רכיבים נסתרים", services)
        {
            Hotspots = hotspots;
            IsOpen = true; // always "open" -- rendered inline in the room, not as a popup
        }

        public override void Open() { /* no-op: always active within its room */ }
        public override void Close() { /* no-op */ }

        /// <summary>Called by the Room/GameplayScene with the room's on-screen draw rectangle.</summary>
        public bool TryClick(Point mousePos, Rectangle roomDrawBounds, out string foundItemId)
        {
            foundItemId = null;
            if (IsSolved) return false;

            foreach (var hotspot in Hotspots)
            {
                if (hotspot.Found) continue;

                var screenPos = new Vector2(
                    roomDrawBounds.X + hotspot.PositionNormalized.X * roomDrawBounds.Width,
                    roomDrawBounds.Y + hotspot.PositionNormalized.Y * roomDrawBounds.Height);

                float radius = 30f;
                if (Vector2.Distance(screenPos, mousePos.ToVector2()) <= radius)
                {
                    hotspot.Found = true;
                    foundItemId = hotspot.ItemId;
                    Services.Audio.PlaySfx("item_pickup.wav");

                    bool allFound = true;
                    foreach (var h in Hotspots) if (!h.Found) allFound = false;
                    if (allFound) MarkSolved();

                    return true;
                }
            }
            return false;
        }

        public override void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys)
        {
            // Click handling is driven externally via TryClick() because this
            // puzzle needs the room's draw rectangle, which only the scene knows.
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            // Drawing is handled by GameplayScene (it needs the room draw rect too);
            // see GameplayScene.DrawHiddenItemHotspots().
        }
    }
}
