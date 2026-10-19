const fs = require('fs');
const path = require('path');

const structure = {
  "css/style.css": `
:root {
    --bg-color: #020204;
    --panel-bg: #07090f;
    --neon-green: #00ff66;
    --neon-red: #ff0055;
    --neon-yellow: #ffcc00;
    --neon-blue: #00ccff;
    --border-color: #141c29;
}
* { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Courier New', monospace; user-select: none; }
body { background: var(--bg-color); color: #fff; display: flex; justify-content: center; align-items: center; height: 100dvh; overflow: hidden; }
#burner-phone { width: 100%; max-width: 440px; height: 100dvh; background: var(--panel-bg); border: 2px solid var(--neon-green); display: flex; flex-direction: column; position: relative; padding-bottom: calc(24px + env(safe-area-inset-bottom)); }
#phone-header { background: #040609; border-bottom: 2px solid var(--border-color); padding: 6px 10px; display: flex; flex-direction: column; gap: 3px; }
.currencies-bar { display: flex; justify-content: space-between; background: #000; padding: 4px 8px; border: 1px solid var(--border-color); }
.curr-item { display: flex; flex-direction: column; align-items: center; font-size: 10px; }
.c-dirty { color: var(--neon-yellow); } .c-clean { color: var(--neon-green); } .c-diamonds { color: var(--neon-blue); }
#viewport { flex: 1; position: relative; overflow: hidden; display: flex; flex-direction: column; }
.tab-content { display: none; flex-direction: column; height: 100%; padding: 10px; overflow-y: auto; gap: 10px; }
.tab-content.active { display: flex; }
.cyber-btn { background: #0d121c; border: 1px solid var(--neon-green); color: var(--neon-green); padding: 10px; font-size: 11px; font-weight: bold; border-radius: 4px; text-align: center; cursor: pointer; text-transform: uppercase; }
.cyber-btn:active { background: var(--neon-green); color: #000; }
#tab-map { padding: 0; justify-content: space-between; height: 100%; }
#map-canvas-container { flex: 1; display: flex; justify-content: center; align-items: center; background: #000; }
canvas { image-rendering: pixelated; max-width: 100%; max-height: 100%; }
#controls-bar { background: #040609; border-top: 2px solid var(--border-color); padding: 8px 12px; display: flex; justify-content: space-between; align-items: center; }
.dpad-grid { display: grid; grid-template-columns: repeat(3, 38px); grid-template-rows: repeat(3, 38px); gap: 3px; }
.dpad-btn { background: #0d121c; border: 1px solid var(--neon-green); color: var(--neon-green); font-size: 14px; font-weight: bold; border-radius: 4px; display: flex; justify-content: center; align-items: center; }
.btn-action-a { width: 68px; height: 68px; border-radius: 50%; background: #0d121c; border: 2px solid var(--neon-green); color: var(--neon-green); font-size: 11px; font-weight: bold; display: flex; justify-content: center; align-items: center; }
#phone-nav { background: #040609; border-top: 2px solid var(--border-color); display: flex; justify-content: space-around; padding: 10px 0 6px 0; }
.nav-btn { background: transparent; border: none; color: #555; font-size: 9px; display: flex; flex-direction: column; align-items: center; cursor: pointer; width: 20%; }
.nav-btn.active { color: var(--neon-green); }
.modal-overlay { display: none; position: absolute; top:0; left:0; right:0; bottom:0; background: rgba(2,2,4,0.96); z-index: 100; flex-direction: column; justify-content: center; align-items: center; padding: 15px; }
.modal-overlay.active { display: flex; }
`,
  "js/data/animals.js": `
const ANIMALS_DB = [
  { id: "anim_01", name: "Räudige Kanalratte", rarity: "Common", hp: 45, maxHp: 45, atk: 12, def: 5, spd: 18, skill: "Giftspeichel", icon: "🐀" },
  { id: "anim_16", name: "Kupferstecher-Pitbull", rarity: "Uncommon", hp: 80, maxHp: 80, atk: 18, def: 12, spd: 14, skill: "Kiefersperre", icon: "🐕" },
  { id: "anim_41", name: "Alpha-Silberrücken-Gorilla", rarity: "Epic", hp: 220, maxHp: 220, atk: 45, def: 25, spd: 10, skill: "Brusttrommeln", icon: "🦍" }
];
`,
  "js/data/npcs.js": `
const NPCS_DB = [
  { id: "npc_01", name: "Hinterhof-Crackhead", rarity: "Common", hp: 35, maxHp: 35, atk: 15, def: 2, spd: 20, skill: "Gebrauchte Nadel", icon: "💉" },
  { id: "npc_16", name: "Club-Hostess 'Chantal'", rarity: "Uncommon", hp: 60, maxHp: 60, atk: 10, def: 8, spd: 16, skill: "Shirt hochziehen", icon: "💃" }
];
`,
  "js/data/buildings.js": `
const BUILDINGS_DB = {
    2: { name: "Dope Shop", code: "DOPE", desc: "Verbotene Spritzen & Adrenalin." },
    3: { name: "CRISPR-Labor", code: "CRISPR", desc: "Gen-Mutationen & Fleisch-Transplantate." },
    5: { name: "Waschsalon", code: "WASH", desc: "Geldwäsche: Schmutzgeld reinigen." },
    6: { name: "Arena", code: "ARENA", desc: "Illegale Kämpfe & Wetten." }
};
`,
  "js/core/state.js": `
const GameState = {
    dirtyCash: 350, cleanCash: 800, dirtyDiamonds: 150, roster: [], activeBeastIndex: 0, suspicion: 0,
    save() { localStorage.setItem('GHETTO_BEASTS_SAVE', JSON.stringify({ dirtyCash: this.dirtyCash, cleanCash: this.cleanCash, dirtyDiamonds: this.dirtyDiamonds, roster: this.roster })); },
    load() {
        const saved = localStorage.getItem('GHETTO_BEASTS_SAVE');
        if (saved) {
            const parsed = JSON.parse(saved);
            this.dirtyCash = parsed.dirtyCash; this.cleanCash = parsed.cleanCash; this.dirtyDiamonds = parsed.dirtyDiamonds; this.roster = parsed.roster;
        } else { this.roster = [{ ...ANIMALS_DB[1], instability: 0 }]; }
    }
};
`,
  "js/core/audio.js": `
const AudioEngine = {
    ctx: null, enabled: false,
    init() { if (!this.ctx) this.ctx = new (window.AudioContext || window.webkitAudioContext)(); if (this.ctx.state === 'suspended') this.ctx.resume(); this.enabled = true; },
    playTone(freq, duration) {
        if (!this.enabled) return;
        const osc = this.ctx.createOscillator(); const gain = this.ctx.createGain();
        osc.frequency.setValueAtTime(freq, this.ctx.currentTime); gain.gain.setValueAtTime(0.08, this.ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.001, this.ctx.currentTime + duration);
        osc.connect(gain); gain.connect(this.ctx.destination); osc.start(); osc.stop(this.ctx.currentTime + duration);
    }
};
`,
  "js/engine/graphics.js": `
const GraphicsEngine = {
    drawTile(ctx, tile, x, y, size) {
        ctx.fillStyle = '#090d14'; ctx.fillRect(x, y, size, size);
        if (tile === 1) { ctx.fillStyle = '#1a0d18'; ctx.fillRect(x, y, size, size); ctx.strokeStyle = '#ff0055'; ctx.strokeRect(x, y, size, size); }
        else if (BUILDINGS_DB[tile]) { ctx.fillStyle = '#091810'; ctx.fillRect(x+1, y+1, size-2, size-2); ctx.fillStyle = '#00ff66'; ctx.font = '8px monospace'; ctx.fillText(BUILDINGS_DB[tile].code, x+4, y+18); }
    },
    drawSprite(ctx, x, y, color) { ctx.fillStyle = color; ctx.fillRect(x + 8, y + 8, 16, 16); }
};
`,
  "js/engine/map.js": `
const MapEngine = {
    tileSize: 32, mapSize: 12, player: { x: 1, y: 2 },
    grid: [[1,1,1,1,1,1,1,1,1,1,1,1],[1,2,0,0,1,3,0,0,0,0,0,1],[1,0,0,0,1,0,0,0,0,0,0,1],[1,0,0,0,0,0,0,0,0,0,0,1],[1,0,0,0,0,0,0,0,0,0,0,1],[1,1,0,1,1,1,0,1,1,0,0,1],[1,5,0,0,0,1,0,0,0,0,0,1],[1,0,0,0,0,1,0,0,0,0,0,1],[1,0,0,0,0,0,0,0,0,0,0,1],[1,0,0,0,0,0,0,0,0,0,6,1],[1,0,0,0,0,0,0,0,0,0,0,1],[1,1,1,1,1,1,1,1,1,1,1,1]],
    draw() {
        const canvas = document.getElementById('mapCanvas'); if (!canvas) return;
        const ctx = canvas.getContext('2d'); ctx.clearRect(0, 0, canvas.width, canvas.height);
        for (let r = 0; r < this.mapSize; r++) for (let c = 0; c < this.mapSize; c++) GraphicsEngine.drawTile(ctx, this.grid[r][c], c*this.tileSize, r*this.tileSize, this.tileSize);
        GraphicsEngine.drawSprite(ctx, this.player.x * this.tileSize, this.player.y * this.tileSize, '#00ff66');
    },
    move(dx, dy) {
        const nx = this.player.x + dx; const ny = this.player.y + dy;
        if (this.grid[ny][nx] !== 1) { this.player.x = nx; this.player.y = ny; AudioEngine.playTone(150, 0.05); }
        this.draw();
    }
};
`,
  "js/mechanics/economy.js": `
const Economy = {
    updateHUD() {
        document.getElementById('val-dirty').innerText = '$' + GameState.dirtyCash;
        document.getElementById('val-clean').innerText = '$' + GameState.cleanCash;
        document.getElementById('val-diamonds').innerText = '💎 ' + GameState.dirtyDiamonds;
    }
};
`,
  "js/ui/hud.js": `
const UI = {
    switchTab(tabId, btn) {
        document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
        document.querySelectorAll('.nav-btn').forEach(el => el.classList.remove('active'));
        document.getElementById('tab-' + tabId).classList.add('active');
        if (btn) btn.classList.add('active');
        if (tabId === 'map') MapEngine.draw();
    }
};
`,
  "js/main.js": `
window.onload = () => {
    GameState.load();
    Economy.updateHUD();
    MapEngine.draw();
};
`,
  "index.html": `
<!DOCTYPE html>
<html lang="de">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>GHETTO BEASTS: Cheat to Win</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
<div id="burner-phone">
    <div id="phone-header">
        <div style="font-size:9px; color:#555;">BURNER-OS v4.02 // SECURE</div>
        <div class="currencies-bar">
            <div class="curr-item"><span>SCHMUTZGELD</span><span id="val-dirty" class="c-dirty">$0</span></div>
            <div class="curr-item"><span>SAUBERES CASH</span><span id="val-clean" class="c-clean">$0</span></div>
            <div class="curr-item"><span>DIAMANTEN</span><span id="val-diamonds" class="c-diamonds">💎 0</span></div>
        </div>
    </div>
    <div id="viewport">
        <div id="tab-map" class="tab-content active">
            <div id="map-canvas-container"><canvas id="mapCanvas" width="384" height="384"></canvas></div>
            <div id="controls-bar">
                <div class="dpad-grid">
                    <div></div><button class="dpad-btn" onclick="MapEngine.move(0,-1)">▲</button><div></div>
                    <button class="dpad-btn" onclick="MapEngine.move(-1,0)">◄</button><div></div><button class="dpad-btn" onclick="MapEngine.move(1,0)">►</button>
                    <div></div><button class="dpad-btn" onclick="MapEngine.move(0,1)">▼</button><div></div>
                </div>
                <button class="btn-action-a">[A]</button>
            </div>
        </div>
        <div id="tab-shop" class="tab-content"><h3>STORE</h3></div>
        <div id="tab-roster" class="tab-content"><h3>KÄFIG</h3></div>
        <div id="tab-hub" class="tab-content"><h3>HUBS</h3></div>
        <div id="tab-combat" class="tab-content"><h3>KAMPF</h3></div>
    </div>
    <div id="phone-nav">
        <button class="nav-btn active" onclick="UI.switchTab('map', this)">🗺️ MAP</button>
        <button class="nav-btn" onclick="UI.switchTab('shop', this)">💎 STORE</button>
        <button class="nav-btn" onclick="UI.switchTab('roster', this)">🦧 KÄFIG</button>
        <button class="nav-btn" onclick="UI.switchTab('hub', this)">🏢 HUBS</button>
        <button class="nav-btn" onclick="UI.switchTab('combat', this)">🥊 KAMPF</button>
    </div>
</div>
<script src="js/data/animals.js"></script>
<script src="js/data/npcs.js"></script>
<script src="js/data/buildings.js"></script>
<script src="js/core/state.js"></script>
<script src="js/core/audio.js"></script>
<script src="js/engine/graphics.js"></script>
<script src="js/engine/map.js"></script>
<script src="js/mechanics/economy.js"></script>
<script src="js/ui/hud.js"></script>
<script src="js/main.js"></script>
</body>
</html>
`
};

Object.keys(structure).forEach(filePath => {
    const dir = path.dirname(filePath);
    if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
    fs.writeFileSync(filePath, structure[filePath].trim());
    console.log("Erstellt: " + filePath);
});
console.log("\n Fertig! Alle Ordner und Dateien wurden erstellt.");
