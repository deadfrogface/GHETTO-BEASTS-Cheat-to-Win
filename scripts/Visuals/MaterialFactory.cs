using GhettoBeasts.Core;
using Godot;

namespace GhettoBeasts.Visuals
{
    public static class MaterialFactory
    {
        private const string CardShaderPath = "res://shaders/holo_card.gdshader";
        private const string CrtShaderPath = "res://shaders/crt_overlay.gdshader";

        private static Shader? cardShader;
        private static Shader? crtShader;

        public static ShaderMaterial Toon(Color albedo, float outlineWidth = VisualManager.DefaultOutlineWidth, Color? rimColor = null) =>
            VisualManager.CreateToonMaterial(albedo, outlineWidth, rimColor);

        /// <summary>Accent emissive only — keep energy modest so the world stays muted.</summary>
        public static StandardMaterial3D Emissive(Color color, float energy = 1.6f) => new()
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = energy,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };

        public static ShaderMaterial HoloCard(Color frame, Color inner)
        {
            cardShader ??= ResourceLoader.Load<Shader>(CardShaderPath);
            if (cardShader is null) GD.PrintErr($"[MaterialFactory] Shader fehlt: {CardShaderPath}");
            var material = new ShaderMaterial { Shader = cardShader ?? new Shader() };
            material.SetShaderParameter("frame_color", frame);
            material.SetShaderParameter("inner_color", inner);
            return material;
        }

        public static ShaderMaterial Crt()
        {
            crtShader ??= ResourceLoader.Load<Shader>(CrtShaderPath);
            if (crtShader is null) GD.PrintErr($"[MaterialFactory] Shader fehlt: {CrtShaderPath}");
            var material = new ShaderMaterial { Shader = crtShader ?? new Shader() };
            // Near-invisible grading only — no heavy 2000s CRT look.
            material.SetShaderParameter("scanline_strength", 0.02f);
            material.SetShaderParameter("vignette", 0.22f);
            material.SetShaderParameter("aberration", 0.0003f);
            material.SetShaderParameter("glitch", 0f);
            return material;
        }
    }
}
