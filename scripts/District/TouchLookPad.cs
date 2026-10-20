using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Right-side drag pad for mobile camera yaw (and light pitch hint).</summary>
    public partial class TouchLookPad : Control
    {
        [Signal] public delegate void LookDeltaEventHandler(Vector2 delta);

        private bool dragging;
        private int finger = -1;
        private Vector2 last;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Stop;
            SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight);
            OffsetLeft = -200;
            OffsetTop = -200;
            OffsetRight = -24;
            OffsetBottom = -24;
            CustomMinimumSize = new Vector2(176, 176);
        }

        public override void _Draw()
        {
            Vector2 c = Size * 0.5f;
            DrawCircle(c, 60f, new Color(0.05f, 0.02f, 0.1f, 0.4f));
            DrawArc(c, 52f, 0, Mathf.Tau, 40, new Color(1f, 0.35f, 0.7f, 0.45f), 2.5f);
            DrawString(ThemeDB.FallbackFont, c + new Vector2(-28f, 6f), "LOOK", HorizontalAlignment.Left, -1, 14,
                new Color(1f, 1f, 1f, 0.55f));
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed && !dragging)
                {
                    dragging = true;
                    finger = touch.Index;
                    last = touch.Position;
                }
                else if (!touch.Pressed && touch.Index == finger)
                {
                    dragging = false;
                    finger = -1;
                }
            }
            else if (@event is InputEventScreenDrag drag && drag.Index == finger)
            {
                Vector2 d = drag.Position - last;
                last = drag.Position;
                EmitSignal(SignalName.LookDelta, d);
            }
            else if (@event is InputEventMouseButton mouse)
            {
                if (mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
                {
                    dragging = true;
                    last = mouse.Position;
                }
                else if (!mouse.Pressed)
                    dragging = false;
            }
            else if (@event is InputEventMouseMotion motion && dragging && finger < 0)
            {
                EmitSignal(SignalName.LookDelta, motion.Relative);
            }
        }
    }
}
