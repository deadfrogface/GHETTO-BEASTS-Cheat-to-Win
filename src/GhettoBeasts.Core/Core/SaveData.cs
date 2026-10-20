using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GhettoBeasts.Core
{
    /// <summary>Per-beast CRISPR loadout persisted in the savegame.</summary>
    public sealed class BeastLoadout
    {
        public List<string> MutationIds { get; set; } = new();
        /// <summary>Each stack reduces combat instability by <see cref="CrisprLab.StabilizerReduction"/>.</summary>
        public int StabilizerStacks { get; set; }
    }

    public sealed class SaveData
    {
        public const int CurrentVersion = 3;

        public int Version { get; set; } = CurrentVersion;
        public int DirtyCash { get; set; } = 300;
        public int CleanCash { get; set; } = 100;
        public int Diamonds { get; set; } = 5;

        /// <summary>Card ids the player owns (roster).</summary>
        public List<string> OwnedCardIds { get; set; } = new();

        /// <summary>CRISPR gene ids unlocked in the lab inventory.</summary>
        public List<string> OwnedMutationIds { get; set; } = new();

        /// <summary>Active fighter used when entering the arena from the hub.</summary>
        public string ActiveFighterId { get; set; } = "kupferstecher_pitbull";

        /// <summary>Legacy alias kept in sync with <see cref="ActiveFighterId"/>.</summary>
        public string SelectedCardId { get; set; } = "kupferstecher_pitbull";

        /// <summary>Legacy flat mutation list; mirrored from the active fighter loadout.</summary>
        public List<string> EquippedMutationIds { get; set; } = new();

        public Dictionary<string, BeastLoadout> Loadouts { get; set; } = new(StringComparer.Ordinal);

        /// <summary>Flavor inventory for DOPE/WEAP shop stubs.</summary>
        public List<string> InventoryItems { get; set; } = new();

        public string LastOpponentId { get; set; } = "kanalratte";
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Raids { get; set; }
        public int PacksOpened { get; set; }

        /// <summary>Street heat 0–100; at 100 a SWAT van rolls into the district.</summary>
        public int StreetHeat { get; set; }

        public bool LawyerUnlocked { get; set; }
        public bool RefUnlocked { get; set; }
        public LaundryJob Laundry { get; set; } = new();
        public PhoneSettings Phone { get; set; } = new();

        public static SaveData CreateDefault()
        {
            var data = new SaveData();
            Roster.EnsureDefaults(data);
            return data;
        }

        /// <summary>Lawyer contact shrinks SWAT suspicion gains.</summary>
        public double SuspicionGainScale => LawyerUnlocked ? ContactsService.LawyerSuspicionScale : 1.0;
    }

    public static class SaveSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string Serialize(SaveData data) => JsonConvert.SerializeObject(data, Settings);

        public static SaveData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return SaveData.CreateDefault();
            try
            {
                var data = JsonConvert.DeserializeObject<SaveData>(json, Settings) ?? new SaveData();
                data.Laundry ??= new LaundryJob();
                data.Phone ??= new PhoneSettings();
                data.Loadouts ??= new Dictionary<string, BeastLoadout>(StringComparer.Ordinal);
                Roster.EnsureDefaults(data);
                return data;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Savegame ist beschädigt.", ex);
            }
        }
    }
}
