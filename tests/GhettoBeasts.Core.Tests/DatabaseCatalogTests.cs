using GhettoBeasts.Data;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class DatabaseCatalogTests
{
    [Fact]
    public void Registry_indexes_full_roster()
    {
        var db = DatabaseCatalog.Instance;
        var snap = db.Snapshot();

        Assert.True(snap.Cards >= 100);
        Assert.True(snap.Beasts >= 50);
        Assert.True(snap.Npcs >= 50);
        Assert.True(snap.Mutations >= 9);
        Assert.True(db.TryGetCard("cyber_pit", out var cyber));
        Assert.Equal(CardKind.Beast, cyber.Kind);
        Assert.True(db.TryGetCard("el_capo", out var capo));
        Assert.Equal(Rarity.Boss, capo.Rarity);
        Assert.Contains(db.GetMutations(MutationSlot.Head), m => m.Id == "gorilla_kiefer");
        Assert.Contains(db.Search("Chantal"), c => c.Id == "chantal");
        Assert.Equal(db.BeastCount, db.GetByKind(CardKind.Beast).Count);
    }
}
