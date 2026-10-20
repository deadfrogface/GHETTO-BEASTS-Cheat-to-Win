using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Combat;
using GhettoBeasts.Data;

namespace GhettoBeasts.Core
{
    /// <summary>Owns roster, loadouts and starter unlocks. Pure C# — no Godot.</summary>
    public static class Roster
    {
        public static readonly string[] StarterCards =
        {
            "kupferstecher_pitbull",
            "kanalratte",
            "hinterhof_crackhead",
            "chantal"
        };

        public static readonly string[] StarterMutations =
        {
            "gorilla_kiefer",
            "klingen_pranken",
            "chitin_panzer",
            "piranha_gebiss",
            "kaenguru_sprungbeine",
            "aal_strom_haut"
        };

        public static void EnsureDefaults(SaveData save)
        {
            if (save is null) throw new ArgumentNullException(nameof(save));
            save.OwnedCardIds ??= new List<string>();
            save.OwnedMutationIds ??= new List<string>();
            save.EquippedMutationIds ??= new List<string>();
            save.InventoryItems ??= new List<string>();
            save.Loadouts ??= new Dictionary<string, BeastLoadout>(StringComparer.Ordinal);

            if (save.OwnedCardIds.Count == 0)
                save.OwnedCardIds.AddRange(StarterCards);
            foreach (var id in StarterMutations)
            {
                if (!save.OwnedMutationIds.Contains(id))
                    save.OwnedMutationIds.Add(id);
            }

            // Legacy saves only had SelectedCardId — prefer it and unlock if missing from roster.
            if (!string.IsNullOrWhiteSpace(save.SelectedCardId) && ContentCatalog.TryGetCard(save.SelectedCardId, out _))
            {
                TryAwardCard(save, save.SelectedCardId);
                save.ActiveFighterId = save.SelectedCardId;
            }
            if (string.IsNullOrWhiteSpace(save.ActiveFighterId) || !OwnsCard(save, save.ActiveFighterId))
                save.ActiveFighterId = save.OwnedCardIds[0];
            save.SelectedCardId = save.ActiveFighterId;

            SyncLegacyEquipped(save);
            StreetProgression.EnsureDefaults(save);
            save.Version = SaveData.CurrentVersion;
        }

        public static bool OwnsCard(SaveData save, string cardId) =>
            !string.IsNullOrEmpty(cardId) && save.OwnedCardIds.Contains(cardId);

        public static bool OwnsMutation(SaveData save, string mutationId) =>
            !string.IsNullOrEmpty(mutationId) && save.OwnedMutationIds.Contains(mutationId);

        public static bool TryAwardCard(SaveData save, string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || !ContentCatalog.TryGetCard(cardId, out _))
                return false;
            if (OwnsCard(save, cardId)) return false;
            save.OwnedCardIds.Add(cardId);
            return true;
        }

        public static bool TryAwardMutation(SaveData save, string mutationId)
        {
            if (string.IsNullOrEmpty(mutationId) || ContentCatalog.Mutations.All(m => m.Id != mutationId))
                return false;
            if (OwnsMutation(save, mutationId)) return false;
            save.OwnedMutationIds.Add(mutationId);
            return true;
        }

        public static void SetActiveFighter(SaveData save, string cardId)
        {
            if (!OwnsCard(save, cardId))
                throw new InvalidOperationException($"Karte '{cardId}' ist nicht im Roster.");
            save.ActiveFighterId = cardId;
            save.SelectedCardId = cardId;
            SyncLegacyEquipped(save);
        }

        public static BeastLoadout GetOrCreateLoadout(SaveData save, string cardId)
        {
            if (!save.Loadouts.TryGetValue(cardId, out var loadout) || loadout is null)
            {
                loadout = new BeastLoadout();
                save.Loadouts[cardId] = loadout;
            }
            loadout.MutationIds ??= new List<string>();
            return loadout;
        }

        public static IReadOnlyList<string> GetEquippedMutationIds(SaveData save, string cardId)
        {
            if (!save.Loadouts.TryGetValue(cardId, out var loadout) || loadout?.MutationIds is null)
            {
                if (cardId == save.ActiveFighterId || cardId == save.SelectedCardId)
                    return save.EquippedMutationIds;
                return Array.Empty<string>();
            }
            return loadout.MutationIds;
        }

        public static IReadOnlyList<MutationDefinition> ResolveMutations(SaveData save, string cardId) =>
            GetEquippedMutationIds(save, cardId)
                .Where(id => ContentCatalog.Mutations.Any(m => m.Id == id))
                .Select(ContentCatalog.GetMutation)
                .ToList();

        public static int PreviewInstability(SaveData save, string cardId)
        {
            var card = ContentCatalog.GetCard(cardId);
            var mutations = ResolveMutations(save, cardId);
            int raw = Combatant.PreviewInstability(card, mutations);
            var loadout = GetOrCreateLoadout(save, cardId);
            return CrisprLab.ApplyStabilizers(raw, loadout.StabilizerStacks);
        }

        public static IEnumerable<CardDefinition> OwnedCards(SaveData save)
        {
            foreach (var id in save.OwnedCardIds)
            {
                if (ContentCatalog.TryGetCard(id, out var card))
                    yield return card;
            }
        }

        public static void SyncLegacyEquipped(SaveData save)
        {
            var ids = GetEquippedMutationIds(save, save.ActiveFighterId).ToList();
            save.EquippedMutationIds = ids;
            if (save.Loadouts.TryGetValue(save.ActiveFighterId, out var loadout) && loadout is not null)
                loadout.MutationIds = ids;
            else if (ids.Count > 0)
                GetOrCreateLoadout(save, save.ActiveFighterId).MutationIds = ids;
        }
    }
}
