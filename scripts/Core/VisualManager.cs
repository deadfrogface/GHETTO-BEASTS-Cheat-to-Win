using System.Collections.Generic;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>
    /// Enforces the anime cel-shader on every <see cref="MeshInstance3D"/> in a subtree.
    /// Never leave StandardMaterial3D / PBR on gameplay meshes.
    /// </summary>
    public static class VisualManager
    {
        public const string ToonShaderPath = "res://shaders/toon_cel.gdshader";
        public const string OutlineShaderPath = "res://shaders/outline.gdshader";

        private static Shader? toonShader;
        private static Shader? outlineShader;

        /// <summary>Walks <paramref name="root"/> and replaces every mesh material with a toon material.</summary>
        public static int EnforceCelShading(Node root, float outlineWidth = 0.02f, Color? rimColor = null)
        {
            if (root is null) return 0;
            int count = 0;
            ApplyRecursive(root, outlineWidth, rimColor ?? Palette.NeonCyan, ref count);
            return count;
        }

        /// <summary>Applies cel-shading to a single mesh, preserving albedo when possible.</summary>
        public static void ApplyToMesh(MeshInstance3D mesh, float outlineWidth = 0.02f, Color? rimColor = null, Color? albedoOverride = null)
        {
            if (mesh is null) return;
            Color albedo = albedoOverride ?? ExtractAlbedo(mesh) ?? Colors.Gray;
            mesh.MaterialOverride = CreateToonMaterial(albedo, outlineWidth, rimColor ?? Palette.NeonCyan);
            if (mesh.Mesh is null) return;
            for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
                mesh.SetSurfaceOverrideMaterial(i, null);
        }

        public static ShaderMaterial CreateToonMaterial(Color albedo, float outlineWidth = 0.02f, Color? rimColor = null)
        {
            EnsureShaders();
            var material = new ShaderMaterial { Shader = toonShader };
            material.SetShaderParameter("albedo", albedo);
            material.SetShaderParameter("shade_color", albedo.Darkened(0.55f).Lerp(Palette.NeonPurple, 0.35f));
            if (rimColor.HasValue) material.SetShaderParameter("rim_color", rimColor.Value);

            if (outlineWidth > 0f && outlineShader is not null)
            {
                var outline = new ShaderMaterial { Shader = outlineShader };
                outline.SetShaderParameter("outline_width", outlineWidth);
                outline.SetShaderParameter("outline_color", new Color(0.03f, 0f, 0.06f));
                material.NextPass = outline;
            }
            return material;
        }

        public static bool IsToonMaterial(Material? material) =>
            material is ShaderMaterial shader &&
            shader.Shader is not null &&
            (shader.Shader.ResourcePath == ToonShaderPath || shader.Shader == toonShader);

        public static bool IsEmissiveVfx(Material? material) =>
            material is StandardMaterial3D { EmissionEnabled: true };

        private static void ApplyRecursive(Node node, float outlineWidth, Color rimColor, ref int count)
        {
            if (node is MeshInstance3D mesh)
            {
                // Keep pure emissive VFX (eyes, neon strips, particles) – only body meshes get cel-shading.
                if (IsEmissiveVfx(mesh.MaterialOverride) || IsToonMaterial(mesh.MaterialOverride))
                {
                    // already styled
                }
                else
                {
                    ApplyToMesh(mesh, outlineWidth, rimColor);
                    count++;
                }
            }

            foreach (var child in node.GetChildren())
                ApplyRecursive(child, outlineWidth, rimColor, ref count);
        }

        private static Color? ExtractAlbedo(MeshInstance3D mesh)
        {
            if (mesh.MaterialOverride is StandardMaterial3D standard) return standard.AlbedoColor;
            if (mesh.MaterialOverride is ShaderMaterial existing && existing.GetShaderParameter("albedo").VariantType != Variant.Type.Nil)
                return existing.GetShaderParameter("albedo").AsColor();
            if (mesh.Mesh is not null && mesh.Mesh.GetSurfaceCount() > 0 &&
                mesh.Mesh.SurfaceGetMaterial(0) is StandardMaterial3D surface)
                return surface.AlbedoColor;
            return null;
        }

        private static void EnsureShaders()
        {
            toonShader ??= ResourceLoader.Load<Shader>(ToonShaderPath);
            outlineShader ??= ResourceLoader.Load<Shader>(OutlineShaderPath);
            if (toonShader is null) GD.PrintErr($"[VisualManager] Shader fehlt: {ToonShaderPath}");
            if (outlineShader is null) GD.PrintErr($"[VisualManager] Shader fehlt: {OutlineShaderPath}");
        }
    }
}
