using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GhettoBeasts.Core
{
    public sealed class SaveData
    {
        public int Version { get; set; } = 1;
        public int DirtyCash { get; set; } = 300;
        public int CleanCash { get; set; } = 100;
        public int Diamonds { get; set; } = 5;
        public string SelectedCardId { get; set; } = "kupferstecher_pitbull";
        public List<string> EquippedMutationIds { get; set; } = new();
        public string LastOpponentId { get; set; } = "kanalratte";
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Raids { get; set; }
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
            if (string.IsNullOrWhiteSpace(json)) return new SaveData();
            try
            {
                return JsonConvert.DeserializeObject<SaveData>(json, Settings) ?? new SaveData();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Savegame ist beschädigt.", ex);
            }
        }
    }
}
