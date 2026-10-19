import {createPackagedCharacter} from './oss-character.js';
import {createCreatureRig} from './creature-rig.js';
import {PARTS,SLOT_TYPES,mutate,deriveStats} from './mutations.mjs';
import * as THREE from 'three';
import {SAVE_KEY,LEGACY_SAVE_KEY,newGame,migrateSave,playerTurn,enemyTurn,ALL_CHEATS} from './rules.mjs';
const $=id=>document.getElementById(id);
let state;
try{const raw=localStorage.getItem(SAVE_KEY)||localStorage.getItem(LEGACY_SAVE_KEY);const parsed=raw?JSON.parse(raw):null;state=migrateSave(parsed)||newGame();}catch{state=newGame();}
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
let playerRig=createCreatureRig(state.loadout,'rat'),player=playerRig.root,enemyRig=createCreatureRig({head:'stone-head',arms:'steel-arms',legs:'base-legs',skin:'armor',tail:'none-tail',horns:'none-horns',extraArms:'none-extra',wings:'none-wings'},'golem'),enemy=enemyRig.root;
player.position.x=-2.7;enemy.position.x=2.7;scene.add(player,enemy);
player.rotation.y=0.4;enemy.rotation.y=-0.4;

const mutationPanel=$('mutation-slots');
const slotLabels={head:'Kopf',arms:'Arme',legs:'Beine',skin:'Haut',tail:'Schwanz',horns:'Hörner',extraArms:'Zusatzarme',wings:'Flügel'};
for(const slot of SLOT_TYPES){
 const label=document.createElement('label');label.textContent=slotLabels[slot];
 const select=document.createElement('select');select.dataset.slot=slot;
 for(const part of PARTS.filter(p=>p.slot===slot)){const option=document.createElement('option');option.value=part.id;option.textContent=part.name;select.appendChild(option);}
 label.appendChild(select);mutationPanel.appendChild(label);
 select.addEventListener('change',()=>{
  if(state.phase!=='player'){select.value=state.loadout[slot];return;}
  const next=mutate(state.loadout,slot,select.value);
  if(next===state.loadout)return;
  const oldMax=deriveStats(state.loadout).maxHp,newMax=deriveStats(next).maxHp;
  state={...state,loadout:next,playerHp:Math.max(1,Math.min(newMax,state.playerHp+newMax-oldMax))};
  rebuildPlayer();persist();update();
 });
}
function rebuildPlayer(){
 if(playerModel){playerModel.dispose();playerModel=null;}
 scene.remove(player);playerRig.dispose();
 playerRig=createCreatureRig(state.loadout,'rat');player=playerRig.root;
 player.position.x=-2.7;player.rotation.y=.4;scene.add(player);
}

let busy=false,hitAt=0,hitTarget=null,enemyTimeout=null;
let playerModel=null,enemyModel=null,lastFrame=0;
async function loadPackagedModels(){
 const models=[['./assets/characters/rat.glb','player'],['./assets/characters/golem.glb','enemy']];
 for(const [url,who] of models){try{
  const rig=await createPackagedCharacter(url,{scale:1});
  if(who==='player'){scene.remove(player);playerRig.dispose();playerRig=rig;player=rig.root;player.position.x=-2.7;player.rotation.y=.4;scene.add(player);playerModel=rig;}
  else{scene.remove(enemy);enemyRig.dispose();enemyRig=rig;enemy=rig.root;enemy.position.x=2.7;enemy.rotation.y=-.4;scene.add(enemy);enemyModel=rig;}
 }catch{/* No bundled GLB yet: use low-poly procedural fallback. */}}
}

