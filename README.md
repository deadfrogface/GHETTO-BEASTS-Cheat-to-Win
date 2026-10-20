# GHETTO BEASTS: DNA & Corruption

Cel-shaded Cyberpunk-Kreaturenkampf für **Godot 4 (.NET / C#)**: physische 3D-Gasse, Burner-Phone-Menü, Companion-Bestie, CRISPR, Darknet-Gacha, Trainer-Sabotage – bis das SWAT-Team die Arena stürmt.

## Spielbarer Slice (F5)

```text
Main District (3D Alley)
  → Companion folgt · Wildtiere / Cops
  → E an Gebäude (CRISPR / Wash / Darknet / DOPE / WEAP / ARENA)
  → ESC Burner Phone (Käfig / Darknet / GPS / Wallet)
  → Kampf + Sabotage → Raid → Knast-Escape ODER Sieg → zurück in die Gasse
```

| Modul | Inhalt |
| --- | --- |
| **Main District** | Hauptszene (`scenes/district/main_district.tscn`) – Third-Person-Gasse |
| **Burner Phone** | ESC/TAB – diegetisches Meta-UI (Roster, Gacha, GPS, Wallet) |
| **Companion** | Aktive Roster-Bestie folgt dem Spieler |
| **Gebäude** | `Area3D`-Zonen, zu Fuß + **E** (keine 2D-Hub-Buttons) |
| **Arena** | Kampf mit Setup, Summon, Sabotage, Raid |
| **CRISPR / Wash / Shops** | Bestehende Building-Szenen als Interiors |
| **Audio** | Autoload `AudioBus` – Dateien optional, sonst Blips |
| **Tests** | xUnit Core inkl. Gacha/Lab/Roster |

Open-Source-Referenzen (Controller, Pet-Follow, Pause-UI): **[OPEN_SOURCE.md](OPEN_SOURCE.md)**.

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
3. **F5** → **Main District** (3D-Gasse). ESC = Burner Phone.

Live-Sync: `tools\auto-sync.bat` (pull alle 10 s).

## Steuerung

| Kontext | Input |
| --- | --- |
| Gasse | **WASD** / Stick, **Q/R** Kamera, RMB Orbit, **E** Gebäude |
| Phone | **ESC** / **TAB** – Käfig, Darknet, GPS, Wallet |
| Kampf | **1** Angriff · **2** Skill · **3** Backstein · **4** Doping · **5** Schiri |
| Knast | Pfeiltasten graben |

## Headless / CI

```bash
dotnet test tests/GhettoBeasts.Core.Tests
dotnet build GhettoBeasts.sln

# Autobattle (GameManager leitet --autobattle früh in die Arena um):
godot --path . --headless -- --autobattle
godot --path . --headless -- --autobattle --card=kupferstecher_pitbull --cheats=none --stay
```

## Projektstruktur

```text
res://
├── scenes/district/     # Main District (F5) + legacy district
├── scenes/combat/       # Arena + Setup + Prison
├── scenes/buildings/    # CRISPR, Wash, Darknet, DOPE, WEAP
├── scenes/hub/          # Stub → redirect zur Gasse
├── scripts/District/    # MainWorld, Player, Companion, WildBeast, Buildings
├── scripts/UI/          # BurnerPhone, HUD, BuildingShell
├── src/GhettoBeasts.Core/
├── tests/
├── assets/
└── shaders/             # toon_cel, outline, holo_card, crt
```

## Speichern

`user://savegame.json` – Roster, Loadouts, Stabilizer-Stacks, Inventar, Währungen, Bilanz.

## Cursor-Regeln

`.cursorrules`: C#-only, Stateless für Kampf-FSM, VisualManager/Toon (kein PBR), Logik in Core getrennt von UI.
