using Godot;

namespace GhettoBeasts.Data
{
    [GlobalClass]
    public partial class NpcData : CardData
    {
        [Export] public Color SkinColor { get; set; } = new(0.85f, 0.65f, 0.5f);
        [Export] public bool HasHat { get; set; }

        public NpcData()
        {
            Shape = BodyShape.Humanoid;
        }
    }
}
