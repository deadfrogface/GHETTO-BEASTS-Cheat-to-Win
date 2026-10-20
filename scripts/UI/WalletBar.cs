using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    /// <summary>Compact currency strip reused by hub and building screens.</summary>
    public partial class WalletBar : HBoxContainer
    {
        private Label label = null!;

        public override void _Ready()
        {
            AddThemeConstantOverride("separation", 12);
            label = UiKit.Label("", 15, Palette.DirtyCash);
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            AddChild(label);
            if (GameManager.Instance is not null)
            {
                GameManager.Instance.CurrencyChanged += OnCurrency;
                Refresh();
            }
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance is not null)
                GameManager.Instance.CurrencyChanged -= OnCurrency;
        }

        private void OnCurrency(int currency, int value) => Refresh();

        public void Refresh()
        {
            if (GameManager.Instance is null) return;
            var e = GameManager.Instance.Economy;
            var s = GameManager.Instance.Save;
            label.Text = $"$ {e.DirtyCash} dirty   ·   $ {e.CleanCash} clean   ·   ◆ {e.Diamonds}   ·   Roster {s.OwnedCardIds.Count}   ·   {s.Wins}W/{s.Losses}L/{s.Raids}R";
        }
    }
}
