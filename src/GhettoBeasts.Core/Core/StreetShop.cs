using System;
using System.Collections.Generic;
using System.Linq;

namespace GhettoBeasts.Core
{
    public enum ShopKind
    {
        Dope,
        Weapons
    }

    public sealed record ShopOffer(string Id, string Name, string Description, Currency Currency, int Price, string ItemId);

    public sealed record ShopPurchase(bool Success, string Message, string? ItemId);

    /// <summary>Simple DOPE / WEAP stubs — buy flavor items into SaveData.InventoryItems.</summary>
    public static class StreetShop
    {
        private static readonly IReadOnlyList<ShopOffer> DopeOffers = new List<ShopOffer>
        {
            new("needle_pack", "Gebrauchte Nadeln", "Kampf-Flavor. +1 Inventar-Stack.", Currency.DirtyCash, 40, "needle_pack"),
            new("street_boost", "Straßen-Boost", "Dosen-Zeug für späteres Doping.", Currency.DirtyCash, 90, "street_boost"),
            new("clean_serum", "Labor-Serum", "Mit Clean Cash gekauft — wirkt seriöser.", Currency.CleanCash, 60, "clean_serum")
        };

        private static readonly IReadOnlyList<ShopOffer> WeaponOffers = new List<ShopOffer>
        {
            new("brass_knuckles", "Schlagring", "Für die Gasse. Pure Optik.", Currency.DirtyCash, 55, "brass_knuckles"),
            new("switchblade", "Springmesser", "Klappmesser vom Schwarzmarkt.", Currency.DirtyCash, 75, "switchblade"),
            new("taser_clone", "Billig-Taser", "Funken und schlechte Garantie.", Currency.CleanCash, 110, "taser_clone")
        };

        public static IReadOnlyList<ShopOffer> Offers(ShopKind kind) => kind switch
        {
            ShopKind.Dope => DopeOffers,
            ShopKind.Weapons => WeaponOffers,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        public static ShopPurchase TryBuy(SaveData save, Economy economy, ShopKind kind, string offerId)
        {
            Roster.EnsureDefaults(save);
            var offer = Offers(kind).FirstOrDefault(o => o.Id == offerId);
            if (offer is null) return new ShopPurchase(false, "Unbekanntes Angebot.", null);
            if (!economy.TrySpend(offer.Currency, offer.Price))
                return new ShopPurchase(false, $"Nicht genug {offer.Currency}.", null);
            save.InventoryItems.Add(offer.ItemId);
            return new ShopPurchase(true, $"{offer.Name} gekauft.", offer.ItemId);
        }
    }
}
