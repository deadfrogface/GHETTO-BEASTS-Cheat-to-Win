// P4: deterministic modular morphology and stat mutations.
export const SLOT_TYPES=['head','arms','legs','skin','tail','horns'];
export const PARTS=[
 {id:'base-head',slot:'head',name:'Standardkopf',hp:0,attack:0,defense:0,tags:[]},
 {id:'rat-head',slot:'head',name:'Rattenkopf',hp:0,attack:2,defense:0,tags:['beast']},
 {id:'stone-head',slot:'head',name:'Steinschädel',hp:8,attack:0,defense:2,tags:['stone']},
 {id:'base-arms',slot:'arms',name:'Normale Arme',hp:0,attack:0,defense:0,tags:[]},
 {id:'claws',slot:'arms',name:'Krallen',hp:0,attack:5,defense:0,tags:['beast']},
 {id:'steel-arms',slot:'arms',name:'Stahlarme',hp:0,attack:3,defense:3,tags:['mechanical']},
 {id:'base-legs',slot:'legs',name:'Normale Beine',hp:0,attack:0,defense:0,tags:[]},
 {id:'spring-legs',slot:'legs',name:'Sprungbeine',hp:0,attack:3,defense:0,tags:['mechanical']},
 {id:'base-skin',slot:'skin',name:'Normale Haut',hp:0,attack:0,defense:0,tags:[]},
 {id:'armor',slot:'skin',name:'Panzerhaut',hp:12,attack:0,defense:4,tags:['stone']},
 {id:'spikes',slot:'skin',name:'Stachelhaut',hp:0,attack:4,defense:1,tags:['beast']},
 {id:'none-tail',slot:'tail',name:'Kein Schwanz',hp:0,attack:0,defense:0,tags:[]},
 {id:'rat-tail',slot:'tail',name:'Rattenschwanz',hp:0,attack:2,defense:0,tags:['beast']},
 {id:'none-horns',slot:'horns',name:'Keine Hörner',hp:0,attack:0,defense:0,tags:[]},
 {id:'horns',slot:'horns',name:'Hörner',hp:0,attack:3,defense:0,tags:['beast']}
];
export const BASE_LOADOUT={head:'base-head',arms:'base-arms',legs:'base-legs',skin:'base-skin',tail:'none-tail',horns:'none-horns'};
export const findPart=id=>PARTS.find(p=>p.id===id);
export function validateLoadout(loadout){
 return Boolean(loadout&&typeof loadout==='object'&&SLOT_TYPES.every(slot=>{const p=findPart(loadout[slot]);return p?.slot===slot;})&&Object.keys(loadout).length===SLOT_TYPES.length);
}
export function mutate(loadout,slot,id){
 if(!validateLoadout(loadout)||!SLOT_TYPES.includes(slot)||findPart(id)?.slot!==slot)return loadout;
 const next={...loadout,[slot]:id};return validateLoadout(next)?next:loadout;
}
export function deriveStats(loadout){
 if(!validateLoadout(loadout))throw Error('Invalid loadout');
 const result={maxHp:90,attack:0,defense:0,tags:[]};
 for(const slot of SLOT_TYPES){const part=findPart(loadout[slot]);result.maxHp+=part.hp;result.attack+=part.attack;result.defense+=part.defense;result.tags.push(...part.tags);}
 result.tags=[...new Set(result.tags)];return result;
}
export function appearance(loadout){
 if(!validateLoadout(loadout))throw Error('Invalid loadout');
 return {head:loadout.head,arms:loadout.arms,legs:loadout.legs,skin:loadout.skin,tail:loadout.tail,horns:loadout.horns};
}
