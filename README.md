# GHETTO BEASTS: DNA & Corruption

Cel-shaded Cyberpunk-Kreaturenkampf für **Godot 4 (.NET / C#)**: physische 3D-Gasse (Multi-Block), Burner-Phone-Apps, Companion, CRISPR, Darknet-Gacha, Trainer-Sabotage – bis das SWAT-Team die Arena stürmt.

## Full Loop (F5)

```text
Main District (3D Alley + unlockable Back Alley / Dock)
  → Companion folgt · Wildtiere / Trainer / Cops
  → E an Gebäude (CRISPR / Wash / Darknet / DOPE* / WEAP* / ARENA)
  → ESC Burner Phone (Käfig+CRISPR · Darknet · GPS+Quests · Wallet · Contacts · Settings)
  → Kampf (+ optional 2v2 Stub) · Sabotage → Raid → Knast-Escape → zurück
  * freischalten durch Street-Wins / Quests
```

| Modul | Inhalt |
| --- | --- |
| **Main District** | Hauptszene (`scenes/district/main_district.tscn`) – Multi-Block, Props, Touch |
| **Burner Phone** | ESC/TAB – Käfig CRISPR-Equip, Gacha inkl. Gen-Corrupted, Live-GPS, Laundry, Bribes, Cel/Audio/2v2 |
| **Progression** | Street Wins → DOPE / WEAP / Back Alley / Dock Block |
| **Companion** | Aktive Roster-Bestie (GLB via AssetPipeline oder procedural toon) |
| **Arena** | 1v1 + 2v2-Partner-Assist-Stub, Sabotage, Raid, Prison |
| **Audio** | Autoload `AudioBus` – Dateien optional, sonst Blips + Loop-Pads, Crossfade, Spatial |
| **Tests** | xUnit Core inkl. Progression / Gacha / Lab / Roster |

Open-Source-Referenzen: **[OPEN_SOURCE.md](OPEN_SOURCE.md)**. Windows-Start: **[LAUNCH.md](LAUNCH.md)**.

---

## Voraussetzungen

- **Godot 4.3+ .NET-Edition**: <https://godotengine.org/download/windows/>
- **.NET 8 SDK**: <https://dotnet.microsoft.com/download/dotnet/8.0>
- Git

## Direkt in Godot öffnen (Windows)

```powershell
cd C:\Users\Lenovo\Documents
git clone https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git ghetto-beasts
cd ghetto-beasts
git pull
```

1. **Godot 4.3+ .NET** → `project.godot` öffnen.
2. **Build** (Hammer).
3. **F5** → **Main District**. ESC = Burner Phone.

Live-Sync: `tools\auto-sync.bat` (pull alle 10 s).

## Steuerung

| Kontext | Input |
| --- | --- |
| Gasse | **WASD** / linker Stick, **LOOK**-Pad / **Q/R** / RMB, **E** Gebäude |
| Phone | **ESC** / **TAB** / PHONE-Button |
| Kampf | **1** Angriff · **2** Skill · **3** Backstein · **4** Doping · **5** Schiri |
| Knast | Pfeiltasten graben |

## Headless / CI

```bash
dotnet test tests/GhettoBeasts.Core.Tests
dotnet build GhettoBeasts.sln

godot --path . --headless -- --autobattle
godot --path . --headless -- --autobattle --card=kupferstecher_pitbull --cheats=none --stay
```

## Mobile

Touch-Joystick + LOOK-Pad, große Phone-Buttons. Für Exports: **Vulkan Mobile** Renderer (`rendering/renderer/rendering_method=mobile`) — Notes in `project.godot` / `OPEN_SOURCE.md`.

## Speichern

`user://savegame.json` – Roster, Loadouts, Progression, Laundry, Phone-Settings, Währungen.

## Cursor-Regeln

`.cursorrules`: C#-only, Stateless für Kampf-FSM, VisualManager/Toon (kein PBR), Logik in Core getrennt von UI.
