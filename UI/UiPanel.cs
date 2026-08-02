using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSpaceStation.UI
{
    /// <summary>
    /// Shared drawing helpers for the sci-fi "holographic panel" look used by
    /// every puzzle overlay and menu screen: a dark translucent backdrop with
    /// a glowing colored border and clipped corner notches, plus full-screen
    /// atmosphere effects (scanlines, electrical glitch flicker).
    /// </summary>
    public static class UiPanel
    {
        public static readonly Color DefaultAccent = new Color(0, 210, 255);

        /// <summary>Draws a dark glass panel with a glowing accent-colored border and corner notches.</summary>
        public static void DrawFramedPanel(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, Color? accent = null)
        {
            Color accentColor = accent ?? DefaultAccent;

            // Drop shadow
            spriteBatch.Draw(pixel, new Rectangle(bounds.X + 8, bounds.Y + 8, bounds.Width, bounds.Height), Color.Black * 0.45f);

            // Backdrop (dark glass)
            spriteBatch.Draw(pixel, bounds, new Color(8, 12, 18) * 0.94f);

            // Subtle inner gradient band at top for depth
            var topBand = new Rectangle(bounds.X, bounds.Y, bounds.Width, 6);
            spriteBatch.Draw(pixel, topBand, accentColor * 0.5f);

            // Border
            int t = 3;
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, t), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - t, bounds.Width, t), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, t, bounds.Height), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - t, bounds.Y, t, bounds.Height), accentColor);

            // Corner notches (small accent squares) for a technical HUD look
            int n = 12;
            spriteBatch.Draw(pixel, new Rectangle(bounds.X - 2, bounds.Y - 2, n, n), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - n + 2, bounds.Y - 2, n, n), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X - 2, bounds.Bottom - n + 2, n, n), accentColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - n + 2, bounds.Bottom - n + 2, n, n), accentColor);
        }

        /// <summary>Draws a simple rectangular button with hover highlight; returns nothing, caller checks click separately via UiButton.</summary>
        public static void DrawButtonBackground(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, bool hovered, Color? accent = null)
        {
            Color accentColor = accent ?? DefaultAccent;
            Color fill = hovered ? Color.Lerp(new Color(20, 28, 38), accentColor, 0.25f) : new Color(16, 22, 30);
            spriteBatch.Draw(pixel, bounds, fill);
            int t = 2;
            Color borderColor = hovered ? accentColor : accentColor * 0.6f;
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, t), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - t, bounds.Width, t), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, t, bounds.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - t, bounds.Y, t, bounds.Height), borderColor);
        }

        /// <summary>Draws horizontal scanline bands across the full screen for a CRT/hologram atmosphere.</summary>
        public static void DrawScanlines(SpriteBatch spriteBatch, Texture2D pixel, Rectangle screenBounds, float alpha = 0.06f)
        {
            for (int y = screenBounds.Y; y < screenBounds.Bottom; y += 4)
            {
                spriteBatch.Draw(pixel, new Rectangle(screenBounds.X, y, screenBounds.Width, 1), Color.Black * alpha);
            }
        }

        /// <summary>
        /// Draws a brief full-screen electrical "glitch" flicker: random horizontal
        /// colored slivers and a global tint flash. Intended to be triggered for a
        /// fraction of a second (e.g. when a power system activates or fails).
        /// </summary>
        public static void DrawGlitchFlicker(SpriteBatch spriteBatch, Texture2D pixel, Rectangle screenBounds, Random rng, float intensity)
        {
            if (intensity <= 0f) return;

            spriteBatch.Draw(pixel, screenBounds, new Color(0, 210, 255) * (0.05f * intensity));

            int sliverCount = (int)(6 * intensity);
            for (int i = 0; i < sliverCount; i++)
            {
                int y = rng.Next(screenBounds.Y, screenBounds.Bottom);
                int h = rng.Next(2, 10);
                int xOffset = rng.Next(-40, 40);
                var rect = new Rectangle(screenBounds.X + xOffset, y, screenBounds.Width, h);
                spriteBatch.Draw(pixel, rect, Color.White * (0.08f * intensity));
            }
        }
    }
}
