import {getOpponent,chooseCounter,difficulty} from './opponents.mjs';
import {BASE_LOADOUT,validateLoadout,deriveStats,normalizeLoadout} from './mutations.mjs';
// Pure deterministic rules. No browser or renderer dependency.
import {findCheat,ALL_CHEATS,applyCheat,START_CASH} from './cheats.mjs';
export {ALL_CHEATS};
export const SAVE_KEY = 'ghetto-beasts-p3-combat-v1';
export const LEGACY_SAVE_KEY = 'ghetto-beasts-p2-combat-v1';
export const INITIAL = Object.freeze({round:1,phase:'player',cash:START_CASH,suspicion:0,cooldowns:{},loadout:BASE_LOADOUT,playerHp:90,enemyHp:110,opponentId:'golem',difficulty:1,playerGuard:false,enemyGuard:false,enemyWeaken:0,skipEnemy:false,seed:17389,history:[]});
export const clamp=(n,min,max)=>Math.min(max,Math.max(min,n));
export function roll(seed){const next=(Math.imul(seed,1664525)+1013904223)>>>0;return [next,next/4294967296];}
export function newGame(opponentId='golem',wins=0){const enemy=getOpponent(opponentId),level=difficulty(wins);return {...INITIAL,opponentId:enemy.id,difficulty:level,enemyHp:enemy.maxHp,cooldowns:{},loadout:{...BASE_LOADOUT},history:['Ein neuer Kampf gegen '+enemy.name+' beginnt.']};}
export function migrateSave(s){if(!s||typeof s!=='object')return null;const t={...s,cash:s.cash??START_CASH,suspicion:s.suspicion??0,cooldowns:s.cooldowns??{},loadout:normalizeLoadout(s.loadout),opponentId:s.opponentId??'golem',difficulty:s.difficulty??1,enemyWeaken:s.enemyWeaken??0,skipEnemy:s.skipEnemy??false};return validSave(t)?t:null;}
function deriveStatsSafeMax(loadout){return validateLoadout(loadout)?deriveStats(loadout).maxHp:90;}
export function validSave(s){return Boolean(s&&validateLoadout(s.loadout)&&Number.isInteger(s.cash)&&s.cash>=0&&s.cash<=1000&&Number.isInteger(s.suspicion)&&s.suspicion>=0&&s.suspicion<=100&&s.cooldowns&&typeof s.cooldowns==='object'&&!Array.isArray(s.cooldowns)&&Object.keys(s.cooldowns).length<=100&&Object.entries(s.cooldowns).every(([k,v])=>/^[a-z][a-z0-9_+-]{0,70}$/.test(k)&&Number.isInteger(v)&&v>=0&&v<=10020)&&Number.isInteger(s.round)&&s.round>=1&&s.round<=10000&&['player','enemy','over'].includes(s.phase)&&Number.isInteger(s.playerHp)&&s.playerHp>=0&&s.playerHp<=deriveStatsSafeMax(s.loadout)&&Number.isInteger(s.enemyHp)&&s.enemyHp>=0&&s.enemyHp<=getOpponent(s.opponentId).maxHp&&Number.isInteger(s.seed)&&s.seed>=0&&s.seed<=4294967295&&typeof s.playerGuard==='boolean'&&typeof s.enemyGuard==='boolean'&&(s.enemyWeaken===undefined||(Number.isInteger(s.enemyWeaken)&&s.enemyWeaken>=0&&s.enemyWeaken<=20))&&(s.skipEnemy===undefined||typeof s.skipEnemy==='boolean')&&Array.isArray(s.history)&&s.history.length<=12&&s.history.every(v=>typeof v==='string'&&v.length<250));}
function record(s,message){return {...s,history:[...s.history,message].slice(-10)};}
export function playerTurn(s,action){
 if(s.phase!=='player'||!['attack','guard','cheat'].includes(action)&&!ALL_CHEATS.some(d=>d.id===action))return s;
 if(action==='cheat'||findCheat(action)){
  const definition=ALL_CHEATS.find(d=>d.id===(action==='cheat'?'brick':action));
  if(!definition)return s;
  const [seed,random]=roll(s.seed);const result=applyCheat(s,definition,random);return result===s?s:{...result,seed};
 }
 let t={...s,phase:'enemy',playerGuard:false};let rng;t.seed=(rng=roll(t.seed))[0];
 if(action==='attack'){const damage=12+Math.floor(rng[1]*9)+deriveStats(s.loadout).attack;t.enemyHp=clamp(t.enemyHp-Math.max(1,damage-getOpponent(s.opponentId).defense),0,getOpponent(s.opponentId).maxHp);t=record(t,'Kanalratte trifft für '+damage+' Schaden.');}
 if(action==='guard'){t.playerGuard=true;t=record(t,'Kanalratte geht in Deckung.');}

 if(t.enemyHp===0){t.phase='over';t=record(t,'Sieg! Der Beton-Golem ist besiegt.');}
 else if(t.suspicion>=100){t.phase='over';t=record(t,'Polizeirazzia! Du wurdest enttarnt.');}
 else if(t.playerHp===0){t.phase='over';t=record(t,'Niederlage durch gescheiterten Betrug.');}
 return t;
}
export function enemyTurn(s){
 if(s.phase!=='enemy')return s;
 if(s.skipEnemy){return record({...s,skipEnemy:false,enemyWeaken:0,phase:'player',round:s.round+1},'Beton-Golem ist abgelenkt und setzt aus.');}
 let t={...s},rng;[t.seed,rng]=roll(t.seed);
 const response=chooseCounter(t,getOpponent(t.opponentId),rng);
 const base=Math.max(1,Math.round(response.damage*(t.difficulty||1))-(t.enemyWeaken||0)-deriveStats(s.loadout).defense),damage=t.playerGuard?Math.ceil(base*0.35):base;
 if(response.type==='expose')t.suspicion=clamp(t.suspicion+response.heat,0,100);
 t.playerHp=clamp(t.playerHp-damage,0,deriveStats(t.loadout).maxHp);
 t=record(t,response.message+' '+damage+' Schaden'+(s.playerGuard?' (Deckung)':'')+'.');
 t.playerGuard=false;t.enemyWeaken=0;
 if(t.playerHp===0){t.phase='over';t=record(t,'Niederlage! Kanalratte ist kampfunfähig.');}
 else {t.phase='player';t.round+=1;}
 return t;
}
