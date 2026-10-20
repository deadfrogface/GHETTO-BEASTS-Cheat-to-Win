using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;
using Side = GhettoBeasts.Combat.Side;

namespace GhettoBeasts.Entities
{
    /// <summary>3D body of a summoned card. Uses <see cref="CardData.ModelScene"/> if set, otherwise a procedural placeholder facing +Z.</summary>
    public partial class Beast3D : Node3D
    {
        private static readonly StatusType[] GlowPriority =
        {
            StatusType.Enraged, StatusType.Doped, StatusType.Confused, StatusType.Poison,
            StatusType.Bleed, StatusType.Stunned, StatusType.Banished
        };

        private readonly List<ShaderMaterial> toonMaterials = new();
        private Node3D model = null!;
        private OmniLight3D statusLight = null!;
        private Tween? flashTween;
        private Vector3 headLocal = new(0f, 1f, 0f);
        private float baseScale = 1f;

        public Side Side { get; private set; }
        public Vector3 HeadGlobalPosition => model.ToGlobal(headLocal);

        public void Build(CardDefinition definition, CardData data, IEnumerable<MutationDefinition> mutations, Side side)
        {
            Side = side;
            Name = $"Beast_{side}";
            baseScale = Mathf.Max(0.1f, data.ModelScale);

            model = new Node3D { Name = "Model" };
            AddChild(model);

            var mutationList = mutations.ToList();
            if (data.ModelScene is not null)
            {
                var instance = data.ModelScene.Instantiate();
                if (instance is Node3D node3D) model.AddChild(node3D);
                else
                {
                    GD.PrintErr($"[Beast3D] ModelScene von '{definition.Id}' ist kein Node3D – nutze Platzhalter.");
                    instance.QueueFree();
                    BuildPlaceholder(definition, data, mutationList);
                }
            }
            else
            {
                BuildPlaceholder(definition, data, mutationList);
            }

            VisualManager.EnforceCelShading(model);
            model.Scale = Vector3.One * 0.001f;
            statusLight = new OmniLight3D { LightEnergy = 0f, OmniRange = 2.6f, Position = new Vector3(0f, 0.9f, 0f) };
            AddChild(statusLight);
            CollectToonMaterials(model);
        }

