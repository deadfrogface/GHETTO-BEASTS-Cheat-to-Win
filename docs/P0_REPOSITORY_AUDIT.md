# P0 — Bestandsaufnahme und Migrationssicherung

Stand: 19.10.2026. Basis: `main` des Repositories. Diese Bestandsaufnahme beschreibt **tatsächlich sichtbaren Code**, nicht abgeschlossene Endnutzerfunktionen.

## Aktueller Aufbau

| Pfad | Ist-Zustand | Konsequenz |
| --- | --- | --- |
| `index.html` | ca. 38 KB, CSS und Spiellogik weitgehend inline, HTML-Canvas-2D | Aktueller Prototyp; **nicht überschreiben** |
| `build.js` | Node-Skript mit hartcodierter Dateistruktur und `writeFileSync` auf `index.html` | **Nicht als normalen Build ausführen**: würde Dateien ersetzen |
| `css/style.css` | 1 Byte | Kein aktiver ausgelagerter Stylesheet-Code |
| `js/main.js` | 1 Byte | Kein aktiver modularer Einstiegspunkt |
| `js/core/{audio,events,state}.js` | jeweils 1 Byte | Platzhalter |
| `js/engine/{controls,graphics}.js` | jeweils 1 Byte | Platzhalter |
| `README.md` | Älteres GDD mit 2D-Canvas-, 8-Bit- und Monetarisierungsannahmen | Historische Spezifikation; widerspricht neuer 3D-Zielrichtung |

Die Ordner `js/data`, `js/mechanics` und `js/ui` fehlen derzeit auf `main`, obwohl `build.js` sie erzeugen würde. Die bisherige `index.html` lädt keine externen Spielskripte. Es ist kein Paketmanifest (`package.json`), Testframework oder CI-Workflow im Root sichtbar.

## Erkennbarer Funktionsumfang in `index.html`

- **Karte/Navigation:** `drawMap`, `movePlayer`, `checkSightLines`, `interactMap`, `updateMapMsg`
- **Hubs/Ökonomie:** `openHub`, `washMoney`, `addDiamonds`, `buyDope`, `buyBoosterPack`
- **Kämpfe/Fahndung:** `startCombat`, `updateCombatUI`, `logCombat`, `combatAction`, `checkSuspicion`
- **Gefängnis/Minispiel:** `triggerPrisonRaid`, `prisonBribe`, `closePrison`, `startTunnelMinigame`, `drawTunnel`, `moveTunnelPlayer`
- **UI/Team/Audio:** `renderRoster`, `setActiveBeast`, `updateHUD`, `switchTab`, `toggleAudio`

Diese Funktionsnamen beweisen **nicht**, dass alle Mechaniken fehlerfrei funktionieren. Es wurden noch keine Browser- oder Gerätetests ausgeführt.

## Vor der Migration zu beachten

1. `index.html` auf `main` als funktionalen Ausgangspunkt erhalten; Änderungen nur in eigenem Branch und in kleinen PRs.
2. `build.js` nicht unbeaufsichtigt ausführen, solange es bestehende Dateien überschreibt.
3. Renderer, Game-Logic und Touch-Eingabe in getrennte Module aufteilen, ohne gleichzeitig sämtliche Spielmechaniken umzuschreiben.
4. Keine bestehende In-Game-Währung oder Bezahlfunktion ungeprüft in das neue Mobile-Produkt übernehmen.
5. Offline-Fähigkeit des bisherigen Prototyps gesondert testen: externe Stylesheets sind eingebunden.
6. Browserbasierte 3D-Testszene **isoliert** unter `prototype/3d/` entwickeln; erst nach Performance- und Gameplaytests integrieren.

## P0-Abnahme

- [x] Vorhandene Dateien und aktive Codebasis identifiziert
- [x] Vorhandene Funktionen inventarisiert
- [x] Risiko des überschreibenden `build.js` dokumentiert
- [x] Migrationsweg ohne Eingriff in die bestehende App festgelegt
- [ ] Aktuellen 2D-Prototyp im Browser manuell durchtesten
- [ ] Test- und CI-Grundlage implementieren
- [ ] 3D-Spike auf echtem Galaxy S20 FE messen

## P1-Nächster technischer Schritt

Separater Three.js-WebGL-Spike mit eigener HTML-Einstiegsdatei, mobiler Kamera, einfacher Arena, animierten Low-Poly-Figuren und Frametiming-Anzeige. Die Performance kann **nur auf realer Hardware** bestätigt werden. Kein Umbau von `index.html` vor dieser Validierung.
