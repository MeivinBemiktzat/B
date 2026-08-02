using System;
using System.IO;
using System.Text.Json;

namespace EscapeSpaceStation.Systems
{
    public class GameSettings
    {
        public float MusicVolume { get; set; } = 0.6f;
        public float SfxVolume { get; set; } = 0.8f;
        public bool Fullscreen { get; set; } = false;
    }

    /// <summary>Persists user preferences (volume, display) separately from save games.</summary>
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        public GameSettings Current { get; private set; }

        public SettingsManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "EscapeSpaceStation");
            Directory.CreateDirectory(dir);
            _settingsFilePath = Path.Combine(dir, "settings.json");
            Current = Load();
        }

        private GameSettings Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var loaded = JsonSerializer.Deserialize<GameSettings>(json);
                    if (loaded != null) return loaded;
                }
            }
            catch { /* fall through to defaults */ }
            return new GameSettings();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
