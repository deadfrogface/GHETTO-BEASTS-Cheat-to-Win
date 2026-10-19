# GHETTO BEASTS: Cheat to Win — Entwicklungsplan (Stand: 19.10.2026)

> **Status:** Beschlossene Zielrichtung und geplante Arbeit. Keine Behauptung, dass die folgenden Systeme bereits implementiert oder die Leistungsziele erreicht sind.
>
> **Hinweis:** Die bisherige `README.md` dokumentiert einen älteren 2D-Canvas-Entwurf unter dem Arbeitstitel „DNA & CORRUPTION“. Dieser Plan legt die neue technische und visuelle Zielrichtung für „Cheat to Win“ fest. Bestehender Code bleibt bis zu einer geprüften Migration unangetastet.

## 1. Produktvision

Ein satirisches, strategisch-chaotisches Monster-Sammel- und Kampfspiel für Smartphones. Gewonnen wird nicht allein durch stärkere Kämpfer, sondern durch kreative, miteinander kombinierbare Betrugsmethoden, Mutationen, Sabotage und den Umgang mit deren Konsequenzen.

- Rundenbasierte Kämpfe mit Kreaturen, gewöhnlichen Tieren und erwachsenen NPC-Kämpfern.
- Betrügen ist ein vollwertiges System mit Voraussetzungen, Risiken, Gegenmaßnahmen und Folgen.
- Gegner erkennen Muster, reagieren auf Tricks und können ihrerseits betrügen.
- Überzeichnete, humorvolle Darstellung statt realistischer Gewalt.
- Offline spielbarer Kern ohne Serverpflicht oder kostenpflichtige KI-APIs.

## 2. Verbindliche Designrichtung

| Bereich | Vorgabe |
| --- | --- |
| Zielplattform | Primär Android; später iOS; Browser als Entwicklungs- und zusätzliche Laufzeit |
| Grafikstil | **Stilisiertes Low-Poly-3D mit leichtem Cel-Shading**, kräftigen Farben und karikaturhaften Figuren |
| Perspektive | Leicht schräge, performante Stadtansicht; lebendigere Kamera in Kämpfen |
| Rendering | **Three.js** als bevorzugte WebGL-3D-Bibliothek (vor Integration technisch evaluieren) |
| Code | Bestehendes JavaScript schrittweise modularisieren; TypeScript optional/mittelfristig |
| Mobile Packaging | Capacitor als Kandidat; Android und iOS tatsächlich auf Geräten testen |
| Ausschlüsse | **Kein Unreal, Unity, Meshy; vorerst kein Godot** |
| Assets | Eigenständiger Stil; keine Übernahme geschützter Pokémon-Modelle, Namen oder Texturen |
| Grundsatz | Open-Source-Software bevorzugen; Lizenzen, Wartung und Paketgröße prüfen |

Die **bestehende HTML/Canvas-Version ist ein Prototyp**, keine bereits erfolgte 3D-Migration. Bestehende Spiellogik nur nach Prüfung übernehmen.

## 3. Leistungs- und Hardwareziele

- Primäres Testgerät: **Samsung Galaxy S20 FE 4G**; vorläufig konservativ als Exynos-990/Mali-G77-Version angenommen, *tatsächliche Variante noch verifizieren*.
- Gewünschter Mindestwert des Projektinhabers: **mindestens 69 berechnete FPS** auf dem Referenzgerät unter definierter Testlast. Zieloptimierung: **90 FPS**, sofern das Gerät im 120-Hz-Modus und mit ausreichender Leistung läuft.
- Ältere Geräte wie das Galaxy S9: **60 FPS anstreben**, aber keine Garantie ohne Messung und klar definierte Grafikstufe.
- Messgrößen: durchschnittliche FPS **und** 1%-Low/Frametimes, RAM, Start- und Ladezeiten, Temperatur/Throttling, Akkuverbrauch, Touch-Latenz.
- 69 FPS sind **noch nicht gemessen**; 60-/120-Hz-Displaymodi und Framerate-Limits müssen gesondert überprüft werden.
- Optimierungen **von Anfang an**: Low-Poly-Modelle, Instancing, Material-/Texturatlanten, Frustum-Culling, LOD, begrenzte Lichteffekte/Schatten, Chunk-Streaming, Pooling, sparsame Partikel, geringe Draw Calls.
- Leistungsbudgets werden nach dem ersten Benchmark anhand *echter Messungen* festgelegt. Ein früher Vorschlag von 2 GB RAM ist keine belegte Grenze.

