using Godot;

namespace GhettoBeasts.Data
{
    public enum BodyShape
    {
        Rodent,
        Quadruped,
        Reptile,
        Primate,
        Insect,
        Humanoid
    }

    /// <summary>
    /// Visual/editor data for a card. Gameplay values live in <see cref="ContentCatalog"/> and are looked up via <see cref="CatalogId"/>.
    /// Assign a <see cref="ModelScene"/> to replace the procedural placeholder with a real 3D model.
    /// </summary>
    [GlobalClass]
    public partial class CardData : Resource
    {
        [Export] public string CatalogId { get; set; } = string.Empty;
        [Export] public BodyShape Shape { get; set; } = BodyShape.Quadruped;
        [Export] public Color PrimaryColor { get; set; } = new(0.6f, 0.6f, 0.65f);
        [Export] public Color SecondaryColor { get; set; } = new(0.3f, 0.3f, 0.35f);
        [Export] public Color EyeColor { get; set; } = new(1f, 0.2f, 0.3f);
        [Export(PropertyHint.Range, "0.2,3,0.05")] public float ModelScale { get; set; } = 1f;
        [Export] public Texture2D? Artwork { get; set; }
        [Export] public PackedScene? ModelScene { get; set; }

        public CardDefinition? Definition
        {
            get
            {
                if (ContentCatalog.TryGetCard(CatalogId, out var card)) return card;
                GD.PrintErr($"[CardData] Unbekannte CatalogId '{CatalogId}' in {ResourcePath}.");
                return null;
            }
        }
    }
}
