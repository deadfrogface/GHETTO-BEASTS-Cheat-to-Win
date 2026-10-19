// Data-driven opponents and deterministic responses. No paid services.
export const OPPONENTS=[
 {id:'golem',name:'Beton-Golem',maxHp:98,attack:10,defense:2,cheatSense:.1,reward:12,style:'bruiser'},
 {id:'ratking',name:'Rattenkönig',maxHp:82,attack:10,defense:1,cheatSense:.35,reward:14,style:'trickster'},
 {id:'enforcer',name:'Inkasso-Igor',maxHp:112,attack:12,defense:3,cheatSense:.5,reward:22,style:'enforcer'},
 {id:'pigeon',name:'Tauben-Toni',maxHp:78,attack:9,defense:0,cheatSense:.42,reward:15,style:'trickster'},
 {id:'scrapper',name:'Schrott-Sabine',maxHp:100,attack:12,defense:3,cheatSense:.3,reward:19,style:'bruiser'},
 {id:'boss',name:'Bürgermeister Beton',maxHp:132,attack:14,defense:4,cheatSense:.58,reward:32,style:'enforcer'}
];
export const getOpponent=id=>OPPONENTS.find(o=>o.id===id)||OPPONENTS[0];
export function chooseCounter(state,opponent,random){
 if(!Number.isFinite(random)||random<0||random>=1)throw Error('Invalid RNG');
 const suspicion=state.suspicion||0;
 if(suspicion>=45&&random<opponent.cheatSense)return {type:'expose',damage:5,heat:12,message:opponent.name+' durchschaut den Trick!'};
 if(opponent.style==='trickster'&&random<.24)return {type:'sabotage',damage:Math.max(1,opponent.attack-4),heat:5,debuff:2,message:opponent.name+' sabotiert deine Ausrüstung!'};
 if(opponent.style==='enforcer'&&suspicion>=25&&random<.32)return {type:'expose',damage:Math.max(1,opponent.attack-7),heat:8,message:opponent.name+' ruft die Ordnungshüter!'};
 if(state.playerGuard&&random<.35)return {type:'feint',damage:Math.max(1,opponent.attack-3),message:opponent.name+' täuscht einen Schlag an!'};
 return {type:'attack',damage:opponent.attack+Math.floor(random*7),message:opponent.name+' greift an!'};
}
export function difficulty(wins){return Math.min(1.25,1+Math.floor(Math.max(0,wins)/4)*.05);}
export const ENCOUNTER_ROSTERS={kanalviertel:['golem','ratking'],schrottplatz:['scrapper','pigeon'],rathaus:['enforcer','boss']};
export function opponentForDistrict(district,wins){const roster=ENCOUNTER_ROSTERS[district]||ENCOUNTER_ROSTERS.kanalviertel;return getOpponent(roster[Math.max(0,wins)%roster.length]);}
