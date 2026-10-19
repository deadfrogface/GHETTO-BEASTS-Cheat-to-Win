# P1 — Isolierter mobiler 3D-Performance-Spike

**Status:** Implementierter Quellcode, noch **kein** auf Galaxy S20 FE gemessener Benchmark und noch **keine** native Android-/iOS-App.

## Zweck

Diesen Prototyp parallel zum bestehenden 2D-Spiel betreiben, ohne `/index.html`, `build.js` oder Spielstände zu verändern. Er enthält:

- Stilisierten Low-Poly-3D-Kampfring, einfache Stadt-Silhouetten und animierte Cartoon-Kreaturen
- Zwei Kameraperspektiven, Touch-bedienbare Buttons
- Lastprofile 2/6/12/24 Kreaturen und einstellbare Render-Skalierung
- Laufende Anzeige von FPS, geschätztem 1%-Low, durchschnittlichen Frametimes und Draw Calls
- JSON-Messbericht per Kopieren, inklusive Gerätekennung und Render-Konfiguration

## Start

Auf einem lokalen HTTP-Server im Repository-Root (z. B. `python -m http.server 8000`) die URL `http://localhost:8000/prototype/3d/` öffnen. Für den Test auf dem Smartphone muss der Server im lokalen Netz erreichbar sein und eine passende URL verwendet werden; Clipboard kann HTTPS erfordern.

**Wichtig:** Der Spike lädt Three.js **derzeit über ein fest gepinntes CDN** (`three@0.180.0`). Deshalb ist dieser isolierte Entwicklungs-Spike **noch nicht offlinefähig**. Vor der App-Integration Three.js lokal installieren/bündeln, Versions- und Lizenzprüfung durchführen und offline testen. Die bestehende Anwendung wird dadurch nicht verändert.

## Testprotokoll (Galaxy S20 FE)

1. Prüfen, ob das Telefon 60 oder 120 Hz nutzt, Energiesparen deaktivieren und Akkustand/Temperatur notieren.
2. Browser schließen/neustarten, 6 Figuren und Render-Skalierung 1.0 einstellen.
3. Mindestens 2 Minuten laufen lassen, anschließend FPS und 1%-Low ablesen.
4. Lastprofile 12 und 24 testen; dabei Wärme und Ruckler beobachten.
5. Messbericht kopieren und zusammen mit Browser-Version und Telefon-Variante ins Issue/PR einfügen.
6. Mehrfach nach Aufwärmen testen, um thermisches Drosseln zu erkennen.

## Technische Einschränkungen

- Keine automatisierte Browser-Ausführung oder echte Gerätemessung durch diesen Commit.
- Die Messung verwendet `requestAnimationFrame` und ist durch Browser-/Display-Limits beeinflusst.
- Die 1%-Low-Schätzung ist aus den schlechtesten 1% der letzten maximal 1800 Frames berechnet; kein standardisierter Benchmark.
- Der Spike verwendet bewusst einfache Geometrien und Materialien; seine FPS sind **kein** Beweis für die spätere Spielleistung.
- Die Referenzforderung 69+ FPS auf dem S20 FE bleibt ein **noch zu prüfendes Ziel**.
- Für native Android/iOS-Auslieferung fehlen weiterhin Packaging, lokale Assets und Geräteintegration.
