using System;
using System.Collections.Generic;
using System.Linq;

namespace GhettoBeasts.Data
{
    /// <summary>
    /// Automated registry over <see cref="ContentCatalog"/> for 100+ units:
    /// indexed lookups by id, kind, rarity, category and mutation slot.
    /// </summary>
    public sealed class DatabaseCatalog
    {
        private static readonly Lazy<DatabaseCatalog> LazyInstance = new(() => new DatabaseCatalog());

        private readonly Dictionary<string, CardDefinition> cardsById;
        private readonly Dictionary<string, MutationDefinition> mutationsById;
        private readonly Dictionary<CardKind, List<CardDefinition>> byKind;
        private readonly Dictionary<Rarity, List<CardDefinition>> byRarity;
        private readonly Dictionary<string, List<CardDefinition>> byCategory;
        private readonly Dictionary<MutationSlot, List<MutationDefinition>> mutationsBySlot;

        public DatabaseCatalog()
        {
            cardsById = ContentCatalog.AllCards.ToDictionary(c => c.Id, StringComparer.Ordinal);
            mutationsById = ContentCatalog.Mutations.ToDictionary(m => m.Id, StringComparer.Ordinal);
            byKind = ContentCatalog.AllCards.GroupBy(c => c.Kind).ToDictionary(g => g.Key, g => g.ToList());
            byRarity = ContentCatalog.AllCards.GroupBy(c => c.Rarity).ToDictionary(g => g.Key, g => g.ToList());
            byCategory = ContentCatalog.AllCards.GroupBy(c => c.Category).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            mutationsBySlot = ContentCatalog.Mutations.GroupBy(m => m.Slot).ToDictionary(g => g.Key, g => g.ToList());
        }

        public static DatabaseCatalog Instance => LazyInstance.Value;

        public int CardCount => cardsById.Count;
        public int BeastCount => byKind.TryGetValue(CardKind.Beast, out var beasts) ? beasts.Count : 0;
        public int NpcCount => byKind.TryGetValue(CardKind.Npc, out var npcs) ? npcs.Count : 0;
        public int MutationCount => mutationsById.Count;

        public IReadOnlyCollection<string> AllCardIds => cardsById.Keys;
        public IReadOnlyCollection<string> AllMutationIds => mutationsById.Keys;
        public IReadOnlyCollection<string> Categories => byCategory.Keys;

        public bool TryGetCard(string id, out CardDefinition card) => cardsById.TryGetValue(id, out card!);

        public CardDefinition GetCard(string id) =>
            cardsById.TryGetValue(id, out var card)
                ? card
                : throw new KeyNotFoundException($"DatabaseCatalog: unbekannte Karte '{id}'.");

        public bool TryGetMutation(string id, out MutationDefinition mutation) =>
            mutationsById.TryGetValue(id, out mutation!);

        public MutationDefinition GetMutation(string id) =>
            mutationsById.TryGetValue(id, out var mutation)
                ? mutation
                : throw new KeyNotFoundException($"DatabaseCatalog: unbekannte Mutation '{id}'.");

        public IReadOnlyList<CardDefinition> GetByKind(CardKind kind) =>
            byKind.TryGetValue(kind, out var list) ? list : Array.Empty<CardDefinition>();

        public IReadOnlyList<CardDefinition> GetByRarity(Rarity rarity) =>
            byRarity.TryGetValue(rarity, out var list) ? list : Array.Empty<CardDefinition>();

        public IReadOnlyList<CardDefinition> GetByCategory(string category) =>
            byCategory.TryGetValue(category, out var list) ? list : Array.Empty<CardDefinition>();

        public IReadOnlyList<MutationDefinition> GetMutations(MutationSlot slot) =>
            mutationsBySlot.TryGetValue(slot, out var list) ? list : Array.Empty<MutationDefinition>();

        public IEnumerable<CardDefinition> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return ContentCatalog.AllCards;
            string q = query.Trim();
            return ContentCatalog.AllCards.Where(c =>
                c.Id.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.AbilityName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public CatalogSnapshot Snapshot() => new(
            CardCount,
            BeastCount,
            NpcCount,
            MutationCount,
            byCategory.Keys.OrderBy(k => k).ToArray(),
            byRarity.ToDictionary(kv => kv.Key, kv => kv.Value.Count));

        public readonly record struct CatalogSnapshot(
            int Cards,
            int Beasts,
            int Npcs,
            int Mutations,
            IReadOnlyList<string> Categories,
            IReadOnlyDictionary<Rarity, int> CountsByRarity);
    }
}
