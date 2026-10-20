using GhettoBeasts.Core;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public abstract partial class ShopViewBase : BuildingShell
    {
        protected abstract ShopKind Kind { get; }
        private Label inventoryLabel = null!;

        protected override void BuildContent()
        {
            foreach (var offer in StreetShop.Offers(Kind))
            {
                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 10);
                Body.AddChild(row);

                var info = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                info.AddChild(UiKit.Label(offer.Name, 16, Colors.White));
                info.AddChild(UiKit.Label($"{offer.Description}  ·  {offer.Price} {offer.Currency}", 12, Palette.TextDim));
                row.AddChild(info);

                string offerId = offer.Id;
                var buy = UiKit.Button("Kaufen");
                buy.CustomMinimumSize = new Vector2(110f, 42f);
                buy.Pressed += () => OnBuy(offerId);
                row.AddChild(buy);
            }

            inventoryLabel = UiKit.Label("", 13, Palette.TextDim);
            inventoryLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            Body.AddChild(inventoryLabel);
            RefreshInventory();
        }

        private void OnBuy(string offerId)
        {
            var result = StreetShop.TryBuy(GameManager.Instance.Save, GameManager.Instance.Economy, Kind, offerId);
            if (!result.Success)
            {
                SetStatus(result.Message, true);
                return;
            }
            Persist();
            RefreshInventory();
            SetStatus(result.Message);
        }

        private void RefreshInventory()
        {
            var items = GameManager.Instance.Save.InventoryItems;
            inventoryLabel.Text = items.Count == 0
                ? "Inventar leer."
                : $"Inventar ({items.Count}): {string.Join(", ", items)}";
        }
    }
}
