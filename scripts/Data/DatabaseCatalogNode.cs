using Godot;

namespace GhettoBeasts.Data
{
    /// <summary>Optional scene helper exposing <see cref="DatabaseCatalog"/> counts to the Godot inspector/debug HUD.</summary>
    public partial class DatabaseCatalogNode : Node
    {
        [Export] public int CardCount { get; private set; }
        [Export] public int BeastCount { get; private set; }
        [Export] public int NpcCount { get; private set; }
        [Export] public int MutationCount { get; private set; }

        public DatabaseCatalog Catalog => DatabaseCatalog.Instance;

        public override void _Ready()
        {
            var snap = Catalog.Snapshot();
            CardCount = snap.Cards;
            BeastCount = snap.Beasts;
            NpcCount = snap.Npcs;
            MutationCount = snap.Mutations;
        }
    }
}
