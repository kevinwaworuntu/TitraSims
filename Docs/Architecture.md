# TitraSims — Architecture

The single reference for this codebase: the runtime layers, the flow from menu
to a completed tahapan, the touch system, and the conventions you need to follow
to add content.

---

## Table of Contents

1. [Project facts](#project-facts)
2. [Repository layout](#repository-layout)
3. [Runtime architecture](#runtime-architecture)
4. [Application flow](#application-flow)
5. [Progression & persistence](#progression--persistence)
6. [AR content pipeline](#ar-content-pipeline)
7. [The tahapan interaction system](#the-tahapan-interaction-system)
8. [Stage managers](#stage-managers)
9. [Touch & gesture system](#touch--gesture-system)
10. [UI system](#ui-system)
11. [Audio](#audio)
12. [Data assets](#data-assets)
13. [Scene & prefab conventions](#scene--prefab-conventions)
14. [Editor tooling](#editor-tooling)
15. [Recipes](#recipes)
16. [Open backlog](#open-backlog)

---

## Project facts

| | |
|---|---|
| Unity | `6000.4.3f1` |
| Render pipeline | URP `17.4.0` (`Assets/Settings/Mobile_RPAsset.asset`) |
| AR | Vuforia Engine `11.4.4` (local tarball in `Packages/`) |
| Input | Input System `1.19.0` (`EnhancedTouch`); legacy `Input` in a few older scripts |
| Tweening | DOTween (`Assets/Plugins/Demigiant`) |
| Text | TextMesh Pro |
| Target | Android, min SDK 29, ARM64, IL2CPP |
| App id | `com.Unpad.TitraSims`, bundle version `2.0` |
| Build scene | `Assets/_TitraSims/Scenes/ARPharma_Kevin.unity` (the only enabled entry in Build Settings) |

The app is **single-scene**. Everything — menu, mode selection, AR session, all
twenty tahapan — happens inside `ARPharma_Kevin.unity`; there are no scene loads
at runtime. Stage content is instantiated as prefabs under Vuforia ImageTargets
and destroyed when the tahapan ends.

Also in `Packages/`: AR Foundation + ARCore `6.4.2` and XR Interaction Toolkit
`3.4.0`. Neither drives the shipping flow — Vuforia handles tracking, and the
project's own `InteractionLogic` components handle touch (XRI was evaluated and
not adopted). XRI is present as sample content only.

---

## Repository layout

```
Assets/
  _TitraSims/            ← everything the project owns
    Scenes/              ARPharma_Kevin.unity is the build scene; the rest are scratch
    Scripts/             see the breakdown below
    Prefabs/
      Stage/             Stage_TBA_01..10, Stage_TK_01..10 — one per tahapan
      Anim*.prefab       older animation-only prefabs
      GameManager.prefab, ContextualButtonController.prefab, …
    Data/                ScriptableObject instances (Audio/, Config/, Data/TBA, Data/TK)
    Animations/          AnimatorControllers + clips
    Art/                 models, materials, textures
    SFX/                 audio clips
    Editor/              editor windows and maintenance tools
  Editor/Vuforia/        ImageTarget source textures + printables
  Resources/             VuforiaConfiguration.asset, DOTweenSettings.asset
  Settings/              URP assets, input actions, build profiles
  Plugins/Demigiant/     DOTween
  ExternalResources/     TextMesh Pro + XRI samples (vendor)
  _External/             third-party content packs
  _Recovery/             recovered scene backups — not part of the build
Docs/                    this file, plus the storyboard / timeline / QA source files
README.md                entry point and setup instructions
```

`Assets/_TitraSims/Scripts/` is organized by role:

| Folder | Contains |
|---|---|
| `Core/` | `GameManager`, `UIManager`, `AudioManager`, `SFXChannel`, `ARContentManager` |
| `Gameplay/` | Tahapan state machine + the specialised stage managers |
| `InteractionLogic/` | Touch/gesture components — see [Touch & gesture system](#touch--gesture-system) |
| `UI/`, `UI/Button/` | Panels, presenter, button components |
| `Data/` | ScriptableObject definitions + `InfoTextBank` |
| `Config/` | `AnimationConfig`, `InfoStyleConfig` |
| `Animation/` | Animation-event notify relays |
| `Utility/` | `UIAnimator`, `LabelHelper`, `ScreenTapDetection`, debug cursor |
| `Enum/` | `GameMode`, `PanelType` |

---

## Runtime architecture

Four layers. Dependencies point downward; the layer below never references the
layer above by type.

```
   ┌──────────────────────────────────────────────────────────────┐
   │  SESSION                                                     │
   │  GameManager   — mode, progression, marker spawn/despawn     │
   │  UIManager     — panel stack, shared AR buttons              │
   │  AudioManager  — SFX channel + narration channel             │
   │  (GameManager and AudioManager are DontDestroyOnLoad)        │
   └───────────────┬──────────────────────────────────────────────┘
                   │  C# events (OnModeSet, OnTahapStarted, …)
   ┌───────────────▼──────────────────────────────────────────────┐
   │  STAGE  — lives on the spawned Stage_* prefab                │
   │  ARContentManager                                            │
   │    • owns the Vuforia found/lost edge for this spawn         │
   │    • drives the shared Lanjut / Selesai buttons              │
   │  ↳ BuretteFill… / GelasUkurFill… / Timbangan… / Penambahan…  │
   └───────────────┬──────────────────────────────────────────────┘
                   │
   ┌───────────────▼──────────────────────────────────────────────┐
   │  STEP                                                        │
   │  TahapanInteractionController  — step index state machine    │
   │  TahapanInteractionPlayer      — plays clip + narration      │
   │  TahapanInteractionUI          — mirrors state onto panels   │
   │  TahapanInteractionData (SO)   — title/description/audio/clip│
   └───────────────┬──────────────────────────────────────────────┘
                   │
   ┌───────────────▼──────────────────────────────────────────────┐
   │  INPUT / PRESENTATION                                        │
   │  GestureController → ObjectManipulator, SnapZone, Swirl…     │
   │  UIAnimator (DOTween helpers), ContextualButtonController    │
   └──────────────────────────────────────────────────────────────┘
```

### Singletons

| Class | Lives on | `DontDestroyOnLoad` |
|---|---|---|
| `GameManager` | `GameManager.prefab`, instanced in the scene | yes |
| `AudioManager` | scene GameObject | yes |
| `UIManager` | scene GameObject | no |
| `ContextualButtonController` | inside `CanvasContextualAction.prefab` | no |
| `ScreenTapDetection` | optional helper | no |
| `GestureController` | scene GameObject | no |

`GameManager` is deliberately event-only in the upward direction — it fires
`OnModeSet`, `OnTahapStarted`, `OnTahapCompleted` and `OnProgressReset`, and
never holds a reference to `UIManager` or any button. Subscribers wire
themselves up in `OnEnable` and unsubscribe in `OnDisable`/`OnDestroy`.

---

## Application flow

```
HomePage
  └─ Start ─────────────► ModeSelection
                            └─ TBA / Kompleksometri
                                 │  MainMenu.SetGameModeTBA()
                                 ▼
                            GameManager.SetMode(mode)
                                 │  OnModeSet(mode, lastCompleted)
                                 ▼
                            PanelTBA / PanelTK — list of 10 tahapan buttons
                                 │  TahapanControllerButton.OnTahapClicked()
                                 ▼
                            GameManager.StartTahap(index)
                                 ├─ IsTahapUnlocked?           → bail if locked
                                 ├─ resolve marker + prefab    → bail if unmapped
                                 ├─ enable Vuforia camera
                                 ├─ arm that ImageTarget's observer
                                 └─ Instantiate(prefab, imageTarget)
                                 │  OnTahapStarted
                                 ▼
                            PanelScanAR  ("arahkan kamera ke marker")
                                 │  Vuforia reports TRACKED
                                 ▼
                            ARContentManager.OnTargetFound()
                                 └─ TahapanInteractionController.StartInteraction()
                                        step 0 → step 1 → … → step N
                                 │  OnInteractionComplete
                                 ▼
                            "Selesai" → GameManager.CompleteCurrentTahap()
                                 ├─ destroy spawned content
                                 ├─ disarm observer, disable AR camera
                                 ├─ persist progress
                                 └─ OnTahapCompleted → UIManager.GoBack()
```

The Back button short-circuits this: while `PanelScanAR` is active it calls
`GameManager.BackFromCurrentTahap()` (destroy + disarm + camera off, **no**
progress write) before the normal `UIManager.GoBack()`.

---

## Progression & persistence

Progress is a single integer per mode — the index of the last completed tahapan
— stored in `PlayerPrefs`:

| Key | Mode |
|---|---|
| `LastCompletedTahapTBA` | `GameMode.TBA` |
| `LastCompletedTahapKomp` | `GameMode.Kompleksometri` |

Default is `-1` (nothing completed). Unlock rule:

```csharp
tahapIndex <= GetLastCompletedTahapIndex(mode) + 1
```

so exactly one tahapan ahead of your progress is playable. `CompleteCurrentTahap`
only ever raises the stored value, so replaying an earlier tahapan cannot
regress it. `ResetAllProgress()` deletes both keys and fires `OnProgressReset`.

### Development override

`GameManager.usePlayerPrefsForProgress` (Inspector) switches the model:

- **enabled** (default, ship builds) — the sequential `PlayerPrefs` rule above.
- **disabled** — `PlayerPrefs` is ignored entirely. Unlock comes from the
  bitmasks `devUnlockedMaskTBA` / `devUnlockedMaskKomp`, where bit *N* set means
  tahapan *N* is enterable. In this mode `SetLastCompletedTahapIndex` refuses to
  write and logs a warning.

Edit the masks through **TitraSims ▸ Tahap Progress Debug** rather than typing
integers.

---

## AR content pipeline

Each mode has two parallel arrays on `GameManager`, indexed by tahapan:

| Field | Holds |
|---|---|
| `markerTBA[]` / `markerTK[]` | the scene's ImageTarget GameObjects (`TBA_Marker1…10`, `TK_Marker1…10`) |
| `markerTBAPrefabsMapping[]` / `markerTKPrefabsMapping[]` | the `Stage_*` prefab to spawn on that target |

`StartTahap` does three things **in this order**, and the order matters:

1. `SetARCameraActive(true)` — enables `VuforiaBehaviour`.
2. `SetObserverEnabled(target, true)` — arms that one ImageTarget's observer.
3. `Instantiate(prefab, target.transform, false)` at local identity.

Vuforia's found/lost callback is **edge-triggered** and its state lives on the
ImageTarget, which outlives the spawned content. So observers are disarmed on
exit (`DisarmCurrentMarker`) and re-armed on entry — otherwise a target still
being tracked when the previous tahapan ended never produces a fresh "found"
edge for the next one.

The mirror of this lives in `ARContentManager`:

- `OnEnable` sets `isTargetFound = false`, so every spawn computes its own edge
  instead of inheriting the ImageTarget's shared handler state.
- `Start` seeds from `observerBehaviour.TargetStatus.Status` — *after* every
  subclass `OnEnable` has run — so a target already tracked at spawn time still
  starts the interaction.
- `EvaluateTargetStatus` treats `TRACKED` and `EXTENDED_TRACKED` as found,
  mirroring the ImageTargets' `Tracked_ExtendedTracked` configuration.

### Markers

The twenty markers are Vuforia **Instant Image Targets** (`mImageTargetType: 3`)
built straight from textures in the project — there is no device database to
maintain. Sources live in `Assets/_TitraSims/Art/Markers/V2/` as `tba1.png` …
`tba10.png` and `tk1.png` … `tk10.png`, and each ImageTarget's `mTrackableName`
matches its file name. Default physical size is 0.2 m square.

The Vuforia license key sits in `Assets/Resources/VuforiaConfiguration.asset`.
`Assets/Editor/Vuforia/` holds only Vuforia's own sample targets, not this
project's.

---

## The tahapan interaction system

A tahapan is an ordered list of **steps**. `TahapanInteractionController` holds
that list as `interactionDataActionMappings[]`, where each entry is:

| Field | Meaning |
|---|---|
| `InteractionData` | the `TahapanInteractionData` asset — title, description, narration clip, animation clip |
| `IsAutoContinue` | advance by itself once playback finishes (after `AnimationConfig.InteractionEndDelay`) |
| `IsContinueByOtherEvent` | **do nothing** when playback finishes — some gameplay script must call in. Defaults to `true` |
| `UniqueEvent` | `UnityEvent` fired on entering the step; used to arm props, spawn contextual buttons, etc. |

### State machine

```
EnterInteractionState(i)
   guard: index in range, data assigned, not already playing
   isPlaying = true
   OnInteractionEnter(data, hasAnimationClip)   → TahapanInteractionUI
   ExecuteInteractionState()
       mapping.UniqueEvent.Invoke()
       player.Play(data, onFinished)
                    │
                    ├─ IsContinueByOtherEvent → return, wait for gameplay
                    ├─ !IsAutoContinue        → OnStartWaitingForPlayerInputToContinue
                    │                            (ARContentManager shows "Lanjut")
                    └─ IsAutoContinue         → wait InteractionEndDelay, then exit
ExitInteractionState()
   isPlaying = false; index++
   OnFinishPlayingInteraction
   if index >= count → OnInteractionComplete   (ARContentManager shows "Selesai")
```

`ContinueInteraction()` re-enters at the current index, which after an exit is
already the *next* step — that is how the chain advances.

### Events

| Event | Consumer |
|---|---|
| `OnInteractionEnter(data, hasAnimation)` | `TahapanInteractionUI` — fills the narration panel, shows/hides play & stop buttons |
| `OnStartWaitingForPlayerInputToContinue` | `ARContentManager` — shows **Lanjut**, wires its click |
| `OnFinishPlayingInteraction` | `ARContentManager` — calls `ContinueInteraction()` |
| `OnInteractionComplete` | `ARContentManager` — shows **Selesai**, wires `CompleteCurrentTahap` |

### Playback

`TahapanInteractionPlayer` is a plain C# class (not a MonoBehaviour) that
borrows the controller as its coroutine runner. It plays the step's animation
clip and narration clip in parallel and only calls back once **both** have
finished.

Two details worth knowing:

- It builds a **per-instance** `AnimatorOverrideController` copy at `Initialize`
  time and swaps the step's clip into the generic entry slot named by
  `AnimationConfig`. It never mutates the shared asset.
- `Play` and `Stop` both cancel any running coroutines first, so re-playing a
  step cannot leave a ghost callback from the previous run.

`AnimationConfig` (a ScriptableObject on `GameManager`) supplies the animator
parameter names (`PlayAnimation` / `StopAnimation`), the generic override
controller, the clip-entry slot names, and `InteractionEndDelay`.

---

## Stage managers

`ARContentManager` is the base. Stages that need bespoke simulation subclass it
and override `OnEnable` / `OnDisable` /
`OnStartWaitingForPlayerInputToContinueHandler` — **always calling `base.`**,
since the base owns observer subscription and the shared buttons.

| Manager | Simulates | Config asset |
|---|---|---|
| `ARContentManager` | narration + animation only | — |
| `BuretteFillInteractionManager` | burette drain, meniscus, drop-by-drop titration, endpoint colour change, sample weight readout | `BuretteStageConfig` |
| `GelasUkurFillInteractionManager` | measuring-cylinder fill, liquid transfer between containers | `GelasUkurStageConfig` |
| `TimbanganInteractionManager` | analytical balance, powder added in `StepAmountMg` increments toward `TargetWeight` | `TimbanganStageConfig` |
| `PenambahanIndikator` | counted indicator drops | — |

Which manager sits on which stage prefab:

| Tahapan | TBA prefab | Manager | TK prefab | Manager |
|---|---|---|---|---|
| 1 Preparasi Alat dan Bahan | `Stage_TBA_01_PersiapanAlat` | base | `Stage_TK_01_PersiapanAlat` | base |
| 2 Preparasi Buret | `Stage_TBA_02_PersiapanBuret` | base | `Stage_TK_02_PersiapanBuretEDTA` | base |
| 3 Preparasi Larutan Baku Primer | `Stage_TBA_03_PersiapanLarutanBaku` | base | `Stage_TK_03_PersiapanLarutanBakuPrimer` | base |
| 4 Preparasi Larutan Blanko | `Stage_TBA_04_PreparasiLarutanBlanko` | GelasUkur | `Stage_TK_04_PersiapanBuffer` | GelasUkur + Indikator |
| 5 Penimbangan Sampel | `Stage_TBA_05_PenimbanganSampel` | Timbangan | `Stage_TK_05_PenimbanganSampel` | Timbangan |
| 6 Preparasi Larutan Sampel | `Stage_TBA_06_PreparasiLarutanSampel` | GelasUkur | `Stage_TK_06_PersiapanLarutanSampel` | GelasUkur |
| 7 Penambahan Indikator | `Stage_TBA_07_PenambahanIndikator` | Indikator | `Stage_TK_07_PenambahanIndikator` | base |
| 8 Simulasi Pembakuan Triplo | `Stage_TBA_08_SimulasiPembakuanSecaraTriplo` | Burette | `Stage_TK_08_SimulasiPembakuanSecaraTriplo` | Burette |
| 9 Simulasi Titrasi Blanko | `Stage_TBA_09_SimulasiTitrasiBlanko` | Burette | `Stage_TK_09_TitrasiBlanko` | Burette |
| 10 Simulasi Titrasi Sampel Triplo | `Stage_TBA_10_TirasiSampel` | Burette | `Stage_TK_10_SimulasiTirasiSampelSecaraTriplo` | Burette |

Supporting components used inside stage prefabs:

| Component | Role |
|---|---|
| `AnimNotify` | relays an animation event as `OnNotify(string)`; listeners filter by name |
| `CairanManipulator` | tweens a liquid renderer's fill/colour, driven by a named notify |
| `BuretteFillController` | maps a 0–1 fill value onto the burette's scale + meniscus transform |
| `SetCairanErlenmeyer` / `ObjectColorChanger` | one-shot colour/fill setters, usually called from `UniqueEvent` |
| `TriggerEventByTimer` | fires a `UnityEvent` after a delay |
| `TimbanganObject`, `TimbanganAnimNotify` | balance display and its animation hooks |

Liquid visuals are driven by hand-tuned constants (`fillObjectScaleModifier`,
`meniskusPositionModifier`, …) that live in the `*StageConfig` assets rather than
in code, so they can be re-tuned per stage without a recompile.

---

## Touch & gesture system

Everything in `Scripts/InteractionLogic/`, namespace `InteractionLogic`, built
on the Input System's `EnhancedTouch`. It replaced five fragmented legacy-input
scripts (`DragObject`, `Rotate`, `RotateObject`, `CSharpScaling`,
`OnClickForScaling`) with one pipeline.

```
   ┌──────────────────────────────────────────┐
   │           GestureController              │
   │            (one per scene)               │
   │  Reads EnhancedTouch + Mouse fallback    │
   │  Raycasts to find ObjectManipulator      │
   │  Classifies: rotate / drag / pinch       │
   └────────────┬────────────────┬────────────┘
                │                │
      ┌─────────▼────────┐  ┌────▼──────────────────┐
      │  Focused object  │  │  Public events        │
      │ ObjectManipulator│  │  (no focused object)  │
      └─────────┬────────┘  └───────────────────────┘
                │
      ┌─────────▼──────────────────────────┐
      │ SnapInteractable                   │
      │ RotationReturn / ScaleReturn       │
      └────────────────────────────────────┘

      SwirlGestureDetector and TapDetector read EnhancedTouch
      directly, independent of GestureController.
```

### Gesture map

| Touch | Action |
|---|---|
| 1 finger on object | Drag / translate the object |
| 2 fingers translating on object | Rotate the object (Y-axis by default) |
| 2 fingers spreading/pinching on object | Scale the object |
| 2 fingers translating on empty space | `OnTwoFingerDragDelta` event |
| 2 fingers spreading/pinching on empty space | `OnPinchUpdate` event (no listener yet) |
| 1 finger circular motion | `SwirlGestureDetector` |

This is the **shipped** mapping, which comes from the `GestureSettings` asset,
not from the code defaults — see below.

Finger counts are not hard-coded: a **`GestureSettings`** asset
(`TitraSims/Gesture Settings`) assigned to `GestureController._settings` sets
`rotateFingersNeeded` / `dragFingersNeeded` / `scaleFingersNeeded`.

**Watch out for this.** The field defaults in `GestureSettings.cs` are rotate 1,
drag 2, scale 2 — and those apply only when no asset is assigned. The project
ships `Assets/_TitraSims/Data/Config/GestureSettings.asset`, wired to the
`GestureController` in every scene, and it **inverts the first two**:

| | Code default | Shipped asset |
|---|---|---|
| `rotateFingersNeeded` | 1 | **2** |
| `dragFingersNeeded` | 2 | **1** |
| `scaleFingersNeeded` | 2 | 2 |

So the real behaviour is one finger to drag and two to rotate. Read the asset,
not the class, when reasoning about what a gesture does.

Rotate and scale may share a count of 2+, but rotate and drag may not — both are
midpoint travel, and drag wins.

### Minimum setup

1. Add **`GestureController`** to one persistent GameObject (e.g. `Managers`).
2. On the equipment prefab: a non-trigger **`Collider`** plus
   **`ObjectManipulator`**.
3. Press Play — in the Editor the mouse acts as a single synthetic touch.

Optional: `SnapInteractable` on the object + `SnapZone` on an empty GameObject
for snapping; `SwirlGestureDetector` for mixing.

### GestureController

**Attach to:** one persistent GameObject. Enables `EnhancedTouchSupport` itself.

| Field | Default | Description |
|---|---|---|
| `_interactableLayer` | Everything | Layers raycast when looking for an `ObjectManipulator` |
| `_raycastDistance` | `100` | Max raycast distance, world units |
| `_pinchCommitRatio` | `0.04` | Fraction of initial finger distance that must change before the gesture commits to Pinch rather than Drag |
| `_dragCommitPixels` | `6` | Screen pixels the two-touch midpoint must travel before committing to Drag rather than Pinch |

```csharp
// First touch contact lands on an ObjectManipulator. Not re-fired when a
// second finger joins an already-focused object.
event Action<ObjectManipulator> OnManipulatorGrabbed;
// All contact with a focused ObjectManipulator ends.
event Action<ObjectManipulator> OnManipulatorReleased;
// A two-touch pinch begins. Arg: initial finger distance in pixels.
event Action<float> OnPinchBegin;
// Every frame during a pinch with NO focused object. Arg: current distance.
event Action<float> OnPinchUpdate;
event Action OnPinchEnd;
// Every frame during a two-finger drag with NO focused object. Arg: screen delta.
event Action<Vector2> OnTwoFingerDragDelta;
```

Two-touch → single-touch transitions (one finger lifts) do **not** re-fire
`OnManipulatorGrabbed`; the object stays continuously focused. `OnPinchBegin`
fires for all pinches including on focused objects, but `OnPinchUpdate` fires
only when nothing is focused.

### ObjectManipulator

**Attach to:** any interactive lab-equipment GameObject. Needs a `Collider`
somewhere in its hierarchy for the raycast.

| Section | Field | Default | Description |
|---|---|---|---|
| Permissions | `canRotate` | `true` | Allow rotation |
| | `canDrag` | `false` | Allow drag |
| | `canScale` | `true` | Allow pinch-scale |
| Rotate | `_rotateSensitivity` | `0.3` | Degrees per screen pixel |
| | `rotateAxis` | `(0,1,0)` | Axis to rotate around |
| Drag | `_dragSensitivity` | `0.005` | World units per screen pixel |
| | `lockToHorizontalPlane` | `false` | Constrain to the XZ plane |
| | `lockDragX` / `lockDragY` / `lockDragZ` | `false`/`false`/`true` | Freeze individual sliding axes |
| Scale | `scaleRange` | `(0.3, 3)` | Min/max as a multiplier of the **original** local scale |

```csharp
void ReceiveRotateDelta(Vector2 screenDelta);  // swipe (2 fingers as shipped)
void ReceiveDragDelta(Vector2 screenDelta);    // translation (1 finger as shipped)
void ReceivePinchDelta(float scaleFactor);     // pinch (2 fingers)
```

These are `public` so other components can drive them directly. `scaleRange` is
always relative to the scale captured in `Awake`, never the current scale, so
pinching cannot walk an object out of bounds. `canDrag`/`canRotate`/`canScale`
are plain public fields that components like `SnapInteractable` write to in
order to take and restore control.

#### Drag-axis locking — why it works the way it does

This took three passes to get right; the reasoning matters if you touch it.

`lockDragX/Y/Z` freeze a sliding axis. The obvious implementation — raycast a
camera-facing plane, then clamp the locked coordinate afterwards — does not work
in AR. The drag plane (`-cam.transform.forward`) is tilted relative to the axes
for almost any camera angle, so snapping one coordinate back couples into the
other two and reads as diagonal drift.

**2026-07-18** — `ResolveDragPlaneNormal()` picks the raycast plane's normal to
match the single locked axis (e.g. `Vector3.forward` when only Z is locked), so
the axis is fixed by the plane itself and no post-hoc clamp is needed. Falls
back to the camera-facing plane when zero or more than one axis is locked;
`lockToHorizontalPlane` still takes priority.

**2026-08-07** — the common case here is the reverse: two axes locked, one free
(a burette clamp that only slides on Y). No plane can represent pure
single-axis movement without a lossy step — raycasting a 2D plane and discarding
two coordinates still leaves the remaining coordinate's sensitivity and apparent
direction dependent on how tilted that plane was relative to the camera, so it
felt wrong depending on where you stood. `ClosestPointOnAxis()` replaces the
plane for that case: it finds the closest point between the camera's screen ray
and the free axis's line — the technique transform gizmos use. Every point it
returns lies exactly on that axis line through the drag-start position, so
movement is pure single-axis by construction. `TryGetSingleFreeAxis()` (via
`GetEffectiveLocks()`) routes between the two paths: exactly one free axis →
`ClosestPointOnAxis`; zero or 2+ free axes → the plane path.

**2026-08-07, later** — all of the above ran in world space and wrote
`transform.position`. These objects are parented under AR image-target anchors
whose world transform is arbitrary, so "world axis" never meant a meaningful
direction relative to the apparatus — only relative to wherever tracking started.
`lockDragX/Y/Z` are meant to describe the object's *own* sliding directions,
which is a parent-relative concept. `ToLocalRay()` now converts the screen ray
into the parent's local space up front, every downstream calculation runs in
that frame, and the result is written to `transform.localPosition`. A straight
line stays straight under this change even with non-uniform parent scale, so
nothing else needed to change. Objects with no parent are unaffected.

> If a new lock combination produces skewed or angle-dependent movement, check
> whether it is landing in the plane path when it should be axis-constrained.
> `GetEffectiveLocks()` is the single source of truth both paths read from.

### SnapZone / SnapInteractable

Paired components: `SnapZone` marks a destination, `SnapInteractable` is the
object that snaps into it (and requires `ObjectManipulator`).

**SnapZone** — attach to an empty GameObject at the exact pose the snapped
object should occupy.

| Field | Default | Description |
|---|---|---|
| `acceptTag` | `""` | Only accept objects with this tag; empty accepts any |
| `_snapRadius` | `0.1` | Snap radius in world units (yellow Gizmo sphere) |
| `snapRotation` | `true` | Align the snapped object's rotation to the zone |
| `_highlightVisual` | — | Shown while a compatible object is dragged in range |
| `_occupiedVisual` | — | Shown while an object is snapped here |

```csharp
float SnapRadius { get; }
bool  IsOccupied { get; }
bool  IsInRange(SnapInteractable candidate);
void  SetHighlight(bool on);
bool  TryAccept(SnapInteractable candidate);  // calls SnapTo(), starts the lerp
void  Release();                              // called when the object is picked up

static readonly List<SnapZone> All;  // active zones; avoids FindObjectsByType
```

**SnapInteractable**

| Field | Default | Description |
|---|---|---|
| `_snapSpeed` | `10` | Lerp speed toward the snap pose |
| `OnSnapped` / `OnUnsnapped` | — | `UnityEvent`s |

```csharp
bool     IsSnapped   { get; }
SnapZone CurrentZone { get; }
void     SnapTo(SnapZone zone);  // called by SnapZone — do not call directly
```

While dragged, the nearest compatible zone in range highlights. On release the
object lerps into it. Grabbing a snapped object unsnaps immediately, re-enables
`canDrag` and fires `OnUnsnapped`.

Typical setup — a stopper in a burette neck: tag the stopper `"Stopper"`, set
the zone's `acceptTag` to `"Stopper"`, tune `_snapRadius` against the Gizmo, and
assign a translucent ghost mesh to `_highlightVisual`.

### SwirlGestureDetector

Attach to the Erlenmeyer or anything that responds to circular mixing. Reads
`EnhancedTouch` directly, independent of `GestureController`.

Rather than tracking position around a fixed centre, it measures how much the
finger's **direction of travel** rotates. When the signed cumulative rotation
reaches `_completionAngle`, a swirl fires — so it works regardless of circle
size, start position or hand size.

```
Straight swipe → direction rotates ~15°   → no swirl
Large arc      → direction rotates ~150°  → no swirl
Full circle    → direction rotates 360°   → OnSwirl fires
```

| Field | Default | Description |
|---|---|---|
| `_completionAngle` | `360` | Cumulative rotation needed, degrees |
| `_timeWindow` | `2.5` | Max seconds to complete; resets on timeout |
| `_minSpeedPixels` | `80` | Min finger speed (px/s) to count as movement — filters drift |
| `_reverseResetDegrees` | `45` | Direction reversal beyond this resets progress |
| `OnSwirl` | — | `UnityEvent`, Inspector-wirable |
| `_showDebugGizmo` | `false` | Editor overlay showing % completion and direction |

```csharp
// Arg: seconds the swirl took — use it to scale animation speed.
event Action<float> OnSwirlDetected;
```

Progress resets after each completion but tracking continues, so continuous
stirring fires once per revolution without needing a new touch-down.

```csharp
void OnEnterMixingStep()
{
    var swirl = erlenmeyerGO.GetComponent<SwirlGestureDetector>();
    swirl.enabled = true;
    swirl.OnSwirlDetected += HandleMix;
}

void HandleMix(float duration)
{
    _mixCount++;
    if (_mixCount >= _requiredMixes) CompleteStep();
}
```

### Other components

| Component | Role |
|---|---|
| `TapDetector` | Raycasts on touch-down (not release) and fires if the ray hits this object's collider |
| `RotationReturnInteractable` | Alongside `ObjectManipulator`: slerps back to the rotation recorded at enable time once released. Cancelled while grabbed |
| `ScaleReturnInteractable` | Same, for scale |
| `PlayAnimByProgression` | Drives an Animator by normalized progression from discrete external calls |
| `PlayAnimByTouch` | Same, but progression advances continuously while this object is held, freezing on release |
| `PlayAnimOneShot`, `PlayAnimOneShot2`, `PlayAnimByTouchOneShot` | One-shot clip playback variants |
| `IAnimationPlaybackController` | Implemented by all of the above so other systems can stop/reset playback without knowing the concrete type |
| `Rotate` | Legacy Input-based rotation. Superseded, but still attached to `AnimKompleksometri10.prefab` and `Animasi.unity` |

### Migrating a legacy prefab

1. Remove `DragObject`, `Rotate`, `RotateObject`, `CSharpScaling`,
   `OnClickForScaling`.
2. Verify a `Collider` is present.
3. Add `ObjectManipulator` and configure it.
4. Add optional components (`SnapInteractable`, `SwirlGestureDetector`).
5. Ensure exactly one `GestureController` exists in the scene.

| Legacy behaviour | New equivalent |
|---|---|
| `OnMouseDrag` drag | `canDrag = true` (finger count comes from `GestureSettings`) |
| `Rotate.cs` Y rotation | `canRotate = true`, `rotateAxis = (0,1,0)` |
| `RotateObject.cs` X rotation | `rotateAxis = (1,0,0)` |
| `CSharpScaling` pinch scale | `canScale = true` |

---

## UI system

### Panels

`UIManager` maps `PanelType` → `GameObject` through a serialized
`PanelMapping[]`, cached into a dictionary at `Start`. Navigation goes through a
history stack:

- `ShowPanelAndAddToHistory(type)` pushes the current panel, then activates the
  new one (exclusively by default).
- `GoBack()` pops until it finds a real destination and falls back to
  `PanelHomePage` when the stack empties.
- `PanelType.None` (startup sentinel) and `PanelType.PanelInfo` (transient
  overlay) are never valid `GoBack` destinations and are skipped when popping.

`UIManager` also owns the buttons that are *shared* across every stage —
`btnNextInteraction`, `btnCompleteTahapan`, play/stop animation, AR next/prev —
because they live on the persistent canvas rather than on the spawned prefab.
`ARContentManager` re-wires their listeners per step (`RemoveAllListeners` then
`AddListener`).

### Panel roles

| `PanelType` | Panel |
|---|---|
| `PanelHomePage` | title screen |
| `PanelModeSelection` | TBA vs Kompleksometri |
| `PanelTBA` / `PanelTK` | the 10-tahapan list for that mode |
| `PanelScanAR` | live AR view + step controls |
| `PanelHowToPlay`, `PanelTeamInformation` | static info screens |
| `PanelInfo` | per-tahapan theory overlay (`InfoPanel`) |
| `PanelNaration` | per-step title + description (`NarationPanel`) |

`InfoPanel` text comes from `InfoTextBank`, a static class holding two parallel
`string[]` per mode (`TBA_Title`/`TBA`, `TK_Title`/`TK`) indexed by tahapan.
`NarationPanel` text comes from the step's `TahapanInteractionData`. The
narration panel auto-hides 3 s after the marker is found.

### Buttons

`TitraSims_Button` extends `UnityEngine.UI.Button` and gives every button in the
app a click SFX plus a `UIAnimator.ButtonPress` squash before the handler runs.
Subclasses override `OnClick()` rather than adding listeners.

| Button | Behaviour |
|---|---|
| `TahapanControllerButton` | one per tahapan; subscribes to `GameManager` progress events and dims itself via `CanvasGroup` when locked |
| `BackButton` | context-aware — tears the AR tahapan down when leaving `PanelScanAR` |
| `ContextualButton` | pooled; text + `Action` assigned at runtime |
| `InfoButton`, `NarationButton`, `ResetButton`, … | thin wrappers over `UIManager` / `GameManager` calls |

`ContextualButtonController` is the runtime factory: `GenerateContextualButton(n)`
spawns *n* buttons, `RegisterText`/`RegisterAction` fill them in, and
`DestroyButtons()` clears them at the end of a step. Stage managers use this for
choices that only exist for one step ("tuang 10 mL", "tambahkan 3 tetes").

`UIAnimator` is a static DOTween helper library — `PopIn`, `ShowPanel`,
`ButtonPress`, `Pulse`, `Shake`, `DrawList` and friends. Use it instead of
hand-rolling tweens so timing stays consistent.

---

## Audio

`AudioManager` owns two `AudioSource`s with independent volume:

- **SFX** — wrapped in an `SFXChannel`, played by key, by `SoundType`, or as a
  random clip of a type. `PlayNonInterrupt` drops the request if the source is
  already busy (used for marker found/lost so they cannot stack).
- **Narration** — `PlayNarration(clip)` stops whatever is playing and starts the
  new clip; `TahapanInteractionPlayer` drives it per step.

`SoundBank` is a ScriptableObject list of `{ key, soundType, priority, clip }`
with dictionary lookups rebuilt in `OnEnable`/`OnValidate`, so Inspector edits
take effect immediately. `SoundType` is the enum of well-known cues
(`UI_ButtonClick`, `Gameplay_MarkerFound`, …).

---

## Data assets

Everything authored lives in ScriptableObjects under `Assets/_TitraSims/Data/`.

| Asset | Create menu | Purpose |
|---|---|---|
| `TahapanInteractionData` | `AR Pharma/TahapanInteractionData` | one step: title, description, narration clip, animation clip |
| `AnimationConfig` | `AR Pharma/Create Animation Config` | animator parameter + clip-slot names, `InteractionEndDelay` |
| `BuretteStageConfig` | `AR Pharma/BuretteStageConfig` | burette visual constants, endpoint colours, per-step weight/floor/ceiling targets |
| `GelasUkurStageConfig` | `AR Pharma/GelasUkurStageConfig` | measuring-cylinder fill constants |
| `TimbanganStageConfig` | `AR Pharma/TimbanganStageConfig` | target weight, step increment, scale constants |
| `CairanErlenmeyerConfig` | `AR Pharma/Cairan Erlenmeyer Config` | a liquid's colour + fill level |
| `InfoTextData` | `AR Pharma/Create Info Text Data` | key→text pairs (linear lookup) |
| `InfoStyleConfig` | `AR Pharma/Create Info Style Config` | TMP styling for the info panel |
| `SoundBank` | `TitraSims/Sound Bank` | SFX registry |
| `LabelContainer` | — (singleton asset) | key→`Material` map for equipment labels |

The one exception is `InfoTextBank`, which is a hard-coded static class rather
than an asset — the per-tahapan theory text is compiled in.

---

## Scene & prefab conventions

- **Marker GameObjects** are named `TBA_Marker1…10` and `TK_Marker1…10` and carry
  Vuforia `ObserverBehaviour`, each backed by the matching `tba<N>.png` /
  `tk<N>.png` in `Art/Markers/V2/`. They stay in the scene permanently; only
  their observer is armed and disarmed.
- **Stage prefabs** are named `Stage_<MODE>_<NN>_<NamaTahapan>` and live in
  `Prefabs/Stage/`. `NN` is 1-based in the name but the array index that
  addresses it is 0-based — `Stage_TBA_01_*` sits at `markerTBAPrefabsMapping[0]`.
- A stage prefab's root carries the `ARContentManager` (or subclass) **and** the
  `TahapanInteractionController`; `[RequireComponent]` pulls in
  `TahapanInteractionUI` automatically. The `Animator` is found with
  `GetComponentInChildren`.
- Spawned content is parented to the ImageTarget at local position zero,
  identity rotation and unit scale — author prefabs around that origin.
- `Anim*.prefab` (e.g. `AnimBebasAir4`, `AnimKompleksometri10`) are the older
  animation-only prefabs that predate the stage system. Some are still
  referenced; treat them as legacy.

---

## Editor tooling

| Menu | Tool |
|---|---|
| **TitraSims ▸ Tahap Progress Debug** | live view of progression; toggles `usePlayerPrefsForProgress`, edits the dev unlock bitmasks, jumps to a tahapan |
| **TitraSims ▸ Interaction Debug** | inspects the active `TahapanInteractionController` — current step index, playing state, force-next; can spawn a tahapan without a marker |
| **TitraSims ▸ Interaction ▸ Add Return Components To All Manipulators** | idempotent project-wide pass that adds the return-to-origin components to every `ObjectManipulator`; has a preview-only variant |

`LabelHelperEditor` gives `LabelHelper` a dropdown of keys read from
`LabelContainer` instead of a raw string field.

Both debug windows compile only in the editor; the runtime hooks they use sit
inside `#if UNITY_EDITOR` blocks on `GameManager` and
`TahapanInteractionController`.

---

## Recipes

### Add a step to an existing tahapan

1. Create a `TahapanInteractionData` asset (`AR Pharma/TahapanInteractionData`)
   under `Data/Data/<MODE>/`. Fill title, description, narration clip and/or
   animation clip. Any field may be left empty.
2. Open the `Stage_*` prefab, find `TahapanInteractionController`, add an entry
   to **Interaction Data Action Mappings** and assign the asset.
3. Choose how the step ends:
   - player taps **Lanjut** → leave `IsAutoContinue` off and
     `IsContinueByOtherEvent` **off**;
   - advances by itself → `IsAutoContinue` **on**, `IsContinueByOtherEvent` off;
   - a gameplay script decides → `IsContinueByOtherEvent` **on** (the default),
     and make sure something eventually calls back in.
4. Use `UniqueEvent` for anything that must happen on entry — spawning
   contextual buttons, enabling a prop, setting a liquid colour.

### Add a whole tahapan

1. Author the `Stage_<MODE>_<NN>_<Nama>` prefab with an `ARContentManager`
   subclass + `TahapanInteractionController`.
2. Add the marker image to `Art/Markers/V2/`, then add an ImageTarget to the
   scene using it as an Instant Image Target — GameObject named to match the
   `TBA_Marker<N>` / `TK_Marker<N>` convention, trackable name matching the file.
3. On `GameManager`, append the ImageTarget to `markerTBA`/`markerTK` and the
   prefab to `markerTBAPrefabsMapping`/`markerTKPrefabsMapping` **at the same
   index**.
4. Add the tahapan's title and theory text to `InfoTextBank` at that index.
5. Add a `TahapanControllerButton` to the mode's panel with `TahapIndex` set.

### Add a sound

Add an entry to the `SoundBank` asset (key, `SoundType`, clip), then call
`AudioManager.Instance?.SFX.Play(SoundType.X)`. Add a new `SoundType` member only
if no existing category fits.

### Reset progress while testing

Either the in-app **Reset** button (`GameManager.ResetAllProgress`), or switch
`usePlayerPrefsForProgress` off in the Tahap Progress Debug window and drive
unlocks from the checkbox mask.

---

## Open backlog

Known problems, still unfixed. Check here before starting work in these areas.

### Bugs

| File | Issue |
|---|---|
| `UI/TahapanControllerButton.cs` | *(deferred)* `OnTahapClicked()` and `OnInfoButtonClicked()` both fire on the same click, so the info panel opens automatically every time a stage starts. Needs two distinct handlers. |
| `Core/UIManager.cs` | `GetPanelByType()` returns `null` for an unmapped `PanelType` and several call sites dereference it without a guard (`IsPanelInfoActive`, `IsPanelNarationActive`). Add a `TryGetPanel()` or guard the callers. |

### Design debt

| File | Issue |
|---|---|
| `Core/ARContentManager.cs` | Reaches into `UIManager` directly in a dozen places, each marked `// Todo : Revisit move to better place`. This is the largest piece of outstanding debt — the stage layer should not know about panels and buttons. |
| `Core/GameManager.cs` | `currentMode` is `public`, so it can be mutated externally and bypass `SetMode()`. Make it private with a read-only property. |
| `Core/GameManager.cs` | `PlayerPrefs` calls are inline. Wrap them in a `SaveSystem` + `SaveData` pair — but only once save data outgrows two integers (see below). |
| `Core/UIManager.cs` | `HideAllARPopups()` only hides popups and clears listeners; it does not reset AR state. Needs a real teardown. `narationButton` and `ScanMarkerTextGO` also live here but belong elsewhere. |
| `Gameplay/TahapanInteractionController.cs` | `StartInteraction()` and `ContinueInteraction()` are identical one-liners. Consolidate, or document why both names exist. |
| `Data/InfoTextBank.cs` | All theory text is hardcoded static strings, so content edits need a recompile. Migrate to ScriptableObject assets, one per mode or stage. |
| `Gameplay/BuretteFill…`, `GelasUkurFill…`, `TimbanganInteractionManager` | `isCheckWeightToContinue`, `SetIsCheckWeightToContinue`, `SetButtonEnabledState`, `PlayAnimation(AnimationClip)` and `RestartCurrentInteraction()` are copy-pasted across all three. Move the shared members up into `ARContentManager`. |
| `Core/ARContentManager.cs` | `GetComponent<DefaultObserverEventHandler>()` is called in both `OnEnable()` and `OnDisable()`. Cache it in `Awake()`. |
| `Animation/TimbanganObject.cs` | `LerpValue` has a hardcoded `0.1f` duration with two `// ToDo: Make duration listen to anim length` comments. Wire it to `AnimationConfig` or expose it. |
| `InteractionLogic/Rotate.cs` | Marked legacy and superseded by `ObjectManipulator`, but still attached to `AnimKompleksometri10.prefab` and `Animasi.unity`. Migrate those two, then delete. |

### Polish

| File | Issue |
|---|---|
| `UI/TahapanControllerButton.cs` | Unused `OnInfoButtonClicked()` — remove once the bug above is fixed. |

### Deferred by design

**Save system.** Progress is two `PlayerPrefs` integers. If it grows, the path
is: a plain `SaveData` class holding every persistent field, a static
`SaveSystem` in `Scripts/Utility/` wrapping `PlayerPrefs` behind
`Load()` / `Save(SaveData)` / `Reset()`, and a `SaveData` field on `GameManager`
loaded in `Awake()`. Do **not** refactor until something new actually needs
saving — score, timestamps, per-stage attempts.

**Navigation history.** The `Stack<PanelType>` rules are deliberate:
`PanelHomePage` is never pushed (it is the implicit floor), `PanelInfo` is never
pushed (it overlays without adding history), and every other panel pushes the
*origin* panel when navigating away.
