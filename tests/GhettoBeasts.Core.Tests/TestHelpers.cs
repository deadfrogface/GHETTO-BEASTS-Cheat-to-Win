using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Data;

namespace GhettoBeasts.Core.Tests;

/// <summary>Returns queued values, then a fallback of 0.99 (no chance-based effect procs).</summary>
internal sealed class ScriptedRandom : IRandom
{
    private readonly Queue<double> values;

    public ScriptedRandom(params double[] values) => this.values = new Queue<double>(values);

    public void Enqueue(params double[] next)
    {
        foreach (var v in next) values.Enqueue(v);
    }

    public double NextDouble() => values.Count > 0 ? values.Dequeue() : 0.99;
}

internal static class Fixtures
{
    public static Combatant Player(string id, params string[] mutations) =>
        Combatant.Create(Side.Player, ContentCatalog.GetCard(id), mutations.Select(ContentCatalog.GetMutation));

    public static Combatant Enemy(string id, params string[] mutations) =>
        Combatant.Create(Side.Enemy, ContentCatalog.GetCard(id), mutations.Select(ContentCatalog.GetMutation));

    public static (BattleEngine Engine, List<BattleEvent> Events) Battle(
        Combatant player, Combatant enemy, Economy? economy = null, IRandom? random = null)
    {
        var engine = new BattleEngine(player, enemy, economy ?? new Economy(500, 0, 0), random ?? new ScriptedRandom());
        var events = new List<BattleEvent>();
        engine.EventRaised += events.Add;
        engine.Start();
        return (engine, events);
    }
}
