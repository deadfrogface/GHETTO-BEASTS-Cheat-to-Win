using System.Collections.Generic;
using GhettoBeasts.Visuals;
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
                data = CreateFallback(catalogId);
            }

            Cache[catalogId] = data;
            return data;
        }

        public static void ClearCache() => Cache.Clear();

        private static CardData CreateFallback(string catalogId)
        {
            if (!ContentCatalog.TryGetCard(catalogId, out var card))
            {
                GD.PushWarning($"[CardLibrary] Unbekannte CatalogId '{catalogId}' – neutrales Placeholder.");
                return new BeastData { CatalogId = catalogId };
            }

            var archetype = ContentCatalog.GetBodyArchetype(catalogId);
            var shape = ToBodyShape(archetype);
            var (primary, secondary, eyes) = ColorsFor(card, archetype);

            if (card.Kind == CardKind.Npc)
            {
                return new NpcData
                {
                    CatalogId = catalogId,
                    Shape = BodyShape.Humanoid,
                    PrimaryColor = primary,
                    SecondaryColor = secondary,
                    EyeColor = eyes,
                    ModelScale = card.Rarity >= Rarity.Epic ? 1.1f : 1f,
                    HasHat = card.Rarity >= Rarity.Rare
                };
            }

            return new BeastData
            {
                CatalogId = catalogId,
                Shape = shape,
                PrimaryColor = primary,
                SecondaryColor = secondary,
                EyeColor = eyes,
                ModelScale = card.Rarity >= Rarity.Legendary ? 1.05f : 0.95f,
                HasEars = archetype is ContentCatalog.BodyArchetype.Rodent or ContentCatalog.BodyArchetype.Quadruped,
                HasTail = archetype is not ContentCatalog.BodyArchetype.Primate and not ContentCatalog.BodyArchetype.Insect
            };
        }

        private static BodyShape ToBodyShape(ContentCatalog.BodyArchetype archetype) => archetype switch
        {
            ContentCatalog.BodyArchetype.Rodent => BodyShape.Rodent,
            ContentCatalog.BodyArchetype.Reptile => BodyShape.Reptile,
            ContentCatalog.BodyArchetype.Primate => BodyShape.Primate,
            ContentCatalog.BodyArchetype.Insect => BodyShape.Insect,
            ContentCatalog.BodyArchetype.Humanoid => BodyShape.Humanoid,
            _ => BodyShape.Quadruped
        };

        private static (Color Primary, Color Secondary, Color Eyes) ColorsFor(CardDefinition card, ContentCatalog.BodyArchetype archetype)
        {
            float hue = (Hash(card.Id) % 1000) / 1000f;
            // Desaturated bases — neon reserved for eyes / cyber accents.
            Color primary = card.Traits.HasFlag(CardTraits.Cyber)
                ? new Color(0.42f, 0.48f, 0.55f)
                : Color.FromHsv(hue, 0.28f + (int)card.Rarity * 0.03f, 0.52f);
            Color secondary = primary.Darkened(0.28f).Lerp(PaletteSafe(card.Rarity), 0.2f);
            Color eyes = card.Traits.HasFlag(CardTraits.Cyber)
                ? new Color(0.9f, 0.28f, 0.35f)
                : Color.FromHsv((hue + 0.35f) % 1f, 0.55f, 0.9f);
            if (archetype == ContentCatalog.BodyArchetype.Reptile) primary = primary.Lerp(new Color(0.28f, 0.38f, 0.26f), 0.55f);
            if (archetype == ContentCatalog.BodyArchetype.Insect) primary = primary.Lerp(new Color(0.36f, 0.28f, 0.2f), 0.45f);
            return (primary, secondary, eyes);
        }

        private static Color PaletteSafe(Rarity rarity) => Palette.ForRarity(rarity);

        private static int Hash(string text)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in text) hash = hash * 31 + c;
                return hash < 0 ? -hash : hash;
            }
        }
    }
}
