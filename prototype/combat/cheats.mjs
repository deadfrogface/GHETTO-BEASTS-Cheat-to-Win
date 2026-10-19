// P3: pure, data-driven cheat effects; no browser dependency.
export const CHEAT_DEFS=Object.freeze([
 {id:'brick',name:'Backstein',method:'projectile',target:'enemy',effects:[{type:'damage',amount:27}],chance:0.58,fail:[{type:'damage',target:'self',amount:8}],tags:['illegal','trainer','ranged'],cost:0},
 {id:'bribe',name:'Bestechung',method:'corruption',target:'enemy',effects:[{type:'weaken',amount:5}],chance:0.78,fail:[{type:'damage',target:'self',amount:5}],tags:['illegal','social'],cost:1},
 {id:'distract',name:'Ablenkung',method:'distraction',target:'enemy',effects:[{type:'skip',amount:1}],chance:0.42,fail:[],tags:['social','trick'],cost:0},
 {id:'sabotage',name:'Sabotage',method:'equipment',target:'enemy',effects:[{type:'damage',amount:17},{type:'weaken',amount:2}],chance:0.63,fail:[{type:'damage',target:'self',amount:4}],tags:['illegal','mechanical'],cost:1},
]);
const EFFECTS=new Set(['damage','heal','weaken','skip']);
export function validateDef(d){return Boolean(d&&typeof d.id==='string'&&/^[a-z][a-z0-9_-]{0,40}$/.test(d.id)&&typeof d.name==='string'&&d.name.length<=60&&['enemy','self'].includes(d.target)&&Number.isFinite(d.chance)&&d.chance>=0&&d.chance<=1&&Number.isInteger(d.cost)&&d.cost>=0&&d.cost<=10&&Array.isArray(d.tags)&&d.tags.length<=12&&d.tags.every(t=>typeof t==='string'&&/^[a-z-]{1,25}$/.test(t))&&[d.effects,d.fail].every(arr=>Array.isArray(arr)&&arr.length<=4&&arr.every(e=>e&&EFFECTS.has(e.type)&&Number.isInteger(e.amount)&&e.amount>=1&&e.amount<=50&&(e.target===undefined||['enemy','self'].includes(e.target)))));}
export function validateCatalog(defs){return Array.isArray(defs)&&defs.every(validateDef)&&new Set(defs.map(d=>d.id)).size===defs.length;}
export const findCheat=id=>CHEAT_DEFS.find(d=>d.id===id);
export function combinations(defs=CHEAT_DEFS){if(!validateCatalog(defs))throw Error('Invalid catalog');const out=[];for(let i=0;i<defs.length;i++)for(let j=i+1;j<defs.length;j++){const a=defs[i],b=defs[j];if(a.method===b.method||a.tags.includes('exclusive')||b.tags.includes('exclusive'))continue;out.push({id:a.id+'+'+b.id,name:a.name+' + '+b.name,components:[a.id,b.id],chance:Math.min(0.95,Math.max(0.1,a.chance*b.chance+0.12)),cost:a.cost+b.cost,effects:[...a.effects,...b.effects],fail:[...a.fail,...b.fail],target:'enemy',tags:[...new Set([...a.tags,...b.tags])],method:'combo'});}return out;}
export const ALL_CHEATS=Object.freeze([...CHEAT_DEFS,...combinations()]);
export function applyCheat(state,definition,random){
 if(!validateDef(definition)||!Number.isFinite(random)||random<0||random>=1)throw Error('Invalid cheat or roll');
 if(state.phase!=='player')return state;
 const success=random<definition.chance;
 const effects=success?definition.effects:definition.fail;
 const t={...state,history:[...state.history],playerGuard:false,phase:'enemy'};
 const safe=(n,max)=>Math.min(max,Math.max(0,n));
 let skip=false,weaken=0;
 for(const effect of effects){const self=(effect.target||definition.target)==='self';const key=self?'playerHp':'enemyHp';const max=self?90:110;
 if(effect.type==='damage')t[key]=safe(t[key]-effect.amount,max);
 if(effect.type==='heal')t[key]=safe(t[key]+effect.amount,max);
 if(effect.type==='skip'&&!self)skip=true;
 if(effect.type==='weaken'&&!self)weaken=Math.min(20,weaken+effect.amount);
 }
 t.enemyWeaken=Math.min(20,(state.enemyWeaken||0)+weaken);
 t.skipEnemy=Boolean(skip);
 t.history.push(definition.name+(success?' gelungen!':' fehlgeschlagen!'));
 t.history=t.history.slice(-10);
 if(t.enemyHp===0){t.phase='over';t.history.push('Sieg! Der Beton-Golem ist besiegt.');}
 else if(t.playerHp===0){t.phase='over';t.history.push('Niederlage durch gescheiterten Betrug.');}
 return t;
}
