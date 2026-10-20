using Godot;

namespace GhettoBeasts.Visuals
{
    public static class MaterialFactory
    {
        private const string ToonShaderPath = "res://shaders/toon_cel.gdshader";
        private const string OutlineShaderPath = "res://shaders/outline.gdshader";
        private const string CardShaderPath = "res://shaders/holo_card.gdshader";
        private const string CrtShaderPath = "res://shaders/crt_overlay.gdshader";

        private static Shader? toonShader;
        private static Shader? outlineShader;
        private static Shader? cardShader;
        private static Shader? crtShader;

        public static ShaderMaterial Toon(Color albedo, float outlineWidth = 0.02f, Color? rimColor = null)
        {
            var material = new ShaderMaterial { Shader = toonShader ??= LoadShader(ToonShaderPath) };
            material.SetShaderParameter("albedo", albedo);
            material.SetShaderParameter("shade_color", albedo.Darkened(0.55f).Lerp(Palette.NeonPurple, 0.35f));
            if (rimColor.HasValue) material.SetShaderParameter("rim_color", rimColor.Value);

            if (outlineWidth > 0f)
            {
                var outline = new ShaderMaterial { Shader = outlineShader ??= LoadShader(OutlineShaderPath) };
                outline.SetShaderParameter("outline_width", outlineWidth);
                material.NextPass = outline;
            }
            return material;
        }

        public static StandardMaterial3D Emissive(Color color, float energy = 2.5f) => new()
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = energy
        };

        public static ShaderMaterial HoloCard(Color frame, Color inner)
        {
            var material = new ShaderMaterial { Shader = cardShader ??= LoadShader(CardShaderPath) };
            material.SetShaderParameter("frame_color", frame);
            material.SetShaderParameter("inner_color", inner);
            return material;
        }

        public static ShaderMaterial Crt() => new() { Shader = crtShader ??= LoadShader(CrtShaderPath) };

        private static Shader LoadShader(string path)
        {
            var shader = ResourceLoader.Load<Shader>(path);
            if (shader is null) GD.PrintErr($"[MaterialFactory] Shader fehlt: {path}");
            return shader ?? new Shader();
        }
    }
}
