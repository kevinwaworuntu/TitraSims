# TitraSims

AR-based interactive simulation of pharmaceutical titration procedures —
non-aqueous (*Titrasi Bebas Air*) and complexometric — built with Unity and
Vuforia for Android.

Students scan a printed marker to overlay virtual lab equipment onto a real
surface, then work through a guided, step-by-step procedure. Each mode has ten
tahapan (stages) that unlock sequentially.

---

## Getting started

**Requirements**

- Unity `6000.4.3f1` (Android Build Support + IL2CPP)
- A Vuforia Engine developer license
- An ARCore-capable Android device (min SDK 29)

**Open the project**

1. Clone, then open the folder in Unity Hub with `6000.4.3f1`. The Vuforia
   package installs itself from `Packages/com.ptc.vuforia.engine-11.4.4.tgz` on
   first load — let it finish and restart the editor if prompted.
2. Open `Assets/_TitraSims/Scenes/ARPharma_Kevin.unity`. This is the only scene
   in the build; everything runs inside it.
3. Paste your Vuforia license key into
   `Assets/Resources/VuforiaConfiguration.asset` if the project has not been set
   up on this machine before.
4. Press Play. The twenty markers to point the camera at are the PNGs in
   `Assets/_TitraSims/Art/Markers/V2/` (`tba1`–`tba10`, `tk1`–`tk10`) — print
   them or display them on a second screen. They are Instant Image Targets, so
   there is no device database to import.

**Build**

Target Android / ARM64 / IL2CPP. App id is `com.Unpad.TitraSims`.

**Testing without a marker**

Use **TitraSims ▸ Interaction Debug** to spawn a tahapan directly, and
**TitraSims ▸ Tahap Progress Debug** to unlock stages without playing through
them.

---

## Documentation

| Document | For |
|---|---|
| [`Docs/Architecture.md`](Docs/Architecture.md) | **Developers.** The single reference for this codebase — runtime layers, app flow, progression, the AR content pipeline, stage managers, the touch/gesture system, recipes for adding content, and the open backlog |
| [`Docs/UserGuide.md`](Docs/UserGuide.md) | **Students.** Installing, scanning markers, the gestures, what each of the twenty tahapan asks for, recording burette volumes, troubleshooting |
| [`Docs/InstructorGuide.md`](Docs/InstructorGuide.md) | **Lecturers and lab staff.** Device requirements, deploying the APK, printing markers, room setup, how progression locking works, running a session, reporting problems |

Both user guides are written in English and quote the app's Indonesian on-screen
labels verbatim, so readers can match the two.

Also in `Docs/`: the storyboard spreadsheet, the proposed timeline, and QA
feedback for both modes.

Earlier docs (`PRD.md`, `CHANGELOG.md`, `REFACTOR.md`, `TitraSims.md`,
`Docs/InteractionSystem.md`, `Docs/XRI-AR-Components.md`) were consolidated into
`Architecture.md` and removed; they remain in git history if you need the
product requirements or the per-session refactor log.

---

## Layout at a glance

```
Assets/_TitraSims/     everything the project owns
  Scenes/              ARPharma_Kevin.unity is the build scene
  Scripts/             Core, Gameplay, InteractionLogic, UI, Data, Config, Utility
  Prefabs/Stage/       Stage_TBA_01..10 and Stage_TK_01..10 — one per tahapan
  Data/                ScriptableObject assets (steps, stage configs, sounds)
  Art/ Animations/ SFX/
Docs/                  documentation
```

See [`Docs/Architecture.md`](Docs/Architecture.md#repository-layout) for the
full tree.
