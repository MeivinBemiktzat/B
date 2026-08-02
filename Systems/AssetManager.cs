using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// Loads textures and sound effects directly from the Content/Images and
    /// Content/Audio folders at runtime (NOT through the MGCB content pipeline),
    /// so the game never fails to build or crash just because some art or audio
    /// files haven't been supplied yet.
    ///
    /// Every lookup accepts a base name without extension (e.g. "control_room")
    /// and will try, in order: .png, .jpg, .jpeg. If nothing is found, a solid
    /// color placeholder texture is generated and cached instead, and a warning
    /// is written to the debug output -- the game keeps running either way.
    /// </summary>
    public class AssetManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly string _imagesRoot;
        private readonly string _audioRoot;

        private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, SoundEffect> _sfxCache = new Dictionary<string, SoundEffect>();
        private readonly HashSet<string> _missingWarned = new HashSet<string>();

        public Texture2D Pixel { get; private set; }

        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };

        public AssetManager(GraphicsDevice graphicsDevice, string contentRootDir)
        {
            _graphicsDevice = graphicsDevice;
            _imagesRoot = Path.Combine(contentRootDir, "Content", "Images");
            _audioRoot = Path.Combine(contentRootDir, "Content", "Audio");

            Pixel = new Texture2D(_graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Loads a room/ui/items/fx texture by subfolder + base filename (no extension).
        /// Example: GetTexture("rooms", "control_room")
        /// </summary>
        public Texture2D GetTexture(string subfolder, string baseName, int placeholderW = 1920, int placeholderH = 1080)
        {
            string cacheKey = subfolder + "/" + baseName;
            if (_textureCache.TryGetValue(cacheKey, out var cached))
                return cached;

            string folder = Path.Combine(_imagesRoot, subfolder);
            foreach (var ext in ImageExtensions)
            {
                string fullPath = Path.Combine(folder, baseName + ext);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(fullPath))
                        {
                            var tex = Texture2D.FromStream(_graphicsDevice, stream);
                            _textureCache[cacheKey] = tex;
                            return tex;
                        }
                    }
                    catch (Exception ex)
                    {
                        WarnOnce(cacheKey, $"Failed to load image '{fullPath}': {ex.Message}");
                        break;
                    }
                }
            }

            WarnOnce(cacheKey, $"Missing image asset '{subfolder}/{baseName}' (.png/.jpg) - using placeholder.");
            var placeholder = CreatePlaceholderTexture(placeholderW, placeholderH, baseName);
            _textureCache[cacheKey] = placeholder;
            return placeholder;
        }

        /// <summary>Loads a sound effect by subfolder ("music" or "sfx") + filename including extension.</summary>
        public SoundEffect GetSoundEffect(string subfolder, string fileName)
        {
            string cacheKey = subfolder + "/" + fileName;
            if (_sfxCache.TryGetValue(cacheKey, out var cached))
                return cached;

            string fullPath = Path.Combine(_audioRoot, subfolder, fileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    using (var stream = File.OpenRead(fullPath))
                    {
                        var sfx = SoundEffect.FromStream(stream);
                        _sfxCache[cacheKey] = sfx;
                        return sfx;
                    }
                }
                catch (Exception ex)
                {
                    WarnOnce(cacheKey, $"Failed to load audio '{fullPath}': {ex.Message}");
                }
            }
            else
            {
                WarnOnce(cacheKey, $"Missing audio asset '{subfolder}/{fileName}' - sound will be skipped.");
            }

            _sfxCache[cacheKey] = null; // cache the miss so we don't keep hitting disk
            return null;
        }

        private Texture2D CreatePlaceholderTexture(int w, int h, string seedName)
        {
            w = MathHelper.Clamp(w, 4, 2048);
            h = MathHelper.Clamp(h, 4, 2048);

            // Deterministic pastel-ish color derived from the asset name so different
            // placeholders are visually distinguishable from each other.
            int hash = 0;
            foreach (char c in seedName) hash = hash * 31 + c;
            byte r = (byte)(60 + (Math.Abs(hash) % 120));
            byte g = (byte)(60 + (Math.Abs(hash / 7) % 120));
            byte b = (byte)(80 + (Math.Abs(hash / 13) % 120));
            Color fill = new Color(r, g, b);
            Color border = Color.Lerp(fill, Color.White, 0.35f);

            var tex = new Texture2D(_graphicsDevice, w, h);
            var data = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool edge = x < 4 || y < 4 || x >= w - 4 || y >= h - 4;
                    bool diagonal = Math.Abs((x % 80) - (y % 80)) < 2; // subtle diagonal hatch pattern
                    data[y * w + x] = edge ? border : (diagonal ? Color.Lerp(fill, Color.Black, 0.15f) : fill);
                }
            }
            tex.SetData(data);
            return tex;
        }

        private void WarnOnce(string key, string message)
        {
            if (_missingWarned.Add(key))
            {
                System.Diagnostics.Debug.WriteLine("[AssetManager] " + message);
            }
        }
    }
}
