# Audio placeholders

Drop optional files here (OGG/WAV). `AudioBus` (AudioManager) falls back to procedural blips + loop pads if missing.

Preferred free sources (see root `OPEN_SOURCE.md`): **Kenney** (CC0), **Freesound** (CC0/CC-BY — keep attribution).

Suggested filenames:

- `hit.ogg`, `hit_bass.ogg`, `hit_crack.ogg` (layered attack stack via `PlayAttackLayered`)
- `summon.ogg`, `raid_siren.ogg`, `ui_click.ogg`, `pack_open.ogg`, `whoosh.ogg`
- `bgm_district.ogg`, `bgm_combat.ogg`, `bgm_prison.ogg` (loops — Music bus / `CrossfadeMusic` / explore→battle helpers)
- `ambient_alley.ogg`, `ambient_neon.ogg` (looping `AudioStreamPlayer3D` beds via `AttachAmbient`)

Without files, `MakeLoop("district"|"combat"|"prison")` generates seamless PCM pads so F5 always has BGM.

Buses: **Master** → **Music**, **SFX** (created at runtime by `AudioBus`). Spatial SFX via `PlaySpatial`.
