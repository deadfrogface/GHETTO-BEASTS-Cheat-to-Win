# GHETTO-BEASTS-Cheat-to-Win

Dieses Dokument dient als das finale Game Design Document (GDD) & Entwicklungs-Architektur-Spezifikation, die alle Systeme, Datenstrukturen, Grafik-/Audio-Konzepte und den mobilen Tech-Stack nahtlos zusammenfasst.
🦧 GHETTO BEASTS: DNA & CORRUPTION
Master Game Design Document (GDD) & App Architecture
🛠️ 1. ARCHITEKTUR & MOBILE TECH-STACK
Das Spiel wird als native Mobile App für Android (.apk) und iOS (.ipa) auf Basis eines hybriden Web-Frameworks (Cordova / Capacitor) umgesetzt.
 * Engine / Core: HTML5 Canvas (2D GPU-Accelerated), CSS3 Animation Systems & Pure JavaScript (ES6+).
 * Audio-Engine: Native Web Audio API (Vollständig synthetisierte 8-Bit Cyberpunk-BGM, Soundeffekte & Rausch-Generatoren ohne MP3-Files).
 * Grafik & Shader: Prozedurales Grid-Rendering, dynamisches Sprite-Assembly für CRISPR-Mutationen, CRT-Scanline Overlays, CSS Canvas-Shakes & Glitch-Matrizen.
 * In-App Purchasing (IAP): Open-Source Native Bridge (cordova-plugin-inapppurchase) zur Anbindung von Google Play & App Store Abrechnungssystemen.
 * App-Größe & Performance: Unter 10 MB App-Package, 60 FPS Lock, responsive UI-Skalierung für alle Smartphone-Auflösungen.
🗺️ 2. DIE 6 CORE-ENTWICKLUNGS-MODULE
                   ┌────────────────────────────────────────┐
                   │    GHETTO BEASTS: SYSTEM ENGINE       │
                   └───────────────────┬────────────────────┘
                                       │
  ┌──────────────────┬─────────────────┼──────────────────┬──────────────────┐
  ▼                  ▼                 ▼                  ▼                  ▼
Modul 1:           Modul 2:          Modul 3:           Modul 4:           Modul 5 & 6:
• Burner Phone UI  • 2D Ghetto Map   • CRISPR-Labor     • Combat Engine    • Justiz & Knast
• Gacha (Booster)  • Touch D-Pad     • Dope-Shop        • Trainer-Sabotage • Geldwäsche
• Währungen        • Sight Triggers  • Mutationen       • Suspicion Meter  • Audio Synthesizer

