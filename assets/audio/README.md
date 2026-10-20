# Audio placeholders

Drop optional files here (OGG/WAV). `AudioBus` falls back to procedural blips + loop pads if missing.

Preferred free sources (see root `OPEN_SOURCE.md`): **Kenney** (CC0), **Freesound** (CC0/CC-BY — keep attribution).

Suggested filenames:

- `hit.ogg`, `summon.ogg`, `raid_siren.ogg`, `ui_click.ogg`, `pack_open.ogg`, `whoosh.ogg`
- `bgm_district.ogg`, `bgm_combat.ogg`, `bgm_prison.ogg` (loops — Music bus / `CrossfadeMusic`)

Without files, `MakeLoop("district"|"combat"|"prison")` generates seamless PCM pads so F5 always has BGM.

Buses: **Master** → **Music**, **SFX** (created at runtime by `AudioBus`). Spatial SFX via `PlaySpatial`.
