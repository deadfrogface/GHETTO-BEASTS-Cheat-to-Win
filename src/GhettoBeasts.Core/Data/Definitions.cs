namespace GhettoBeasts.Data
{
    public sealed record CardDefinition(
        string Id,
        string Name,
        CardKind Kind,
        Rarity Rarity,
        string Category,
        Stats BaseStats,
        AbilityId Ability,
        string AbilityName,
        string AbilityDescription,
        string Description,
        CardTraits Traits = CardTraits.Biological,
        int BaseInstability = 0)
    {
        public bool IsCyber => Traits.HasFlag(CardTraits.Cyber);
        public bool CanMutate => Kind == CardKind.Beast;
    }

    public sealed record MutationDefinition(
        string Id,
        string Name,
        MutationSlot Slot,
        Stats StatBonus,
        int Instability,
        MutationEffect Effect,
        string Description);
}
