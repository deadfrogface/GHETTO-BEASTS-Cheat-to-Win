import test from 'node:test';
import assert from 'node:assert/strict';
import {newGame,playerTurn,enemyTurn,validSave,roll} from './rules.mjs';

test('initial game is valid and player starts',()=>{const s=newGame();assert.equal(s.phase,'player');assert.ok(validSave(s));});
test('invalid actions do not change state',()=>{const s=newGame();assert.equal(playerTurn(s,'unknown'),s);assert.equal(enemyTurn(s),s);});
test('guard reduces enemy damage',()=>{const s=newGame();const guarded=enemyTurn(playerTurn(s,'guard'));const unguarded=enemyTurn({...playerTurn(s,'guard'),playerGuard:false});assert.ok(guarded.playerHp>unguarded.playerHp);});
test('damage remains in bounds for thousands of seeds',()=>{for(let seed=0;seed<2000;seed++){let s={...newGame(),seed};for(let i=0;i<30&&s.phase!=='over';i++){s=playerTurn(s,['attack','guard','cheat'][i%3]);if(s.phase==='enemy')s=enemyTurn(s);assert.ok(validSave(s));}}});
test('same seed and actions produce identical combat',()=>{const run=()=>{let s=newGame();for(let i=0;i<20&&s.phase!=='over';i++){s=playerTurn(s,'attack');s=enemyTurn(s);}return s;};assert.deepEqual(run(),run());});
test('roll stays within unsigned integer and [0,1)',()=>{let seed=1;for(let i=0;i<1000;i++){let n;[seed,n]=roll(seed);assert.ok(n>=0&&n<1);}});
test('save validator rejects malformed payloads',()=>{assert.equal(Boolean(validSave({})),false);assert.equal(Boolean(validSave({...newGame(),playerHp:-1})),false);});
