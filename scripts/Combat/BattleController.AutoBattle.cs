using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Combat
{
    /// <summary>
    /// Dev hook: <c>godot --path . -- --autobattle [--card=id] [--mutations=a,b] [--opponent=id]
    /// [--cheats=brick,doping,bribe|none] [--fresh] [--stay]</c> skips the setup screen, plays a full
    /// battle automatically and quits with exit code 0 (victory), 1 (defeat), 2 (raid) or 3 (error).
    /// </summary>
    public partial class BattleController
    {
        private const int AutoBattleMaxTurns = 80;

        private sealed record AutoBattleOptions(
            string CardId, string[] MutationIds, string OpponentId, CheatType[] Cheats, bool Fresh, bool Stay);

        private static AutoBattleOptions? ParseAutoBattleArgs()
        {
            var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var parts in OS.GetCmdlineUserArgs().Select(a => a.Split('=', 2)))
                args[parts[0]] = parts.Length > 1 ? parts[1] : string.Empty;
            if (!args.ContainsKey("--autobattle")) return null;

            string Get(string key, string fallback) =>
                args.TryGetValue(key, out var value) && value.Length > 0 ? value : fallback;
            static string[] List(string value) =>
                value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string cheatArg = Get("--cheats", "brick,doping,bribe");
            var cheats = cheatArg.Equals("none", StringComparison.OrdinalIgnoreCase)
                ? Array.Empty<CheatType>()
                : List(cheatArg).Select(c => Enum.Parse<CheatType>(c, ignoreCase: true)).ToArray();

            return new AutoBattleOptions(
                Get("--card", "kupferstecher_pitbull"),
                List(Get("--mutations", "piranha_gebiss,klingen_pranken,chitin_panzer")),
                Get("--opponent", "kanalratte"),
                cheats,
                args.ContainsKey("--fresh"),
                args.ContainsKey("--stay"));
        }

        private async Task AutoBattleAsync(AutoBattleOptions options)
        {
            try
            {
                await PlayAutoBattleAsync(options);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AutoBattle] {ex}");
                FinishAutoBattle(options, 3, "Ausnahme");
            }
        }

        private async Task PlayAutoBattleAsync(AutoBattleOptions options)
        {
            GD.Print($"[AutoBattle] {options.CardId} [{string.Join(", ", options.MutationIds)}] vs {options.OpponentId}, " +
                     $"Cheats: {(options.Cheats.Length == 0 ? "keine" : string.Join(", ", options.Cheats))}");
            if (options.Fresh) Game.ResetProgress();

            await StartBattleAsync(options.CardId, options.MutationIds, options.OpponentId);
            if (engine is null)
            {
                FinishAutoBattle(options, 3, "Setup fehlgeschlagen");
                return;
            }

            int turn = 0;
            while (engine is { IsOver: false } && turn < AutoBattleMaxTurns)
            {
                if (!engine.IsPlayersTurn)
                {
                    await Vfx.Delay(this, 0.1);
                    continue;
                }

                if (options.Cheats.Length > 0)
                {
                    var cheat = options.Cheats[turn % options.Cheats.Length];
                    if (!engine.CheatUsedThisTurn && engine.CanCheat(cheat))
                    {
                        GD.Print($"[AutoBattle] Runde {engine.Round}: Cheat {cheat}");
                        await PerformAsync(e => e.PlayerCheat(cheat));
                        if (engine is null || engine.IsOver) break;
                    }
                }

                bool skill = engine.CanUseSkill;
                GD.Print($"[AutoBattle] Runde {engine.Round}: {(skill ? "Skill" : "Angriff")} " +
                         $"(HP {engine.Player.Hp}/{engine.Player.Stats.MaxHp} vs {engine.Enemy.Hp}/{engine.Enemy.Stats.MaxHp}, " +
                         $"Verdacht {engine.Suspicion.Value})");
                await PerformAsync(skill ? e => e.PlayerSkill() : e => e.PlayerAttack());
                turn++;
            }

            if (engine?.Outcome is not { } outcome)
            {
                FinishAutoBattle(options, 3, $"kein Ergebnis nach {turn} Zügen");
                return;
            }

            await Vfx.Delay(this, 1.0);
            int code = outcome switch
            {
                BattleOutcome.Victory => 0,
                BattleOutcome.Defeat => 1,
                _ => 2
            };
            FinishAutoBattle(options, code, $"{outcome} nach {turn} Zügen, Schmutzgeld {Game.Economy.DirtyCash} $");
        }

        private void FinishAutoBattle(AutoBattleOptions options, int exitCode, string summary)
        {
            GD.Print($"[AutoBattle] Ergebnis: {summary}");
            if (!options.Stay) GetTree().Quit(exitCode);
        }
    }
}
