// P4: deterministic modular morphology and stat mutations.
export const SLOT_TYPES=['head','arms','legs','skin','tail','horns','extraArms','wings'];
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
 {id:'horns',slot:'horns',name:'Hörner',hp:0,attack:3,defense:0,tags:['beast']},
 {id:'none-extra',slot:'extraArms',name:'Keine Extra-Arme',hp:0,attack:0,defense:0,tags:[]},
 {id:'extra-arms',slot:'extraArms',name:'Zusatzarme',hp:0,attack:4,defense:1,tags:['mutation']},
 {id:'none-wings',slot:'wings',name:'Keine Flügel',hp:0,attack:0,defense:0,tags:[]},
 {id:'bat-wings',slot:'wings',name:'Fledermausflügel',hp:0,attack:3,defense:0,tags:['flight']},
 {id:'steel-wings',slot:'wings',name:'Metallflügel',hp:5,attack:2,defense:2,tags:['flight','mechanical']}
];
export const BASE_LOADOUT={head:'base-head',arms:'base-arms',legs:'base-legs',skin:'base-skin',tail:'none-tail',horns:'none-horns',extraArms:'none-extra',wings:'none-wings'};
export const SPECIES={
 rat:{name:'Kanalratte',baseHp:90,baseAttack:0,baseDefense:0,bodyScale:1,colors:['#a4eb75','#4d7444'],allowedSlots:SLOT_TYPES},
 golem:{name:'Beton-Golem',baseHp:110,baseAttack:2,baseDefense:3,bodyScale:1.25,colors:['#dc9678','#73566a'],allowedSlots:SLOT_TYPES.filter(x=>x!=='wings')},
 human:{name:'Straßenkämpfer',baseHp:85,baseAttack:1,baseDefense:1,bodyScale:.92,colors:['#edb58a','#715a76'],allowedSlots:SLOT_TYPES}
};
export function normalizeLoadout(loadout){return {...BASE_LOADOUT,...(loadout&&typeof loadout==='object'?Object.fromEntries(Object.entries(loadout).filter(([key])=>SLOT_TYPES.includes(key))):{})};}
export const findPart=id=>PARTS.find(p=>p.id===id);
export function validateLoadout(loadout,species='rat'){
 const template=SPECIES[species];if(!template)return false;
 return Boolean(loadout&&typeof loadout==='object'&&SLOT_TYPES.every(slot=>{const p=findPart(loadout[slot]);return p?.slot===slot&& (template.allowedSlots.includes(slot)||loadout[slot]===BASE_LOADOUT[slot]);})&&Object.keys(loadout).length===SLOT_TYPES.length);
}
export function mutate(loadout,slot,id,species='rat'){
 if(!validateLoadout(loadout,species)||!SLOT_TYPES.includes(slot)||!SPECIES[species].allowedSlots.includes(slot)||findPart(id)?.slot!==slot)return loadout;
 const next={...loadout,[slot]:id};return validateLoadout(next,species)?next:loadout;
}
export function deriveStats(loadout,species='rat'){
 if(!validateLoadout(loadout,species))throw Error('Invalid loadout');
 const base=SPECIES[species];const result={maxHp:base.baseHp,attack:base.baseAttack,defense:base.baseDefense,tags:[]};
 for(const slot of SLOT_TYPES){const part=findPart(loadout[slot]);result.maxHp+=part.hp;result.attack+=part.attack;result.defense+=part.defense;result.tags.push(...part.tags);}
 result.tags=[...new Set(result.tags)];return result;
}
export function appearance(loadout){
 if(!validateLoadout(loadout))throw Error('Invalid loadout');
 return Object.fromEntries(SLOT_TYPES.map(slot=>[slot,loadout[slot]]));
}
