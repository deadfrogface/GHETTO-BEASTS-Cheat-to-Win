# GHETTO BEASTS: DNA & Corruption

Cel-shaded Cyberpunk-Rundenkampf für **Godot 4 (.NET / C#)**: Bestien und Ghetto-NPCs werden als Hologramm-Karten auf den Asphalt geschmettert, mit CRISPR-Genen mutiert und im Ring mit schmutzigen Tricks zum Sieg geprügelt – bis das SWAT-Team die Arena stürmt.

Dieser Stand ist das **spielbare Fundament** (Fahrplan-Schritte 1–2, Schritt 3 als erste Version):

| Bereich | Stand |
| --- | --- |
| Daten-Systeme | 8 Bestien, 7 NPCs, 9 CRISPR-Mutationen, Seltenheiten, Kategorien |
| Kampf-Engine | `Stateless`-Zustandsmaschine (Summon → PlayerTurn ⇄ CheatPhase → Resolving → EnemyTurn → Victory/Defeat/Raid) |
| Sabotage | Backstein-Wurf, Illegale Injektion, Schiri-Bestechung, SWAT-Verdachts-Meter mit Razzia bei 100 % |
| CRISPR | Kopf/Gliedmaßen/Haut-Slots, Gen-Instabilität (>50 % Verwirrung, >80 % Zellausfall/Zorn, 100 % Mutation/Kollaps) |
| Wirtschaft | $ Dirty Cash, $ Clean Cash, 💎 Dirty Diamonds, Waschsalon (20 % Gebühr), Beschlagnahmung bei Razzia |
| Speichern | JSON-Savegame via `Newtonsoft.Json` unter `user://savegame.json` |
| Visuals | Toon-Shader + Outline, Holo-Karten-Summon mit Partikel-Burst & Screen-Shake, Neon-Hinterhof-Arena, CRT-Overlay |

Noch nicht enthalten (nächste Schritte): Oberwelt & Stealth, Gacha-Pack-Opening, Knast-Tunnel-Minispiel, LiteDB, Audio, Touch-Steuerung, echte 3D-Modelle.

---

## Voraussetzungen

- **Godot 4.3+ .NET-Edition** (die Variante mit C#-Support): <https://godotengine.org/download>
- **.NET 8 SDK**: <https://dotnet.microsoft.com/download/dotnet/8.0>
- Git

## Live-Testen auf deinem Laptop (Auto-Sync)

Die Entwicklung läuft in einem Cloud-Agenten, der jeden fertigen Teilschritt committet und pusht. Damit du den Fortschritt **on the fly** in Godot siehst:

1. Repository klonen (oder deinen leeren Godot-Ordner ersetzen):
   ```powershell
   cd C:\Users\Lenovo\Documents
   git clone <REPO-URL> ghetto-beasts
   ```
   Falls der Ordner `ghetto-beasts` schon existiert (leeres Godot-Projekt): Inhalt vorher löschen bzw. in einen Backup-Ordner verschieben – das Repo bringt ein eigenes `project.godot` und `GhettoBeasts.csproj` mit.
2. **Auto-Sync starten**: `tools\auto-sync.bat` doppelklicken. Das Fenster offen lassen – es holt alle 10 Sekunden neue Commits (`git pull --ff-only`) und piept bei jedem Update.
3. **Godot öffnen** → *Importieren* → `ghetto-beasts/project.godot`. Beim ersten Start oben rechts **Build** (Hammer) klicken.
4. **F5** startet das Spiel. Nach einem Update vom Auto-Sync einfach Godot fokussieren (Szenen/Ressourcen werden neu geladen) und erneut **F5** drücken – C# wird beim Start automatisch neu kompiliert.

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
├── shaders/                  # toon_cel, outline, holo_card, crt_overlay
└── tools/                    # auto-sync.ps1 / .bat
```

Die Spielwerte (HP, Angriff, Skills) stehen in `src/GhettoBeasts.Core/Data/ContentCatalog.cs`. Das Aussehen jeder Karte steuerst du im Godot-Inspector über `data/cards/<id>.tres` – dort kannst du unter **Model Scene** auch ein echtes `.glb`/`.tscn`-Modell einhängen, das den Platzhalter ersetzt.

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
