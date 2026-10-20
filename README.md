# GHETTO BEASTS: DNA & Corruption

Cel-shaded Cyberpunk-Rundenkampf für **Godot 4 (.NET / C#)**: Bestien und Ghetto-NPCs als Hologramm-Karten, CRISPR-Mutationen, Trainer-Sabotage, Stealth-Gasse, Waschsalon, Darknet-Gacha – bis das SWAT-Team die Arena stürmt.

## Spielbarer Full Loop (F5)

```text
Hub → Gasse (Stealth) → Gebäude / Arena → Kampf + Sabotage
  → Raid → Knast-Escape  ODER  Sieg
  → CRISPR Lab / Waschsalon / Darknet Gacha / Shops → Save
```

| Modul | Inhalt |
| --- | --- |
| **Hub** | Hauptszene (`scenes/hub/hub.tscn`) – Navigation zu allem |
| **District** | Isometrische Gasse, Player, Cop-Sichtkegel, Hotspots |
| **Arena** | Bestehender Kampf mit Setup, Summon, Sabotage, Raid |
| **CRISPR Lab** | Head/Limbs/Skin an Roster-Bestien, Stabilisatoren (Clean $) |
| **Waschsalon** | Dirty → Clean (20 % Gebühr, `Economy.Launder`) |
| **Darknet Gacha** | Packs für Diamonds, Seltenheits-Gewichte, 3D-Pack-VFX |
| **DOPE / WEAP** | Einfache Shop-Stubs → Inventar-Items |
| **Roster** | Owned cards/genes + Loadouts in `SaveData` |
| **Audio** | Autoload `AudioBus` – Dateien optional, sonst Prozedural-Blips |
| **Tests** | xUnit Core inkl. Gacha/Lab/Roster (~43) |

---

## Voraussetzungen

- **Godot 4.3+ .NET-Edition**: <https://godotengine.org/download/windows/>
- **.NET 8 SDK**: <https://dotnet.microsoft.com/download/dotnet/8.0>
- Git

## Direkt in Godot öffnen (Windows)

Details: **[LAUNCH.md](LAUNCH.md)**.

```powershell
cd C:\Users\Lenovo\Documents
git clone https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git ghetto-beasts
cd ghetto-beasts
git pull
```

1. **Godot 4.3+ .NET** → `project.godot` öffnen.
2. **Build** (Hammer).
3. **F5** → **District Hub**. Von dort Gasse, Arena, Lab, Wash, Gacha, Shops.

Live-Sync: `tools\auto-sync.bat` (pull alle 10 s).

## Steuerung

| Kontext | Input |
| --- | --- |
| Hub / Menüs | Maus / Touch |
| Gasse | **WASD** / Pfeile, virtueller Stick (unten links), **E** Hotspot |
| Kampf | **1** Angriff · **2** Skill · **3** Backstein · **4** Doping · **5** Schiri |
| Knast | Pfeiltasten graben |

## Headless / CI

```bash
dotnet test tests/GhettoBeasts.Core.Tests
dotnet build GhettoBeasts.sln

# Autobattle (Hub leitet --autobattle automatisch in die Arena um):
godot --path . --headless -- --autobattle
godot --path . --headless -- --autobattle --card=kupferstecher_pitbull --cheats=none --stay
```

## Projektstruktur

```text
res://
├── scenes/hub/          # Hauptszene (F5)
├── scenes/district/     # Stealth-Oberwelt
├── scenes/combat/       # Arena + Setup + Prison
├── scenes/buildings/    # CRISPR, Wash, Darknet, DOPE, WEAP
├── scripts/             # Godot-C#
├── src/GhettoBeasts.Core/   # Pure C# (Roster, Gacha, Lab, Battle…)
├── tests/
├── assets/3d_models/    # optionale .glb
├── assets/audio/        # optionale SFX (sonst Blips)
└── shaders/             # toon_cel, outline, holo_card, crt
```

## Speichern

`user://savegame.json` – Roster, Loadouts, Stabilizer-Stacks, Inventar, Währungen, Bilanz.

## Cursor-Regeln

`.cursorrules`: C#-only, Stateless für Kampf-FSM, VisualManager/Toon (kein PBR), Logik in Core getrennt von UI.
