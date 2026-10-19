import {OPPONENTS,getOpponent,opponentForDistrict} from './opponents.mjs';
import {createDistrict} from './district-3d.js';
import {DISTRICT,QUESTS,SHOP,itemPrice,districtNpcs,availableDistricts,travel,validWorld,loadWorld,saveWorld,buy,unlockPart,equip,recordWin,claimQuest,move} from './world.mjs';
import {attachModularParts} from './modular-assets.js';
import {createPackagedCharacter} from './oss-character.js';
import {createCreatureRig} from './creature-rig.js';
import {PARTS,SLOT_TYPES,mutate,deriveStats} from './mutations.mjs';
import * as THREE from 'three';
import {SAVE_KEY,LEGACY_SAVE_KEY,newGame,migrateSave,playerTurn,enemyTurn,ALL_CHEATS} from './rules.mjs';
const $=id=>document.getElementById(id);
const SESSION_KEY='ghetto-beasts-p6-session-v1';
let sessionSnapshot=null;
try{const saved=JSON.parse(localStorage.getItem(SESSION_KEY));if(saved?.version===1&&validWorld(saved.world)&&migrateSave(saved.combat))sessionSnapshot=saved;}catch{}
let state;
try{const raw=localStorage.getItem(SAVE_KEY)||localStorage.getItem(LEGACY_SAVE_KEY);const parsed=raw?JSON.parse(raw):null;state=migrateSave(sessionSnapshot?.combat)||migrateSave(parsed)||newGame();}catch{state=newGame();}
let world=loadWorld(),lastWorldWin=false,exploring=false;
if(sessionSnapshot){const candidate=sessionSnapshot.world;if(validWorld(candidate))world=candidate;}
function persistSession(){if(!validWorld(world))return false;try{localStorage.setItem(SESSION_KEY,JSON.stringify({version:1,combat:state,world}));return true;}catch{return false;}}
// A completed battle can be recovered after a reload without paying twice.
if(state.phase==='over'&&state.enemyHp===0&&!state.rewardClaimed){
 const reward=getOpponent(state.opponentId).reward;
 const updated=recordWin(world,reward);
 if(updated!==world){world=updated;state={...state,rewardClaimed:true};persistSession();saveWorld(world);try{localStorage.setItem(SAVE_KEY,JSON.stringify(state));}catch{}}
}
const scene=new THREE.Scene();scene.background=new THREE.Color('#91b9cf');
const camera=new THREE.PerspectiveCamera(43,1,0.1,60);camera.position.set(9,8,13);camera.lookAt(0,1.1,0);
const renderer=new THREE.WebGLRenderer({canvas:$('scene'),antialias:false,powerPreference:'high-performance'});
renderer.outputColorSpace=THREE.SRGBColorSpace;
const district=createDistrict(scene);district.setTheme(world.district);
scene.add(new THREE.HemisphereLight(0xffffff,0x354153,2.3));
const sun=new THREE.DirectionalLight(0xffdfb1,2);sun.position.set(-4,9,7);scene.add(sun);
const box=new THREE.BoxGeometry(1,1,1),ball=new THREE.IcosahedronGeometry(1,1),cone=new THREE.ConeGeometry(1,1,5),cyl=new THREE.CylinderGeometry(1,1,1,16);
const material=c=>new THREE.MeshLambertMaterial({color:c,flatShading:true});
const arena=new THREE.Group();scene.add(arena);
function add(g,m,p,x,y,z,sx=1,sy=1,sz=1){const mesh=new THREE.Mesh(g,m);mesh.position.set(x,y,z);mesh.scale.set(sx,sy,sz);p.add(mesh);return mesh;}
add(box,material('#4c6573'),arena,0,-0.5,0,30,1,30);
add(cyl,material('#c7a783'),arena,0,-0.05,0,11,0.2,11);
for(let i=0;i<8;i++){const a=i*Math.PI/4;add(box,material('#687c8b'),arena,Math.sin(a)*13,1.5,Math.cos(a)*13,2,3+(i%3)*2,2);}
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
state={...state,loadout:{...world.loadout},playerHp:Math.min(state.playerHp,deriveStats(world.loadout).maxHp)};
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
  if(busy||state.phase==='enemy'){select.value=state.loadout[slot];return;}
  if(!exploring||district.nearest(world.position)?.role!=='lab'){select.value=state.loadout[slot];return;}
  const next=mutate(state.loadout,slot,select.value);
  if(!world.unlocked.includes(select.value)){select.value=state.loadout[slot];return;}
  if(next===state.loadout||next[slot]===state.loadout[slot])return;
  const oldMax=deriveStats(state.loadout).maxHp,newMax=deriveStats(next).maxHp;
  const equipped=equip(world,slot,select.value);
  if(equipped===world){select.value=state.loadout[slot];return;}
  world=equipped;
  state={...state,loadout:next,playerHp:Math.min(state.playerHp,newMax)};
  persist();saveWorld(world);rebuildPlayer();update();
 });
}
let modularController=null,modularAbort=null;
function rebuildPlayer(){
 modularAbort?.abort();modularController?.dispose();modularController=null;
 scene.remove(player);playerRig.dispose();
 playerRig=createCreatureRig(state.loadout,'rat');player=playerRig.root;
 player.position.x=-2.7;player.rotation.y=.4;scene.add(player);
 const controller=new AbortController();modularAbort=controller;
 void attachModularParts(playerRig,state.loadout,{signal:controller.signal}).then(parts=>{if(!controller.signal.aborted)modularController=parts;}).catch(()=>{});
}

