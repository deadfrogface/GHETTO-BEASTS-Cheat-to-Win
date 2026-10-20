# Open-Source References (Godot 4 / C#)

Research notes for Main District / Burner Phone / Companion / Juice / Audio.
Prefer MIT/CC0 addons; when vendoring would fight the C#-only + toon pipeline, we implement a minimal equivalent inspired by common patterns.

| Need | Candidate | License | Decision |
| --- | --- | --- | --- |
| Third-person controller (C#) | [vaporvee/gd-net-thirdpersoncontroller](https://github.com/vaporvee/gd-net-thirdpersoncontroller) · [Asset Library #1603](https://godotengine.org/asset-library/asset/1603) | MIT | **Inspired** — camera-relative move + smooth mesh yaw in `PlayerController.cs`. |
| Companion / pet follow | [KidsCanCode — Pet Following](https://kidscancode.org/godot_recipes/4.x/ai/pet_following/index.html) | Recipe | **Inspired** — lag point + `BeastFollowAI` (`NavigationAgent3D` when a nav map exists). |
| Pause / overlay UI | [Godot docs — Pausing games](https://docs.godotengine.org/en/latest/tutorials/scripting/pausing_games.html) | Docs | **Inspired** — `BurnerPhone` CanvasLayer + `ProcessMode.WhenPaused`. |
| Cinematic camera | [Phantom Camera](https://github.com/ramokz/phantom-camera) | MIT | **Not vendored** — `CameraRig` + `CameraJuice` (shake, FOV punch, dramatic action, hit-stop). |
| Cel / toon shading | Community toon shaders (Godot Shaders) | Varies / CC0 | **In-tree** — `shaders/toon_cel.gdshader` + `outline.gdshader` via `VisualManager`. |
| Character animations | [Mixamo](https://www.mixamo.com/) | Adobe Mixamo ToS | Optional GLB pipeline (`AssetPipeline`); procedural humanoids until licensed clips land. |
| Props / SFX kits | [Kenney](https://kenney.nl/) | CC0 | Preferred drop-in for `assets/audio/` + props when available. |
| Ambient / SFX | [Freesound](https://freesound.org/) | CC0 / CC-BY | Document attribution in `assets/audio/README.md` when files are added. |
| Fallback SFX | Procedural PCM in `AudioBus.MakeBlip` | Project | Used when OGG/WAV missing so F5 never blocks. |

## Audio buses

`AudioBus` (autoload) creates **Music** and **SFX** child buses under Master, supports volume apply from Burner Settings, BGM crossfade (`CrossfadeMusic`), and `PlaySpatial` (`AudioStreamPlayer3D`).

## Why not vendor wholesale

- Project is **C# only**; many polished controllers/cameras are GDScript.
- World meshes must go through **VisualManager / toon shaders**.
- Diegetic **Burner Phone** apps are game-specific.

Asset Library / GitHub links above remain preferred starting points if a future pass vendors a trimmed MIT addon under `addons/` with a thin C# bridge.
