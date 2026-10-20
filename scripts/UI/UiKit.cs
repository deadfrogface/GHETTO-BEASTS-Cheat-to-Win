using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    public static class UiKit
    {
        public const string FontPath = "res://assets/fonts/Outfit-Regular.ttf";

        private static FontFile? uiFont;

        public static FontFile? Font
        {
            get
            {
                if (uiFont is not null) return uiFont;
                if (ResourceLoader.Exists(FontPath))
                    uiFont = ResourceLoader.Load<FontFile>(FontPath);
                return uiFont;
            }
        }

        public static Theme CreateTheme()
        {
            var theme = new Theme { DefaultFontSize = 14 };
            if (Font is { } font) theme.DefaultFont = font;

            theme.SetStylebox("panel", "PanelContainer", Box(Palette.PanelBg, Palette.PanelBorder, 1, 10, 10));
            theme.SetStylebox("panel", "PopupMenu", Box(new Color(0.08f, 0.09f, 0.12f, 0.95f), Palette.NeonCyan.Darkened(0.35f), 1, 8, 6));
            theme.SetColor("font_color", "Label", Colors.White);
            theme.SetColor("default_color", "RichTextLabel", Colors.White);

            foreach (var type in new[] { "Button", "OptionButton" })
            {
                theme.SetStylebox("normal", type, Box(Palette.ButtonBg, Palette.PanelBorder, 1, 10, 8));
                theme.SetStylebox("hover", type, Box(new Color(0.16f, 0.18f, 0.24f, 0.92f), Palette.NeonCyan.Darkened(0.2f), 1, 10, 8));
                theme.SetStylebox("pressed", type, Box(new Color(0.2f, 0.24f, 0.3f, 0.95f), Palette.NeonCyan, 1, 10, 8));
                theme.SetStylebox("focus", type, Box(new Color(0f, 0f, 0f, 0f), Palette.NeonCyan, 1, 10, 8));
                theme.SetStylebox("disabled", type, Box(new Color(0.1f, 0.1f, 0.12f, 0.7f), new Color(0.3f, 0.3f, 0.35f), 1, 10, 8));
                theme.SetColor("font_color", type, Colors.White);
                theme.SetColor("font_hover_color", type, Colors.White);
                theme.SetColor("font_pressed_color", type, Palette.NeonCyan);
                theme.SetColor("font_disabled_color", type, new Color(0.45f, 0.46f, 0.5f));
                theme.SetFontSize("font_size", type, 14);
                if (Font is { } f) theme.SetFont("font", type, f);
            }

            theme.SetStylebox("background", "ProgressBar", Box(new Color(0.05f, 0.05f, 0.07f, 0.85f), new Color(0.28f, 0.3f, 0.35f, 0.6f), 1, 5, 0));
            theme.SetStylebox("fill", "ProgressBar", Box(Palette.NeonGreen, Palette.NeonGreen.Lightened(0.15f), 0, 5, 0));
            theme.SetStylebox("panel", "TooltipPanel", Box(new Color(0.07f, 0.08f, 0.11f, 0.96f), Palette.NeonCyan.Darkened(0.3f), 1, 6, 8));
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

        public static Label Label(string text, int size = 14, Color? color = null, HorizontalAlignment align = HorizontalAlignment.Left)
        {
            var label = new Label { Text = text, HorizontalAlignment = align, VerticalAlignment = VerticalAlignment.Center };
            label.AddThemeFontSizeOverride("font_size", size);
            if (Font is { } font) label.AddThemeFontOverride("font", font);
            if (color.HasValue) label.AddThemeColorOverride("font_color", color.Value);
            label.AddThemeConstantOverride("outline_size", size >= 28 ? 5 : 0);
            label.AddThemeColorOverride("font_outline_color", Palette.Outline);
            return label;
        }

        public static ProgressBar Bar(Color fill, float height = 10f)
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
            bar.AddThemeStyleboxOverride("fill", Box(fill, fill.Lightened(0.2f), 0, 5, 0));

        public static Button Button(string text, string tooltip = "")
        {
            return new Button
            {
                Text = text,
                TooltipText = tooltip,
                CustomMinimumSize = new Vector2(128f, 36f),
                FocusMode = Control.FocusModeEnum.None
            };
        }

        public static Button CompactButton(string text, string tooltip = "")
        {
            var button = Button(text, tooltip);
            button.CustomMinimumSize = new Vector2(108f, 34f);
            button.AddThemeFontSizeOverride("font_size", 13);
            button.AddThemeStyleboxOverride("normal", Box(new Color(0.1f, 0.11f, 0.15f, 0.78f), Palette.PanelBorder, 1, 12, 6));
            button.AddThemeStyleboxOverride("hover", Box(new Color(0.15f, 0.17f, 0.22f, 0.88f), Palette.NeonCyan.Darkened(0.15f), 1, 12, 6));
            button.AddThemeStyleboxOverride("pressed", Box(new Color(0.18f, 0.22f, 0.28f, 0.92f), Palette.NeonCyan, 1, 12, 6));
            button.AddThemeStyleboxOverride("disabled", Box(new Color(0.08f, 0.08f, 0.1f, 0.55f), new Color(0.25f, 0.25f, 0.3f), 1, 12, 6));
            return button;
        }

        public static string Hex(Color color) => color.ToHtml(false);
    }
}
