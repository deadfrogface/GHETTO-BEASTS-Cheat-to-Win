// Pure deterministic rules. No browser or renderer dependency.
import {findCheat,ALL_CHEATS,applyCheat} from './cheats.mjs';
export {ALL_CHEATS};
export const SAVE_KEY = 'ghetto-beasts-p2-combat-v1';
export const INITIAL = Object.freeze({round:1,phase:'player',playerHp:90,enemyHp:110,playerGuard:false,enemyGuard:false,enemyWeaken:0,skipEnemy:false,seed:17389,history:[]});
export const clamp=(n,min,max)=>Math.min(max,Math.max(min,n));
export function roll(seed){const next=(Math.imul(seed,1664525)+1013904223)>>>0;return [next,next/4294967296];}
export function newGame(){return {...INITIAL,history:['Ein neuer Kampf beginnt.']};}
export function validSave(s){return s&&Number.isInteger(s.round)&&s.round>=1&&s.round<=10000&&['player','enemy','over'].includes(s.phase)&&Number.isInteger(s.playerHp)&&s.playerHp>=0&&s.playerHp<=90&&Number.isInteger(s.enemyHp)&&s.enemyHp>=0&&s.enemyHp<=110&&Number.isInteger(s.seed)&&s.seed>=0&&s.seed<=4294967295&&typeof s.playerGuard==='boolean'&&typeof s.enemyGuard==='boolean'&&(s.enemyWeaken===undefined||(Number.isInteger(s.enemyWeaken)&&s.enemyWeaken>=0&&s.enemyWeaken<=20))&&(s.skipEnemy===undefined||typeof s.skipEnemy==='boolean')&&Array.isArray(s.history)&&s.history.length<=12&&s.history.every(v=>typeof v==='string'&&v.length<250);}
function record(s,message){return {...s,history:[...s.history,message].slice(-10)};}
export function playerTurn(s,action){
 if(s.phase!=='player'||!['attack','guard','cheat'].includes(action)&&!ALL_CHEATS.some(d=>d.id===action))return s;
 if(action==='cheat'||findCheat(action)||ALL_CHEATS.some(d=>d.id===action)){
  const definition=ALL_CHEATS.find(d=>d.id===(action==='cheat'?'brick':action));
  const [seed,random]=roll(s.seed);return applyCheat({...s,seed},definition,random);
 }
 let t={...s,phase:'enemy',playerGuard:false};let rng;t.seed=(rng=roll(t.seed))[0];
 if(action==='attack'){const damage=12+Math.floor(rng[1]*9);t.enemyHp=clamp(t.enemyHp-damage,0,110);t=record(t,'Kanalratte trifft für '+damage+' Schaden.');}
 if(action==='guard'){t.playerGuard=true;t=record(t,'Kanalratte geht in Deckung.');}

 if(t.enemyHp===0){t.phase='over';t=record(t,'Sieg! Der Beton-Golem ist besiegt.');}
 else if(t.playerHp===0){t.phase='over';t=record(t,'Niederlage durch gescheiterten Betrug.');}
 return t;
}
export function enemyTurn(s){
 if(s.phase!=='enemy')return s;
 if(s.skipEnemy){return record({...s,skipEnemy:false,phase:'player',round:s.round+1},'Beton-Golem ist abgelenkt und setzt aus.');}
 let t={...s},rng;[t.seed,rng]=roll(t.seed);
 const base=Math.max(1,10+Math.floor(rng*9)-(t.enemyWeaken||0)),damage=t.playerGuard?Math.ceil(base*0.35):base;
 t.playerHp=clamp(t.playerHp-damage,0,90);
 t=record(t,'Beton-Golem greift an: '+damage+' Schaden'+(s.playerGuard?' (Deckung)':'')+'.');
 t.playerGuard=false;t.enemyWeaken=0;
 if(t.playerHp===0){t.phase='over';t=record(t,'Niederlage! Kanalratte ist kampfunfähig.');}
 else {t.phase='player';t.round+=1;}
 return t;
}
