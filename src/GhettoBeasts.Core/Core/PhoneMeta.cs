using System;

namespace GhettoBeasts.Core
{
    /// <summary>Burner-phone contacts, laundry jobs and presentation settings persisted in the save.</summary>
    public sealed class PhoneSettings
    {
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.75f;
        public float SfxVolume { get; set; } = 1f;
        public bool CelShading { get; set; } = true;
        public bool Outline { get; set; } = true;
        public int TargetFps { get; set; } = 60;
        public bool CloudSyncStub { get; set; }
    }

    public sealed class LaundryJob
    {
        public int DirtyAmount { get; set; }
        /// <summary>UTC unix seconds when the wash finishes.</summary>
        public long ReadyUnix { get; set; }
        public bool Active => DirtyAmount > 0 && ReadyUnix > 0;

        public static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public int SecondsRemaining(long nowUnix = 0)
        {
            if (!Active) return 0;
            long now = nowUnix > 0 ? nowUnix : NowUnix();
            return (int)Math.Max(0, ReadyUnix - now);
        }

        public bool IsReady(long nowUnix = 0) => Active && SecondsRemaining(nowUnix) <= 0;
    }

    /// <summary>Meta unlocks bought via the Burner Phone Contacts app.</summary>
    public static class ContactsService
    {
        public const int LawyerCostClean = 250;
        public const int RefCostClean = 180;
        /// <summary>Lawyer reduces all SWAT suspicion gains by 15%.</summary>
        public const double LawyerSuspicionScale = 0.85;
        /// <summary>Unlocked ref bribe uses a flat clean-cash fee instead of dirty cash.</summary>
        public const int RefFlatrateClean = 40;

        public static bool TryUnlockLawyer(SaveData save, Economy economy, out string message)
        {
            if (save.LawyerUnlocked)
            {
                message = "Anwalt ist schon im Telefonbuch.";
                return false;
            }
            if (!economy.TrySpend(Currency.CleanCash, LawyerCostClean))
            {
                message = $"Braucht {LawyerCostClean} $ Clean für den Anwalt.";
                return false;
            }
            save.LawyerUnlocked = true;
            message = "Anwalt freigeschaltet: −15% SWAT-Verdacht.";
            return true;
        }

        public static bool TryUnlockRef(SaveData save, Economy economy, out string message)
        {
            if (save.RefUnlocked)
            {
                message = "Schiri-Nummer schon gespeichert.";
                return false;
            }
            if (!economy.TrySpend(Currency.CleanCash, RefCostClean))
            {
                message = $"Braucht {RefCostClean} $ Clean für die Schiri-Nummer.";
                return false;
            }
            save.RefUnlocked = true;
            message = $"Schiri-Flatrate: Bestechung kostet {RefFlatrateClean} $ Clean.";
            return true;
        }
    }

    public static class LaundryService
    {
        public const int DefaultDurationSeconds = 45;
        public const int MinAmount = 20;

        public static bool TryStart(SaveData save, Economy economy, int dirtyAmount, out string message, int durationSeconds = DefaultDurationSeconds)
        {
            if (save.Laundry is { Active: true })
            {
                message = "Waschgang läuft schon — warte auf den Timer.";
                return false;
            }
            if (dirtyAmount < MinAmount)
            {
                message = $"Mindestens {MinAmount} $ Dirty.";
                return false;
            }
            if (!economy.TrySpend(Currency.DirtyCash, dirtyAmount))
            {
                message = "Nicht genug Schmutzgeld.";
                return false;
            }
            save.Laundry = new LaundryJob
            {
                DirtyAmount = dirtyAmount,
                ReadyUnix = LaundryJob.NowUnix() + Math.Max(5, durationSeconds)
            };
            message = $"Waschgang gestartet: {dirtyAmount} $ Dirty · fertig in {durationSeconds}s.";
            return true;
        }

        public static bool TryClaim(SaveData save, Economy economy, out string message)
        {
            var job = save.Laundry;
            if (job is null || !job.Active)
            {
                message = "Kein Waschgang aktiv.";
                return false;
            }
            if (!job.IsReady())
            {
                message = $"Noch {job.SecondsRemaining()}s bis Clean-Cash.";
                return false;
            }
            int clean = (int)Math.Floor(job.DirtyAmount * (1.0 - Economy.LaunderFee));
            economy.Add(Currency.CleanCash, clean);
            message = $"Fertig: {job.DirtyAmount} $ Dirty → {clean} $ Clean.";
            save.Laundry = new LaundryJob();
            return true;
        }
    }
}
