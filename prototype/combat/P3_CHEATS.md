# P3 — Datengetriebene Betrugs-Engine

## Implementiert

- Separates, renderer-unabhängiges Effektregister in `cheats.mjs`.
- Vier Grundaktionen: Backstein, Bestechung, Ablenkung, Sabotage.
- Automatisch generierte Kombinationen **verschiedener Methoden**, ohne handgeschriebene Paarregeln.
- Definierte Effekte: `damage`, `heal`, `weaken`, `skip`; deterministische Zufallswürfe.
- Katalog-Validierung mit Mengen-/Typgrenzen, Eindeutigkeit und Ausschluss inkompatibler Methodengleichheit.
- Anbindung an den Kampfablauf und die mobile Betrugs-Auswahl.
- Tests für Katalog, Kombinationen, deterministisches Verhalten, Wertebereiche und Fehlschläge.

## Designgrenzen / offene Punkte

- **Keine unbegrenzte kreative KI**: Kombinationen bestehen aus vordefinierten Effekten.
- Die `cost`-Eigenschaft ist bislang **nur Metadatum**; es gibt noch kein Ressourceninventar und daher keine Abbuchung. Vor produktivem Gameplay zwingend Kostenprüfung implementieren.
- Tag-Regeln sind derzeit einfach (verschiedene Methoden, `exclusive`); komplexere Ziel- und Zustandskompatibilität folgt später.
- Effekte werden innerhalb einer Aktion nacheinander angewendet; spätere Prioritäts-/Resistenzregeln sind noch nicht implementiert.
- Savegame-Schema erlaubt optionale neue Felder für alte P2-Spielstände; Migration/Versionierung folgt.
- Node-Tests sind geschrieben, aber **nicht hier ausgeführt**. Browser-/Gerätetest und Balancing fehlen.
- Bestehende 2D-App bleibt unberührt; P3 baut auf P2 auf.

## Tests

```sh
node --test prototype/combat/rules.test.mjs prototype/combat/cheats.test.mjs
```

Zum manuellen Test `prototype/combat/` per HTTP-Server öffnen, Betrug im Dropdown wählen und über die Betrugs-Schaltfläche ausführen. Testen: Erfolg, Fehlschlag, Ablenkung (Gegnerzug aussetzen), Schwächung, Kombinationen, Sieg/Niederlage, Reload.

## Abnahme

- [x] Datengetriebener Effektbaukasten
- [x] Mehrere Methoden und generierte Kombinationen
- [x] Eingabevalidierung und definierte Grenzen
- [x] Einbindung in den P2-Kampf
- [x] Testfälle geschrieben
- [ ] Automatisierte Tests ausgeführt und grün
- [ ] Ressourcen-/Kostenregeln und umfassende Kompatibilitätsregeln
- [ ] Balancing und echte Android-/iOS-Tests
