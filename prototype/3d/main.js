import * as THREE from 'three';

const $ = id => document.getElementById(id);
const canvas = $('scene');
const scene = new THREE.Scene();
scene.background = new THREE.Color('#91b9d0');
scene.fog = new THREE.Fog('#91b9d0', 20, 44);
const camera = new THREE.PerspectiveCamera(45, 1, 0.1, 65);
const renderer = new THREE.WebGLRenderer({ canvas, antialias: false, powerPreference: 'high-performance', alpha: false });
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.shadowMap.enabled = false;
renderer.setPixelRatio(1);
const hemi = new THREE.HemisphereLight(0xeafaff, 0x354456, 2.2); scene.add(hemi);
const sun = new THREE.DirectionalLight(0xfff0ca, 2.0); sun.position.set(-4, 10, 7); scene.add(sun);
const mat = (hex) => new THREE.MeshLambertMaterial({ color: hex, flatShading: true });
const ground = mat('#556878'), ring = mat('#c3a37c'), curb = mat('#303d50'), white = mat('#f0f1df');
const geo = {box:new THREE.BoxGeometry(1,1,1), sphere:new THREE.IcosahedronGeometry(1,1), cone:new THREE.ConeGeometry(1,5), cyl:new THREE.CylinderGeometry(1,1,1,8)};
function mesh(g,m,x,y,z,sx=1,sy=1,sz=1,parent=scene){const o=new THREE.Mesh(g,m);o.position.set(x,y,z);o.scale.set(sx,sy,sz);parent.add(o);return o;}
mesh(geo.box,ground,0,-0.7,0,35,1,35);
mesh(geo.cyl,ring,0,-0.13,0,13,0.3,13);
mesh(geo.cyl,curb,0,-0.31,0,13.7,0.25,13.7);
for(let i=0;i<12;i++){const a=i*Math.PI/6;mesh(geo.box,white,Math.sin(a)*6.1,0.04,Math.cos(a)*6.1,0.55,0.06,0.13).rotation.y=a;}
const buildings=[[-13,-11,3,7,3],[-9,-13,4,4,3],[11,-13,4,8,4],[14,-5,3,5,5],[-13,8,4,9,3],[12,10,5,6,4],[-6,13,5,7,3],[5,13,5,10,3]];
const buildingMats=[mat('#506a7d'),mat('#56727b'),mat('#b47764'),mat('#526c65')];
buildings.forEach(([x,z,w,h,d],i)=>{mesh(geo.box,buildingMats[i%4],x,h/2-0.2,z,w,h,d);for(let f=1;f<Math.floor(h);f+=2){mesh(geo.box,mat('#f5d68c'),x, f, z+d/2+0.03,w*0.55,0.36,0.05);}});
const palettes=[['#9be063','#466a4b'],['#d983aa','#693b65'],['#f7bc5d','#70532f'],['#72d6db','#34677c']];
function creature(i){
 const group=new THREE.Group(), a=palettes[i%palettes.length], skin=mat(a[0]), dark=mat(a[1]), eye=mat('#141a29');
 const body=mesh(geo.sphere,skin,0,1.2,0,0.6,0.8,0.5,group);
 mesh(geo.sphere,skin,0,2.2,0.08,0.54,0.5,0.5,group);
 for(const side of [-1,1]){
   mesh(geo.cone,dark,side*0.35,2.76,0,0.2,0.48,0.2,group);
   mesh(geo.sphere,eye,side*0.2,2.3,0.48,0.075,0.09,0.05,group);
   mesh(geo.box,dark,side*0.8,1.35,0,0.25,0.75,0.3,group).rotation.z=side*0.3;
   mesh(geo.box,dark,side*0.31,0.45,0,0.25,0.7,0.33,group);
 }
 group.position.set(Math.cos(i*Math.PI/12)*4.3,0,Math.sin(i*Math.PI/12)*4.3);
 group.rotation.y=-i*Math.PI/12-Math.PI/2;
 scene.add(group);return {group,body,phase:i*0.71};
}
let actors=[];function setLoad(n){actors.forEach(a=>scene.remove(a.group));actors=Array.from({length:n},(_,i)=>creature(i));$('status').textContent=n+' animierte Figuren · Echtzeit-Messung aktiv';}
let scale=1, paused=false, cameraMode=0, raf=0, last=0, elapsed=0;
const frameTimes=[], MAX=1800; let current={fps:null,low:null,frame:null,draws:0};
function resize(){
 const w=Math.max(1,innerWidth),h=Math.max(1,innerHeight);
 camera.aspect=w/h;camera.updateProjectionMatrix();
 renderer.setSize(w,h,false);
 const ratio=Math.min(window.devicePixelRatio||1,2)*scale;
 renderer.setPixelRatio(Math.min(ratio,2));
}
function cameraPosition(){if(cameraMode===0)camera.position.set(12,12,17);else camera.position.set(0,7,18);camera.lookAt(0,0.5,0);}
function calculate(){
 if(frameTimes.length<20)return;
 const sorted=frameTimes.slice().sort((a,b)=>b-a);
 const average=frameTimes.reduce((a,b)=>a+b,0)/frameTimes.length;
 const worst=sorted.slice(0,Math.max(1,Math.ceil(sorted.length*0.01)));
 const lowMs=worst.reduce((a,b)=>a+b,0)/worst.length;
 current={fps:1000/average,low:1000/lowMs,frame:average,draws:renderer.info.render.calls};
 $('fps').textContent=current.fps.toFixed(0);
 $('low').textContent=current.low.toFixed(0);
 $('frame').textContent=current.frame.toFixed(1);
 $('draws').textContent=current.draws;
}
function tick(now){
 raf=requestAnimationFrame(tick);
 if(!last){last=now;return;}
 const dt=now-last;last=now;
 if(dt<=0||dt>250)return;
 frameTimes.push(dt);if(frameTimes.length>MAX)frameTimes.shift();
 if(!paused){elapsed+=dt/1000;actors.forEach(a=>{a.group.position.y=Math.sin(elapsed*2.2+a.phase)*0.13;a.body.rotation.z=Math.sin(elapsed*2+a.phase)*0.09;});}
 renderer.render(scene,camera);
 if(now-lastStats>1000){calculate();lastStats=now;}
}
let lastStats=0;
$('toggle').addEventListener('click',()=>{paused=!paused;$('toggle').textContent=paused?'▶ Animation fortsetzen':'⏸ Animation pausieren';});
$('camera').addEventListener('click',()=>{cameraMode=(cameraMode+1)%2;cameraPosition();});
$('scale').addEventListener('input',e=>{scale=Number(e.target.value);$('scale-value').textContent=scale.toFixed(1)+'×';resize();frameTimes.length=0;});
$('load').addEventListener('change',e=>{setLoad(Number(e.target.value));frameTimes.length=0;});
$('export').addEventListener('click',async()=>{
 const report={app:'GHETTO BEASTS P1 3D Benchmark',timestamp:new Date().toISOString(),userAgent:navigator.userAgent,screen:{width:innerWidth,height:innerHeight,dpr:devicePixelRatio},load:actors.length,renderScale:scale,paused,...current,samples:frameTimes.length,notes:'1%-low approximated from recent frame times; not a native-app benchmark'};
 try{await navigator.clipboard.writeText(JSON.stringify(report,null,2));$('status').textContent='Messbericht kopiert';}
 catch{$('status').textContent='Kopieren blockiert: HTTPS oder Browser-Berechtigung erforderlich';}
});
window.addEventListener('resize',resize);
window.addEventListener('pagehide',()=>cancelAnimationFrame(raf),{once:true});
try{
 if(!renderer.capabilities.isWebGL2) $('mode').textContent='WEBGL 1';
 else $('mode').textContent='WEBGL 2';
 cameraPosition();setLoad(6);resize();raf=requestAnimationFrame(tick);
}catch(error){$('status').textContent='3D-Start fehlgeschlagen: '+error.message;throw error;}
