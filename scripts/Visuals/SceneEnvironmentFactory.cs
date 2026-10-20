using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>Shared Palworld/Scarlet-style WorldEnvironment: ACES, soft screen bloom, depth fog.</summary>
    public static class SceneEnvironmentFactory
    {
        public static Godot.Environment Create(
            Color? background = null,
            Color? ambient = null,
            Color? fog = null)
        {
            return new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = background ?? Palette.Background,
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = ambient ?? Palette.Ambient,
                AmbientLightEnergy = 0.7f,
                TonemapMode = Godot.Environment.ToneMapper.Aces,
                TonemapExposure = 1.05f,
                TonemapWhite = 6f,
                GlowEnabled = true,
                GlowIntensity = 0.5f,
                GlowStrength = 0.9f,
                GlowBloom = 0.15f,
                GlowHdrThreshold = 0.8f,
                GlowHdrScale = 1.2f,
                GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Screen,
                FogEnabled = true,
                FogMode = Godot.Environment.FogModeEnum.Depth,
                FogLightColor = fog ?? Palette.Fog,
                FogLightEnergy = 0.85f,
                FogSunScatter = 0.05f,
                FogDensity = 0.55f,
                FogAerialPerspective = 0.4f,
                FogSkyAffect = 0.6f,
                FogDepthBegin = 10f,
                FogDepthEnd = 42f,
                FogDepthCurve = 0.9f
            };
        }

        public static WorldEnvironment Attach(Node3D parent, Color? background = null, Color? ambient = null, Color? fog = null)
        {
            var world = new WorldEnvironment { Environment = Create(background, ambient, fog) };
            parent.AddChild(world);
            return world;
        }
    }
}
