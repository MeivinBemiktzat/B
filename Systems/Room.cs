using System.Collections.Generic;

namespace EscapeSpaceStation.Systems
{
    /// <summary>Static data describing one explorable area of the station.</summary>
    public class Room
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string BackgroundImageKey { get; }

        /// <summary>
        /// Ids of puzzles that live in this room. Solving every puzzle in
        /// UnlockRequirementPuzzleIds elsewhere is what unlocks this room
        /// from the corridor hub (see GameState.IsRoomUnlocked).
        /// </summary>
        public List<string> PuzzleIds { get; } = new List<string>();

        /// <summary>Puzzle id(s) that must be solved (usually in the corridor hub) before this room unlocks.</summary>
        public List<string> UnlockRequirementPuzzleIds { get; } = new List<string>();

        public Room(string id, string displayName, string backgroundImageKey)
        {
            Id = id;
            DisplayName = displayName;
            BackgroundImageKey = backgroundImageKey;
        }
    }

    /// <summary>A collectible inventory item, awarded by solving a puzzle or finding a hidden component.</summary>
    public class Item
    {
        public string Id { get; }
        public string DisplayName { get; }

        public Item(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}
