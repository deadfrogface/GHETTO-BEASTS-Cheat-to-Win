# Direkt in Godot öffnen (Windows)

Plug-and-play: Projekt klonen → Godot .NET öffnen → Build → F5.

## 1. Voraussetzungen

| Tool | Download |
| --- | --- |
| **Godot 4.3+ .NET** (nicht die Standard-Edition) | https://godotengine.org/download/windows/ → **.NET** wählen |
| **.NET 8 SDK** | https://dotnet.microsoft.com/download/dotnet/8.0 |

Nach der Godot-.NET-Installation einmal `dotnet --version` in PowerShell prüfen (sollte `8.x` sein).

## 2. Repo klonen

In PowerShell (Ordner anpassen, falls anders):

```powershell
cd C:\Users\Lenovo\Documents
git clone https://github.com/deadfrogface/GHETTO-BEASTS-Cheat-to-Win.git ghetto-beasts
cd ghetto-beasts
```

Falls `ghetto-beasts` schon existiert und leer/veraltet ist: Ordner umbenennen oder löschen, dann erneut klonen.

Optional Live-Updates vom Cloud-Agent: `tools\auto-sync.bat` doppelklicken (Fenster offen lassen).

## 3. In Godot öffnen

1. **Godot 4.3+ .NET** starten.
2. **Import** / **Open** → Datei `project.godot` im Ordner `ghetto-beasts` wählen.
3. Oben rechts **Build** (Hammer-Symbol) klicken – C# muss einmal kompilieren.
4. **F5** (Play) drücken.

Schnellhilfe: `tools\open-in-godot.bat` doppelklicken (öffnet den Projektordner; mit gesetzter `GODOT`-Umgebungsvariable startet es Godot direkt).

## 4. Was du sehen solltest (Full Loop)

1. **District Hub** – Titel, Wallet, Buttons zu Gasse / Arena / Lab / Wash / Gacha / Shops.
2. **Gasse (Stealth)** – isometrische Alley, blaue Cop-Sichtkegel meiden, Hotspot-Pads mit **E** betreten.
3. **Arena-Setup** – nur **Roster**-Karten (Starter + Gacha), Gene aus dem Lab, Gegner wählen → Summon.
4. **Kampf** – Sabotage / Angriff / Skill; bei 100 % Verdacht → SWAT → Knast-Tunnel.
5. **CRISPR / Wash / Darknet** – Gene + Stabilisatoren, Dirty waschen, Packs für Diamonds.

Nach Kampf/Knast kehrst du zum Hub (oder zur Gasse) zurück. Spielstand: automatisch + Hub-Button „Speichern“.

## 5. Tasten (Kurz)

| Taste | Aktion |
| --- | --- |
| **WASD / Stick** | Gasse bewegen |
| **E** | Hotspot betreten |
| **1–5** | Kampf-Aktionen |
| **F5** | Spiel starten (Editor) → Hub |

Bei Problemen: Godot-.NET-Edition prüfen (nicht Standard), erneut **Build**, dann F5.  
`git pull` (oder `tools\auto-sync.bat`), falls der Cloud-Agent neue Commits gepusht hat.