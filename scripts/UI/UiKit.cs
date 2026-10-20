using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    public static class UiKit
    {
        public static Theme CreateTheme()
        {
            var theme = new Theme { DefaultFontSize = 15 };

            theme.SetStylebox("panel", "PanelContainer", Box(Palette.PanelBg, Palette.NeonPurple.Darkened(0.2f), 2, 8, 12));
            theme.SetStylebox("panel", "PopupMenu", Box(new Color(0.06f, 0.03f, 0.12f, 0.98f), Palette.NeonPink, 2, 6, 6));
            theme.SetColor("font_color", "Label", Colors.White);
            theme.SetColor("default_color", "RichTextLabel", Colors.White);

            foreach (var type in new[] { "Button", "OptionButton" })
            {
                theme.SetStylebox("normal", type, Box(new Color(0.12f, 0.05f, 0.2f, 0.95f), Palette.NeonPink.Darkened(0.25f), 2, 6, 10));
                theme.SetStylebox("hover", type, Box(new Color(0.22f, 0.07f, 0.32f, 0.98f), Palette.NeonPink, 2, 6, 10));
                theme.SetStylebox("pressed", type, Box(new Color(0.35f, 0.08f, 0.4f, 1f), Palette.NeonCyan, 2, 6, 10));
                theme.SetStylebox("focus", type, Box(new Color(0f, 0f, 0f, 0f), Palette.NeonCyan, 2, 6, 10));
                theme.SetStylebox("disabled", type, Box(new Color(0.08f, 0.06f, 0.1f, 0.8f), new Color(0.3f, 0.25f, 0.35f), 1, 6, 10));
                theme.SetColor("font_color", type, Colors.White);
                theme.SetColor("font_hover_color", type, Colors.White);
                theme.SetColor("font_pressed_color", type, Palette.NeonCyan);
                theme.SetColor("font_disabled_color", type, new Color(0.5f, 0.45f, 0.55f));
                theme.SetFontSize("font_size", type, 15);
            }

            theme.SetStylebox("background", "ProgressBar", Box(new Color(0.02f, 0.01f, 0.05f, 0.9f), new Color(0.25f, 0.2f, 0.35f), 1, 4, 0));
            theme.SetStylebox("fill", "ProgressBar", Box(Palette.NeonGreen, Palette.NeonGreen, 0, 4, 0));
            theme.SetStylebox("panel", "TooltipPanel", Box(new Color(0.05f, 0.02f, 0.1f, 0.97f), Palette.NeonCyan, 1, 4, 8));
            return theme;
        }

        public static StyleBoxFlat Box(Color background, Color border, int borderWidth, int radius, int padding)
        {
            var box = new StyleBoxFlat { BgColor = background, BorderColor = border };
            box.SetBorderWidthAll(borderWidth);
            box.SetCornerRadiusAll(radius);
            box.SetContentMarginAll(padding);
            return box;
        }

        public static Label Label(string text, int size = 15, Color? color = null, HorizontalAlignment align = HorizontalAlignment.Left)
        {
            var label = new Label { Text = text, HorizontalAlignment = align, VerticalAlignment = VerticalAlignment.Center };
            label.AddThemeFontSizeOverride("font_size", size);
            if (color.HasValue) label.AddThemeColorOverride("font_color", color.Value);
            label.AddThemeConstantOverride("outline_size", size >= 24 ? 6 : 0);
            label.AddThemeColorOverride("font_outline_color", new Color(0.02f, 0f, 0.05f));
            return label;
        }

        public static ProgressBar Bar(Color fill, float height = 14f)
        {
            var bar = new ProgressBar
            {
                ShowPercentage = false,
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                CustomMinimumSize = new Vector2(0f, height),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
            };
            SetBarColor(bar, fill);
            return bar;
        }

        public static void SetBarColor(ProgressBar bar, Color fill) =>
            bar.AddThemeStyleboxOverride("fill", Box(fill, fill.Lightened(0.3f), 0, 4, 0));

        public static Button Button(string text, string tooltip = "")
        {
            return new Button
            {
                Text = text,
                TooltipText = tooltip,
                CustomMinimumSize = new Vector2(150f, 44f),
                FocusMode = Control.FocusModeEnum.None
            };
        }

        public static string Hex(Color color) => color.ToHtml(false);
    }
}