let busy=false,hitAt=0,hitTarget=null,enemyTimeout=null;
let enemyModel=null,lastFrame=0;
function rebuildEnemy(){const opponent=getOpponent(state.opponentId);const layouts={golem:{head:'stone-head',arms:'steel-arms',skin:'armor'},ratking:{head:'rat-head',arms:'claws',tail:'rat-tail'},enforcer:{head:'base-head',arms:'steel-arms',skin:'armor'},pigeon:{head:'rat-head',arms:'claws',wings:'bat-wings'},scrapper:{head:'stone-head',arms:'steel-arms',skin:'spikes'},boss:{head:'stone-head',arms:'steel-arms',skin:'armor',horns:'horns'}};const species=['golem','boss'].includes(opponent.id)?'golem':opponent.id==='enforcer'||opponent.id==='scrapper'?'human':'rat';const loadout={head:'base-head',arms:'base-arms',legs:'base-legs',skin:'base-skin',tail:'none-tail',horns:'none-horns',extraArms:'none-extra',wings:'none-wings',...layouts[opponent.id]};if(species==='golem')loadout.wings='none-wings';scene.remove(enemy);enemyRig.dispose();enemyRig=createCreatureRig(loadout,species);enemy=enemyRig.root;enemy.position.x=2.7;enemy.rotation.y=-.4;enemy.visible=!exploring;scene.add(enemy);}
async function loadPackagedModels(){
 // The player must stay modular; a whole-body GLB would hide active mutations.
 if(state.opponentId!=='golem')return;
 const models=[['./assets/characters/golem.glb','enemy']];
 for(const [url,who] of models){try{
  const rig=await createPackagedCharacter(url,{scale:1});if(state.opponentId!=='golem'){rig.dispose();continue;}
  {scene.remove(enemy);enemyRig.dispose();enemyRig=rig;enemy=rig.root;enemy.position.x=2.7;enemy.rotation.y=-.4;enemy.visible=!exploring;scene.add(enemy);enemyModel=rig;}
 }catch{/* No bundled GLB yet: use low-poly procedural fallback. */}}
}

