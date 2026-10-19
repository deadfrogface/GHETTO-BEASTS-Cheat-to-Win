// Uses Three.js OSS glTF loader, SkeletonUtils and AnimationMixer.
// Models are optional; the procedural creature remains the offline fallback.
import * as THREE from 'three';
import {GLTFLoader} from 'three/addons/loaders/GLTFLoader.js';
import {clone as cloneSkeleton} from 'three/addons/utils/SkeletonUtils.js';
const loader=new GLTFLoader(),cache=new Map();
export async function preloadCharacter(url){
 if(typeof url!=='string'||!/^\.\/assets\/characters\/[a-z0-9_-]+\.glb$/i.test(url))throw Error('Only packaged character assets allowed');
 if(!cache.has(url))cache.set(url,loader.loadAsync(url).catch(e=>{cache.delete(url);throw e;}));
 return cache.get(url);
}
export function instanceCharacter(gltf,{scale=1,animation='idle'}={}){
 const scene=cloneSkeleton(gltf.scene),root=new THREE.Group();root.add(scene);root.scale.setScalar(scale);
 const mixer=new THREE.AnimationMixer(scene);
 const clips=gltf.animations||[];
 let active=null;
 function play(name=animation){
  const clip=clips.find(c=>c.name.toLowerCase()===name.toLowerCase())||clips[0];
  if(!clip)return false;
  const next=mixer.clipAction(clip);
  if(active&&active!==next)active.fadeOut(.15);
  next.reset().fadeIn(.15).play();active=next;return true;
 }
 play(animation);
 return {root,animate(_time,delta=0){mixer.update(Math.max(0,Math.min(.1,delta)));},play,dispose(){mixer.stopAllAction();mixer.uncacheRoot(scene);}};
}
export async function createPackagedCharacter(url,options){return instanceCharacter(await preloadCharacter(url),options);}
