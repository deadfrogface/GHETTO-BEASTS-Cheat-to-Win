# Direkt in Godot öffnen (Windows)

Plug-and-play: Projekt klonen → Godot .NET öffnen → Build → F5.

## 1. Voraussetzungen

| Tool | Download |
| --- | --- |
| **Godot 4.3+ .NET** (nicht die Standard-Edition) | https://godotengine.org/download/windows/ → **.NET** wählen |
| **.NET 8 SDK** | https://dotnet.microsoft.com/download/dotnet/8.0 |

Nach der Godot-.NET-Installation einmal `dotnet --version` in PowerShell prüfen (sollte `8.x` sein).

## 2. Repo klonen / updaten

In PowerShell (Ordner anpassen, falls anders):

```powershell
cd C:\Users\Lenovo\Documents
git clone https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git ghetto-beasts
cd ghetto-beasts
```

Bereits geklont? Auf den neuesten Stand bringen:

```powershell
cd C:\Users\Lenovo\Documents\ghetto-beasts
git checkout main
git pull origin main
# falls der Remote anders heißt:
git pull https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git main
```

Danach in Godot **Build** (Hammer), dann **F5**.

**Check dass das Update wirklich da ist:**

```powershell
git log -1 --oneline
# erwartet etwas mit STREET-v2 / street-tier / campaign
```

Im Spiel oben links muss stehen: **★ STREET-v2 LIVE** — plus ein cyan Billboard über der Arena. Fehlt das → falscher Ordner oder kein Rebuild.

Optional Live-Updates: `tools\auto-sync.bat` (zieht `main` alle 10 s).

## 3. In Godot öffnen

1. **Godot 4.3+ .NET** starten.
2. **Import** / **Open** → Datei `project.godot` im Ordner `ghetto-beasts` wählen.
3. Oben rechts **Build** (Hammer-Symbol) klicken – C# muss einmal kompilieren.
4. **F5** (Play) drücken.

Schnellhilfe: `tools\open-in-godot.bat` doppelklicken (öffnet den Projektordner; mit gesetzter `GODOT`-Umgebungsvariable startet es Godot direkt).

## 4. Was du sehen solltest (Full Loop)

1. **Main District** – 3D-Cyberpunk-Gasse (+ Back Alley / Dock nach Quests), Companion folgt.
2. **Gebäude** – Neon-Pads, **E** → CRISPR / Wash / Darknet / DOPE / WEAP / ARENA (manche erst nach Wins).
3. **Burner Phone** – **ESC** / **TAB** / großer PHONE-Button: Käfig (+ CRISPR-Equip), Darknet-Packs (inkl. Gen-Corrupted), GPS-Live-Map + Quests, Wallet-Timer, Contacts, Settings (Cel/Outline/FPS/Audio/2v2).
4. **Touch** – linker Stick, rechter LOOK-Pad.
5. **Wildtiere / Trainer / Cops** → Kampf; Sabotage → SWAT → Knast-Escape → zurück in die Gasse.
6. **Save** – Phone → Save & Sync (auch auto bei Transitions).

## 5. Tasten (Kurz)

| Taste | Aktion |
| --- | --- |
| **WASD / Stick** | Gasse bewegen (kamera-relativ) |
| **LOOK-Pad / Q·R / RMB** | Kamera drehen |
| **E** | Gebäude betreten |
| **ESC / TAB** | Burner Phone |
| **1–5** | Kampf-Aktionen |
| **F5** | Spiel starten (Editor) → Main District |

## 6. Mobile / Vulkan Mobile

Für Android/iOS-Export: **Project Settings → Rendering → Renderer → Rendering Method = mobile** (Vulkan Mobile). Cel/Outline-Shader sind leichtgewichtig; ETC2/ASTC-Import ist in `project.godot` aktiv.

Bei Problemen: Godot-.NET-Edition prüfen (nicht Standard), erneut **Build**, dann F5.  
`git pull`, falls der Cloud-Agent neue Commits gepusht hat.
