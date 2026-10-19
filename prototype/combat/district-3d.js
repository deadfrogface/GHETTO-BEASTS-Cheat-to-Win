// Low-poly walkable district, using Three.js instancing and shared geometry.
import * as THREE from 'three';
import {DISTRICT} from './world.mjs';
export function createDistrict(scene){
 const root=new THREE.Group();root.visible=false;scene.add(root);
 const ground=new THREE.Mesh(new THREE.PlaneGeometry(24,24),new THREE.MeshLambertMaterial({color:0x344c56}));ground.rotation.x=-Math.PI/2;ground.position.y=-.06;root.add(ground);
 const street=new THREE.Mesh(new THREE.PlaneGeometry(5,24),new THREE.MeshLambertMaterial({color:0x222d3b}));street.rotation.x=-Math.PI/2;street.position.y=-.045;root.add(street);
 const buildingGeometry=new THREE.BoxGeometry(1,1,1);
 const buildingMaterial=new THREE.MeshLambertMaterial({color:0x8392a6,flatShading:true});
 const buildings=new THREE.InstancedMesh(buildingGeometry,buildingMaterial,16);
 const matrix=new THREE.Object3D();let n=0;
 for(const x of [-9,-6,6,9])for(const z of [-8,-3,3,8]){
  const height=2+((n*7)%5);matrix.position.set(x,height/2,z);matrix.scale.set(2.3,height,2.4);matrix.updateMatrix();buildings.setMatrixAt(n++,matrix.matrix);
 }
 buildings.instanceMatrix.needsUpdate=true;root.add(buildings);
 const npcGeo=new THREE.IcosahedronGeometry(.52,1),npcMat=new THREE.MeshLambertMaterial({color:0xffd06b,flatShading:true});
 const markers=new THREE.Group();root.add(markers);
 for(const npc of DISTRICT.npcs){const m=new THREE.Mesh(npcGeo,npcMat);m.position.set(npc.x,.7,npc.z);m.userData.npc=npc;markers.add(m);}
 const avatar=new THREE.Group();root.add(avatar);
 const body=new THREE.Mesh(new THREE.IcosahedronGeometry(.46,1),new THREE.MeshLambertMaterial({color:0x9bea66,flatShading:true}));body.position.y=.58;avatar.add(body);
 const cameraPosition=new THREE.Vector3();
 return {root,avatar,markers,setTheme(id){const palette={kanalviertel:[0x344c56,0x8392a6],schrottplatz:[0x62513c,0x9c7149],rathaus:[0x4c516b,0xb2a8bf]}[id]||[0x344c56,0x8392a6];ground.material.color.setHex(palette[0]);buildingMaterial.color.setHex(palette[1]);},update(position,camera){
  avatar.position.set(position.x,0,position.z);
  cameraPosition.set(position.x+9,9,position.z+11);
  camera.position.lerp(cameraPosition,.14);camera.lookAt(position.x,0,position.z);
 },nearest(position,maxDistance=2.1){
  return DISTRICT.npcs.find(npc=>Math.hypot(npc.x-position.x,npc.z-position.z)<maxDistance)||null;
 },dispose(){scene.remove(root);root.traverse(obj=>{if(obj.geometry)obj.geometry.dispose();if(obj.material)obj.material.dispose();});}};
}
