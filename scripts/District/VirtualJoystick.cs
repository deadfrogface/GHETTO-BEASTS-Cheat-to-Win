using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Mobile-friendly left-side virtual stick. Optional — keyboard still works.</summary>
    public partial class VirtualJoystick : Control
    {
        [Signal] public delegate void StickChangedEventHandler(Vector2 value);

        private const float Radius = 56f;
        private Vector2 center;
        private Vector2 knob;
        private bool dragging;
        private int finger = -1;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Stop;
            SetAnchorsAndOffsetsPreset(LayoutPreset.BottomLeft);
            OffsetLeft = 24;
            OffsetTop = -200;
            OffsetRight = 200;
            OffsetBottom = -24;
            CustomMinimumSize = new Vector2(176, 176);
        }

        public override void _Draw()
        {
            center = Size * 0.5f;
            DrawCircle(center, Radius + 8f, new Color(0.05f, 0.02f, 0.1f, 0.55f));
            DrawArc(center, Radius, 0, Mathf.Tau, 48, new Color(0.2f, 0.9f, 1f, 0.5f), 3f);
            DrawCircle(center + knob, 22f, new Color(1f, 0.2f, 0.7f, 0.85f));
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed && !dragging)
                {
                    dragging = true;
                    finger = touch.Index;
                    UpdateKnob(touch.Position);
                }
                else if (!touch.Pressed && touch.Index == finger)
                    Release();
            }
            else if (@event is InputEventScreenDrag drag && drag.Index == finger)
                UpdateKnob(drag.Position);
            else if (@event is InputEventMouseButton mouse)
            {
                if (mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
                {
                    dragging = true;
                    UpdateKnob(mouse.Position);
                }
                else if (!mouse.Pressed)
                    Release();
            }
            else if (@event is InputEventMouseMotion motion && dragging && finger < 0)
                UpdateKnob(motion.Position);
        }

        private void UpdateKnob(Vector2 local)
        {
            Vector2 delta = local - Size * 0.5f;
            if (delta.Length() > Radius) delta = delta.Normalized() * Radius;
            knob = delta;
            EmitSignal(SignalName.StickChanged, knob / Radius);
            QueueRedraw();
        }

        private void Release()
        {
            dragging = false;
            finger = -1;
            knob = Vector2.Zero;
            EmitSignal(SignalName.StickChanged, Vector2.Zero);
            QueueRedraw();
        }
    }
}
