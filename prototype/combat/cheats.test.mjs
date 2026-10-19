import test from 'node:test';import assert from 'node:assert/strict';
import {CHEAT_DEFS,ALL_CHEATS,validateCatalog,validateDef,combinations,applyCheat} from './cheats.mjs';
import {newGame} from './rules.mjs';
test('catalog and generated combinations are valid',()=>{assert.ok(validateCatalog(CHEAT_DEFS));assert.ok(validateCatalog(ALL_CHEATS));assert.ok(combinations().length>0);});
test('rejects invalid definitions and effects',()=>{assert.equal(validateDef({...CHEAT_DEFS[0],chance:2}),false);assert.equal(validateDef({...CHEAT_DEFS[0],effects:[{type:'exec',amount:1}]}),false);});
test('deterministic result and no state mutation',()=>{const s=newGame();const before=structuredClone(s);assert.deepEqual(applyCheat(s,CHEAT_DEFS[0],0.1),applyCheat(s,CHEAT_DEFS[0],0.1));assert.deepEqual(s,before);});
test('generated cheats never overflow hp',()=>{for(const d of ALL_CHEATS){for(const roll of [0,0.5,0.999]){const s=applyCheat(newGame(),d,roll);assert.ok(s.playerHp>=0&&s.playerHp<=90&&s.enemyHp>=0&&s.enemyHp<=110);}}});
test('skip, weaken and fail have bounded effects',()=>{const s=applyCheat(newGame(),CHEAT_DEFS[2],0);assert.equal(s.skipEnemy,true);const f=applyCheat(newGame(),CHEAT_DEFS[0],0.99);assert.equal(f.playerHp,82);});
