using Godot;

namespace GhettoBeasts.Data
{
    [GlobalClass]
    public partial class BeastData : CardData
    {
        [Export] public bool HasTail { get; set; } = true;
        [Export] public bool HasEars { get; set; } = true;
    }
}
