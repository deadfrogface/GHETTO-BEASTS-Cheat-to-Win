using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class RosterGachaLabTests
{
    [Fact]
    public void Defaults_unlock_starter_roster_and_genes()
    {
        var save = SaveData.CreateDefault();
        Assert.True(save.OwnedCardIds.Count >= 4);
        Assert.Contains("kupferstecher_pitbull", save.OwnedCardIds);
        Assert.True(save.OwnedMutationIds.Count >= 6);
        Assert.Equal("kupferstecher_pitbull", save.ActiveFighterId);
    }

    [Fact]
    public void Lab_equips_one_gene_per_slot_and_persists_loadout()
    {
        var save = SaveData.CreateDefault();
        Assert.True(CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Head, "piranha_gebiss", out _));
        Assert.True(CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Limbs, "klingen_pranken", out _));
        Assert.True(CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Skin, "chitin_panzer", out _));
        Assert.False(CrisprLab.TryEquip(save, "chantal", MutationSlot.Head, "gorilla_kiefer", out var npcError));
        Assert.Contains("Bestien", npcError);

        var ids = Roster.GetEquippedMutationIds(save, "kupferstecher_pitbull");
        Assert.Equal(3, ids.Count);
        Assert.Contains("piranha_gebiss", ids);

        // Replacing head slot removes previous head gene
        Assert.True(CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Head, "gorilla_kiefer", out _));
        ids = Roster.GetEquippedMutationIds(save, "kupferstecher_pitbull");
        Assert.DoesNotContain("piranha_gebiss", ids);
        Assert.Contains("gorilla_kiefer", ids);
    }

    [Fact]
    public void Stabilizer_costs_clean_cash_and_reduces_instability()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(0, 200, 0);
        CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Head, "piranha_gebiss", out _);
        CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Limbs, "klingen_pranken", out _);
        CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Skin, "chitin_panzer", out _);

        int before = Roster.PreviewInstability(save, "kupferstecher_pitbull");
        Assert.True(CrisprLab.TryBuyStabilizer(save, economy, "kupferstecher_pitbull", out _));
        Assert.Equal(200 - CrisprLab.StabilizerCostClean, economy.CleanCash);
        int after = Roster.PreviewInstability(save, "kupferstecher_pitbull");
        Assert.Equal(before - CrisprLab.StabilizerReduction, after);

        var fighter = CrisprLab.BuildFighter(save, "kupferstecher_pitbull");
        Assert.Equal(after, fighter.Instability);
    }

    [Fact]
    public void Gacha_spends_diamond_and_awards_card_or_gene()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(0, 0, 3);
        int ownedBefore = save.OwnedCardIds.Count + save.OwnedMutationIds.Count;

        var result = DarknetGacha.OpenPack(save, economy, new ScriptedRandom(0.01, 0.9));
        Assert.True(result.Success);
        Assert.NotNull(result.Pull);
        Assert.Equal(2, economy.Diamonds);
        Assert.Equal(1, save.PacksOpened);
        Assert.True(save.OwnedCardIds.Count + save.OwnedMutationIds.Count >= ownedBefore);
    }

    [Fact]
    public void Gacha_fails_without_diamonds()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(100, 100, 0);
        var result = DarknetGacha.OpenPack(save, economy, new ScriptedRandom(0.5));
        Assert.False(result.Success);
        Assert.Null(result.Pull);
    }

    [Fact]
    public void Duplicate_gacha_refunds_diamonds()
    {
        var save = SaveData.CreateDefault();
        // Force mutation roll (chance 30 → roll 0.10) then pick first gene index 0
        var economy = new Economy(0, 0, 2);
        string firstGene = ContentCatalog.Mutations[0].Id;
        Assert.True(Roster.OwnsMutation(save, firstGene));

        var result = DarknetGacha.OpenPack(save, economy, new ScriptedRandom(0.0, 0.10, 0.0));
        Assert.True(result.Success);
        Assert.Equal(GachaRewardKind.DuplicateDiamonds, result.Pull!.Kind);
        Assert.True(economy.Diamonds >= 1); // spent 1, got refund
    }

    [Fact]
    public void Street_shop_buys_into_inventory()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(200, 100, 0);
        var buy = StreetShop.TryBuy(save, economy, ShopKind.Dope, "needle_pack");
        Assert.True(buy.Success);
        Assert.Contains("needle_pack", save.InventoryItems);
        Assert.Equal(160, economy.DirtyCash);
    }

    [Fact]
    public void Save_roundtrip_keeps_roster_and_loadouts()
    {
        var save = SaveData.CreateDefault();
        CrisprLab.TryEquip(save, "kupferstecher_pitbull", MutationSlot.Head, "gorilla_kiefer", out _);
        Roster.TryAwardCard(save, "cyber_pit");
        save.DirtyCash = 77;
        var restored = SaveSerializer.Deserialize(SaveSerializer.Serialize(save));
        Assert.Contains("cyber_pit", restored.OwnedCardIds);
        Assert.Contains("gorilla_kiefer", Roster.GetEquippedMutationIds(restored, "kupferstecher_pitbull"));
        Assert.Equal(77, restored.DirtyCash);
    }

    [Fact]
    public void Laundering_still_works_via_economy()
    {
        var economy = new Economy(dirtyCash: 100);
        Assert.Equal(80, economy.Launder(100));
    }
}
