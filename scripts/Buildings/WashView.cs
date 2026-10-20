using GhettoBeasts.Core;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public partial class WashView : BuildingShell
    {
        private const int DefaultAmount = 100;
        private SpinBox amountBox = null!;

        protected override string Title => "WASCHSALON";
        protected override string Subtitle => "Dirty Cash wird gewaschen. 20 % Gebühr für den Laden.";
        protected override Color Accent => Palette.CleanCash;

        protected override void BuildContent()
        {
            Body.AddChild(UiKit.Label("Wie viel Schmutzgeld reinigen?", 16, Palette.TextDim));

            amountBox = new SpinBox
            {
                MinValue = 10,
                MaxValue = 10000,
                Step = 10,
                Value = DefaultAmount,
                CustomMinimumSize = new Vector2(0f, 40f),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            Body.AddChild(amountBox);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);
            Body.AddChild(row);

            foreach (int preset in new[] { 50, 100, 200, 500 })
            {
                int capture = preset;
                var p = UiKit.Button($"{capture} $");
                p.CustomMinimumSize = new Vector2(90f, 40f);
                p.Pressed += () => amountBox.Value = capture;
                row.AddChild(p);
            }

            var wash = UiKit.Button("WASCHEN", "Dirty → Clean mit Gebühr");
            wash.CustomMinimumSize = new Vector2(0f, 52f);
            wash.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            wash.Pressed += OnWash;
            Body.AddChild(wash);

            Body.AddChild(UiKit.Label(
                $"Formel: Clean = Dirty × (1 − {Economy.LaunderFee:P0}). Beispiel: 100 $ → {(int)(100 * (1 - Economy.LaunderFee))} $ Clean.",
                13, Palette.TextDim));
        }

        private void OnWash()
        {
            int amount = (int)amountBox.Value;
            int clean = GameManager.Instance.Economy.Launder(amount);
            if (clean <= 0)
            {
                SetStatus("Nicht genug Schmutzgeld.", true);
                return;
            }
            Persist();
            SetStatus($"Gewaschen: {amount} $ Dirty → {clean} $ Clean.");
        }
    }
}
