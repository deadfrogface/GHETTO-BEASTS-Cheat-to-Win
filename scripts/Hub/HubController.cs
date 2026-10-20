using GhettoBeasts.Core;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Hub
{
    /// <summary>
    /// Legacy stub — 2D hub buttons are retired. Redirects into the physical Main District.
    /// Burner Phone (ESC in district) holds meta apps; buildings are walked into.
    /// </summary>
    public partial class HubController : Control
    {
        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[Hub] GameManager-Autoload fehlt.");
                return;
            }

            foreach (var arg in OS.GetCmdlineUserArgs())
            {
                if (arg.Equals("--autohub", System.StringComparison.OrdinalIgnoreCase))
                {
                    GD.Print("[Hub] --autohub smoke OK (stub, no redirect)");
                    BuildStubUi(smoke: true);
                    GetTree().CreateTimer(0.4).Timeout += () => GetTree().Quit(0);
                    return;
                }
            }

            // Immediate redirect into the 3D alley
            GameManager.Instance.GoTo(GameManager.MainDistrictScene);
        }

        private void BuildStubUi(bool smoke)
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();
            var bg = new ColorRect { Color = Palette.Background };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(bg);
            AddChild(UiKit.Label(
                smoke ? "Hub stub · autohub OK" : "Weiterleitung → Main District…",
                18, Palette.NeonCyan, HorizontalAlignment.Center));
        }
    }
}
