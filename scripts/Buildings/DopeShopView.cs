using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public partial class DopeShopView : ShopViewBase
    {
        protected override string Title => "DOPE SHOP";
        protected override string Subtitle => "Schwarzmarkt-Chemie. Items landen im Inventar (Stub).";
        protected override Color Accent => Palette.NeonGreen;
        protected override ShopKind Kind => ShopKind.Dope;
    }
}