## 4. Architektur und Module

### 4.1 Technisches Fundament
- Entkoppelte Module für Rendering, Eingabe/Touch, Szenen, Kämpfe, Inventar, Welt, Audio und Speichersystem.
- Spiellogik unabhängig vom Renderer testen; späterer Rendererwechsel soll nicht alle Kampfregeln zerstören.
- Deterministischer RNG mit Seed für reproduzierbare Kämpfe und generierte Kombinationen.
- Savegame-Versionierung, Migrationen, robuste lokale Sicherung, Offline-Start.

### 4.2 Kampf-Engine
- Rundenbasiert: Initiative, Aktionen, Status, Widerstände, Sieg/Niederlage, Balancing.
- Zieltypen: Trainer, Kämpfer, Kreatur, Umgebung, Gegenstand, Schiedsrichter.
- Gegner-KI mit Betrugsstrategien und situationsabhängigen Gegenmaßnahmen.
- Klare Protokollierung von Wirkungen, Risiken und Auslösern.

### 4.3 Prozedurale Betrugs-Engine (Alleinstellungsmerkmal)
- Datengetriebener **Regelbaukasten** aus Aktion, Ziel, Effekt, Kosten, Voraussetzung, Risiko, Gegenmaßnahme.
- Tag-basierte Kombinatorik, z. B. `mechanisch`, `biologisch`, `illegal`, `ablendend`, `trainer-target`.
- Validator verhindert inkompatible, unendliche und unfaire Kombinationen; Balance-Limits und reproduzierbare Tests.
- Beispielkategorien: Bestechung, Ablenkung, Sabotage, manipulierte Ausrüstung, Doping als fiktiver Spieleffekt, Trainerattacken.
- **Wichtige Machbarkeitsgrenze:** Viele neue Kombinationen innerhalb definierter Grundmechaniken sind möglich; ein Offline-Regelsystem erfindet nicht unbegrenzt völlig neuartige Programmfunktionalität.

### 4.4 Mutations- und Charakterbaukasten
- Kreaturen, Tiere und erwachsene menschliche NPCs mit Attributen, Fähigkeiten, Resistenz-Tags und Spezialreaktionen.
- Modul-Steckplätze für Kopf, Arme, Beine, Panzer/Haut, Schwanz, Flügel, Implantate.
- 3D-Mutation: austauschbare Meshes, Attachment-Points, Materialvarianten, Morphs wo sinnvoll.
- Morphologie- und Animationsregeln: Kompatibilitätsprüfung für Extra-Arme, Tentakel usw.; Fallback-Animationen.
- Genetische Instabilität/Nachteile als Balancing-Mittel; bisherige CRISPR-Ideen nur nach Gameplay-Prüfung übernehmen.
- Satirische NPC-Fähigkeiten und Reaktionen als datengetriebene, jugendgerechte Cartoon-Inszenierung.

### 4.5 Stadt, Progression und Wirtschaft
- Kleine nachladbare Stadtviertel, Shops, Arena, Labor, Schwarzmarkt, NPCs, Quests.
- Sammlung, Inventar, Freischaltungen, Entwicklung, Ressourcen und Balancing.
- Verdachts-/Fahndungssystem, Polizei, Strafen, Gefängnis und Flucht als komödiantische Spielmechaniken.
- Vorhandene Konzepte wie Dirty/Clean Cash und Booster-Packs bewerten, nicht automatisch als Monetarisierung übernehmen.
- Bei realen In-App-Käufen/zufallsbasierten Packs: gesonderte rechtliche, Store- und Jugendschutzprüfung; **keine Festlegung auf Pay-to-Win**.

### 4.6 Mobile, Audio und UI
- Touch-first: virtuelle Steuerung, gut erreichbare Aktionen, skalierbare Oberflächen, verschiedene Displayformate und Notches.
- Audio vorzugsweise Web Audio API; Howler.js nur bei echtem Mehrwert.
- Offline-Speicherung lokal (IndexedDB/Dexie als Kandidat); Recovery bei App-Schließung.
- Capacitor-Kandidatur für Android/iOS; WebGL-Unterstützung und Memory-Verhalten in WebViews verifizieren.

## 5. Open-Source-Strategie

