# Open-Source References (Godot 4 / C#)

Research notes for Main District / Burner Phone / Companion / Juice / Audio / Mobile.
Prefer MIT/CC0 addons; when vendoring would fight the C#-only + toon pipeline, we implement a minimal equivalent inspired by common patterns.

| Need | Candidate | License | Decision |
| --- | --- | --- | --- |
| Third-person controller (C#) | [vaporvee/gd-net-thirdpersoncontroller](https://github.com/vaporvee/gd-net-thirdpersoncontroller) · [Asset Library #1603](https://godotengine.org/asset-library/asset/1603) | MIT | **Inspired** — camera-relative move + smooth mesh yaw in `PlayerController.cs`. |
| Companion / pet follow | [KidsCanCode — Pet Following](https://kidscancode.org/godot_recipes/4.x/ai/pet_following/index.html) | Recipe | **Inspired** — lag point + `BeastFollowAI` (`NavigationAgent3D` when a nav map exists). |
| Pause / overlay UI | [Godot docs — Pausing games](https://docs.godotengine.org/en/latest/tutorials/scripting/pausing_games.html) | Docs | **Inspired** — `BurnerPhone` CanvasLayer + `ProcessMode.WhenPaused`. |
| Cinematic camera | [Phantom Camera](https://github.com/ramokz/phantom-camera) | MIT | **Not vendored** — `CameraRig` (district follow, over-shoulder combat, action cuts) + `CameraJuice` / `TimeScaleManager` (directional shake, hit-pause, speedlines, KO flash). |
| Cel / toon shading | Community toon shaders (Godot Shaders) | Varies / CC0 | **In-tree** — `shaders/toon_cel.gdshader` + `outline.gdshader` via `VisualManager`. |
| Character animations | [Mixamo](https://www.mixamo.com/) | Adobe Mixamo ToS | Optional GLB pipeline (`AssetPipeline`); procedural humanoids until licensed clips land. |
| Urban props / kits | [Kenney](https://kenney.nl/) City Kit / Props | CC0 | **Preferred** — drop GLBs into `assets/3d_models/props/`; procedural blocks until pack is vendored. |
| Sample GLBs | [Khronos glTF-Sample-Models](https://github.com/KhronosGroup/glTF-Sample-Models) | Sample / Apache-style | **Vendored** — `Duck`, `Box`, `Lantern` under `shared/` + `props/` for pipeline smoke tests. |
| Ambient / SFX | [Freesound](https://freesound.org/) · Kenney Audio | CC0 / CC-BY | Document attribution in `assets/audio/README.md` when files are added. |
| Fallback SFX / BGM | Procedural PCM in `AudioBus` (`MakeBlip` / `MakeLoop`) | Project | Used when OGG/WAV missing so F5 never blocks; loops crossfade district↔combat. |
| Touch UI | Godot InputEventScreen* | Engine | `VirtualJoystick` + `TouchLookPad` + large Burner buttons. |

## Audio buses

`AudioBus` / AudioManager (autoload) creates **Music** and **SFX** child buses under Master, supports volume apply from Burner Settings, BGM crossfade (`CrossfadeMusic` / explore→battle helpers), layered attack stubs (`PlayAttackLayered`), procedural loop pads, `PlaySpatial`, and looping `AttachAmbient` (`AudioStreamPlayer3D`).

## Mobile / Vulkan Mobile

- Touch: left stick + right LOOK pad; phone app tiles ≥ 160×84.
- `project.godot`: `Mobile` feature flag, ETC2/ASTC import, emulate-touch-from-mouse.
- Export tip: set **Rendering → Renderer → mobile** (Vulkan Mobile) for Android/iOS; cel shaders avoid heavy PBR.

## Why not vendor wholesale

- Project is **C# only**; many polished controllers/cameras are GDScript.
- World meshes must go through **VisualManager / toon shaders**.
- Diegetic **Burner Phone** apps are game-specific.

Asset Library / GitHub links above remain preferred starting points if a future pass vendors a trimmed MIT/CC0 addon under `addons/` with a thin C# bridge.
