// Data-driven opponents and deterministic responses. No paid services.
export const OPPONENTS=[
 {id:'golem',name:'Beton-Golem',maxHp:110,attack:12,defense:2,cheatSense:.1,reward:8,style:'bruiser'},
 {id:'ratking',name:'Rattenkönig',maxHp:85,attack:10,defense:1,cheatSense:.35,reward:12,style:'trickster'},
 {id:'enforcer',name:'Inkasso-Igor',maxHp:125,attack:14,defense:4,cheatSense:.55,reward:18,style:'enforcer'}
];
export const getOpponent=id=>OPPONENTS.find(o=>o.id===id)||OPPONENTS[0];
export function chooseCounter(state,opponent,random){
 if(!Number.isFinite(random)||random<0||random>=1)throw Error('Invalid RNG');
 const suspicion=state.suspicion||0;
 if(suspicion>=45&&random<opponent.cheatSense)return {type:'expose',damage:5,heat:12,message:opponent.name+' durchschaut den Trick!'};
 if(state.playerGuard&&random<.35)return {type:'feint',damage:Math.max(1,opponent.attack-3),message:opponent.name+' täuscht einen Schlag an!'};
 return {type:'attack',damage:opponent.attack+Math.floor(random*7),message:opponent.name+' greift an!'};
}
export function difficulty(wins){return Math.min(1.4,1+Math.floor(Math.max(0,wins)/3)*.08);}
