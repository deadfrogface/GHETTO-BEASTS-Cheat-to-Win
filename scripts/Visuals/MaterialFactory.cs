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

        public static ShaderMaterial Toon(Color albedo, float outlineWidth = 0.02f, Color? rimColor = null) =>
            VisualManager.CreateToonMaterial(albedo, outlineWidth, rimColor);

        public static StandardMaterial3D Emissive(Color color, float energy = 2.5f) => new()
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = energy
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
            return new ShaderMaterial { Shader = crtShader ?? new Shader() };
        }
    }
}
