using Godot;

namespace GhettoBeasts.Data
{
    [GlobalClass]
    public partial class MutationData : Resource
    {
        [Export] public string CatalogId { get; set; } = string.Empty;
        [Export] public Color GlowColor { get; set; } = new(0.7f, 0.2f, 1f);
        [Export] public Texture2D? Icon { get; set; }

        public MutationDefinition? Definition
        {
            get
            {
                foreach (var mutation in ContentCatalog.Mutations)
                    if (mutation.Id == CatalogId) return mutation;
                GD.PrintErr($"[MutationData] Unbekannte CatalogId '{CatalogId}' in {ResourcePath}.");
                return null;
            }
        }
    }
}
