// P5: offline district, quests, economy and persistent progression.
import {BASE_LOADOUT,PARTS,mutate} from './mutations.mjs';
export const WORLD_KEY='ghetto-beasts-p5-world-v1';
export const DISTRICT={id:'kanalviertel',name:'Kanalviertel',npcs:[
 {id:'dealer',name:'Schrauben-Sigi',role:'shop',x:-3,z:-2},
 {id:'scientist',name:'Dr. Murks',role:'lab',x:3,z:-2},
 {id:'challenger',name:'Kalle Krawall',role:'arena',x:0,z:3},
 {id:'informant',name:'Petze-Petra',role:'quest',x:-4,z:3}
]};
export const QUESTS=[
 {id:'first-win',name:'Erster Arenasieg',goal:'wins',target:1,reward:15},
 {id:'three-wins',name:'Dreifacher Ärger',goal:'wins',target:3,reward:30},
 {id:'mutate',name:'Wissenschaft? Na ja.',goal:'mutations',target:1,reward:10},
 {id:'five-wins',name:'Kanal-Legende',goal:'wins',target:5,reward:45},
 {id:'ten-wins',name:'Stadtmeister',goal:'wins',target:10,reward:80},
 {id:'three-mutations',name:'Mutanten-Mania',goal:'mutations',target:3,reward:25}
];
export const SHOP=[{id:'medkit',name:'Verbandszeug',price:6},{id:'scrap',name:'Schrottteile',price:4}];
export const DISTRICT_UNLOCKS=[{id:'kanalviertel',name:'Kanalviertel',wins:0},{id:'schrottplatz',name:'Schrottplatz',wins:3},{id:'rathaus',name:'Rathausplatz',wins:7}];
export function availableDistricts(wins){return DISTRICT_UNLOCKS.filter(d=>wins>=d.wins);}
export function newWorld(){return {version:1,district:DISTRICT.id,coins:12,inventory:{medkit:0,scrap:0},unlocked:['base-head','base-arms','base-legs','base-skin','none-tail','none-horns','none-extra','none-wings'],loadout:{...BASE_LOADOUT},wins:0,mutations:0,completed:[],position:{x:0,z:0},events:[]};}
export function validWorld(s){return Boolean(s&&s.version===1&&s.district===DISTRICT.id&&Number.isSafeInteger(s.coins)&&s.coins>=0&&s.coins<=1000000&&s.inventory&&['medkit','scrap'].every(k=>Number.isSafeInteger(s.inventory[k])&&s.inventory[k]>=0)&&Array.isArray(s.unlocked)&&Array.isArray(s.completed)&&Number.isSafeInteger(s.wins)&&s.wins>=0&&Number.isSafeInteger(s.mutations)&&s.mutations>=0&&s.position&&Number.isFinite(s.position.x)&&Number.isFinite(s.position.z));}
export function buy(s,id){const item=SHOP.find(i=>i.id===id);if(!validWorld(s)||!item||s.coins<item.price)return s;return {...s,coins:s.coins-item.price,inventory:{...s.inventory,[id]:s.inventory[id]+1},events:[...s.events,'Gekauft: '+item.name].slice(-10)};}
export function unlockPart(s,id){if(!validWorld(s)||!PARTS.some(p=>p.id===id)||s.unlocked.includes(id)||s.inventory.scrap<2)return s;return {...s,inventory:{...s.inventory,scrap:s.inventory.scrap-2},unlocked:[...s.unlocked,id],events:[...s.events,'Erforscht: '+id].slice(-10)};}
export function equip(s,slot,id){if(!validWorld(s)||!s.unlocked.includes(id))return s;const next=mutate(s.loadout,slot,id);return next===s.loadout?s:{...s,loadout:next,mutations:s.mutations+1};}
export function recordWin(s,reward=8){if(!validWorld(s)||!Number.isSafeInteger(reward)||reward<0||reward>100)return s;return {...s,wins:s.wins+1,coins:Math.min(1000000,s.coins+reward),events:[...s.events,'Arenasieg! +'+reward+' 
export function claimQuest(s,id){const q=QUESTS.find(q=>q.id===id);if(!validWorld(s)||!q||s.completed.includes(id)||s[q.goal]<q.target)return s;return {...s,coins:s.coins+q.reward,completed:[...s.completed,id],events:[...s.events,'Quest geschafft: '+q.name].slice(-10)};}
export function move(s,x,z){if(!validWorld(s)||!Number.isFinite(x)||!Number.isFinite(z))return s;return {...s,position:{x:Math.max(-9,Math.min(9,x)),z:Math.max(-9,Math.min(9,z))}};}
export function saveWorld(s,storage=localStorage){if(!validWorld(s))return false;try{storage.setItem(WORLD_KEY,JSON.stringify(s));return true;}catch{return false;}}
export function loadWorld(storage=localStorage){try{const s=JSON.parse(storage.getItem(WORLD_KEY));return validWorld(s)?{...s,loadout:{...BASE_LOADOUT,...s.loadout}}:newWorld();}catch{return newWorld();}}
].slice(-10)};}
export function claimQuest(s,id){const q=QUESTS.find(q=>q.id===id);if(!validWorld(s)||!q||s.completed.includes(id)||s[q.goal]<q.target)return s;return {...s,coins:s.coins+q.reward,completed:[...s.completed,id],events:[...s.events,'Quest geschafft: '+q.name].slice(-10)};}
export function move(s,x,z){if(!validWorld(s)||!Number.isFinite(x)||!Number.isFinite(z))return s;return {...s,position:{x:Math.max(-9,Math.min(9,x)),z:Math.max(-9,Math.min(9,z))}};}
export function saveWorld(s,storage=localStorage){if(!validWorld(s))return false;try{storage.setItem(WORLD_KEY,JSON.stringify(s));return true;}catch{return false;}}
export function loadWorld(storage=localStorage){try{const s=JSON.parse(storage.getItem(WORLD_KEY));return validWorld(s)?{...s,loadout:{...BASE_LOADOUT,...s.loadout}}:newWorld();}catch{return newWorld();}}
