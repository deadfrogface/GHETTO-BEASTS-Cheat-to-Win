# GHETTO BEASTS: DNA & Corruption

Cel-shaded Cyberpunk-Rundenkampf für **Godot 4 (.NET / C#)**: Bestien und Ghetto-NPCs werden als Hologramm-Karten auf den Asphalt geschmettert, mit CRISPR-Genen mutiert und im Ring mit schmutzigen Tricks zum Sieg geprügelt – bis das SWAT-Team die Arena stürmt.

Dieser Stand ist das **spielbare Fundament** (Fahrplan-Schritte 1–2, Schritt 3 als erste Version):

| Bereich | Stand |
| --- | --- |
| Daten-Systeme | **50+ Bestien, 50+ NPCs** (15 Helden + Straßen-Roster), 15 CRISPR-Mutationen, Seltenheiten, Kategorien |
| Kampf-Engine | `Stateless`-Zustandsmaschine (Summon → PlayerTurn ⇄ CheatPhase → Resolving → EnemyTurn → Victory/Defeat/Raid) |
| Sabotage | Backstein-Wurf, Illegale Injektion, Schiri-Bestechung, SWAT-Verdachts-Meter mit Razzia bei 100 % |
| CRISPR | Kopf/Gliedmaßen/Haut-Slots, Gen-Instabilität (>50 % Verwirrung, >80 % Zellausfall/Zorn, 100 % Mutation/Kollaps) |
| Wirtschaft | $ Dirty Cash, $ Clean Cash, 💎 Dirty Diamonds, Waschsalon (20 % Gebühr), Beschlagnahmung bei Razzia |
| Speichern | JSON via `Newtonsoft.Json` (`user://savegame.json`); optional `LiteDbSaveStore` für NoSQL |
| Visuals | `VisualManager` erzwingt Cel-Shader auf Meshes; Holo-Karten-Summon, Neon-Arena, CRT-Overlay |
| Asset-Pipeline | Autoload `AssetPipeline` lädt `res://assets/3d_models/**/*.glb`, Mixamo-Anims, Cel-Pass, Placeholder-Fallback |
| Katalog | `DatabaseCatalog` – Registry über 100+ Einheiten (Lookup nach Id/Rarity/Kategorie) |
| Knast | Grid-Tunnel-Escape nach SWAT-Raid (Graben, Werkzeuge, Wärter, Diamond-Bestechung) |

Noch nicht enthalten (nächste Schritte): Oberwelt & Stealth, Gacha-Pack-Opening, Audio, Touch-Steuerung, echte 3D-Modelle, 2v2-Kämpfe.

---

## Voraussetzungen

- **Godot 4.3+ .NET-Edition** (die Variante mit C#-Support): <https://godotengine.org/download/windows/>
- **.NET 8 SDK**: <https://dotnet.microsoft.com/download/dotnet/8.0>
- Git

## Direkt in Godot öffnen (Windows)

Kurzfassung — Details und Troubleshooting: **[LAUNCH.md](LAUNCH.md)**.

```powershell
cd C:\Users\Lenovo\Documents
git clone https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git ghetto-beasts
cd ghetto-beasts
```

1. **Godot 4.3+ .NET** starten → `project.godot` öffnen/importieren.
2. Oben rechts **Build** (Hammer) klicken.
3. **F5** → Setup-Screen → Karte summieren → Kampf. Tasten **1–5** für Schnellwahl.

Optional: `tools\open-in-godot.bat` (öffnet den Ordner bzw. Godot, wenn `GODOT` gesetzt ist).

## Live-Testen auf deinem Laptop (Auto-Sync)

Die Entwicklung läuft in einem Cloud-Agenten, der jeden fertigen Teilschritt committet und pusht. Damit du den Fortschritt **on the fly** in Godot siehst:

1. Repo wie oben nach `C:\Users\Lenovo\Documents\ghetto-beasts` klonen (oder dort `git pull`).
2. **Auto-Sync starten**: `tools\auto-sync.bat` doppelklicken. Das Fenster offen lassen – es holt alle 10 Sekunden neue Commits (`git pull --ff-only`) und piept bei jedem Update.
3. Nach einem Update Godot fokussieren und erneut **F5** – C# wird beim Start neu kompiliert.

> Tipp: Bearbeite Dateien, die der Agent gerade ändert, lokal nicht parallel – sonst schlägt der Fast-Forward-Pull fehl. Eigene Experimente am besten in neuen Dateien oder einem eigenen Branch.

## So wird gespielt

1. **Setup-Screen**: Karte wählen (Bestie oder NPC), bis zu drei CRISPR-Mutationen ausrüsten (nur Bestien), Gegner wählen. Die Vorschau zeigt Werte und Gen-Instabilität.
2. **Karte ausspielen**: Die Hologramm-Karte wird auf den Asphalt geschmettert, die Kreatur manifestiert sich.
3. **Pro Zug**: optional **eine** Sabotage, danach **Angriff** oder **Skill** (Skill hat 3 Züge Cooldown).
   - Backstein-Wurf: 20 % Chance auf Sofort-Sieg, +35 % Verdacht
   - Illegale Injektion (60 $): +50 % Angriff für 3 Züge, +20 % Verdacht, +8 % Instabilität
   - Schiri-Bestechung (120 $): −35 % Verdacht (15 % Risiko, dass der Schiri dich trotzdem meldet)
4. **SWAT-Meter 100 %** → Razzia: Kampf abgebrochen, gesamtes Schmutzgeld beschlagnahmt.
5. **Waschsalon** im Setup: 100 $ Dirty → 80 $ Clean.

## Projektstruktur

```text
res://
├── project.godot / GhettoBeasts.csproj / GhettoBeasts.sln
├── src/GhettoBeasts.Core/    # Reine C#-Spiellogik (kein Godot) – Data, Core, Combat
├── tests/                    # xUnit-Tests für die Core-Logik
├── scripts/                  # Godot-C#: Data-Resources, GameManager, Combat, Entities, UI, Visuals
├── scenes/combat/            # combat.tscn (Hauptszene)
├── data/cards/               # BeastData/NpcData-.tres pro Karte (Farben, Form, Skalierung, eigenes Modell)
├── assets/3d_models/         # Meshy/Tripo/Mixamo .glb Drops (beasts/, npcs/, …) – siehe README dort
├── shaders/                  # toon_cel, outline, holo_card, crt_overlay
└── tools/                    # auto-sync, open-in-godot.bat
```

Windows-Schnellstart: [`LAUNCH.md`](LAUNCH.md)

Die Spielwerte (HP, Angriff, Skills) stehen in `src/GhettoBeasts.Core/Data/ContentCatalog.cs` und werden über `DatabaseCatalog` indexiert.  
**3D-Modelle:** `.glb` als `<catalogId>.glb` nach `assets/3d_models/beasts/` oder `npcs/` legen – `AssetPipeline` erkennt sie beim Start, weist den Cel-Shader zu und spielt Idle-Clips. Fehlt eine Datei, entsteht ein toon-Platzhalter. Details: [`assets/3d_models/README.md`](assets/3d_models/README.md).

## Tests & Build (Kommandozeile)

```bash
dotnet test tests/GhettoBeasts.Core.Tests     # Logik-Tests
dotnet build GhettoBeasts.sln                 # Godot-C#-Projekt kompilieren
```

---

## Open-Source-Bausteine

### NuGet-Pakete

| Paket | Nutzen | Status |
| --- | --- | --- |
| [Stateless](https://github.com/dotnet-state-machine/stateless) | Zustandsmaschine für den Rundenkampf | eingebunden |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | Savegame-Serialisierung | eingebunden |
| [LiteDB](https://www.litedb.org/) | NoSQL-Savegames für Käfig/Inventar | geplant (Schritt 5) |

### Shader & Visuals

- [Godot Shaders – Toon/Cel, Outline, CRT, Glitch](https://godotshaders.com/)
- [Godot Asset Library](https://godotengine.org/asset-library/asset)
- Eigene schlanke Versionen liegen in `shaders/` und können jederzeit durch Community-Shader ersetzt werden.

### Karten-Systeme, Turn-Based & Input

- [Godot Demo Projects](https://github.com/godotengine/godot-demo-projects) (Turn-Based-, 3D- und GUI-Beispiele)
- [pokemon-js](https://github.com/chase-manning/pokemon-js) als Referenz für Kampf-/Stat-Logik
- Für 3D-Karten-Handling, Touch-Input und virtuelle Joysticks: in der [Godot Asset Library](https://godotengine.org/asset-library/asset) nach „card“, „touch“ bzw. „virtual joystick“ suchen (Godot-4-kompatible Versionen wählen).

## Cursor-Regeln

`.cursorrules` im Root enthält die Projekt-Regeln (C#-only, Godot-Resources, Stateless, Logik getrennt von UI).
