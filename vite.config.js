import {defineConfig} from 'vite';import {resolve} from 'node:path';
export default defineConfig({base:'./',build:{rollupOptions:{input:{main:resolve('index.html'),combat:resolve('prototype/combat/index.html'),benchmark:resolve('prototype/3d/index.html')}}}});
