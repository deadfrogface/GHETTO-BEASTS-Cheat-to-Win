using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class CatalogAndEconomyTests
{
    [Fact]
    public void Catalog_contains_full_design_roster()
    {
        Assert.Equal(8, ContentCatalog.Beasts.Count);
        Assert.Equal(7, ContentCatalog.Npcs.Count);
        Assert.Equal(9, ContentCatalog.Mutations.Count);
        foreach (MutationSlot slot in Enum.GetValues<MutationSlot>())
            Assert.Equal(3, ContentCatalog.MutationsForSlot(slot).Count());
        Assert.Equal(ContentCatalog.AllCards.Count(), ContentCatalog.AllCards.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void Mutations_add_stats_and_instability()
    {
        var pit = Fixtures.Player("kupferstecher_pitbull", "gorilla_kiefer", "klingen_pranken", "chitin_panzer");
        var baseStats = ContentCatalog.GetCard("kupferstecher_pitbull").BaseStats;

        Assert.Equal(baseStats.Attack + 15 + 22, pit.Stats.Attack);
        Assert.Equal(baseStats.Defense + 20, pit.Stats.Defense);
        Assert.Equal(15 + 30 + 15, pit.Instability);
    }

    [Fact]
    public void Npcs_cannot_be_mutated_and_slots_are_exclusive()
    {
        Assert.Throws<InvalidOperationException>(() => Fixtures.Player("chantal", "gorilla_kiefer"));
        Assert.Throws<InvalidOperationException>(() => Fixtures.Player("kanalratte", "gorilla_kiefer", "piranha_gebiss"));
    }

    [Fact]
    public void Laundering_takes_twenty_percent_fee()
    {
        var economy = new Economy(dirtyCash: 200);
        int clean = economy.Launder(100);

        Assert.Equal(80, clean);
        Assert.Equal(100, economy.DirtyCash);
        Assert.Equal(80, economy.CleanCash);
        Assert.Equal(0, economy.Launder(1000));
    }

    [Fact]
    public void Confiscation_wipes_dirty_cash_only()
    {
        var economy = new Economy(350, 120, 3);
        Assert.Equal(350, economy.ConfiscateDirtyCash());
        Assert.Equal(0, economy.DirtyCash);
        Assert.Equal(120, economy.CleanCash);
        Assert.Equal(3, economy.Diamonds);
    }

    [Fact]
    public void Save_roundtrip_preserves_values()
    {
        var data = new SaveData { DirtyCash = 42, SelectedCardId = "cyber_pit", EquippedMutationIds = { "fledermaus_sonar" }, Wins = 3 };
        var restored = SaveSerializer.Deserialize(SaveSerializer.Serialize(data));

        Assert.Equal(42, restored.DirtyCash);
        Assert.Equal("cyber_pit", restored.SelectedCardId);
        Assert.Equal(new[] { "fledermaus_sonar" }, restored.EquippedMutationIds);
        Assert.Equal(3, restored.Wins);
    }

    [Theory]
    [InlineData(40, 0.0, InstabilityOutcome.None)]
    [InlineData(60, 0.10, InstabilityOutcome.Confusion)]
    [InlineData(60, 0.20, InstabilityOutcome.None)]
    [InlineData(85, 0.30, InstabilityOutcome.CellFailure)]
    [InlineData(85, 0.40, InstabilityOutcome.None)]
    [InlineData(100, 0.40, InstabilityOutcome.WildMutation)]
    [InlineData(100, 0.60, InstabilityOutcome.Collapse)]
    public void Instability_thresholds(int instability, double roll, InstabilityOutcome expected)
    {
        // Critical rolls consume a second value (cell failure vs. rage); 0.30 -> cell failure.
        var random = new ScriptedRandom(roll, roll);
        Assert.Equal(expected, InstabilityResolver.Roll(instability, random));
    }
}
