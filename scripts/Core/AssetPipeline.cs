using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>
    /// Discovers AI-exported <c>.glb</c>/<c>.gltf</c> models under <see cref="ModelsRoot"/>,
    /// instantiates them with Mixamo-friendly animation wiring and forces cel-shading via
    /// <see cref="VisualManager"/>. Missing files fall back to procedural primitive meshes.
    /// </summary>
    public partial class AssetPipeline : Node
    {
        public const string ModelsRoot = "res://assets/3d_models";
        public const string BeastsFolder = ModelsRoot + "/beasts";
        public const string NpcsFolder = ModelsRoot + "/npcs";
        public const string MutationsFolder = ModelsRoot + "/mutations";
        public const string SharedFolder = ModelsRoot + "/shared";

        private static readonly string[] MixamoIdleNames =
        {
            "Idle", "idle", "Idle_Loop", "mixamo.com", "Armature|Idle", "Root|Idle", "Take 001"
        };

        private static readonly string[] MixamoAttackNames =
        {
            "Attack", "attack", "Punch", "Slash", "Bite", "Combat_Idle"
        };

        private static readonly string[] ModelExtensions = { ".glb", ".gltf" };

        private readonly Dictionary<string, string> modelPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PackedScene> packedCache = new(StringComparer.OrdinalIgnoreCase);

        public static AssetPipeline? Instance { get; private set; }

        public int DiscoveredModelCount => modelPaths.Count;
        public IReadOnlyDictionary<string, string> ModelPaths => modelPaths;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            EnsureFolderHints();
            Rescan();
            var snap = DatabaseCatalog.Instance.Snapshot();
            GD.Print($"[AssetPipeline] {DiscoveredModelCount} Modelle unter {ModelsRoot}. " +
                     $"Katalog: {snap.Beasts} Bestien, {snap.Npcs} NPCs, {snap.Mutations} Mutationen.");
        }

        /// <summary>Re-scan the models directory (call after dropping new Meshy/Tripo/Mixamo exports).</summary>
        public void Rescan()
        {
            modelPaths.Clear();
            packedCache.Clear();
            ScanFolder(BeastsFolder);
            ScanFolder(NpcsFolder);
            ScanFolder(MutationsFolder);
            ScanFolder(SharedFolder);
            ScanFolder(ModelsRoot);

            foreach (var id in DatabaseCatalog.Instance.AllCardIds)
            {
                if (modelPaths.ContainsKey(id)) continue;
                string? discovered = FindLooseMatch(id);
                if (discovered is not null) modelPaths[id] = discovered;
            }
        }

        public bool HasModel(string catalogId) =>
            ResolvePath(catalogId) is not null;

        public string? ResolvePath(string catalogId)
        {
            if (string.IsNullOrWhiteSpace(catalogId)) return null;
            if (modelPaths.TryGetValue(catalogId, out var path)) return path;

            foreach (string folder in new[] { BeastsFolder, NpcsFolder, SharedFolder, ModelsRoot })
            foreach (string ext in ModelExtensions)
            {
                string candidate = $"{folder}/{catalogId}{ext}";
                if (ResourceLoader.Exists(candidate) || FileAccess.FileExists(candidate))
                {
                    modelPaths[catalogId] = candidate;
                    return candidate;
                }
            }
            return null;
        }

        /// <summary>
        /// Instantiates the model for <paramref name="catalogId"/>, applies cel-shading and starts an idle animation.
        /// Returns <c>null</c> if no file exists (caller should use a procedural fallback).
        /// </summary>
        public Node3D? InstantiateModel(string catalogId, Color? rimColor = null, bool playIdle = true)
        {
            string? path = ResolvePath(catalogId);
            if (path is null) return null;

            try
            {
                if (!packedCache.TryGetValue(path, out var packed))
                {
                    packed = ResourceLoader.Load<PackedScene>(path);
                    if (packed is null)
                    {
                        GD.PrintErr($"[AssetPipeline] Konnte '{path}' nicht als PackedScene laden.");
                        return null;
                    }
                    packedCache[path] = packed;
                }

                var instance = packed.Instantiate();
                if (instance is not Node3D root)
                {
                    GD.PrintErr($"[AssetPipeline] Root von '{path}' ist kein Node3D.");
                    instance.QueueFree();
                    return null;
                }

                int shaded = VisualManager.EnforceCelShading(root, rimColor: rimColor);
                NormalizeFacing(root);
                if (playIdle) TryPlayAnimation(root, MixamoIdleNames);
                GD.Print($"[AssetPipeline] '{catalogId}' ← {path} (cel-pass auf {shaded} Meshes).");
                return root;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AssetPipeline] Fehler beim Laden von '{catalogId}': {ex.Message}");
                return null;
            }
        }

        /// <summary>Builds a toon-shaded primitive fallback and logs a warning.</summary>
        public Node3D CreateFallbackMesh(CardDefinition definition, CardData? visual = null)
        {
            GD.PushWarning($"[AssetPipeline] Kein .glb/.gltf für '{definition.Id}' – Platzhalter-Mesh.");
            var root = new Node3D { Name = $"Fallback_{definition.Id}" };
            Color primary = visual?.PrimaryColor ?? Palette.ForRarity(definition.Rarity);
            Color secondary = visual?.SecondaryColor ?? primary.Darkened(0.3f);
            Color eyes = visual?.EyeColor ?? Palette.NeonPink;
            var shape = visual?.Shape ?? MapShape(definition);

            switch (shape)
            {
                case BodyShape.Humanoid:
                    Shapes.Humanoid(root, primary, secondary.Lightened(0.2f), eyes, definition.Rarity >= Rarity.Rare);
                    break;
                case BodyShape.Rodent:
                    Shapes.Toon(root, Shapes.Capsule(0.22f, 0.75f), new Vector3(0f, 0.35f, 0f), primary, new Vector3(90f, 0f, 0f));
                    Shapes.Toon(root, Shapes.Sphere(0.18f), new Vector3(0f, 0.45f, 0.38f), primary);
                    Shapes.Part(root, Shapes.Sphere(0.03f), new Vector3(-0.07f, 0.5f, 0.5f), MaterialFactory.Emissive(eyes, 3f));
                    Shapes.Part(root, Shapes.Sphere(0.03f), new Vector3(0.07f, 0.5f, 0.5f), MaterialFactory.Emissive(eyes, 3f));
                    break;
                case BodyShape.Insect:
                    Shapes.Toon(root, Shapes.Sphere(0.45f), new Vector3(0f, 0.35f, 0f), primary, scale: new Vector3(0.9f, 0.35f, 1.3f));
                    Shapes.Toon(root, Shapes.Sphere(0.2f), new Vector3(0f, 0.35f, 0.55f), primary.Darkened(0.2f));
                    Shapes.Part(root, Shapes.Sphere(0.05f), new Vector3(-0.12f, 0.4f, 0.65f), MaterialFactory.Emissive(eyes, 3f));
                    Shapes.Part(root, Shapes.Sphere(0.05f), new Vector3(0.12f, 0.4f, 0.65f), MaterialFactory.Emissive(eyes, 3f));
                    break;
                case BodyShape.Reptile:
                    Shapes.Toon(root, Shapes.Capsule(0.28f, 1.4f), new Vector3(0f, 0.28f, 0f), primary, new Vector3(90f, 0f, 0f), new Vector3(1.2f, 1f, 0.55f));
                    Shapes.Toon(root, Shapes.Box(0.45f, 0.22f, 0.55f), new Vector3(0f, 0.35f, 0.7f), primary);
                    break;
                case BodyShape.Primate:
                    Shapes.Toon(root, Shapes.Capsule(0.4f, 1.2f), new Vector3(0f, 0.95f, 0f), primary);
                    Shapes.Toon(root, Shapes.Sphere(0.26f), new Vector3(0f, 1.7f, 0.15f), primary);
                    break;
                default:
                    Shapes.Toon(root, Shapes.Capsule(0.28f, 1.1f), new Vector3(0f, 0.65f, 0f), primary, new Vector3(90f, 0f, 0f));
                    Shapes.Toon(root, Shapes.Sphere(0.24f), new Vector3(0f, 0.95f, 0.55f), primary);
                    Shapes.Part(root, Shapes.Sphere(0.035f), new Vector3(-0.09f, 1.0f, 0.72f), MaterialFactory.Emissive(eyes, 3f));
                    Shapes.Part(root, Shapes.Sphere(0.035f), new Vector3(0.09f, 1.0f, 0.72f), MaterialFactory.Emissive(eyes, 3f));
                    break;
            }

            VisualManager.EnforceCelShading(root);
            return root;
        }

        /// <summary>Preferred entry: AI model if present, otherwise procedural fallback.</summary>
        public Node3D InstantiateOrFallback(CardDefinition definition, CardData? visual = null, Color? rimColor = null)
        {
            var model = InstantiateModel(definition.Id, rimColor);
            if (model is not null) return model;
            return CreateFallbackMesh(definition, visual);
        }

        public static bool TryPlayAttack(Node3D root) => TryPlayAnimation(root, MixamoAttackNames);

        public static bool TryPlayIdle(Node3D root) => TryPlayAnimation(root, MixamoIdleNames);

        public static bool TryPlayAnimation(Node root, IEnumerable<string> preferredNames)
        {
            var player = FindAnimationPlayer(root);
            if (player is null) return false;

            var names = preferredNames.ToArray();
            foreach (string preferred in names)
            {
                if (player.HasAnimation(preferred))
                {
                    player.Play(preferred);
                    return true;
                }
            }

            // Fuzzy: any clip containing a preferred token.
            foreach (string animName in player.GetAnimationList())
            {
                foreach (string preferred in names)
                {
                    if (animName.Contains(preferred, StringComparison.OrdinalIgnoreCase))
                    {
                        player.Play(animName);
                        return true;
                    }
                }
            }

            var list = player.GetAnimationList();
            if (list.Length > 0)
            {
                player.Play(list[0]);
                return true;
            }
            return false;
        }

        public static AnimationPlayer? FindAnimationPlayer(Node root)
        {
            if (root is AnimationPlayer direct) return direct;
            foreach (var child in root.GetChildren())
            {
                var found = FindAnimationPlayer(child);
                if (found is not null) return found;
            }
            return null;
        }

        private void ScanFolder(string folder)
        {
            if (!DirAccess.DirExistsAbsolute(ProjectSettings.GlobalizePath(folder)) &&
                !DirAccess.DirExistsAbsolute(folder))
            {
                // Also try ResourceLoader-friendly existence.
                using var probe = DirAccess.Open(folder);
                if (probe is null) return;
            }

            using var dir = DirAccess.Open(folder);
            if (dir is null) return;

            dir.ListDirBegin();
            for (string fileName = dir.GetNext(); fileName.Length > 0; fileName = dir.GetNext())
            {
                if (dir.CurrentIsDir()) continue;
                string lower = fileName.ToLowerInvariant();
                if (!ModelExtensions.Any(ext => lower.EndsWith(ext))) continue;
                // Skip Godot import sidecars if they ever appear as files.
                if (lower.EndsWith(".import")) continue;

                string id = System.IO.Path.GetFileNameWithoutExtension(fileName);
                string path = $"{folder}/{fileName}";
                modelPaths[id] = path;
            }
            dir.ListDirEnd();
        }

        private string? FindLooseMatch(string catalogId)
        {
            foreach (var (id, path) in modelPaths)
            {
                if (catalogId.EndsWith("_" + id, StringComparison.OrdinalIgnoreCase) ||
                    id.EndsWith("_" + catalogId, StringComparison.OrdinalIgnoreCase) ||
                    catalogId.Contains(id, StringComparison.OrdinalIgnoreCase))
                    return path;
            }
            return null;
        }

        private static BodyShape MapShape(CardDefinition definition)
        {
            if (definition.Kind == CardKind.Npc) return BodyShape.Humanoid;
            return ContentCatalog.GetBodyArchetype(definition.Id) switch
            {
                ContentCatalog.BodyArchetype.Rodent => BodyShape.Rodent,
                ContentCatalog.BodyArchetype.Reptile => BodyShape.Reptile,
                ContentCatalog.BodyArchetype.Primate => BodyShape.Primate,
                ContentCatalog.BodyArchetype.Insect => BodyShape.Insect,
                ContentCatalog.BodyArchetype.Humanoid => BodyShape.Humanoid,
                _ => BodyShape.Quadruped
            };
        }

        private static void NormalizeFacing(Node3D root)
        {
            // Many Mixamo exports face +Z or -Z; we keep authored orientation and only ensure a sane scale.
            if (root.Scale.LengthSquared() < 0.0001f) root.Scale = Vector3.One;
        }

        private static void EnsureFolderHints()
        {
            // Godot's res:// folders must exist as real directories for DirAccess scans.
            foreach (string folder in new[] { ModelsRoot, BeastsFolder, NpcsFolder, MutationsFolder, SharedFolder })
            {
                string global = ProjectSettings.GlobalizePath(folder);
                if (!DirAccess.DirExistsAbsolute(global))
                    DirAccess.MakeDirRecursiveAbsolute(global);
            }
        }
    }
}
