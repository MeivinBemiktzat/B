using System.Collections.Generic;
using System.Linq;
using EscapeSpaceStation.Puzzles;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// The live, in-memory representation of a playthrough: which rooms are
    /// unlocked, which puzzles are solved, what's in the inventory, and how
    /// much time has elapsed. GameplayScene reads/writes this every frame;
    /// SaveManager persists a snapshot of it (SaveData) to disk.
    ///
    /// Room layout / story structure:
    ///   corridor_hub (always unlocked) contains: control_panel, cable_wiring
    ///     -> solving both unlocks control_room
    ///   control_room contains: sequence_activation, binary_decode
    ///     -> solving both unlocks engine_room
    ///   engine_room contains: circuit_breaker, hidden_items
    ///     -> solving both unlocks laboratory
    ///   laboratory contains: symbol_match, document_code
    ///     -> solving both unlocks comms_room
    ///   comms_room contains: frequency_tuner
    ///     -> solving it unlocks emergency_room
    ///   emergency_room contains: airlock_override (the final puzzle -> Victory)
    /// </summary>
    public class GameState
    {
        public Dictionary<string, Room> Rooms { get; } = new Dictionary<string, Room>();
        public Dictionary<string, Puzzle> Puzzles { get; } = new Dictionary<string, Puzzle>();
        public Dictionary<string, Item> ItemCatalog { get; } = new Dictionary<string, Item>();

        public string CurrentRoomId { get; set; } = "corridor_hub";
        public HashSet<string> UnlockedRoomIds { get; } = new HashSet<string> { "corridor_hub" };
        public HashSet<string> CollectedItemIds { get; } = new HashSet<string>();

        public double PlayTimeSeconds { get; set; } = 0;

        /// <summary>
        /// Countdown-to-disaster in seconds. If it reaches zero before the
        /// player solves the final puzzle, the Defeat scene is triggered.
        /// Set to a generous value so it's a background tension mechanic,
        /// not a punishing hard timer.
        /// </summary>
        public double TimeRemainingSeconds { get; set; } = 45 * 60; // 45 minutes

        public bool HasWon { get; set; }
        public bool HasLost { get; set; }

        public GameState(GameServices services)
        {
            BuildRooms();
            BuildItemCatalog();
            BuildPuzzles(services);
        }

        private void BuildRooms()
        {
            var hub = new Room("corridor_hub", "מסדרון מרכזי", "corridor_hub");
            hub.PuzzleIds.Add("control_panel");
            hub.PuzzleIds.Add("cable_wiring");
            Rooms[hub.Id] = hub;

            var control = new Room("control_room", "חדר בקרה ראשי", "control_room");
            control.PuzzleIds.Add("sequence_activation");
            control.PuzzleIds.Add("binary_decode");
            control.UnlockRequirementPuzzleIds.Add("control_panel");
            control.UnlockRequirementPuzzleIds.Add("cable_wiring");
            Rooms[control.Id] = control;

            var engine = new Room("engine_room", "חדר מנועים", "engine_room");
            engine.PuzzleIds.Add("circuit_breaker");
            engine.PuzzleIds.Add("hidden_items");
            engine.UnlockRequirementPuzzleIds.Add("sequence_activation");
            engine.UnlockRequirementPuzzleIds.Add("binary_decode");
            Rooms[engine.Id] = engine;

            var lab = new Room("laboratory", "מעבדה", "laboratory");
            lab.PuzzleIds.Add("symbol_match");
            lab.PuzzleIds.Add("document_code");
            lab.UnlockRequirementPuzzleIds.Add("circuit_breaker");
            lab.UnlockRequirementPuzzleIds.Add("hidden_items");
            Rooms[lab.Id] = lab;

            var comms = new Room("comms_room", "חדר תקשורת", "comms_room");
            comms.PuzzleIds.Add("frequency_tuner");
            comms.UnlockRequirementPuzzleIds.Add("symbol_match");
            comms.UnlockRequirementPuzzleIds.Add("document_code");
            Rooms[comms.Id] = comms;

            var emergency = new Room("emergency_room", "חדר חירום", "emergency_room");
            emergency.PuzzleIds.Add("airlock_override");
            emergency.UnlockRequirementPuzzleIds.Add("frequency_tuner");
            Rooms[emergency.Id] = emergency;
        }

        private void BuildItemCatalog()
        {
            ItemCatalog["keycard_red"] = new Item("keycard_red", "כרטיס גישה אדום");
            ItemCatalog["keycard_blue"] = new Item("keycard_blue", "כרטיס גישה כחול");
            ItemCatalog["power_cell"] = new Item("power_cell", "תא כוח נייד");
        }

        private void BuildPuzzles(GameServices services)
        {
            Puzzles["control_panel"] = new ControlPanelPuzzle(services);
            Puzzles["cable_wiring"] = new CableWiringPuzzle(services);
            Puzzles["sequence_activation"] = new SequenceActivationPuzzle(services);
            Puzzles["binary_decode"] = new BinaryDecodePuzzle(services);
            Puzzles["circuit_breaker"] = new CircuitBreakerPuzzle(services);

            var hotspots = new List<HiddenItemPuzzle.Hotspot>
            {
                new HiddenItemPuzzle.Hotspot { PositionNormalized = new Microsoft.Xna.Framework.Vector2(0.22f, 0.68f), ItemId = "power_cell" },
                new HiddenItemPuzzle.Hotspot { PositionNormalized = new Microsoft.Xna.Framework.Vector2(0.55f, 0.35f), ItemId = null },
                new HiddenItemPuzzle.Hotspot { PositionNormalized = new Microsoft.Xna.Framework.Vector2(0.80f, 0.72f), ItemId = null },
            };
            Puzzles["hidden_items"] = new HiddenItemPuzzle(services, hotspots);

            Puzzles["symbol_match"] = new SymbolMatchPuzzle(services);
            Puzzles["document_code"] = new DocumentCodePuzzle(services);
            Puzzles["frequency_tuner"] = new FrequencyTunerPuzzle(services);
            Puzzles["airlock_override"] = new AirlockOverridePuzzle(services, this);
        }

        public bool IsPuzzleSolved(string puzzleId) => Puzzles.TryGetValue(puzzleId, out var p) && p.IsSolved;

        public bool HasItem(string itemId) => CollectedItemIds.Contains(itemId);

        public void CollectItem(string itemId)
        {
            if (itemId != null && ItemCatalog.ContainsKey(itemId))
                CollectedItemIds.Add(itemId);
        }

        /// <summary>Call after any puzzle is solved to re-evaluate room unlocks and grant fixed key items.</summary>
        public void RefreshUnlocks()
        {
            foreach (var room in Rooms.Values)
            {
                if (UnlockedRoomIds.Contains(room.Id)) continue;
                if (room.UnlockRequirementPuzzleIds.Count == 0) continue;
                if (room.UnlockRequirementPuzzleIds.All(IsPuzzleSolved))
                    UnlockedRoomIds.Add(room.Id);
            }

            // Award the two keycards the moment their originating rooms are fully cleared,
            // so the player always has what they need for the final airlock puzzle.
            if (IsPuzzleSolved("sequence_activation") && IsPuzzleSolved("binary_decode"))
                CollectItem("keycard_red");
            if (IsPuzzleSolved("symbol_match") && IsPuzzleSolved("document_code"))
                CollectItem("keycard_blue");

            if (IsPuzzleSolved("airlock_override"))
                HasWon = true;
        }

        public IEnumerable<string> SolvedPuzzleIds => Puzzles.Where(kv => kv.Value.IsSolved).Select(kv => kv.Key);

        public SaveData ToSaveData()
        {
            return new SaveData
            {
                CurrentRoomId = CurrentRoomId,
                SolvedPuzzleIds = SolvedPuzzleIds.ToList(),
                UnlockedRoomIds = UnlockedRoomIds.ToList(),
                CollectedItemIds = CollectedItemIds.ToList(),
                PlayTimeSeconds = PlayTimeSeconds
            };
        }

        public void ApplySaveData(SaveData data)
        {
            if (data == null) return;

            CurrentRoomId = data.CurrentRoomId;
            PlayTimeSeconds = data.PlayTimeSeconds;

            UnlockedRoomIds.Clear();
            foreach (var id in data.UnlockedRoomIds) UnlockedRoomIds.Add(id);

            CollectedItemIds.Clear();
            foreach (var id in data.CollectedItemIds) CollectedItemIds.Add(id);

            foreach (var puzzleId in data.SolvedPuzzleIds)
            {
                if (Puzzles.TryGetValue(puzzleId, out var puzzle))
                    puzzle.MarkSolvedSilently();
            }

            RefreshUnlocks();
        }
    }
}
