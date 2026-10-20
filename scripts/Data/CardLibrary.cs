using System.Collections.Generic;
using Godot;

namespace GhettoBeasts.Data
{
    /// <summary>Loads per-card visual resources from res://data/cards/&lt;id&gt;.tres with a neutral fallback.</summary>
    public static class CardLibrary
    {
        public const string CardDirectory = "res://data/cards";

        private static readonly Dictionary<string, CardData> Cache = new();

        public static CardData Get(string catalogId)
        {
            if (Cache.TryGetValue(catalogId, out var cached)) return cached;

            string path = $"{CardDirectory}/{catalogId}.tres";
            CardData? data = ResourceLoader.Exists(path) ? ResourceLoader.Load<CardData>(path) : null;
            if (data is null)
            {
                GD.PushWarning($"[CardLibrary] Keine Visual-Resource unter {path} – nutze Platzhalter.");
                bool isNpc = ContentCatalog.TryGetCard(catalogId, out var card) && card.Kind == CardKind.Npc;
                data = isNpc ? new NpcData { CatalogId = catalogId } : new BeastData { CatalogId = catalogId };
            }

            Cache[catalogId] = data;
            return data;
        }

        public static void ClearCache() => Cache.Clear();
    }
}
