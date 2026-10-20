using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public partial class WeapShopView : ShopViewBase
    {
        protected override string Title => "WEAP SHOP";
        protected override string Subtitle => "Schlagringe, Messer, Billig-Taser. Pure Street-Optik.";
        protected override Color Accent => Palette.Danger;
        protected override ShopKind Kind => ShopKind.Weapons;
    }
}
