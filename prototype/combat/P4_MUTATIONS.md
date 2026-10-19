# P4 — Modularer Mutationsbaukasten

**Code implementiert; Tests und Geräteabnahme offen.**

## Inhalt

- Sechs Slots: Kopf, Arme, Beine, Haut, Schwanz, Hörner.
- 15 austauschbare Teile mit einfachen Low-Poly-Varianten.
- Datengetriebene Attribute (maximale HP, Angriff, Verteidigung), Tags und Slot-Kompatibilität.
- Veränderung der 3D-Figur zur Laufzeit und Speichern der Auswahl.
- Animationen bleiben auf den vorhandenen einfachen Bewegungen aufgebaut; es gibt **noch kein Rigging** oder komplexe Skelettanimation.

## Grenzen

- Noch keine komplexen zusätzlichen Gliedmaßen, Morph Targets oder universellen Animations-Remapper.
- Keine Shop-/Forschungsfreischaltung und kein Mutationspreis (gehört zur Progression in P5).
- Mutation gilt derzeit für den Spieler im isolierten Kampf-Prototyp, nicht für sämtliche künftigen Kreaturenarten.
- Drei.js weiterhin als CDN im Prototyp: noch keine vollständig offlinefähige App.
- Tests liegen als `mutations.test.mjs` vor, wurden noch nicht ausgeführt.

## Tests

`node --test prototype/combat/*.test.mjs`