Modul 1: Burner Phone, Pay-To-Win Wirtschaft & Gacha
 * Darknet Interface: Cyberpunk-Smartphone-Look (Matrix-Grün #00ff66, SWAT-Rot #ff0055, Neon-Gelb #ffcc00).
 * Dreistufiges Währungssystem:
   * Dirty Cash ($): Auf der Straße & in der Arena verdient. Geht bei einer Polizeirazzia verloren.
   * Clean Cash ($): In Immobilien gewaschenes Geld. Absolut sicher vor Konfiszierung.
   * Dirty Diamonds (Premium): Per In-App-Kauf erworben. Gesichert auf Schweizer Krypto-Konten. Überspringt Timer, schaltet VIP-Anwälte frei & kauft DNA-Booster.
 * DNA- & NPC-Booster-Packs (Gacha):
   * Container Pack (50 Diamonds): 70% Common, 20% Uncommon, 7% Rare, 2.5% Epic, 0.5% Legendary.
Modul 2: Die Oberwelt (Das Kachel-Ghetto)
 * Kachel-Grid (Grid-Map): Slum-Stadt bei Nacht mit Pfützen, Sperrzonen, Neon-Gassen und begehbaren Gebäuden.
 * Steuerung: Fixes, transparentes Touch-D-Pad (Virtueller Joystick) unten links + [A] AKTION-Button.
 * Sichtlinien & Heat-Map: Feindliche Gangs, Dealer und Cops patrouillieren. Kreuzt man ihre Sichtlinie oder steigt die Viertel-Heat, wird ein Screen-Glitch getriggert und der Kampf startet.
Modul 3: CRISPR-Labor, Dope-Shop & Immobilien
 * Dope-Shop (Apotheke des Grauens): Verfeuerte Kampfspritzen (Adrenalin, Rage-Gas, Nerven-Blocker). Pushen Stats drastisch, erzeugen aber Nebenwirkungen oder In-Fight-Verwirrung.
 * CRISPR-Labor (Mutationen & Instabilität): Anbringen von Kopf-, Arm-, Panzer- und Schwanz-Genen. Erhöht permanente Stats, füllt aber die Gen-Instabilität. Bei 100% stirbt das Biest mitten im Kampf an Zellzerfall (Permadeath).
 * Immobilien & Geldwäsche:
   * Waschsalon ($500 Clean): Wäscht $50 Dirty Cash/Min.
   * Nagelstudio ($1,200 Clean): Wäscht $150 Dirty Cash/Min.
   * Underground-Club ($5,000 Clean): Wäscht $500 Dirty Cash/Min + Passiv-Einnahmen.
   * Altmetall-Pfandhaus: Recycelt tote Biester, Waffen und Organe.
Modul 4: Das schmutzige Kampfsystem (Cheat to Win)
 * Rundenbasierte Arena: Biest/NPC vs. Gegner-Biest/NPC.
 * Betrugs- & Trainer-Sabotage:
   * Backstein werfen: 35% Chance auf Trainer-K.O. (Sofort-Sieg) / 65% Fehlschlag \rightarrow 100% Suspicion.
   * Hostess-Distraction ("Shirt hochziehen"): 50% Chance, dass der gegnerische Trainer vor Betörung seinen Zug überspringt.
   * Tier kidnappen: Unter 25% Gegner-HP mit Elektroleine stehlen.
   * Schiedsrichter bestechen: Senkt Verdacht um 50%.
 * Suspicion Meter (Fahndungsbalken): Bei 100% stürmt das SWAT-Team den Ring.
Modul 5: Justiz, Knast-Minispiel & Erpresser-Netzwerk
 * Knast-Mechanik: Bei Verhaftung geht alles Dirty Cash verloren.
 * Ausweg-Optionen:
   * Korrupten Wärter mit 50 Dirty Diamonds bestechen.
   * Kartell-Anwalt 'Better Call Ben' für $1,000 Clean Cash rufen.
   * Tunnelgrab-Minispiel: 2D-Gitter-Minispiel, bei dem man durch den Dreck gräbt und Suchscheinwerfern ausweicht.
 * Burner-SMS-Netzwerk: Dynamischer Feed für Wett-Tipps, Erpressung und Drop-Off-Koordinaten.
Modul 6: Web Audio API Synthesizer & Canvas-Effekte
 * Prozeduraler Sound Engine:
   * Sawtooth-Sub-Basslines (Techno BGM Loop).
   * White-Noise-Rauschen für Treffer-Sounds.
   * Frequenz-Glissandos für Spritzen und Gen-Mutationen.
 * VFX: Screen-Shake bei K.O.-Treffern, CRT-Scanlines & Horizontal-Pixel-Shift (Glitch-Effekte bei SWAT-Invasion).
📊 3. CONTENT-DATENBANKEN (ÜBERSICHT)
A. 50 Mutierte Tiere (Deformed Beasts)
 * Common (15): Kanalratte, Müllkatze, Parktaube, Fledermag, Mops, Kanal-Kröte, Waschbär, Abwasser-Aal, Schrottpark-Dackel, Großstadt-Krabbe, Moorhuhn, Friedhofs-Marder, Fettsack-Karpfen, Tunnel-Maulwurf, Kakerlaken-Schwarm.
 * Uncommon (15): Pitbull, Rottweiler, Dobermann, Schimpanse, Giftschlange, Ghetto-Wildschwein, Kneipen-Papagei, Kanal-Alligator, Kampf-Hornisse, Luchs, Truthahn, Schäferhund, Skorpion, Frettchen, Straußenküken.
 * Rare (10): Hyänen-Mischling, Grizzly-Junge, Jagdleopard, Schnappschildkröte, Königskobra, Drogenboss-Pavian, Kamm-Krokodil, Tschernobyl-Eule, Labor-Wolverine, Ghetto-Strauß.
 * Epic (7): Alpha-Silberrücken-Gorilla, Drogenkartell-Tiger, Schwarzmarkt-Flusspferd, Klapperschlange 'Viper', Nashorn-Baby, Zitteraal-König, Tiefsee-Riesenkracke.
 * Legendary (3): CRISPR-Abomination 'Projekt Chimäre', Kybernetischer Kampfhund 'CYBER-PIT', Mephisto-Kobra.
B. 50 Ghetto-NPCs & Söldner
 * Söldner & Drogen-Gestalten (15): Crackhead, Schwarzfahrer, Hütchenspieler, Parkbank-Säufer, Graffitisprayer, Fluchtwagenfahrer, Schmuggler-Schüler, Schrottsammler, Taschendieb, Imbiss-Grillmeister etc.
 * Profis & Betörer (15): Club-Hostess 'Chantal' (Skill: Shirt hochziehen), Türsteher 'Ede', Sanitäter, Tätowierer, Ghetto-Rapper, Waffenschmuggler, MMA-Kämpfer, Hacker 'Zero-Cool' etc.
 * VIPs & Spezialisten (10): VIP-Stripperin 'Roxy', Gerichtsmediziner, Zivil-Cop, Auftragskiller 'Cleaner', Kartell-Anwalt 'Better Call Ben', Molotow-Chaot, Profi-Geldwäscher etc.
 * Boss-Legenden (10): Drogen-König 'El Capo', Voodoo-Priesterin, Dr. CRISPR, SWAT-Rebell, Yakuza-Jin, Der Cyber-Pate 'Don Corleone 2.0', Schatten-Stripperin 'Lilith' etc.
C. 100 Unique CRISPR-Mutationen
 * 20 Kopf- & Kiefer-Gene: Gorilla-Kiefer, Kobra-Giftzähne, Haifisch-Gebiss, Nashorn-Horn, Chamäleon-Augen, Zyklopen-Auge, Drachen-Schlund, Doppel-Kopf etc.
 * 30 Gliedmaßen-Gene: Gorilla-Arme, Känguru-Sprungbeine, Grizzly-Pranken, Gottesanbeterin-Klingen, Gepard-Sehnen, Oktopus-Tentakeln, Wolverin-Stahlkrallen etc.
 * 25 Haut- & Panzer-Gene: Chitin-Panzer, Skorpion-Schuppenhaut, Pfeilgiftfrosch-Haut, Nashorn-Platte, Tschernobyl-Leuchthaut, Graphene-Bio-Gewebe etc.
 * 25 Schwanz-, Flügel- & Extra-Organe: Skorpion-Giftschwanz, Stegosaurus-Keule, Adrenalin-Drüse, Elektro-Herz, Adler-Flügel, Klon-Herz, Synthetisches Schwarzes Loch etc.
D. Schwarzmarkt-Inventar
 * Dope: Adrenalin-Spritze, Nerven-Blocker, Rage-Gas, Pferde-Betäubungsmittel, Gen-Stabilisator.
 * Trainer-Waffen: Rot-Backstein, Taser, Militär-Tränengas, Schlagring, Elektro-Hundeleine, Signal-Jammer.
🚀 4. WEITERES VORGEHEN & UMSETZUNG
Der Masterplan ist nun vollständig aufgestellt. Sobald du bereit bist, die Entwicklung in deiner Umgebung umzusetzen, können wir das Projekt Modul für Modul durchgehen:
 * Start mit Modul 1: Erstellen des grundlegenden App-Handhabungs-Codes (Burner-Phone-UI, Währungslogik & Booster-Gacha-System).
 * Schrittweise Erweiterung: Nach erfolgreichem Setup von Modul 1 folgen nacheinander die Oberwelt (Modul 2), das CRISPR-Labor & die Shops (Modul 3), die Combat Engine (Modul 4), das Knast-System (Modul 5) und die Audio/VFX-Integration (Modul 6).
