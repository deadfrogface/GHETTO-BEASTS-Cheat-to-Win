using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Data;

namespace GhettoBeasts.Core
{
    public enum GachaRewardKind
    {
        Card,
        Mutation,
        DuplicateDiamonds
    }

    public sealed record GachaPull(
        GachaRewardKind Kind,
        string Id,
        string DisplayName,
        Rarity Rarity,
        bool IsNew,
        int BonusDiamonds = 0);

    public sealed record GachaResult(bool Success, string Message, GachaPull? Pull);

    /// <summary>Darknet pack opening: spend Diamonds, weighted rarity roll, award card or CRISPR gene.</summary>
    public static class DarknetGacha
    {
        public const int PackCostDiamonds = 1;

        private static readonly (Rarity Rarity, double Weight)[] RarityWeights =
        {
            (Rarity.Common, 45),
            (Rarity.Uncommon, 28),
            (Rarity.Rare, 16),
            (Rarity.Epic, 8),
            (Rarity.Legendary, 3)
        };

        public static Rarity RollRarity(IRandom random)
        {
            double total = RarityWeights.Sum(w => w.Weight);
            double roll = random.NextDouble() * total;
            double cursor = 0;
            foreach (var (rarity, weight) in RarityWeights)
            {
                cursor += weight;
                if (roll < cursor) return rarity;
            }
            return Rarity.Common;
        }

        public static GachaResult OpenPack(SaveData save, Economy economy, IRandom random)
        {
            if (save is null) throw new ArgumentNullException(nameof(save));
            if (economy is null) throw new ArgumentNullException(nameof(economy));
            if (random is null) throw new ArgumentNullException(nameof(random));

            Roster.EnsureDefaults(save);
            if (!economy.TrySpend(Currency.Diamonds, PackCostDiamonds))
                return new GachaResult(false, $"Nicht genug Diamonds (braucht {PackCostDiamonds}).", null);

            var rarity = RollRarity(random);
            bool rollMutation = random.Chance(30);
            if (rollMutation)
            {
                var genes = ContentCatalog.Mutations.Where(m => true).ToList();
                var gene = genes[random.Range(0, genes.Count)];
                bool isNew = Roster.TryAwardMutation(save, gene.Id);
                int bonus = 0;
                if (!isNew)
                {
                    bonus = 1;
                    economy.Add(Currency.Diamonds, bonus);
                }
                save.PacksOpened++;
                return new GachaResult(true, isNew ? $"Neues Gen: {gene.Name}" : $"Duplikat-Gen → +{bonus} ◆",
                    new GachaPull(isNew ? GachaRewardKind.Mutation : GachaRewardKind.DuplicateDiamonds,
                        gene.Id, gene.Name, rarity, isNew, bonus));
            }

            var pool = ContentCatalog.AllCards.Where(c => c.Rarity == rarity).ToList();
            if (pool.Count == 0)
                pool = ContentCatalog.AllCards.Where(c => c.Rarity == Rarity.Common).ToList();
            var card = pool[random.Range(0, pool.Count)];
            bool newCard = Roster.TryAwardCard(save, card.Id);
            int dupeBonus = 0;
            if (!newCard)
            {
                dupeBonus = RarityDupeRefund(card.Rarity);
                economy.Add(Currency.Diamonds, dupeBonus);
            }
            save.PacksOpened++;
            return new GachaResult(true,
                newCard ? $"Neue Karte: {card.Name} [{card.Rarity.DisplayName()}]"
                    : $"Duplikat {card.Name} → +{dupeBonus} ◆",
                new GachaPull(newCard ? GachaRewardKind.Card : GachaRewardKind.DuplicateDiamonds,
                    card.Id, card.Name, card.Rarity, newCard, dupeBonus));
        }

        public static int RarityDupeRefund(Rarity rarity) => rarity switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 1,
            Rarity.Rare => 2,
            Rarity.Epic => 3,
            Rarity.Legendary => 5,
            Rarity.Boss => 8,
            _ => 1
        };

        public static IReadOnlyList<(Rarity Rarity, double Weight)> Weights => RarityWeights;
    }
}
