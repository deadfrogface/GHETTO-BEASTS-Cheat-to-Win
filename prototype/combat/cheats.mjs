// Data-only cheat registry and bounded, deterministic effect interpreter.
import {deriveStats} from './mutations.mjs';
import {getOpponent} from './opponents.mjs';
export const START_CASH=8;
export const CHEAT_DEFS=[
 {id:'brick',name:'Backstein',method:'projectile',target:'enemy',effects:[{type:'damage',amount:27}],chance:.58,fail:[{type:'damage',target:'self',amount:8}],tags:['illegal','ranged'],cost:0,heat:22,cooldown:1},
 {id:'bribe',name:'Bestechung',method:'corruption',target:'enemy',effects:[{type:'weaken',amount:5}],chance:.78,fail:[{type:'damage',target:'self',amount:5}],tags:['illegal','social'],cost:1,heat:14,cooldown:1},
 {id:'distract',name:'Ablenkung',method:'distraction',target:'enemy',effects:[{type:'skip',amount:1}],chance:.42,fail:[],tags:['social','trick'],cost:0,heat:8,cooldown:2},
 {id:'sabotage',name:'Sabotage',method:'equipment',target:'enemy',effects:[{type:'damage',amount:17},{type:'weaken',amount:2}],chance:.63,fail:[{type:'damage',target:'self',amount:4}],tags:['illegal','mechanical'],cost:1,heat:20,cooldown:1},
 {id:'medkit',name:'Manipuliertes Medkit',method:'medical',target:'self',effects:[{type:'heal',amount:20}],chance:.82,fail:[{type:'damage',target:'self',amount:3}],tags:['medical','trick'],cost:2,heat:10,cooldown:2}
];
const TYPES=new Set(['damage','heal','weaken','skip']),TARGETS=new Set(['enemy','self']);
const integer=(v,min,max)=>Number.isInteger(v)&&v>=min&&v<=max;
export function validateDef(d){
 return Boolean(d&&typeof d.id==='string'&&/^[a-z][a-z0-9_+-]{0,70}$/.test(d.id)&&typeof d.name==='string'&&d.name.length<=80&&typeof d.method==='string'&&d.method.length<=30&&TARGETS.has(d.target)&&Number.isFinite(d.chance)&&d.chance>=0&&d.chance<=1&&integer(d.cost,0,20)&&integer(d.heat,0,100)&&integer(d.cooldown,0,10)&&Array.isArray(d.tags)&&d.tags.length<=12&&d.tags.every(t=>typeof t==='string'&&/^[a-z-]{1,25}$/.test(t))&&[d.effects,d.fail].every(arr=>Array.isArray(arr)&&arr.length<=10&&arr.every(e=>e&&TYPES.has(e.type)&&integer(e.amount,1,50)&&(e.target===undefined||TARGETS.has(e.target)))));
}
export function validateCatalog(defs){return Array.isArray(defs)&&defs.every(validateDef)&&new Set(defs.map(d=>d.id)).size===defs.length;}
export function compatible(a,b){
 if(a.id===b.id||a.method===b.method||a.target!==b.target)return false;
 if(a.tags.includes('exclusive')||b.tags.includes('exclusive'))return false;
 if(a.tags.includes('medical')||b.tags.includes('medical'))return false;
 return a.effects.length+b.effects.length<=10&&a.fail.length+b.fail.length<=10;
}
export function combinations(defs=CHEAT_DEFS){
 if(!validateCatalog(defs))throw Error('Invalid cheat catalog');
 const result=[];
 for(let i=0;i<defs.length;i++)for(let j=i+1;j<defs.length;j++){
  const a=defs[i],b=defs[j];if(!compatible(a,b))continue;
  result.push({id:a.id+'+'+b.id,name:a.name+' + '+b.name,method:'combo',target:a.target,components:[a.id,b.id],chance:Math.min(.9,Math.max(.15,a.chance*b.chance+.12)),cost:a.cost+b.cost,heat:Math.min(100,a.heat+b.heat),cooldown:Math.max(a.cooldown,b.cooldown),tags:[...new Set([...a.tags,...b.tags])],effects:[...a.effects,...b.effects],fail:[...a.fail,...b.fail]});
 }
 return result;
}
export const ALL_CHEATS=[...CHEAT_DEFS,...combinations()];
if(!validateCatalog(ALL_CHEATS))throw Error('Invalid generated catalog');
export const findCheat=id=>ALL_CHEATS.find(d=>d.id===id);
export function canUseCheat(s,d){
 if(!validateDef(d)||s.phase!=='player')return {ok:false,reason:'Nicht verfügbar'};
 if(!integer(s.cash,0,1000)||s.cash<d.cost)return {ok:false,reason:'Zu wenig Bargeld'};
 if((s.cooldowns?.[d.id]||0)>s.round)return {ok:false,reason:'Abklingzeit'};
 if(s.suspicion>=100)return {ok:false,reason:'Enttarnt'};
 return {ok:true,reason:''};
}
export function applyCheat(s,d,random){
 if(!validateDef(d)||!Number.isFinite(random)||random<0||random>=1)throw Error('Invalid cheat input');
 const eligibility=canUseCheat(s,d);if(!eligibility.ok)return s;
 const success=random<d.chance;
 const t={...s,history:[...s.history],cooldowns:{...s.cooldowns},phase:'enemy',playerGuard:false,cash:s.cash-d.cost};
 t.cooldowns[d.id]=s.round+d.cooldown+1;
 const clamp=(n,max)=>Math.max(0,Math.min(max,n));
 let weaken=0,skip=false;
 for(const e of (success?d.effects:d.fail)){
  const target=e.target||d.target,key=target==='self'?'playerHp':'enemyHp',max=target==='self'?deriveStats(t.loadout).maxHp:getOpponent(t.opponentId).maxHp;
  if(e.type==='damage')t[key]=clamp(t[key]-e.amount,max);
  if(e.type==='heal')t[key]=clamp(t[key]+e.amount,max);
  if(e.type==='weaken'&&target==='enemy')weaken+=e.amount;
  if(e.type==='skip'&&target==='enemy')skip=true;
 }
 t.enemyWeaken=Math.min(20,(s.enemyWeaken||0)+weaken);
 t.skipEnemy=Boolean(s.skipEnemy||skip);
 t.suspicion=clamp(s.suspicion+d.heat+(success?0:12),100);
 t.history.push(d.name+(success?' gelungen':' gescheitert')+' · -'+d.cost+' $ · Verdacht '+t.suspicion+'/100');
 if(t.enemyHp===0){t.phase='over';t.history.push('Sieg! '+getOpponent(t.opponentId).name+' besiegt.');}
 else if(t.playerHp===0){t.phase='over';t.history.push('Niederlage!');}
 else if(t.suspicion===100){t.phase='over';t.history.push('Enttarnt! Kampf durch Razzia verloren.');}
 t.history=t.history.slice(-10);
 return t;
}
