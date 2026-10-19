# P4 — Modularer Mutationsbaukasten (Stand)

Implementiert: 8 Körper-Slots (Kopf, Arme, Beine, Haut, Schwanz, Hörner, Zusatzarme, Flügel), datengetriebene Teile und Stats, drei Spezies-Templates, Savegame-Migration, einfache Gelenkanimation, lokale optionale glTF-Teile über Three.js GLTFLoader/SkeletonUtils/AnimationMixer, Fallback-Meshes und dynamisches Nachladen bei Mutation.

**Noch offen, auch unabhängig von Tests:** Lizenzgeprüfte echte modulare GLB-Assets fehlen. Der optionale Loader nutzt `prototype/combat/assets/characters/<part-id>.glb`, derzeit ohne gelieferte Dateien. Komplexe Skinned-Mesh-Teile brauchen gemeinsame Skeleton-/Bone-Konventionen, damit sie nahtlos auf einem Rig animieren; aktuelle glTF-Anhänge sind an Gelenkpivots befestigt, kein universelles Retargeting. Keine Freigabe als vollständiges P4.

Testlauf (noch nicht durchgeführt): `npm test`. Smartphone- und Build-Abnahme offen.
