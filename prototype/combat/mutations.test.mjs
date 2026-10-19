import test from 'node:test';import assert from 'node:assert/strict';
import {BASE_LOADOUT,PARTS,SLOT_TYPES,validateLoadout,mutate,deriveStats,appearance} from './mutations.mjs';
test('every part belongs to a known slot',()=>{for(const p of PARTS)assert.ok(SLOT_TYPES.includes(p.slot));});
test('base loadout is valid',()=>assert.ok(validateLoadout(BASE_LOADOUT)));
test('rejects wrong slot and leaves original unchanged',()=>{const base={...BASE_LOADOUT};assert.equal(mutate(base,'head','steel-arms'),base);assert.deepEqual(base,BASE_LOADOUT);});
test('mutations change stats and appearance',()=>{const l=mutate(BASE_LOADOUT,'arms','claws');assert.equal(deriveStats(l).attack,5);assert.equal(appearance(l).arms,'claws');});
test('all parts produce valid loadouts',()=>{for(const p of PARTS){const l=mutate(BASE_LOADOUT,p.slot,p.id);assert.ok(validateLoadout(l));assert.ok(deriveStats(l).maxHp>=90);}});