function persist(){try{localStorage.setItem(SAVE_KEY,JSON.stringify(state));}catch{$('result').textContent='Speichern nicht verfügbar; Kampf läuft nur in dieser Sitzung.';}}
function update(){
 $('resources').textContent='Bargeld: '+state.cash+' $ · Verdacht: '+state.suspicion+'/100';
 const selected=ALL_CHEATS.find(d=>d.id===cheatSelect.value);
 const unavailable=selected&&(state.cash<selected.cost||(state.cooldowns[selected.id]||0)>state.round);
 const cheatButton=document.querySelector('[data-action="cheat"]');
 cheatButton.textContent=selected?'🎭 '+selected.name+' ('+selected.cost+' $)':'🎭 Betrug';
 cheatButton.disabled=busy||state.phase!=='player'||Boolean(unavailable);
 const stats=deriveStats(state.loadout);
 $('player-hp').textContent=state.playerHp+' / '+stats.maxHp+' HP';
 $('mutation-stats').textContent='Max. HP '+stats.maxHp+' · Angriff +'+stats.attack+' · Verteidigung +'+stats.defense;
 mutationPanel.querySelectorAll('select').forEach(select=>{select.value=state.loadout[select.dataset.slot];select.disabled=busy||state.phase!=='player';});
 $('enemy-hp').textContent=state.enemyHp+' / 110 HP';
 $('player-bar').style.width=state.playerHp/stats.maxHp*100+'%';
 $('enemy-bar').style.width=state.enemyHp/110*100+'%';
 $('turn').textContent='Runde '+state.round+' · '+(state.phase==='over'?'Ende':state.phase==='player'?'Dein Zug':'Gegnerzug');
 $('result').textContent=state.history.at(-1)||'Kampfbereit.';
 $('log').textContent=state.history.slice(-5).join('  •  ');
 document.querySelectorAll('[data-action]').forEach(b=>b.disabled=busy||state.phase!=='player'||(b.dataset.action==='cheat'&&Boolean(unavailable)));
 cheatSelect.disabled=busy||state.phase!=='player';
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
const cheatSelect=$('cheat-select');
ALL_CHEATS.forEach(d=>{const option=document.createElement('option');option.value=d.id;option.textContent=d.name+' ('+Math.round(d.chance*100)+'% Erfolg)';cheatSelect.appendChild(option);});
cheatSelect.addEventListener('change',update);
document.querySelectorAll('[data-action]').forEach(b=>b.addEventListener('click',()=>take(b.dataset.action==='cheat'?cheatSelect.value:b.dataset.action)));
$('restart').addEventListener('click',()=>{if(enemyTimeout!==null)clearTimeout(enemyTimeout);enemyTimeout=null;busy=false;state=newGame();rebuildPlayer();persist();update();});
function resize(){const w=innerWidth,h=innerHeight;camera.aspect=w/h;camera.updateProjectionMatrix();renderer.setPixelRatio(Math.min(devicePixelRatio||1,1.5));renderer.setSize(w,h,false);}
window.addEventListener('resize',resize);
let last=0,frames=0,acc=0,raf=0;
function loop(now){raf=requestAnimationFrame(loop);if(last){const dt=now-last;if(dt>0&&dt<250){frames++;acc+=dt;if(acc>=1000){$('fps').textContent='FPS '+Math.round(frames*1000/acc);frames=0;acc=0;}}}last=now;
 const delta=lastFrame?Math.min(.1,(now-lastFrame)/1000):0;lastFrame=now;
 const t=now/1000;player.position.y=Math.sin(t*2)*0.07;enemy.position.y=Math.sin(t*1.8+2)*0.08;playerRig.animate(t,delta);enemyRig.animate(t+.8,delta);
 if(hitTarget){hitTarget.rotation.z=Math.sin((now-hitAt)/170*Math.PI)*0.15;if(now-hitAt>170){hitTarget.rotation.z=0;hitTarget=null;}}
 renderer.render(scene,camera);}
window.addEventListener('pagehide',()=>cancelAnimationFrame(raf),{once:true});
rebuildPlayer();void loadPackagedModels();resize();if(state.phase==='enemy'){busy=true;enemyTimeout=setTimeout(()=>{enemyTimeout=null;enemyAction();},650);}update();raf=requestAnimationFrame(loop);
