namespace GhettoBeasts.Combat
{
    public enum BattleEventType
    {
        Log,
        Summon,
        Attack,
        Damage,
        Heal,
        Miss,
        StatusApplied,
        SkillUsed,
        Instability,
        Cheat,
        Suspicion,
        Cash,
        Raid,
        Result
    }

    public enum CheatType
    {
        Brick,
        Doping,
        Bribe
    }

    /// <param name="Target">Side the event visually applies to (null = global).</param>
    /// <param name="Amount">Delta (damage, heal, suspicion change, cash change).</param>
    /// <param name="ValueAfter">Resulting value (HP, suspicion, instability) after the event.</param>
    public sealed record BattleEvent(
        BattleEventType Type,
        string Message,
        Side? Target = null,
        int Amount = 0,
        int ValueAfter = 0,
        bool Critical = false,
        CheatType? Cheat = null,
        StatusType? Status = null);
}
