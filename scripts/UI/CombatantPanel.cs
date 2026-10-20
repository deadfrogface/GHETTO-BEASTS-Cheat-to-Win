using System.Linq;
using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    public partial class CombatantPanel : PanelContainer
    {
        private Label nameLabel = null!;
        private Label rarityLabel = null!;
        private ProgressBar hpBar = null!;
        private Label hpLabel = null!;
        private ProgressBar instabilityBar = null!;
        private Label instabilityLabel = null!;
        private RichTextLabel statusLabel = null!;
        private int maxHp = 1;
        private Tween? hpTween;

        public override void _Ready()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(220f, 0f);

            var root = new VBoxContainer();
            root.AddThemeConstantOverride("separation", 4);
            AddChild(root);

            var header = new HBoxContainer();
            nameLabel = UiKit.Label("—", 17, Colors.White);
            nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            nameLabel.ClipText = true;
            rarityLabel = UiKit.Label("", 12, Palette.TextDim, HorizontalAlignment.Right);
            header.AddChild(nameLabel);
            header.AddChild(rarityLabel);
            root.AddChild(header);

            var hpRow = new HBoxContainer();
            hpRow.AddChild(UiKit.Label("HP", 12, Palette.TextDim));
            hpBar = UiKit.Bar(Palette.NeonGreen, 16f);
            hpRow.AddChild(hpBar);
            hpLabel = UiKit.Label("0/0", 13, Colors.White, HorizontalAlignment.Right);
            hpLabel.CustomMinimumSize = new Vector2(70f, 0f);
            hpRow.AddChild(hpLabel);
            root.AddChild(hpRow);

            var genRow = new HBoxContainer();
            genRow.AddChild(UiKit.Label("GEN", 12, Palette.TextDim));
            instabilityBar = UiKit.Bar(Palette.NeonPurple, 8f);
            genRow.AddChild(instabilityBar);
            instabilityLabel = UiKit.Label("0%", 12, Palette.NeonPurple, HorizontalAlignment.Right);
            instabilityLabel.CustomMinimumSize = new Vector2(70f, 0f);
            genRow.AddChild(instabilityLabel);
            root.AddChild(genRow);

            statusLabel = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false, CustomMinimumSize = new Vector2(0f, 20f) };
            statusLabel.AddThemeFontSizeOverride("normal_font_size", 12);
            root.AddChild(statusLabel);
        }

        public void Bind(Combatant combatant)
        {
            var color = Palette.ForRarity(combatant.Definition.Rarity);
            nameLabel.Text = combatant.Name;
            nameLabel.AddThemeColorOverride("font_color", color);
            rarityLabel.Text = combatant.Definition.Rarity.DisplayName().ToUpperInvariant();
            rarityLabel.AddThemeColorOverride("font_color", color);
            maxHp = combatant.Stats.MaxHp;
            hpBar.MaxValue = maxHp;
            hpBar.Value = combatant.Hp;
            UpdateHpVisual(combatant.Hp);
            SetInstability(combatant.Instability);
            SetStatuses(combatant);
        }

        public void SetHp(int hp)
        {
            hpTween?.Kill();
            hpTween = CreateTween();
            hpTween.TweenProperty(hpBar, "value", (double)hp, 0.35).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            UpdateHpVisual(hp);
        }

        public void SetInstability(int value)
        {
            instabilityBar.Value = value;
            var color = value >= 100 ? Palette.Danger : value > InstabilityResolver.CriticalThreshold ? new Color(1f, 0.4f, 0.2f)
                : value > InstabilityResolver.ConfusionThreshold ? Palette.NeonPink : Palette.NeonPurple;
            UiKit.SetBarColor(instabilityBar, color);
            instabilityLabel.Text = $"{value}%";
            instabilityLabel.AddThemeColorOverride("font_color", color);
        }

        public void SetStatuses(Combatant combatant)
        {
            var parts = combatant.Statuses
                .Where(s => s.Value > 0)
                .Select(s => $"[color=#{UiKit.Hex(Palette.ForStatus(s.Key))}]{Palette.StatusName(s.Key)} {s.Value}[/color]")
                .ToList();
            if (combatant.HasMutated) parts.Insert(0, $"[color=#{UiKit.Hex(Palette.NeonPurple)}]MUTIERT[/color]");

            string skill = combatant.SkillReady
                ? $"[color=#{UiKit.Hex(Palette.NeonCyan)}]{combatant.Definition.AbilityName} bereit[/color]"
                : $"[color=#{UiKit.Hex(Palette.TextDim)}]{combatant.Definition.AbilityName}: {combatant.SkillCooldown}[/color]";
            parts.Add(skill);
            statusLabel.Text = string.Join("  ·  ", parts);
        }

        private void UpdateHpVisual(int hp)
        {
            hpLabel.Text = $"{hp}/{maxHp}";
            float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;
            var color = ratio > 0.5f ? Palette.NeonGreen : ratio > 0.25f ? Palette.NeonYellow : Palette.Danger;
            UiKit.SetBarColor(hpBar, color);
        }
    }
}
