using GhettoBeasts.Combat;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class PrisonEscapeTests
{
    [Fact]
    public void Digging_right_eventually_reaches_exit_or_gets_caught()
    {
        var economy = new Economy(0, 0, 2);
        var engine = new PrisonEscapeEngine(economy, new ScriptedRandom(), seed: 7);
        var log = new List<string>();
        engine.Log += log.Add;

        for (int i = 0; i < 40 && !engine.IsOver; i++)
            engine.TryMove(DigDirection.Right);

        Assert.True(engine.IsOver);
        Assert.Contains(engine.Outcome, new[] { PrisonOutcome.Escaped, PrisonOutcome.Caught });
        Assert.NotEmpty(log);
    }

    [Fact]
    public void Bribe_costs_one_diamond_and_ends_escape()
    {
        var economy = new Economy(0, 0, 1);
        var engine = new PrisonEscapeEngine(economy, new ScriptedRandom());

        Assert.True(engine.TryBribeGuard());
        Assert.Equal(PrisonOutcome.Bribed, engine.Outcome);
        Assert.Equal(0, economy.Diamonds);
        Assert.False(engine.TryBribeGuard());
    }

    [Fact]
    public void Rock_without_tools_raises_noise()
    {
        var engine = new PrisonEscapeEngine(new Economy(), new ScriptedRandom(0.99), seed: 1);
        // Force a rock neighbor by scanning – if none, skip meaningfully.
        DigDirection? towardRock = null;
        foreach (var dir in new[] { DigDirection.Up, DigDirection.Down, DigDirection.Left, DigDirection.Right })
        {
            var (dx, dy) = dir switch
            {
                DigDirection.Up => (0, -1),
                DigDirection.Down => (0, 1),
                DigDirection.Left => (-1, 0),
                _ => (1, 0)
            };
            if (engine.TileAt(engine.PlayerX + dx, engine.PlayerY + dy) == PrisonTile.Rock)
            {
                towardRock = dir;
                break;
            }
        }

        Assert.NotNull(towardRock);
        int noiseBefore = engine.Noise;
        engine.TryMove(towardRock!.Value);
        Assert.True(engine.Noise > noiseBefore || engine.Outcome == PrisonOutcome.Caught);
        Assert.Equal(0, engine.Tools);
    }
}
