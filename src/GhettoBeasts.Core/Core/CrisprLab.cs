using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Combat;
using GhettoBeasts.Data;

namespace GhettoBeasts.Core
{
    /// <summary>CRISPR lab rules: equip one gene per slot, buy stabilizers with Clean Cash.</summary>
    public static class CrisprLab
    {
        public const int StabilizerCostClean = 40;
        public const int StabilizerReduction = 12;
        public const int MaxStabilizerStacks = 6;

        public static int ApplyStabilizers(int rawInstability, int stacks) =>
            Math.Max(0, Math.Min(Combatant.MaxInstability, rawInstability - Math.Max(0, stacks) * StabilizerReduction));

        public static bool TryEquip(SaveData save, string cardId, MutationSlot slot, string? mutationId, out string error)
        {
            error = string.Empty;
            if (!Roster.OwnsCard(save, cardId))
            {
                error = "Bestie nicht im Roster.";
                return false;
            }
            if (!ContentCatalog.TryGetCard(cardId, out var card) || !card.CanMutate)
            {
                error = "Nur Bestien können CRISPR-Gene tragen.";
                return false;
            }

            var loadout = Roster.GetOrCreateLoadout(save, cardId);
            loadout.MutationIds = loadout.MutationIds
                .Where(id => ContentCatalog.Mutations.Any(m => m.Id == id))
                .Where(id => ContentCatalog.GetMutation(id).Slot != slot)
                .ToList();

            if (!string.IsNullOrEmpty(mutationId))
            {
                if (!Roster.OwnsMutation(save, mutationId))
                {
                    error = "Gen nicht freigeschaltet.";
                    return false;
                }
                var mutation = ContentCatalog.GetMutation(mutationId);
                if (mutation.Slot != slot)
                {
                    error = $"Gen gehört in Slot {mutation.Slot.DisplayName()}.";
                    return false;
                }
                loadout.MutationIds.Add(mutationId);
            }

            if (cardId == save.ActiveFighterId || cardId == save.SelectedCardId)
                Roster.SyncLegacyEquipped(save);
            return true;
        }

        public static bool TryBuyStabilizer(SaveData save, Economy economy, string cardId, out string error)
        {
            error = string.Empty;
            if (!Roster.OwnsCard(save, cardId))
            {
                error = "Bestie nicht im Roster.";
                return false;
            }
            if (!ContentCatalog.TryGetCard(cardId, out var card) || !card.CanMutate)
            {
                error = "Stabilisatoren wirken nur auf Bestien.";
                return false;
            }

            var loadout = Roster.GetOrCreateLoadout(save, cardId);
            if (loadout.StabilizerStacks >= MaxStabilizerStacks)
            {
                error = "Maximale Stabilisator-Dosis erreicht.";
                return false;
            }
            if (!economy.TrySpend(Currency.CleanCash, StabilizerCostClean))
            {
                error = $"Nicht genug Clean Cash ({StabilizerCostClean} $).";
                return false;
            }
            loadout.StabilizerStacks++;
            return true;
        }

        public static Combatant BuildFighter(SaveData save, string cardId, Side side = Side.Player)
        {
            var card = ContentCatalog.GetCard(cardId);
            var mutations = card.CanMutate ? Roster.ResolveMutations(save, cardId) : Array.Empty<MutationDefinition>();
            var combatant = Combatant.Create(side, card, mutations);
            var loadout = Roster.GetOrCreateLoadout(save, cardId);
            int reduced = ApplyStabilizers(combatant.Instability, loadout.StabilizerStacks);
            combatant.ForceInstability(reduced);
            return combatant;
        }
    }
}
