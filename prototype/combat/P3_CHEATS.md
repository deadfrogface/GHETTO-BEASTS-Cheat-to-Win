# P3 — Betrugs-Engine: Implementierungsstand

**Umgesetzt im isolierten Kampf-Prototyp; Tests und Balancing noch nicht abgenommen.**

## Spielregeln

- Fünf datengetriebene Grundaktionen: Backstein, Bestechung, Ablenkung, Sabotage, manipuliertes Medkit.
- Generierte Paar-Kombinationen verschiedener Methoden mit kompatiblen Zielen, ohne exklusive oder medizinische Mischungen.
- Begrenzte Effekte: Schaden, Heilung, Gegner schwächen und Gegnerzug überspringen.
- Pro Aktion: deterministische Erfolgschance, Fehlschlageffekt, Bargeldkosten, Verdachtsanstieg und Abklingzeit.
- Start mit 8 $; unbezahlbare und abklingende Aktionen sind gesperrt. 100 Verdacht führt zur Niederlage durch Razzia.
- Wertebereiche und Katalogschema werden vor Anwendung validiert. Aktionen mit unzulässigen Kosten oder Cooldowns verbrauchen keinen Zug und keinen Zufallswert.
- Savegame enthält Geld, Verdacht, Abklingzeiten und Status; ältere P2-Spielstände werden beim Laden ergänzt. Neue Speicherkennung schützt die ursprüngliche P2-Demo.

## Grenzen

- **Keine automatische Erfindung neuer Grundeffekte:** Kombinationsgenerator verknüpft definierte Regeln.
- Die Gegner-KI bleibt einfach und nicht auf das gesamte Betrugssystem abgestimmt; umfangreiche KI gehört zu P6.
- Die Ökonomie ist eine isolierte Demo mit 8 $ Startkapital, kein vollständiges Inventar- oder Shop-System (P5).
- Keine automatischen oder manuellen Tests hier ausgeführt. Balancing und reale Smartphone-Leistung bleiben offen.
- Three.js kommt im Demo-Prototyp noch vom CDN; für die Offline-App muss es lokal gebündelt werden.
- Der bisherige 2D-Prototyp wurde nicht verändert.

## Testaufruf

`node --test prototype/combat/rules.test.mjs prototype/combat/cheats.test.mjs`

**Abnahme P3:** Effektbaukasten, Kombinationen, Kosten, Abklingzeiten, Verdacht, Gegenmaßnahmen (Gegner-Skip/Schwächung), Spielstand und UI implementiert. Testausführung, Balancing und mobile Abnahme stehen aus.
