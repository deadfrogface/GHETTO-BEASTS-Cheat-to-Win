// Offline progression, three districts and persistent economy.
import {BASE_LOADOUT,PARTS,mutate,validateLoadout} from './mutations.mjs';
export const WORLD_KEY='ghetto-beasts-p5-world-v1';
export const DISTRICT={id:'kanalviertel',name:'Kanalviertel',npcs:[
 {id:'dealer',name:'Schrauben-Sigi',role:'shop',x:-3,z:-2},
 {id:'scientist',name:'Dr. Murks',role:'lab',x:3,z:-2},
 {id:'challenger',name:'Kalle Krawall',role:'arena',x:0,z:3},
 {id:'informant',name:'Petze-Petra',role:'quest',x:-4,z:3}
]};
export const DISTRICT_NPCS={
 kanalviertel:DISTRICT.npcs,
 schrottplatz:[{id:'junkdealer',name:'Rost-Rita',role:'shop',x:-3,z:-2},{id:'welder',name:'Professor Funkenflug',role:'lab',x:3,z:-2},{id:'pit',name:'Schrott-Sabine',role:'arena',x:0,z:3},{id:'collector',name:'Pfand-Paule',role:'quest',x:-4,z:3}],
 rathaus:[{id:'accountant',name:'Kassen-Karla',role:'shop',x:-3,z:-2},{id:'researcher',name:'Dr. Formular',role:'lab',x:3,z:-2},{id:'mayor',name:'Bürgermeister Beton',role:'arena',x:0,z:3},{id:'clerk',name:'Akten-Achim',role:'quest',x:-4,z:3}]
};
export const districtNpcs=id=>DISTRICT_NPCS[id]||DISTRICT.npcs;
export const QUESTS=[
 {id:'first-win',name:'Erster Arenasieg',goal:'wins',target:1,reward:15},
 {id:'three-wins',name:'Dreifacher Ärger',goal:'wins',target:3,reward:30},
 {id:'mutate',name:'Wissenschaft? Na ja.',goal:'mutations',target:1,reward:10},
 {id:'five-wins',name:'Kanal-Legende',goal:'wins',target:5,reward:45},
 {id:'ten-wins',name:'Stadtmeister',goal:'wins',target:10,reward:80},
 {id:'three-mutations',name:'Mutanten-Mania',goal:'mutations',target:3,reward:25},
 {id:'seven-wins',name:'Rathaus-Schreck',goal:'wins',target:7,reward:55},
 {id:'six-mutations',name:'Dr. Murks ist stolz',goal:'mutations',target:6,reward:40}
];
export const SHOP=[{id:'medkit',name:'Verbandszeug',price:6},{id:'scrap',name:'Schrottteile',price:4}];
export const SHOP_MODIFIERS={kanalviertel:{medkit:0,scrap:0},schrottplatz:{medkit:2,scrap:-1},rathaus:{medkit:-1,scrap:2}};
export function itemPrice(district,id){const item=SHOP.find(i=>i.id===id);return item?Math.max(1,item.price+(SHOP_MODIFIERS[district]?.[id]??0)):null;}
export const DISTRICT_UNLOCKS=[{id:'kanalviertel',name:'Kanalviertel',wins:0},{id:'schrottplatz',name:'Schrottplatz',wins:3},{id:'rathaus',name:'Rathausplatz',wins:7}];
export const availableDistricts=wins=>DISTRICT_UNLOCKS.filter(d=>wins>=d.wins);
export function newWorld(){return {version:1,district:DISTRICT.id,coins:12,inventory:{medkit:0,scrap:0},unlocked:Object.values(BASE_LOADOUT),loadout:{...BASE_LOADOUT},wins:0,mutations:0,completed:[],position:{x:0,z:0},events:[]};}
export function validWorld(s){return Boolean(s&&s.version===1&&Number.isSafeInteger(s.wins)&&s.wins>=0&&DISTRICT_UNLOCKS.some(d=>d.id===s.district&&s.wins>=d.wins)&&Number.isSafeInteger(s.coins)&&s.coins>=0&&s.coins<=1000000&&s.inventory&&['medkit','scrap'].every(k=>Number.isSafeInteger(s.inventory[k])&&s.inventory[k]>=0&&s.inventory[k]<=1000000)&&Array.isArray(s.unlocked)&&s.unlocked.every(id=>PARTS.some(p=>p.id===id))&&Array.isArray(s.completed)&&s.completed.every(id=>QUESTS.some(q=>q.id===id))&&validateLoadout(s.loadout)&&Object.values(s.loadout).every(id=>s.unlocked.includes(id))&&Number.isSafeInteger(s.mutations)&&s.mutations>=0&&s.position&&Number.isFinite(s.position.x)&&Number.isFinite(s.position.z)&&Math.abs(s.position.x)<=9&&Math.abs(s.position.z)<=9&&Array.isArray(s.events));}
export function buy(s,id){const item=SHOP.find(i=>i.id===id);if(!validWorld(s)||!item)return s;const price=itemPrice(s.district,id);if(s.coins<price||s.inventory[id]>=1000000)return s;return {...s,coins:s.coins-price,inventory:{...s.inventory,[id]:s.inventory[id]+1},events:[...s.events,'Gekauft: '+item.name].slice(-10)};}
export function unlockPart(s,id){if(!validWorld(s)||!PARTS.some(p=>p.id===id)||s.unlocked.includes(id)||s.inventory.scrap<2)return s;return {...s,inventory:{...s.inventory,scrap:s.inventory.scrap-2},unlocked:[...s.unlocked,id],events:[...s.events,'Erforscht: '+id].slice(-10)};}
export function equip(s,slot,id){if(!validWorld(s)||!s.unlocked.includes(id))return s;const next=mutate(s.loadout,slot,id);return next===s.loadout||next[slot]===s.loadout[slot]?s:{...s,loadout:next,mutations:s.mutations+1};}
export function recordWin(s,reward=8){if(!validWorld(s)||!Number.isSafeInteger(reward)||reward<0||reward>100)return s;return {...s,wins:s.wins+1,coins:Math.min(1000000,s.coins+reward),events:[...s.events,'Arenasieg! +'+reward+' $'].slice(-10)};}
export function claimQuest(s,id){const q=QUESTS.find(q=>q.id===id);if(!validWorld(s)||!q||s.completed.includes(id)||s[q.goal]<q.target)return s;return {...s,coins:Math.min(1000000,s.coins+q.reward),completed:[...s.completed,id],events:[...s.events,'Quest geschafft: '+q.name].slice(-10)};}
export function travel(s,id){if(!validWorld(s)||!availableDistricts(s.wins).some(d=>d.id===id))return s;return {...s,district:id,position:{x:0,z:0},events:[...s.events,'Neuer Bezirk: '+id].slice(-10)};}
export function move(s,x,z){if(!validWorld(s)||!Number.isFinite(x)||!Number.isFinite(z))return s;return {...s,position:{x:Math.max(-9,Math.min(9,x)),z:Math.max(-9,Math.min(9,z))}};}
export function saveWorld(s,storage){if(!validWorld(s))return false;try{(storage??localStorage).setItem(WORLD_KEY,JSON.stringify(s));return true;}catch{return false;}}
export function loadWorld(storage){try{const s=JSON.parse((storage??localStorage).getItem(WORLD_KEY));return validWorld(s)?s:newWorld();}catch{return newWorld();}}