function persist(){if(!persistSession()){$('result').textContent='Speichern nicht verfügbar; Kampf läuft nur in dieser Sitzung.';}try{localStorage.setItem(SAVE_KEY,JSON.stringify(state));}catch{}}
function update(){
 document.querySelector('.fighters>div+div strong').textContent=getOpponent(state.opponentId).name.toUpperCase()+' · '+getOpponent(state.opponentId).style;
 renderWorld();
 $('resources').textContent='Bargeld: '+state.cash+' $ · Verdacht: '+state.suspicion+'/100';
 const selected=ALL_CHEATS.find(d=>d.id===cheatSelect.value);
 const unavailable=selected&&(state.cash<selected.cost||(state.cooldowns[selected.id]||0)>state.round);
 const cheatButton=document.querySelector('[data-action="cheat"]');
 cheatButton.textContent=selected?'🎭 '+selected.name+' ('+selected.cost+' $)':'🎭 Betrug';
 cheatButton.disabled=exploring||busy||state.phase!=='player'||Boolean(unavailable);
 const stats=deriveStats(state.loadout);
 $('player-hp').textContent=state.playerHp+' / '+stats.maxHp+' HP';
 $('mutation-stats').textContent='Max. HP '+stats.maxHp+' · Angriff +'+stats.attack+' · Verteidigung +'+stats.defense;
 mutationPanel.querySelectorAll('select').forEach(select=>{select.value=state.loadout[select.dataset.slot];select.disabled=busy||state.phase==='enemy'||!exploring||district.nearest(world.position)?.role!=='lab';});
 $('enemy-hp').textContent=state.enemyHp+' / '+getOpponent(state.opponentId).maxHp+' HP';
 $('player-bar').style.width=state.playerHp/stats.maxHp*100+'%';
 $('enemy-bar').style.width=state.enemyHp/getOpponent(state.opponentId).maxHp*100+'%';
 $('turn').textContent='Runde '+state.round+' · '+(state.phase==='over'?'Ende':state.phase==='player'?'Dein Zug':'Gegnerzug');
 $('result').textContent=state.history.at(-1)||'Kampfbereit.';
 $('log').textContent=state.history.slice(-5).join('  •  ');
 document.querySelectorAll('[data-action]').forEach(b=>b.disabled=exploring||busy||state.phase!=='player'||(b.dataset.action==='cheat'&&Boolean(unavailable)));
 cheatSelect.disabled=exploring||busy||state.phase!=='player';
}
function strike(target){hitTarget=target;hitAt=performance.now();}
function nextOpponent(){return opponentForDistrict(world.district,world.wins).id;}
function awardVictory(){if(state.phase!=='over'||state.enemyHp!==0||lastWorldWin||state.rewardClaimed)return;const opponent=getOpponent(state.opponentId);const rewarded=recordWin(world,opponent.reward);if(rewarded===world)return;world=rewarded;state={...state,rewardClaimed:true};persist();saveWorld(world);lastWorldWin=true;}
function enemyAction(){
 if(state.phase!=='enemy')return;
 state=enemyTurn(state);strike(player);awardVictory();persist();busy=false;update();
}
function take(action){
 if(exploring||busy||state.phase!=='player')return;
 busy=true;state=playerTurn(state,action);awardVictory();strike(enemy);persist();update();
 if(state.phase==='enemy')enemyTimeout=setTimeout(()=>{enemyTimeout=null;enemyAction();},650);
 else {busy=false;update();}
}
const cheatSelect=$('cheat-select');
ALL_CHEATS.forEach(d=>{const option=document.createElement('option');option.value=d.id;option.textContent=d.name+' ('+Math.round(d.chance*100)+'% Erfolg)';cheatSelect.appendChild(option);});
cheatSelect.addEventListener('change',update);
document.querySelectorAll('[data-action]').forEach(b=>b.addEventListener('click',()=>take(b.dataset.action==='cheat'?cheatSelect.value:b.dataset.action)));
$('restart').addEventListener('click',()=>{if(exploring||busy||state.phase==='enemy')return;const previousHp=state.playerHp,defeated=state.phase==='over'&&state.playerHp===0;if(enemyTimeout!==null)clearTimeout(enemyTimeout);enemyTimeout=null;busy=false;state=newGame(nextOpponent(),world.wins);state.loadout={...world.loadout};state.playerHp=defeated?deriveStats(state.loadout).maxHp:Math.max(1,Math.min(previousHp,deriveStats(state.loadout).maxHp));lastWorldWin=false;rebuildPlayer();rebuildEnemy();persist();update();});
function renderWorld(){
 $('world-status').textContent='🪙 '+world.coins+' $ · 🧰 Schrott '+world.inventory.scrap+' · 🩹 Verbände '+world.inventory.medkit+' · 🏆 Siege '+world.wins;
 const map=$('world-map');map.replaceChildren();
 for(const area of availableDistricts(world.wins)){const button=document.createElement('button');button.textContent=(world.district===area.id?'📍 ':'🗺️ ')+area.name;button.disabled=!exploring||world.district===area.id;button.addEventListener('click',()=>{world=travel(world,area.id);district.setTheme(world.district);persistSession();saveWorld(world);renderWorld();});map.appendChild(button);}
 for(const npc of districtNpcs(world.district)){const b=document.createElement('button');b.textContent=npc.name+' · '+npc.role;b.addEventListener('click',()=>{if(!exploring)return;world=move(world,npc.x,npc.z);persistSession();saveWorld(world);renderWorld();});map.appendChild(b);}
 const actions=$('world-actions');actions.replaceChildren();
 const heading=document.createElement('p');heading.textContent='Shop · Labor · Arena';actions.appendChild(heading);
 if(world.inventory.medkit>0){const heal=document.createElement('button');heal.textContent='🩹 Verband verwenden (+20 HP)';heal.disabled=!exploring||state.phase==='enemy'||state.playerHp>=deriveStats(state.loadout).maxHp;heal.addEventListener('click',()=>{if(!exploring||state.phase==='enemy'||world.inventory.medkit<=0||state.playerHp>=deriveStats(state.loadout).maxHp)return;world={...world,inventory:{...world.inventory,medkit:world.inventory.medkit-1}};state={...state,playerHp:Math.min(deriveStats(state.loadout).maxHp,state.playerHp+20)};persist();saveWorld(world);update();});actions.appendChild(heal);}
 for(const item of SHOP){const b=document.createElement('button');b.textContent='Kaufen: '+item.name+' ('+itemPrice(world.district,item.id)+' $)';b.disabled=!exploring||world.coins<itemPrice(world.district,item.id)||district.nearest(world.position)?.role!=='shop';b.addEventListener('click',()=>{world=buy(world,item.id);persistSession();saveWorld(world);renderWorld();});actions.appendChild(b);}
 for(const part of PARTS.filter(p=>!world.unlocked.includes(p.id))){const b=document.createElement('button');b.textContent='🔬 '+part.name+' (2 Schrott)';b.disabled=!exploring||world.inventory.scrap<2||district.nearest(world.position)?.role!=='lab';b.addEventListener('click',()=>{world=unlockPart(world,part.id);persistSession();saveWorld(world);renderWorld();update();});actions.appendChild(b);}
 const quests=$('world-quests');quests.replaceChildren();
 for(const q of QUESTS){const b=document.createElement('button');b.textContent=(world.completed.includes(q.id)?'✓ ':world[q.goal]>=q.target?'🎁 ':'🔒 ')+q.name+' ('+world[q.goal]+'/'+q.target+')';b.disabled=!exploring||world.completed.includes(q.id)||world[q.goal]<q.target||district.nearest(world.position)?.role!=='quest';b.addEventListener('click',()=>{world=claimQuest(world,q.id);persistSession();saveWorld(world);renderWorld();});quests.appendChild(b);}
 mutationPanel.querySelectorAll('select').forEach(select=>select.querySelectorAll('option').forEach(opt=>{opt.disabled=!world.unlocked.includes(opt.value);}));
}
function toggleExplore(on){
 exploring=on;arena.visible=!on;district.root.visible=on;player.visible=!on;enemy.visible=!on;document.body.classList.toggle('exploring',on);
 $('walk-controls').hidden=!on;$('world-toggle').textContent=on?'⚔️ Zur Arena':'🏙️ Stadt erkunden';
 if(!on){camera.position.set(9,8,13);camera.lookAt(0,1.1,0);}
 renderWorld();
}
$('world-toggle').addEventListener('click',()=>{if(busy||state.phase==='enemy')return;toggleExplore(!exploring);});
document.querySelectorAll('[data-walk]').forEach(button=>button.addEventListener('click',()=>{
 if(!exploring)return;const [dx,dz]=button.dataset.walk.split(',').map(Number);
 world=move(world,world.position.x+dx,world.position.z+dz);persistSession();saveWorld(world);renderWorld();
}));
$('world-interact').addEventListener('click',()=>{
 const npc=district.nearest(world.position);
 if(!exploring||!npc){$('world-status').textContent='Kein NPC in Reichweite.';return;}
 if(npc.role==='arena'){
  // Entering a district arena always starts its own encounter; no stale opponent.
  if(state.phase==='enemy'||busy)return;
  toggleExplore(false);
  if(state.phase==='over'||state.opponentId!==nextOpponent())$('restart').click();
 }
 else{$('world-status').textContent=npc.name+': '+({shop:'Willkommen im Schrottladen!',lab:'Zwei Schrottteile pro Mutation.',quest:'Schau nach deinen Aufträgen.'}[npc.role]||'Hallo!');}
});
$('world-panel').open=true;
function resize(){const w=innerWidth,h=innerHeight;camera.aspect=w/h;camera.updateProjectionMatrix();renderer.setPixelRatio(Math.min(devicePixelRatio||1,1.5));renderer.setSize(w,h,false);}
window.addEventListener('resize',resize);
let last=0,frames=0,acc=0,raf=0;
function loop(now){raf=requestAnimationFrame(loop);if(last){const dt=now-last;if(dt>0&&dt<250){frames++;acc+=dt;if(acc>=1000){$('fps').textContent='FPS '+Math.round(frames*1000/acc);frames=0;acc=0;}}}last=now;
 const delta=lastFrame?Math.min(.1,(now-lastFrame)/1000):0;lastFrame=now;
 const t=now/1000;player.position.y=Math.sin(t*2)*0.07;enemy.position.y=Math.sin(t*1.8+2)*0.08;playerRig.animate(t,delta);modularController?.animate(delta);enemyRig.animate(t+.8,delta);
 if(hitTarget){hitTarget.rotation.z=Math.sin((now-hitAt)/170*Math.PI)*0.15;if(now-hitAt>170){hitTarget.rotation.z=0;hitTarget=null;}}
 if(exploring)district.update(world.position,camera);
 renderer.render(scene,camera);}
window.addEventListener('pagehide',()=>cancelAnimationFrame(raf),{once:true});
rebuildPlayer();rebuildEnemy();void loadPackagedModels();resize();if(state.phase==='enemy'){busy=true;enemyTimeout=setTimeout(()=>{enemyTimeout=null;enemyAction();},650);}update();raf=requestAnimationFrame(loop);
