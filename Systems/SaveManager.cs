using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// Data that gets persisted to disk: current room, solved puzzles,
    /// collected inventory items, and elapsed play time.
    /// </summary>
    public class SaveData
    {
        public string CurrentRoomId { get; set; } = "corridor_hub";
        public List<string> SolvedPuzzleIds { get; set; } = new List<string>();
        public List<string> UnlockedRoomIds { get; set; } = new List<string> { "corridor_hub" };
        public List<string> CollectedItemIds { get; set; } = new List<string>();
        public double PlayTimeSeconds { get; set; } = 0;
        public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Handles writing/reading SaveData as JSON under the user's AppData
    /// (Windows: %APPDATA%\EscapeSpaceStation\savegame.json).
    /// </summary>
    public class SaveManager
    {
        private readonly string _saveFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public SaveManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "EscapeSpaceStation");
            Directory.CreateDirectory(dir);
            _saveFilePath = Path.Combine(dir, "savegame.json");
        }

        public bool SaveExists() => File.Exists(_saveFilePath);

        public void Save(SaveData data)
        {
            data.SavedAtUtc = DateTime.UtcNow;
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(_saveFilePath, json);
        }

        public SaveData Load()
        {
            if (!SaveExists()) return null;
            try
            {
                string json = File.ReadAllText(_saveFilePath);
                return JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
            }
            catch
            {
                // Corrupt save file - treat as no save rather than crashing.
                return null;
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
        }
    }
}
