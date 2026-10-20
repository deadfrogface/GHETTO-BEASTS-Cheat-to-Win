# 3D Model Import Pipeline (Meshy / Tripo / Mixamo → Godot)

Drop AI-generated or Mixamo-rigged models here. At runtime `AssetPipeline` (autoload) scans these folders, applies the cel-shader via `VisualManager`, and wires Mixamo-style animations (`Idle`, `Attack`, …).

## Folder layout

```text
res://assets/3d_models/
├── beasts/       # catalogId.glb  e.g. cyber_pit.glb, kanalratte.glb
├── npcs/         # catalogId.glb  e.g. chantal.glb, el_capo.glb
├── mutations/    # optional attachment meshes (future)
└── shared/       # reusable props / trainers
```

**File name = CatalogId** from `ContentCatalog` / `DatabaseCatalog` (without extension).

Examples:

| File | Unit |
| --- | --- |
| `beasts/cyber_pit.glb` | Cyber-Pitbull 'CYBER-PIT' |
| `beasts/crispr_01.glb` | Giga-Kakerlake 'CRISPR-01' |
| `npcs/chantal.glb` | Club-Hostess 'Chantal' |
| `npcs/el_capo.glb` | Kartell-Boss 'El Capo' |

## Recommended AI → Godot workflow

1. Generate a character in **Meshy.ai** / **Tripo3D** (stylized / low-poly preferred).
2. Rig & animate in **Mixamo** (or export with idle/attack clips).
3. Export **glTF Binary (`.glb`)** – Y-up, +Z forward if possible.
4. Place the file in `beasts/` or `npcs/` named exactly like the catalog id.
5. Open the project in Godot 4.3+ .NET → Godot imports the mesh → press **F5**.
6. `AssetPipeline` loads it, `VisualManager` overwrites all materials with `toon_cel.gdshader`.

## Fallbacks

If no `.glb`/`.gltf` exists for a catalog id, the engine builds a **procedural toon placeholder** (primitive meshes) and logs:

```text
[AssetPipeline] Kein .glb/.gltf für '…' – Platzhalter-Mesh.
```

You can also assign `CardData.ModelScene` or `CardData.ModelPath` on the `.tres` under `res://data/cards/`.

## Open-source sources

- [Kenney.nl](https://kenney.nl/assets) (CC0)
- [OpenGameArt](https://opengameart.org/)
- [Godot Asset Library](https://godotengine.org/asset-library/asset)
- [Quaternius](https://quaternius.com/) (CC0)

Keep licenses in `assets/3d_models/LICENSES.md` when you vendor third-party packs.