Kandidaten: **Three.js** (3D), **Capacitor** (App-Hülle), **Rapier** (Physik, nur falls nötig), **Dexie** (IndexedDB), **Howler.js** (Audio). Erst Bedarf, Lizenz, Bundle-Gewicht, Pflegezustand und Gerätekompatibilität prüfen. Keine unnötigen Bibliotheken importieren.

## 6. Implementierungsreihenfolge und überprüfbare Abnahmekriterien

| Phase | Arbeit | Fertig, wenn … |
| --- | --- | --- |
| **P0 — Bestandsaufnahme** | Vorhandene Dateien, Features und Build überprüfen; 2D-Prototyp konservieren | Funktionsinventar + reproduzierbarer Ausgangsstand dokumentiert |
| **P1 — Mobiler 3D-Spike** | Three.js-Szene, cartoonartige Arena, Kamera, Touch, Android-Testpaket | Auf S20 FE startbar, FPS/Frametimes und thermisches Verhalten messbar |
| **P2 — Vertical Slice Kampf** | Zwei animierte Kämpfer, rundenbasierter Ablauf, lokale Spielstände | Ein vollständiger, wiederholbarer Kampf auf dem Smartphone spielbar |
| **P3 — Betrugs-Engine** | Grundeffekte, Tags, Generator, Risiken, Gegenmaßnahmen | Generierte Kombinationen deterministisch testbar; keine Regelkonflikte |
| **P4 — Mutationen** | Charakter-Module, Steckplätze, Stats, visuelle Mutation | Mehrere sichtbar unterschiedliche, animierte und regelkonforme Mutationen |
| **P5 — Welt und Fortschritt** | Erster Stadtabschnitt, NPCs, Shops, Quests, Inventar | Spielschleife aus Erkunden → Kämpfen → Entwickeln → Speichern funktioniert |
| **P6 — Inhalt und Balance** | Mehr Kämpfer, Bezirke, KI, Humor, Ökonomie | Progression und Gegenstrategien mit automatisierten Tests geprüft |
| **P7 — Mobile-Optimierung** | Profiling, Streaming, Render-Budgets, Bedienbarkeit | Zielmessungen auf echter Hardware dokumentiert; Bottlenecks behoben |
| **P8 — Beta und Release** | QA, Crash-Tests, Offline, Android-Auslieferung, iOS-Pipeline | Installierbare und getestete Builds; Store-Freigaben separat |

**MVP/erste spielbare Version:** 1 mobile 3D-Arena, 3 Kämpfer, grundlegende KI, mehrere Betrugsaktionen, erste sichtbare Mutationen, Touch-Bedienung, Offline-Spielstand, Android-Testbuild und Performancebericht.

## 7. Qualitätssicherung

- Unit-Tests für Status-/Schadensregeln, Betrugsgenerator, Kompatibilität, Speichern und Laden.
- Property-/Fuzz-Tests für tausende erzeugte Kombinationen: keine NaNs, Softlocks, unendlichen Züge oder unerreichbaren Zustände.
- UI-/Integrations-Tests und mindestens ein echter Android-Gerätetest pro größerer Grafikänderung.
- PRs klein und überprüfbar; CI beschleunigen durch sinnvolle Caches und parallele, unabhängige Jobs **ohne Testqualität zu kürzen**.
- Grafik-FPS und Energieverbrauch nicht aufgrund lokaler Browser-Messungen auf andere Geräte extrapolieren.

## 8. Rollen, Grenzen und offene Entscheidungen

**Entwicklung:** Architektur, Implementierung, CI, Dokumentation, Optimierung und reproduzierbare Tests.

**Projektinhaber:** Kreative Freigaben, Gameplay-Feedback, Tests auf dem S20 FE und externe Freigaben/Konten. Für iOS-App-Store-Builds ist üblicherweise macOS/Xcode nötig.

**Noch zu verifizieren:** tatsächliche S20-FE-Variante, Three.js/Capacitor-Performance, Mindesthardware anderer Endgeräte, endgültige 3D-Asset-Pipeline, geeignete Lizenz-/Contentquellen, Monetarisierung und Content-Rating.

## 9. Nächste Aufgabe (direkt umsetzbar)

**P0 + P1 beginnen, ohne bestehenden Spielcode zu überschreiben:** Repository und Funktionen inventarisieren, mobile 3D-Testszene isoliert entwickeln, auf Android messen. Erst nach bestandenem Benchmark und funktionierender Kampf-Demo wird die bestehende 2D-Darstellung schrittweise abgelöst.
