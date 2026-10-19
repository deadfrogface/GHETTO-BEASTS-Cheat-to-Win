// P5 offline deterministic city economy and progression.
import {BASE_LOADOUT,PARTS,mutate,deriveStats} from './mutations.mjs';
export const WORLD_SAVE='ghetto-beasts-world-v1';
export const PLACES=[
 {id:'street',name:'Hinterhof',type:'fight',x:0,z:0},
 {id:'market',name:'Schwarzmarkt',type:'shop',x:5,z:0},
 {id:'lab',name:'Mutationslabor',type:'lab',x:5,z:5},
 {id:'station',name:'Polizeiwache',type:'police',x:0,z:5}
];
export const SHOP=[
 {id:'medkit',name:'Erste-Hilfe-Kit',cost:4,type:'consumable'},
 {id:'claws',name:'Krallen-Mutation',cost:8,type:'part'},
 {id:'steel-arms',name:'Stahlarme',cost:12,type:'part'},
 {id:'armor',name:'Panzerhaut',cost:14,type:'part'},
 {id:'bat-wings',name:'Fledermausflügel',cost:16,type:'part'},
 {id:'extra-arms',name:'Zusatzarme',cost:18,type:'part'}
];
export const QUESTS=[
 {id:'first-win',name:'Erster Sieg',target:1,reward:8},
 {id:'three-wins',name:'Straßenlegende',target:3,reward:16},
 {id:'five-wins',name:'Hinterhof-Champion',target:5,reward:24}
];
export function newWorld(){return {version:1,location:'street',cash:12,dirtyCash:0,wins:0,losses:0,wanted:0,inventory:{medkit:0},unlocked:[...Object.values(BASE_LOADOUT)],loadout:{...BASE_LOADOUT},claimed:[],events:[]};}
export function validWorld(w){return Boolean(w&&w.version===1&&PLACES.some(p=>p.id===w.location)&&Number.isSafeInteger(w.cash)&&w.cash>=0&&Number.isSafeInteger(w.wins)&&w.wins>=0&&Number.isSafeInteger(w.losses)&&w.losses>=0&&Number.isFinite(w.wanted)&&w.wanted>=0&&w.wanted<=100&&w.inventory&&Number.isSafeInteger(w.inventory.medkit)&&w.inventory.medkit>=0&&Array.isArray(w.unlocked)&&Array.isArray(w.claimed)&&w.events&&Array.isArray(w.events));}
const log=(w,message)=>({...w,events:[...w.events.slice(-11),message]});
export function travel(w,id){if(!validWorld(w)||!PLACES.some(p=>p.id===id))return w;return log({...w,location:id},'Du bist bei '+PLACES.find(p=>p.id===id).name+'.');}
export function purchase(w,id){const item=SHOP.find(x=>x.id===id);if(!validWorld(w)||w.location!=='market'||!item||w.cash<item.cost||(item.type==='part'&&w.unlocked.includes(id)))return w;const next={...w,cash:w.cash-item.cost,inventory:{...w.inventory},unlocked:[...w.unlocked]};if(item.type==='part')next.unlocked.push(id);else next.inventory.medkit++;return log(next,item.name+' gekauft.');}
export function equip(w,id){const part=PARTS.find(p=>p.id===id);if(!validWorld(w)||w.location!=='lab'||!part||!w.unlocked.includes(id))return w;const loadout=mutate(w.loadout,part.slot,id);if(loadout===w.loadout)return w;return log({...w,loadout},part.name+' ausgerüstet.');}
export function recordFight(w,won,suspicion=0){if(!validWorld(w))return w;let next={...w,wins:w.wins+(won?1:0),losses:w.losses+(won?0:1),cash:w.cash+(won?5:0),wanted:Math.min(100,w.wanted+Math.max(0,suspicion)*.25)};next=log(next,won?'Sieg! +5 $':'Niederlage.');for(const q of QUESTS){if(next.wins>=q.target&&!next.claimed.includes(q.id)){next={...next,cash:next.cash+q.reward,claimed:[...next.claimed,q.id]};next=log(next,'Quest '+q.name+': +'+q.reward+' $');}}return next;}
export function useMedkit(w){if(!validWorld(w)||w.inventory.medkit<=0)return w;return log({...w,inventory:{...w.inventory,medkit:w.inventory.medkit-1}},'Erste-Hilfe-Kit verwendet.');}
export function coolWanted(w){if(!validWorld(w)||w.location!=='station')return w;return log({...w,wanted:Math.max(0,w.wanted-30)},'Fahndung um 30 gesenkt.');}
export function loadWorld(raw){try{const w=typeof raw==='string'?JSON.parse(raw):raw;return validWorld(w)?w:newWorld();}catch{return newWorld();}}
export function getLoadoutStats(w){return deriveStats(w.loadout);}
