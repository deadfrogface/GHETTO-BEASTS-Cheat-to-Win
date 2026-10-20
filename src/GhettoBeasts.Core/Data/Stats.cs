namespace GhettoBeasts.Data
{
    /// <summary>Evasion and CritChance are percentages (0-100).</summary>
    public readonly record struct Stats(int MaxHp, int Attack, int Defense, int Speed, int Evasion, int CritChance)
    {
        public static readonly Stats Zero = new(0, 0, 0, 0, 0, 0);

        public static Stats operator +(Stats a, Stats b) => new(
            a.MaxHp + b.MaxHp,
            a.Attack + b.Attack,
            a.Defense + b.Defense,
            a.Speed + b.Speed,
            a.Evasion + b.Evasion,
            a.CritChance + b.CritChance);
    }
}
