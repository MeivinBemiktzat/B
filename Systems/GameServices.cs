using Microsoft.Xna.Framework.Graphics;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// Bundles the shared services (asset loading, audio, fonts) that every
    /// scene and puzzle needs, so they don't each have to carry five separate
    /// constructor parameters around.
    /// </summary>
    public class GameServices
    {
        public AssetManager Assets { get; }
        public AudioManager Audio { get; }
        public SettingsManager Settings { get; }
        public SpriteFont Font { get; set; }
        public SpriteFont FontLarge { get; set; }

        public GameServices(AssetManager assets, AudioManager audio, SettingsManager settings)
        {
            Assets = assets;
            Audio = audio;
            Settings = settings;
        }
    }
}
