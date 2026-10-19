# Open-Source-first — GHETTO BEASTS

**Active app:** `prototype/combat/`. Root `index.html` routes to the combat app. The original inline-Canvas 2D game and destructive `build.js` were removed on this refactor branch.

## Dependencies

- **Three.js 0.180.0**, MIT, https://github.com/mrdoob/three.js — rendering, meshes, lights, animation foundations.
- **Vite 7.1.11**, MIT, https://github.com/vitejs/vite — local development and offline-bundled production assets.
- **Capacitor**, MIT, https://github.com/ionic-team/capacitor — intended for future Android/iOS wrapping, **not yet integrated**.
- Three.js includes `GLTFLoader`, `AnimationMixer`, `SkeletonUtils` for production character assets. These are preferred to implementing custom skeletal animation infrastructure. Asset sourcing and actual use still pending.

## Development

```sh
npm install
npm run dev
npm run build
npm test
```

Bundled production output is `dist/`; the browser no longer downloads Three.js from a CDN. npm installation needs internet **once**; running the built app can be offline when served locally or packaged, but offline operation is **not yet verified**. Android/iOS builds and real device tests remain open.

## Original gameplay

The obsolete 2D prototype was deliberately removed from the active branch. It remains recoverable through Git history; do not run the former `build.js`. Bespoke code is retained only for game-specific combat, cheats, mutations, and simple placeholder meshes until appropriately licensed modular glTF assets are selected.

## License handling

Third-party packages keep their own licenses. Before shipping, include their required copyright/license notices in the distribution. Do not assume downloadable character assets are licensed for commercial reuse.
