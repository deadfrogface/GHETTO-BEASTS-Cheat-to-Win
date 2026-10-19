// Lightweight procedural articulation with attachment pivots (not skinned meshes).
import * as THREE from 'three';
import {validateLoadout,SPECIES} from './mutations.mjs';
const box=new THREE.BoxGeometry(1,1,1),ball=new THREE.IcosahedronGeometry(1,1),cone=new THREE.ConeGeometry(1,1,5),cylinder=new THREE.CylinderGeometry(1,1,1,8);
const material=color=>new THREE.MeshLambertMaterial({color,flatShading:true});
function part(parent,geo,mat,pos,scale){const mesh=new THREE.Mesh(geo,mat);mesh.position.set(...pos);mesh.scale.set(...scale);parent.add(mesh);return mesh;}
function pivot(parent,x,y,z){const g=new THREE.Group();g.position.set(x,y,z);parent.add(g);return g;}
export function createCreatureRig(loadout,species='rat'){
 if(!validateLoadout(loadout,species))throw Error('Invalid mutation rig');
 const template=SPECIES[species],root=new THREE.Group(),skin=material(loadout.skin==='armor'?'#909dac':template.colors[0]),trim=material(template.colors[1]),metal=material('#9eafbc'),eye=material('#152330');
 const torso=pivot(root,0,1.3,0);part(torso,ball,skin,[0,0,0],[.65,.78,.52]);
 const head=pivot(torso,0,.92,.08);part(head,ball,loadout.head==='stone-head'?metal:skin,[0,0,0],[.53,.49,.48]);
 for(const side of [-1,1]){
  part(head,ball,eye,[side*.22,.07,.44],[.085,.095,.06]);
  if(loadout.horns==='horns')part(head,cone,trim,[side*.36,.6,0],[.19,.53,.19]);
  else if(loadout.head==='rat-head')part(head,cone,skin,[side*.34,.52,0],[.24,.4,.2]);
 }
 const arms=[],legs=[],wings=[];
 for(const side of [-1,1]){
  const arm=pivot(torso,side*.79,.35,0);arms.push(arm);
  part(arm,box,loadout.arms==='steel-arms'?metal:trim,[side*.07,-.32,0],[.3,.74,.32]);
  if(loadout.arms==='claws')part(arm,cone,metal,[side*.07,-.78,.16],[.16,.4,.16]);
  const leg=pivot(root,side*.33,.86,0);legs.push(leg);
  part(leg,box,loadout.legs==='spring-legs'?metal:trim,[0,-.39,0],[.32,.78,.38]);
  if(loadout.extraArms==='extra-arms'){
   const extra=pivot(torso,side*.75,-.23,-.18);arms.push(extra);
   part(extra,box,trim,[side*.08,-.3,0],[.25,.64,.28]);
  }
  if(loadout.wings!=='none-wings'){
   const wing=pivot(torso,side*.44,.55,-.34);wings.push(wing);
   const membrane=part(wing,box,loadout.wings==='steel-wings'?metal:trim,[side*.65,.05,-.28],[1.25,.13,.75]);membrane.rotation.z=side*.24;
  }
 }
 let tail=null;
 if(loadout.tail==='rat-tail'){tail=pivot(torso,0,-.48,-.46);const segment=part(tail,cylinder,trim,[0,-.2,-.55],[.13,1.25,.13]);segment.rotation.x=1.1;}
 if(loadout.skin==='spikes')for(const x of [-.36,0,.36])part(torso,cone,trim,[x,.57,-.3],[.17,.45,.17]);
 root.scale.setScalar(template.bodyScale);
 function animate(time,mode='idle'){
  const t=time*2.4,attack=mode==='attack'?Math.sin(time*13)*.55:0;
  torso.rotation.z=Math.sin(t)*.035;head.rotation.y=Math.sin(t*.6)*.09;
  arms.forEach((a,i)=>{a.rotation.z=(i%2?1:-1)*(.13+Math.sin(t+i)*.12);a.rotation.x=attack;});
  legs.forEach((l,i)=>l.rotation.x=Math.sin(t+i*Math.PI)*.08);
  wings.forEach((w,i)=>w.rotation.y=(i%2?1:-1)*(.15+Math.sin(t*1.6)*.2));
  if(tail)tail.rotation.y=Math.sin(t)*.25;
 }
 function dispose(){root.traverse(o=>{if(o.isMesh&&o.material&&o.material.dispose)o.material.dispose();});}
 return {root,animate,dispose,pivots:{torso,head,arms,legs,wings,tail}};
}
