import {PLACES,SHOP,QUESTS,WORLD_SAVE,newWorld,loadWorld,travel,purchase,equip,recordFight,coolWanted} from './world.mjs';
import {PARTS} from './mutations.mjs';
export function initWorldUI({onFight,onEquip}){
 let world=loadWorld(localStorage.getItem(WORLD_SAVE));
 const panel=document.createElement('section');panel.id='world-panel';panel.style.cssText='position:fixed;top:10px;right:10px;z-index:20;max-width:min(340px,92vw);max-height:60vh;overflow:auto;background:#18242eee;color:white;padding:12px;border:1px solid #ffffff55;border-radius:12px;font:14px system-ui;';
 document.body.appendChild(panel);
 function commit(next){world=next;try{localStorage.setItem(WORLD_SAVE,JSON.stringify(world));}catch{}render();}
 function button(label,action,disabled=false){const b=document.createElement('button');b.textContent=label;b.disabled=disabled;b.style.cssText='margin:3px;padding:7px;border-radius:8px;cursor:pointer;';b.addEventListener('click',action);panel.appendChild(b);}
 function line(value){const p=document.createElement('p');p.textContent=value;p.style.margin='5px 0';panel.appendChild(p);}
 function render(){
  panel.replaceChildren();const heading=document.createElement('strong');heading.textContent='🏙️ GHETTO CITY';panel.appendChild(heading);
  line('💵 '+world.cash+' $ · 🏆 '+world.wins+' Siege · 🚨 '+Math.round(world.wanted)+'/100');
  for(const place of PLACES)button(place.name,()=>commit(travel(world,place.id)),world.location===place.id);
  if(world.location==='street')button('⚔️ Arena-Kampf',()=>onFight());
  if(world.location==='market')for(const item of SHOP)button(item.name+' – '+item.cost+' $',()=>commit(purchase(world,item.id)),world.cash<item.cost||item.type==='part'&&world.unlocked.includes(item.id));
  if(world.location==='lab')for(const part of PARTS.filter(p=>world.unlocked.includes(p.id)))button('🧬 '+part.name,()=>{const next=equip(world,part.id);if(next!==world){commit(next);onEquip(next.loadout);}});
  if(world.location==='station')button('🚓 Untertauchen',()=>commit(coolWanted(world)));
  line('🎒 Kits: '+world.inventory.medkit+' · Freigeschaltete Teile: '+world.unlocked.length);
  line('📜 Quests: '+QUESTS.map(q=>(world.claimed.includes(q.id)?'✓ ':'○ ')+q.name).join(' · '));
  line(world.events.at(-1)||'Erkunde den ersten Stadtabschnitt.');
 }
 render();
 return {getWorld:()=>world,recordFight:(won,suspicion)=>commit(recordFight(world,won,suspicion)),syncLoadout:loadout=>commit({...world,loadout})};
}
