// Optional locally packaged modular glTF parts. Uses official Three.js loader and SkeletonUtils.
// Parts attach to procedural rig pivots; missing assets preserve built-in meshes.
import {preloadCharacter,instanceCharacter} from './oss-character.js';
import {SLOT_TYPES} from './mutations.mjs';
const safe=id=>/^[a-z][a-z0-9-]{0,50}$/.test(id);
export async function attachModularParts(rig,loadout,{signal}={}){
 if(!rig?.pivots)throw Error('Rig attachment pivots required');
 const attached=[];
 const targets={head:[rig.pivots.head],arms:rig.pivots.arms.slice(0,2),legs:rig.pivots.legs,skin:[rig.pivots.torso],tail:rig.pivots.tail?[rig.pivots.tail]:[],horns:[rig.pivots.head],extraArms:rig.pivots.arms.slice(2),wings:rig.pivots.wings};
 await Promise.all(SLOT_TYPES.map(async slot=>{
  const id=loadout[slot];if(!safe(id)||signal?.aborted)return;
  const path='./assets/characters/'+id+'.glb';
  try{
   const asset=await preloadCharacter(path);
   if(signal?.aborted)return;
   for(const pivot of targets[slot]||[]){
    const instance=instanceCharacter(asset,{animation:'idle'});
    pivot.add(instance.root);
    attached.push({pivot,instance});
   }
  }catch{/* Asset is optional; fallback geometry remains. */}
 }));
 if(signal?.aborted){for(const {pivot,instance} of attached){pivot.remove(instance.root);instance.dispose();}return null;}
 return {
  animate(delta){for(const {instance} of attached)instance.animate(0,delta);},
  dispose(){for(const {pivot,instance} of attached){pivot.remove(instance.root);instance.dispose();}}
 };
}
