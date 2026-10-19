# Phase 2 — 3D Combat Vertical Slice

Isolierte rundenbasierte 3D-Kampf-Demo unter `prototype/combat/`. Der vorhandene 2D-Prototyp bleibt unverändert.

**Implementiert:** zwei unterscheidbare animierte Low-Poly-Kämpfer, Lebenspunkte, Angriff, Deckung, riskanter Backstein-Betrug, deterministischer Pseudozufall, Gegnerzug, Sieg/Niederlage, Neustart, Touch-Buttons, FPS-Anzeige, versionierter separater LocalStorage-Key.

**Nicht implementiert:** volle Spiel-KI, 3D-Asset-Animationen mit Skelett, Inventar, Stadt-Integration, Android/iOS-App-Build, Offline-Bundling, realer Gerätetest. Three.js wird wie beim P1-Spike über ein CDN geladen.

## Testen

Vom Repository-Root `python -m http.server 8000` starten und `http://localhost:8000/prototype/combat/` öffnen. Am Smartphone die lokale IP des Servers verwenden. Die Spielrunde bleibt beim Reload erhalten. Wenn während des Gegnerzuges neu geladen wird, führt die Demo den Gegnerzug nach dem Laden aus.

## Automatische Regeltests

Mit Node.js (ohne npm-Abhängigkeiten): `node --test prototype/combat/rules.test.mjs`

## Akzeptanz

- [x] Zwei animierte 3D-Platzhalter-Kämpfer
- [x] Vollständige rundenbasierte Kampfsequenz mit Gegnerreaktion
- [x] Sieg, Niederlage, Neustart, lokaler Save/Load
- [x] Deterministische Regeln, die ohne 3D-Renderer getestet werden können
- [ ] Browser- und Android-Test auf realen Geräten
- [ ] Offlinefähiges Three.js-Bundle und native App