        public async Task SpawnAsync()
        {
            var tween = CreateTween();
            tween.TweenProperty(model, "scale", Vector3.One * baseScale, 0.7)
                .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
            Flash(Colors.White, 3f, 0.5);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public async Task LungeAsync(Vector3 targetGlobal)
        {
            Vector3 home = Position;
            Vector3 towards = (targetGlobal - GlobalPosition) with { Y = 0f };
            Vector3 strike = home + towards * 0.38f;
            var tween = CreateTween();
            tween.TweenProperty(this, "position", strike, 0.13).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "position", home, 0.2).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public void PlayHit(bool critical)
        {
            Flash(critical ? Palette.NeonYellow : Colors.White, critical ? 5f : 3f, 0.35);
            Vector3 back = -GlobalTransform.Basis.Z.Normalized() * (critical ? 0.35f : 0.18f);
            var tween = CreateTween();
            tween.TweenProperty(model, "position", back with { Y = 0f }, 0.06);
            tween.TweenProperty(model, "position", Vector3.Zero, 0.25).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
            Vfx.Burst(GetParent(), HeadGlobalPosition, critical ? Palette.NeonYellow : Palette.NeonPink, critical ? 60 : 30, 4f, 0.5f, 0.035f);
        }

        public void PlayHeal()
        {
            Flash(Palette.NeonGreen, 2.5f, 0.6);
            Vfx.Burst(GetParent(), GlobalPosition + Vector3.Up * 0.3f, Palette.NeonGreen, 40, 3f, 1f, 0.04f, 2f, 40f);
        }

        public async Task DodgeAsync()
        {
            Vector3 side = GlobalTransform.Basis.X.Normalized() * 0.7f;
            var tween = CreateTween();
            tween.TweenProperty(model, "position", side, 0.1).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(model, "position", Vector3.Zero, 0.22).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public async Task FaintAsync()
        {
            Flash(Palette.Danger, 4f, 0.8);
            var tween = CreateTween().SetParallel();
            tween.TweenProperty(model, "rotation_degrees:z", 90f, 0.5).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(model, "scale", Vector3.One * baseScale * 0.75f, 0.5);
            tween.TweenProperty(statusLight, "light_energy", 0f, 0.3);
            Vfx.Smoke(GetParent(), GlobalPosition);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public async Task WildMutationAsync()
        {
            baseScale *= 1.3f;
            Flash(Palette.NeonPurple, 6f, 1.0);
            Vfx.Burst(GetParent(), GlobalPosition + Vector3.Up * 0.6f, Palette.NeonPurple, 120, 6f, 1.1f, 0.06f);
            var tween = CreateTween();
            tween.TweenProperty(model, "scale", Vector3.One * baseScale, 0.8).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public void UpdateStatusGlow(Combatant combatant)
        {
            if (combatant.IsDown)
            {
                statusLight.LightEnergy = 0f;
                return;
            }

            foreach (var status in GlowPriority)
            {
                if (!combatant.Has(status)) continue;
                statusLight.LightColor = Palette.ForStatus(status);
                statusLight.LightEnergy = 1.8f;
                return;
            }
            statusLight.LightEnergy = 0f;
        }

        private void Flash(Color color, float strength, double duration)
        {
            if (toonMaterials.Count == 0) return;
            flashTween?.Kill();
            flashTween = CreateTween().SetParallel();
            foreach (var material in toonMaterials)
            {
                material.SetShaderParameter("emission_color", color);
                flashTween.TweenShaderParameter(material, "emission_strength", 0f, duration, from: strength);
            }
        }

        private void CollectToonMaterials(Node node)
        {
            if (node is MeshInstance3D mesh && mesh.MaterialOverride is ShaderMaterial shader &&
                shader.Shader is not null && shader.Shader.ResourcePath.EndsWith("toon_cel.gdshader"))
            {
                toonMaterials.Add(shader);
            }
            foreach (var child in node.GetChildren()) CollectToonMaterials(child);
        }

        private void BuildPlaceholder(CardDefinition definition, CardData data, IReadOnlyList<MutationDefinition> mutations)
        {
            Color primary = data.PrimaryColor;
            Color secondary = data.SecondaryColor;
            var skin = mutations.FirstOrDefault(m => m.Slot == MutationSlot.Skin);
            if (skin is not null) primary = SkinTint(skin.Id, primary);

            Vector3[] paws;
            switch (data.Shape)
            {
                case BodyShape.Rodent:
                    paws = BuildRodent(primary, secondary, data.EyeColor);
                    break;
                case BodyShape.Reptile:
                    paws = BuildReptile(primary, secondary, data.EyeColor);
                    break;
                case BodyShape.Primate:
                    paws = BuildPrimate(primary, secondary, data.EyeColor);
                    break;
                case BodyShape.Insect:
                    paws = BuildInsect(primary, secondary, data.EyeColor);
                    break;
                case BodyShape.Humanoid:
                    var npc = data as NpcData;
                    Shapes.Humanoid(model, primary, npc?.SkinColor ?? new Color(0.85f, 0.65f, 0.5f), data.EyeColor, npc?.HasHat ?? false);
                    headLocal = new Vector3(0f, 1.78f, 0f);
                    paws = new[] { new Vector3(-0.4f, 0.8f, 0.15f), new Vector3(0.4f, 0.8f, 0.15f) };
                    break;
                default:
                    var beast = data as BeastData;
                    paws = BuildQuadruped(primary, secondary, data.EyeColor, beast?.HasEars ?? true, beast?.HasTail ?? true);
                    break;
            }

            if (definition.IsCyber) AddCyberParts();
            foreach (var mutation in mutations) AddMutationParts(mutation, paws);
        }

        private Vector3[] BuildQuadruped(Color primary, Color secondary, Color eyes, bool ears, bool tail)
        {
            Shapes.Toon(model, Shapes.Capsule(0.28f, 1.15f), new Vector3(0f, 0.68f, 0f), primary, new Vector3(90f, 0f, 0f));
            Shapes.Toon(model, Shapes.Sphere(0.3f), new Vector3(0f, 0.78f, 0.35f), secondary, scale: new Vector3(1f, 0.9f, 0.8f));
            foreach (var (x, z) in new[] { (-0.17f, 0.36f), (0.17f, 0.36f), (-0.17f, -0.36f), (0.17f, -0.36f) })
                Shapes.Toon(model, Shapes.Cylinder(0.075f, 0.06f, 0.5f), new Vector3(x, 0.25f, z), primary.Darkened(0.2f));

            headLocal = new Vector3(0f, 1.0f, 0.62f);
            Shapes.Toon(model, Shapes.Sphere(0.24f), headLocal, primary);
            Shapes.Toon(model, Shapes.Box(0.2f, 0.16f, 0.28f), headLocal + new Vector3(0f, -0.06f, 0.24f), secondary);
            Part(Shapes.Sphere(0.04f), headLocal + new Vector3(-0.1f, 0.06f, 0.2f), MaterialFactory.Emissive(eyes, 4f));
            Part(Shapes.Sphere(0.04f), headLocal + new Vector3(0.1f, 0.06f, 0.2f), MaterialFactory.Emissive(eyes, 4f));
            if (ears)
            {
                Shapes.Toon(model, Shapes.Cone(0.07f, 0.22f), headLocal + new Vector3(-0.13f, 0.22f, -0.04f), primary.Darkened(0.3f), new Vector3(-10f, 0f, 15f));
                Shapes.Toon(model, Shapes.Cone(0.07f, 0.22f), headLocal + new Vector3(0.13f, 0.22f, -0.04f), primary.Darkened(0.3f), new Vector3(-10f, 0f, -15f));
            }
            if (tail)
                Shapes.Toon(model, Shapes.Cylinder(0.03f, 0.05f, 0.5f), new Vector3(0f, 0.85f, -0.7f), primary.Darkened(0.2f), new Vector3(-55f, 0f, 0f));
            return new[] { new Vector3(-0.17f, 0.1f, 0.42f), new Vector3(0.17f, 0.1f, 0.42f) };
        }

        private Vector3[] BuildRodent(Color primary, Color secondary, Color eyes)
        {
            Shapes.Toon(model, Shapes.Capsule(0.22f, 0.8f), new Vector3(0f, 0.38f, 0f), primary, new Vector3(90f, 0f, 0f), new Vector3(1f, 1f, 0.9f));
            foreach (var (x, z) in new[] { (-0.13f, 0.22f), (0.13f, 0.22f), (-0.13f, -0.22f), (0.13f, -0.22f) })
                Shapes.Toon(model, Shapes.Cylinder(0.045f, 0.04f, 0.26f), new Vector3(x, 0.13f, z), secondary);

            headLocal = new Vector3(0f, 0.5f, 0.42f);
            Shapes.Toon(model, Shapes.Sphere(0.18f), headLocal, primary);
            Shapes.Toon(model, Shapes.Cone(0.1f, 0.26f), headLocal + new Vector3(0f, -0.03f, 0.2f), primary, new Vector3(90f, 0f, 0f));
            Part(Shapes.Sphere(0.03f), headLocal + new Vector3(0f, -0.03f, 0.34f), MaterialFactory.Emissive(new Color(1f, 0.5f, 0.6f), 2f));
            Part(Shapes.Sphere(0.032f), headLocal + new Vector3(-0.08f, 0.06f, 0.13f), MaterialFactory.Emissive(eyes, 4f));
            Part(Shapes.Sphere(0.032f), headLocal + new Vector3(0.08f, 0.06f, 0.13f), MaterialFactory.Emissive(eyes, 4f));
            Shapes.Toon(model, Shapes.Sphere(0.085f), headLocal + new Vector3(-0.12f, 0.15f, -0.04f), secondary, scale: new Vector3(1f, 1f, 0.35f));
            Shapes.Toon(model, Shapes.Sphere(0.085f), headLocal + new Vector3(0.12f, 0.15f, -0.04f), secondary, scale: new Vector3(1f, 1f, 0.35f));
            Shapes.Toon(model, Shapes.Cylinder(0.015f, 0.035f, 0.9f), new Vector3(0f, 0.3f, -0.75f), new Color(0.85f, 0.55f, 0.6f), new Vector3(-78f, 0f, 0f), outline: 0.01f);
            return new[] { new Vector3(-0.13f, 0.06f, 0.28f), new Vector3(0.13f, 0.06f, 0.28f) };
        }

        private Vector3[] BuildReptile(Color primary, Color secondary, Color eyes)
        {
            Shapes.Toon(model, Shapes.Capsule(0.3f, 1.6f), new Vector3(0f, 0.3f, 0f), primary, new Vector3(90f, 0f, 0f), new Vector3(1.3f, 1f, 0.6f));
            Shapes.Toon(model, Shapes.Cone(0.24f, 1.4f), new Vector3(0f, 0.26f, -1.35f), primary.Darkened(0.15f), new Vector3(-90f, 0f, 0f), new Vector3(1.2f, 1f, 0.55f));
            headLocal = new Vector3(0f, 0.4f, 0.85f);
            Shapes.Toon(model, Shapes.Box(0.5f, 0.26f, 0.42f), headLocal, primary);
            Shapes.Toon(model, Shapes.Box(0.38f, 0.14f, 0.7f), headLocal + new Vector3(0f, -0.06f, 0.5f), secondary);
            for (int i = 0; i < 6; i++)
                Part(Shapes.Cone(0.025f, 0.07f), headLocal + new Vector3(-0.15f + i * 0.06f, -0.16f, 0.65f),
                    MaterialFactory.Toon(new Color(0.95f, 0.92f, 0.8f), 0.005f), new Vector3(180f, 0f, 0f));
            Part(Shapes.Sphere(0.05f), headLocal + new Vector3(-0.15f, 0.16f, 0.05f), MaterialFactory.Emissive(eyes, 4f));
            Part(Shapes.Sphere(0.05f), headLocal + new Vector3(0.15f, 0.16f, 0.05f), MaterialFactory.Emissive(eyes, 4f));
            for (int i = 0; i < 5; i++)
                Shapes.Toon(model, Shapes.Cone(0.06f, 0.14f), new Vector3(0f, 0.5f, 0.45f - i * 0.3f), secondary.Darkened(0.3f), outline: 0.008f);
            foreach (var (x, z) in new[] { (-0.42f, 0.45f), (0.42f, 0.45f), (-0.42f, -0.45f), (0.42f, -0.45f) })
                Shapes.Toon(model, Shapes.Cylinder(0.08f, 0.07f, 0.3f), new Vector3(x, 0.14f, z), primary.Darkened(0.25f), new Vector3(0f, 0f, x < 0 ? -35f : 35f));
            return new[] { new Vector3(-0.42f, 0.08f, 0.5f), new Vector3(0.42f, 0.08f, 0.5f) };
        }

        private Vector3[] BuildPrimate(Color primary, Color secondary, Color eyes)
        {
            Shapes.Toon(model, Shapes.Cylinder(0.13f, 0.12f, 0.6f), new Vector3(-0.22f, 0.3f, 0f), primary.Darkened(0.2f));
            Shapes.Toon(model, Shapes.Cylinder(0.13f, 0.12f, 0.6f), new Vector3(0.22f, 0.3f, 0f), primary.Darkened(0.2f));
            Shapes.Toon(model, Shapes.Capsule(0.45f, 1.3f), new Vector3(0f, 1.05f, 0f), primary, new Vector3(15f, 0f, 0f), new Vector3(1.15f, 1f, 0.9f));
            Shapes.Toon(model, Shapes.Sphere(0.42f), new Vector3(0f, 1.2f, -0.22f), secondary, scale: new Vector3(1.1f, 1f, 0.6f));
            Shapes.Toon(model, Shapes.Sphere(0.3f), new Vector3(0f, 1.0f, 0.25f), primary.Lightened(0.15f), scale: new Vector3(1f, 1.1f, 0.5f));
            headLocal = new Vector3(0f, 1.78f, 0.2f);
            Shapes.Toon(model, Shapes.Sphere(0.28f), headLocal, primary);
            Shapes.Toon(model, Shapes.Box(0.42f, 0.09f, 0.16f), headLocal + new Vector3(0f, 0.09f, 0.18f), primary.Darkened(0.4f));
            Shapes.Toon(model, Shapes.Sphere(0.16f), headLocal + new Vector3(0f, -0.1f, 0.2f), secondary.Lightened(0.2f), scale: new Vector3(1.2f, 0.8f, 0.8f));
            Part(Shapes.Sphere(0.04f), headLocal + new Vector3(-0.1f, 0.02f, 0.25f), MaterialFactory.Emissive(eyes, 4f));
            Part(Shapes.Sphere(0.04f), headLocal + new Vector3(0.1f, 0.02f, 0.25f), MaterialFactory.Emissive(eyes, 4f));
            Shapes.Toon(model, Shapes.Cylinder(0.13f, 0.1f, 1.15f), new Vector3(-0.62f, 0.95f, 0.15f), primary, new Vector3(10f, 0f, -12f));
            Shapes.Toon(model, Shapes.Cylinder(0.13f, 0.1f, 1.15f), new Vector3(0.62f, 0.95f, 0.15f), primary, new Vector3(10f, 0f, 12f));
            Shapes.Toon(model, Shapes.Sphere(0.16f), new Vector3(-0.7f, 0.35f, 0.25f), primary.Darkened(0.35f));
            Shapes.Toon(model, Shapes.Sphere(0.16f), new Vector3(0.7f, 0.35f, 0.25f), primary.Darkened(0.35f));
            return new[] { new Vector3(-0.7f, 0.35f, 0.3f), new Vector3(0.7f, 0.35f, 0.3f) };
        }

        private Vector3[] BuildInsect(Color primary, Color secondary, Color eyes)
        {
            Shapes.Toon(model, Shapes.Sphere(0.5f), new Vector3(0f, 0.38f, -0.1f), primary, scale: new Vector3(0.95f, 0.38f, 1.45f));
            Shapes.Toon(model, Shapes.Sphere(0.48f), new Vector3(0f, 0.48f, -0.2f), secondary, scale: new Vector3(0.85f, 0.22f, 1.3f));
            for (int i = 0; i < 4; i++)
                Part(Shapes.Sphere(0.06f), new Vector3(-0.18f + (i % 2) * 0.36f, 0.58f, -0.45f + (i / 2) * 0.45f), MaterialFactory.Emissive(Palette.NeonGreen, 3f));
            headLocal = new Vector3(0f, 0.38f, 0.68f);
            Shapes.Toon(model, Shapes.Sphere(0.22f), headLocal, primary.Darkened(0.2f), scale: new Vector3(1.2f, 0.8f, 1f));
            Part(Shapes.Sphere(0.06f), headLocal + new Vector3(-0.15f, 0.06f, 0.12f), MaterialFactory.Emissive(eyes, 4f));
            Part(Shapes.Sphere(0.06f), headLocal + new Vector3(0.15f, 0.06f, 0.12f), MaterialFactory.Emissive(eyes, 4f));
            Shapes.Toon(model, Shapes.Cylinder(0.008f, 0.015f, 1.0f), headLocal + new Vector3(-0.1f, 0.35f, 0.35f), secondary, new Vector3(55f, 0f, 15f), outline: 0.006f);
            Shapes.Toon(model, Shapes.Cylinder(0.008f, 0.015f, 1.0f), headLocal + new Vector3(0.1f, 0.35f, 0.35f), secondary, new Vector3(55f, 0f, -15f), outline: 0.006f);
            foreach (float z in new[] { 0.35f, 0f, -0.4f })
            {
                Shapes.Toon(model, Shapes.Cylinder(0.025f, 0.02f, 0.65f), new Vector3(-0.55f, 0.22f, z), primary.Darkened(0.4f), new Vector3(0f, 0f, -60f), outline: 0.008f);
                Shapes.Toon(model, Shapes.Cylinder(0.025f, 0.02f, 0.65f), new Vector3(0.55f, 0.22f, z), primary.Darkened(0.4f), new Vector3(0f, 0f, 60f), outline: 0.008f);
            }
            return new[] { new Vector3(-0.55f, 0.1f, 0.4f), new Vector3(0.55f, 0.1f, 0.4f) };
        }

        private void AddCyberParts()
        {
            var glow = MaterialFactory.Emissive(Palette.NeonCyan, 3.5f);
            Part(Shapes.Box(0.04f, 0.04f, 0.9f), new Vector3(-0.2f, 0.85f, 0f), glow);
            Part(Shapes.Box(0.04f, 0.04f, 0.9f), new Vector3(0.2f, 0.85f, 0f), glow);
            Part(Shapes.Cylinder(0.12f, 0.12f, 0.05f), headLocal + new Vector3(0.16f, 0.12f, 0f), glow, new Vector3(0f, 0f, 90f));
            var shield = new MeshInstance3D
            {
                Mesh = Shapes.Sphere(1.0f),
                Position = new Vector3(0f, 0.7f, 0.1f),
                MaterialOverride = new StandardMaterial3D
                {
                    AlbedoColor = new Color(0.2f, 0.9f, 1f, 0.08f),
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                    CullMode = BaseMaterial3D.CullModeEnum.Disabled
                }
            };
            model.AddChild(shield);
        }

        private void AddMutationParts(MutationDefinition mutation, Vector3[] paws)
        {
            var glow = MaterialFactory.Emissive(Palette.NeonPurple, 3f);
            switch (mutation.Id)
            {
                case "gorilla_kiefer":
                    Shapes.Toon(model, Shapes.Box(0.32f, 0.14f, 0.3f), headLocal + new Vector3(0f, -0.18f, 0.12f), new Color(0.25f, 0.22f, 0.25f));
                    Part(Shapes.Box(0.34f, 0.03f, 0.03f), headLocal + new Vector3(0f, -0.1f, 0.27f), glow);
                    break;
                case "piranha_gebiss":
                    for (int i = 0; i < 5; i++)
                        Part(Shapes.Cone(0.025f, 0.09f), headLocal + new Vector3(-0.1f + i * 0.05f, -0.14f, 0.24f),
                            MaterialFactory.Emissive(new Color(1f, 0.95f, 0.9f), 1.5f), new Vector3(180f, 0f, 0f));
                    break;
                case "fledermaus_sonar":
                    Shapes.Toon(model, Shapes.Cylinder(0.14f, 0.14f, 0.03f), headLocal + new Vector3(-0.22f, 0.18f, 0f), new Color(0.3f, 0.2f, 0.3f), new Vector3(0f, 0f, 70f));
                    Shapes.Toon(model, Shapes.Cylinder(0.14f, 0.14f, 0.03f), headLocal + new Vector3(0.22f, 0.18f, 0f), new Color(0.3f, 0.2f, 0.3f), new Vector3(0f, 0f, -70f));
                    Part(Shapes.Sphere(0.05f), headLocal + new Vector3(0f, 0.25f, 0f), MaterialFactory.Emissive(Palette.NeonCyan, 4f));
                    break;
                case "klingen_pranken":
                    foreach (var paw in paws)
                        Part(Shapes.Cone(0.05f, 0.45f), paw + new Vector3(0f, 0.15f, 0.2f), MaterialFactory.Emissive(new Color(0.85f, 0.9f, 1f), 2f), new Vector3(70f, 0f, 0f));
                    break;
                case "kaenguru_sprungbeine":
                    Shapes.Toon(model, Shapes.Sphere(0.2f), new Vector3(-0.25f, 0.35f, -0.3f), new Color(0.75f, 0.5f, 0.3f), scale: new Vector3(0.8f, 1.2f, 1f));
                    Shapes.Toon(model, Shapes.Sphere(0.2f), new Vector3(0.25f, 0.35f, -0.3f), new Color(0.75f, 0.5f, 0.3f), scale: new Vector3(0.8f, 1.2f, 1f));
                    break;
                case "skorpion_stachel":
                    Shapes.Toon(model, Shapes.Cylinder(0.04f, 0.06f, 0.7f), new Vector3(0f, 1.0f, -0.55f), new Color(0.35f, 0.25f, 0.1f), new Vector3(-25f, 0f, 0f));
                    Part(Shapes.Cone(0.07f, 0.22f), new Vector3(0f, 1.38f, -0.4f), MaterialFactory.Emissive(new Color(0.6f, 1f, 0.2f), 3f), new Vector3(120f, 0f, 0f));
                    break;
                case "chitin_panzer":
                    for (int i = 0; i < 3; i++)
                        Shapes.Toon(model, Shapes.Sphere(0.24f), new Vector3(0f, 0.85f, 0.25f - i * 0.28f), new Color(0.3f, 0.18f, 0.08f), scale: new Vector3(1.3f, 0.4f, 0.8f), outline: 0.01f);
                    break;
                case "aal_strom_haut":
                    Part(Shapes.Box(0.03f, 0.03f, 1.0f), new Vector3(0f, 0.95f, 0f), MaterialFactory.Emissive(Palette.NeonCyan, 5f));
                    break;
                case "chamaeleon_tarnhaut":
                    Part(Shapes.Sphere(0.06f), headLocal + new Vector3(0f, 0.22f, 0f), MaterialFactory.Emissive(Palette.NeonGreen, 3f));
                    break;
                default:
                    GD.PushWarning($"[Beast3D] Keine Optik für Mutation '{mutation.Id}'.");
                    break;
            }
        }

        private static Color SkinTint(string mutationId, Color fallback) => mutationId switch
        {
            "chitin_panzer" => fallback.Lerp(new Color(0.4f, 0.26f, 0.12f), 0.6f),
            "aal_strom_haut" => fallback.Lerp(new Color(0.15f, 0.3f, 0.75f), 0.6f),
            "chamaeleon_tarnhaut" => fallback.Lerp(new Color(0.3f, 0.75f, 0.4f), 0.6f),
            _ => fallback
        };

        private void Part(Mesh mesh, Vector3 position, Material material, Vector3? rotationDegrees = null) =>
            Shapes.Part(model, mesh, position, material, rotationDegrees);
    }
}
