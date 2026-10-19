# GHETTO BEASTS: Cheat to Win

Mobile-first, satirical 3D creature-combat game. **The old 2D Canvas prototype is retired.**

## Run

```sh
npm install
npm run dev
npm run build
npm test
```

Open the Vite address; the home page forwards to the active 3D combat prototype. For performance benchmarking visit `/prototype/3d/`.

## Stack

Three.js (MIT) for 3D, Vite (MIT) for development and offline asset bundling. Planned Capacitor (MIT) mobile packaging. See [OPEN_SOURCE.md](OPEN_SOURCE.md) for license and architectural decisions.

## Status

Playable combat, cheating and modular mutation prototypes are implemented on stacked feature branches. **No successful CI/browser/phone test has been confirmed** and native Android/iOS packaging is not yet done. The game is not release-ready.

Development PR chain: #3 → #4 → #5 → #6 → this refactor.
