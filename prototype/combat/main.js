import * as THREE from 'three';
import {SAVE_KEY,newGame,validSave,playerTurn,enemyTurn} from './rules.mjs';
const $=id=>document.getElementById(id);
let state;
try{const raw=localStorage.getItem(SAVE_KEY);const parsed=raw?JSON.parse(raw):null;state=validSave(parsed)?parsed:newGame();}catch{state=newGame();}
const scene=new THREE.Scene();scene.background=new THREE.Color('#91b9cf');
const camera=new THREE.PerspectiveCamera(43,1,0.1,60);camera.position.set(9,8,13);camera.lookAt(0,1.1,0);
const renderer=new THREE.WebGLRenderer({canvas:$('scene'),antialias:false,powerPreference:'high-performance'});
renderer.outputColorSpace=THREE.SRGBColorSpace;
scene.add(new THREE.HemisphereLight(0xffffff,0x354153,2.3));
const sun=new THREE.DirectionalLight(0xffdfb1,2);sun.position.set(-4,9,7);scene.add(sun);
const box=new THREE.BoxGeometry(1,1,1),ball=new THREE.IcosahedronGeometry(1,1),cone=new THREE.ConeGeometry(1,1,5),cyl=new THREE.CylinderGeometry(1,1,1,16);
const material=c=>new THREE.MeshLambertMaterial({color:c,flatShading:true});
function add(g,m,p,x,y,z,sx=1,sy=1,sz=1){const mesh=new THREE.Mesh(g,m);mesh.position.set(x,y,z);mesh.scale.set(sx,sy,sz);p.add(mesh);return mesh;}
add(box,material('#4c6573'),scene,0,-0.5,0,30,1,30);
add(cyl,material('#c7a783'),scene,0,-0.05,0,11,0.2,11);
for(let i=0;i<8;i++){const a=i*Math.PI/4;add(box,material('#687c8b'),scene,Math.sin(a)*13,1.5,Math.cos(a)*13,2,3+(i%3)*2,2);}
function actor(color,accent,x,monster=false){
 const group=new THREE.Group(),skin=material(color),trim=material(accent),eyes=material('#182030');
 add(ball,skin,group,0,1.2,0,monster?0.85:0.6,monster?0.85:0.72,0.58);
 add(ball,skin,group,0,2.15,0.18,monster?0.66:0.53,0.5,0.5);
 for(const s of [-1,1]){
  add(cone,trim,group,s*0.38,2.75,0.08,0.25,0.6,0.25);
  add(ball,eyes,group,s*0.23,2.27,0.63,0.08,0.08,0.07);
  add(box,trim,group,s*0.85,1.35,0,0.27,0.7,0.3);
  add(box,trim,group,s*0.32,0.42,0,0.3,0.75,0.32);
 }
 group.position.x=x;scene.add(group);return group;
}
const player=actor('#a4eb75','#4d7444',-2.7),enemy=actor('#dc9678','#73566a',2.7,true);
player.rotation.y=0.4;enemy.rotation.y=-0.4;
let busy=false,hitAt=0,hitTarget=null,enemyTimeout=null;
function persist(){try{localStorage.setItem(SAVE_KEY,JSON.stringify(state));}catch{$('result').textContent='Speichern nicht verfügbar; Kampf läuft nur in dieser Sitzung.';}}
function update(){
 $('player-hp').textContent=state.playerHp+' / 90 HP';
 $('enemy-hp').textContent=state.enemyHp+' / 110 HP';
 $('player-bar').style.width=state.playerHp/90*100+'%';
 $('enemy-bar').style.width=state.enemyHp/110*100+'%';
 $('turn').textContent='Runde '+state.round+' · '+(state.phase==='over'?'Ende':state.phase==='player'?'Dein Zug':'Gegnerzug');
 $('result').textContent=state.history.at(-1)||'Kampfbereit.';
 $('log').textContent=state.history.slice(-5).join('  •  ');
 document.querySelectorAll('[data-action]').forEach(b=>b.disabled=busy||state.phase!=='player');
}
function strike(target){hitTarget=target;hitAt=performance.now();}
function enemyAction(){
 if(state.phase!=='enemy')return;
 state=enemyTurn(state);strike(player);persist();busy=false;update();
}
function take(action){
 if(busy||state.phase!=='player')return;
 busy=true;state=playerTurn(state,action);strike(enemy);persist();update();
 if(state.phase==='enemy')enemyTimeout=setTimeout(()=>{enemyTimeout=null;enemyAction();},650);
 else {busy=false;update();}
}
document.querySelectorAll('[data-action]').forEach(b=>b.addEventListener('click',()=>take(b.dataset.action)));
$('restart').addEventListener('click',()=>{if(enemyTimeout!==null)clearTimeout(enemyTimeout);enemyTimeout=null;busy=false;state=newGame();persist();update();});
function resize(){const w=innerWidth,h=innerHeight;camera.aspect=w/h;camera.updateProjectionMatrix();renderer.setPixelRatio(Math.min(devicePixelRatio||1,1.5));renderer.setSize(w,h,false);}
window.addEventListener('resize',resize);
let last=0,frames=0,acc=0,raf=0;
function loop(now){raf=requestAnimationFrame(loop);if(last){const dt=now-last;if(dt>0&&dt<250){frames++;acc+=dt;if(acc>=1000){$('fps').textContent='FPS '+Math.round(frames*1000/acc);frames=0;acc=0;}}}last=now;
 const t=now/1000;player.position.y=Math.sin(t*2)*0.07;enemy.position.y=Math.sin(t*1.8+2)*0.08;
 if(hitTarget){hitTarget.rotation.z=Math.sin((now-hitAt)/170*Math.PI)*0.15;if(now-hitAt>170){hitTarget.rotation.z=0;hitTarget=null;}}
 renderer.render(scene,camera);}
window.addEventListener('pagehide',()=>cancelAnimationFrame(raf),{once:true});
resize();if(state.phase==='enemy'){busy=true;enemyTimeout=setTimeout(()=>{enemyTimeout=null;enemyAction();},650);}update();raf=requestAnimationFrame(loop);
