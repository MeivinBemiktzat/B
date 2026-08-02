using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSpaceStation.UI
{
    /// <summary>
    /// A simple clickable rectangular button with sci-fi framed styling,
    /// hover highlight, and centered Hebrew label. Used throughout the menus
    /// and HUD. Caller supplies bounds and label each frame; this class only
    /// tracks nothing internally, keeping it stateless and reusable.
    /// </summary>
    public class UiButton
    {
        public Rectangle Bounds;
        public string Label;
        public bool Enabled = true;

        public UiButton(Rectangle bounds, string label)
        {
            Bounds = bounds;
            Label = label;
        }

        public bool IsHovered(Point mousePos) => Enabled && Bounds.Contains(mousePos);

        /// <summary>Returns true exactly on the frame the button is released while hovered (a "click").</summary>
        public bool WasClicked(MouseState mouse, MouseState prevMouse)
        {
            if (!Enabled) return false;
            bool released = prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            return released && Bounds.Contains(mouse.Position);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Point mousePos, Color? accent = null)
        {
            bool hovered = IsHovered(mousePos);
            Color accentColor = accent ?? UiPanel.DefaultAccent;

            Color fill = !Enabled
                ? new Color(20, 20, 24)
                : (hovered ? Color.Lerp(new Color(20, 28, 38), accentColor, 0.3f) : new Color(16, 22, 30));

            spriteBatch.Draw(pixel, Bounds, fill);

            int t = 2;
            Color borderColor = !Enabled ? new Color(60, 60, 64) : (hovered ? accentColor : accentColor * 0.6f);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, t), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - t, Bounds.Width, t), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, t, Bounds.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - t, Bounds.Y, t, Bounds.Height), borderColor);

            if (font != null && !string.IsNullOrEmpty(Label))
            {
                Vector2 size = font.MeasureString(Label);
                Vector2 pos = new Vector2(
                    Bounds.Center.X - size.X / 2f,
                    Bounds.Center.Y - size.Y / 2f);
                Color textColor = Enabled ? Color.White : new Color(120, 120, 124);
                spriteBatch.DrawString(font, Label, pos, textColor);
            }
        }
    }
}
