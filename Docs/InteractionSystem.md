# TitraSims — Interaction System

Reference for the gesture-driven interaction system built in `Assets/_TitraSims/Scripts/InteractionLogic/`.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Quick Start](#quick-start)
4. [Component Reference](#component-reference)
   - [GestureController](#gesturecontroller)
   - [ObjectManipulator](#objectmanipulator)
   - [SnapZone / SnapInteractable](#snapzone--snapinteractable)
   - [AutoRotateZone](#autorotatezone)
   - [CameraZoomController](#camerazoomcontroller)
   - [SwirlGestureDetector](#swirlgesturedetector)
   - [DragDetach](#dragdetach)
5. [Component Combinations](#component-combinations)
6. [Migration from Legacy Scripts](#migration-from-legacy-scripts)

---

## Overview

The interaction system replaces five fragmented Legacy-Input scripts (`DragObject`, `Rotate`, `RotateObject`, `CSharpScaling`, `OnClickForScaling`) with a unified, New-Input-System pipeline.

**All scripts are in `namespace InteractionLogic` and require `UnityEngine.InputSystem`.**

| Old script | Replaced by |
|---|---|
| `DragObject.cs` | `ObjectManipulator` + `GestureController` |
| `Rotate.cs` | `ObjectManipulator` + `GestureController` |
| `RotateObject.cs` | `ObjectManipulator` + `GestureController` |
| `CSharpScaling.cs` | `ObjectManipulator` + `GestureController` |
| `OnClickForScaling.cs` | `ObjectManipulator` + `GestureController` |

---

## Architecture

```
                    ┌──────────────────────────────────────────┐
                    │           GestureController               │
                    │            (Singleton)                    │
                    │                                           │
                    │  Reads EnhancedTouch + Mouse fallback     │
                    │  Raycasts to find ObjectManipulator       │
                    │  Classifies: rotate / drag / pinch        │
                    └────────────┬────────────────┬─────────────┘
                                 │                │
              ┌──────────────────▼──┐    ┌────────▼─────────────┐
              │  Focused Object     │    │  Public Events        │
              │  ObjectManipulator  │    │  (no focused object)  │
              └──────────────────┬──┘    └────────┬─────────────┘
                                 │                │
          ┌──────────────────────┤                ├──────────────────────┐
          │                      │                │                      │
   ┌──────▼──────┐  ┌────────────▼───┐  ┌────────▼────────┐  ┌─────────▼────────┐
   │  SnapInter- │  │  DragDetach    │  │  CameraZoom-    │  │  (future)        │
   │  actable    │  │                │  │  Controller     │  │  OnTwoFingerDrag │
   └─────────────┘  └────────────────┘  └─────────────────┘  └──────────────────┘

   ┌─────────────────────────────┐    ┌──────────────────────────────────────────┐
   │  AutoRotateZone             │    │  SwirlGestureDetector                    │
   │  (Physics trigger — reads  │    │  (reads EnhancedTouch directly;          │
   │   OnTrigger* callbacks)     │    │   independent of GestureController)      │
   └─────────────────────────────┘    └──────────────────────────────────────────┘
```

**Gesture map (GestureController)**

| Touch | Action |
|---|---|
| 1 finger on object | Rotate the object (Y-axis by default) |
| 2 fingers translating on object | Drag / translate the object |
| 2 fingers spreading/pinching on object | Scale the object |
| 2 fingers translating on empty space | `OnTwoFingerDragDelta` event |
| 2 fingers spreading/pinching on empty space | `OnPinchUpdate` event → `CameraZoomController` |
| 1 finger circular motion | `SwirlGestureDetector` |

---

## Quick Start

### Minimum scene setup (1 interactive object)

1. Create a persistent GameObject (e.g. `Managers`).
   Add **`GestureController`** to it.

2. On your lab equipment prefab:
   - Add a **`Collider`** (any type, non-trigger).
   - Add **`ObjectManipulator`**.

3. Press Play. Drag on the object in the Editor (mouse = simulated single touch).

### Enabling additional features

| Feature | Add to object | Add to scene / zone |
|---|---|---|
| Snap to position | `SnapInteractable` | `SnapZone` on empty GameObject |
| Auto-rotate in area | *(none)* | `AutoRotateZone` on trigger-collider GO |
| Camera zoom (pinch empty space) | *(none)* | `CameraZoomController` on Managers |
| Detect mixing swirl | `SwirlGestureDetector` | *(none)* |
| Pull-to-detach | `DragDetach` | anchor `Transform` in scene |

---

## Component Reference

---

### GestureController

**File:** `Scripts/InteractionLogic/GestureController.cs`  
**Attach to:** one persistent GameObject (e.g. `Managers`).  
**Requires:** none. Enables `EnhancedTouchSupport` automatically.

#### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `_interactableLayer` | `LayerMask` | Everything | Only raycasts against these layers when finding an `ObjectManipulator` |
| `_raycastDistance` | `float` | `100` | Max raycast distance in world units |
| `_pinchCommitRatio` | `float` | `0.04` | Fraction of initial finger distance that must change before the gesture commits to Pinch (vs Drag) |
| `_dragCommitPixels` | `float` | `6` | Screen pixels the two-touch midpoint must travel before committing to Drag (vs Pinch) |

#### Public events

```csharp
// Fired when first touch contact lands on an ObjectManipulator.
// Not re-fired when a second finger is added to an already-focused object.
event Action<ObjectManipulator> OnManipulatorGrabbed;

// Fired when all contact with a focused ObjectManipulator ends.
event Action<ObjectManipulator> OnManipulatorReleased;

// Fired once when a two-touch pinch begins. Arg: initial finger distance (pixels).
event Action<float> OnPinchBegin;

// Fired every frame during a pinch with NO focused object.
// Arg: current finger distance (pixels) — compute your own delta.
event Action<float> OnPinchUpdate;

// Fired when a pinch gesture ends.
event Action OnPinchEnd;

// Fired every frame during a two-finger drag with NO focused object.
// Arg: screen-space delta (pixels).
event Action<Vector2> OnTwoFingerDragDelta;
```

#### Notes

- Mouse (left button) acts as a single synthetic touch in the Editor and on desktop.
- Two-touch → single-touch transitions (one finger lifts) do **not** re-fire `OnManipulatorGrabbed` — the object stays continuously focused.
- `OnPinchBegin` fires for **all** pinches, including on focused objects. `OnPinchUpdate` fires **only** when there is no focused object.

---

### ObjectManipulator

**File:** `Scripts/InteractionLogic/ObjectManipulator.cs`  
**Attach to:** any lab-equipment GameObject that should be interactive.  
**Requires:** a `Collider` anywhere in the object's hierarchy (needed for GestureController's raycast).

#### Inspector fields

| Section | Field | Type | Default | Description |
|---|---|---|---|---|
| **Permissions** | `canRotate` | `bool` | `true` | Allow 1-finger rotation |
| | `canDrag` | `bool` | `true` | Allow 2-finger drag |
| | `canScale` | `bool` | `true` | Allow 2-finger pinch-scale |
| **Rotate** | `_rotateSensitivity` | `float` | `0.3` | Degrees of rotation per screen pixel of horizontal swipe |
| | `rotateAxis` | `Vector3` | `(0,1,0)` | World-space axis to rotate around |
| **Drag** | `_dragSensitivity` | `float` | `0.005` | World units moved per screen pixel |
| | `lockToHorizontalPlane` | `bool` | `true` | Constrain movement to the XZ plane (keeps AR objects flat) |
| **Scale** | `scaleRange` | `Vector2` | `(0.3, 3)` | Min/max scale as a multiplier of the object's **original** local scale |

#### Public methods (called by GestureController)

```csharp
// 1-finger swipe → rotate around rotateAxis.
void ReceiveRotateDelta(Vector2 screenDelta);

// 2-finger translation → move in world space.
void ReceiveDragDelta(Vector2 screenDelta);

// 2-finger pinch → scale clamped to scaleRange × original scale.
void ReceivePinchDelta(float scaleFactor);
```

These methods are `public` so that other components (`AutoRotateZone`, etc.) can call them directly.

#### Notes

- `scaleRange` is always relative to the object's scale at the time `Awake` ran, not its current scale. Pinching cannot push the object outside `[originalScale × scaleRange.x, originalScale × scaleRange.y]`.
- `canDrag`, `canRotate`, `canScale` are plain `public bool` fields — other components (`SnapInteractable`, `AutoRotateZone`, `DragDetach`) read and write them to take or restore control.

---

### SnapZone / SnapInteractable

**Files:** `SnapZone.cs`, `SnapInteractable.cs`

Two paired components: **`SnapZone`** marks a destination; **`SnapInteractable`** is the object that snaps into it.

---

#### SnapZone

**Attach to:** an empty GameObject at the exact position/rotation the snapped object should occupy.

##### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `acceptTag` | `string` | `""` | Only accept `SnapInteractable`s whose GameObject tag matches. Empty = accept any. |
| `_snapRadius` | `float` | `0.1` | World-space radius within which an object can snap (yellow Gizmo sphere) |
| `snapRotation` | `bool` | `true` | Align the snapped object's rotation to this zone's rotation |
| `_highlightVisual` | `GameObject` | — | Shown while a compatible object is being dragged within range |
| `_occupiedVisual` | `GameObject` | — | Shown while an object is snapped here |

##### Public API

```csharp
float SnapRadius { get; }
bool  IsOccupied { get; }

// Returns true if candidate is compatible and within snap radius.
bool IsInRange(SnapInteractable candidate);

// Toggle the highlight visual. Called by SnapInteractable during drag.
void SetHighlight(bool on);

// Attempt to accept candidate. Returns true on success.
// Calls SnapInteractable.SnapTo() which starts the lerp.
bool TryAccept(SnapInteractable candidate);

// Called by SnapInteractable when it is picked up.
void Release();
```

##### Static registry

```csharp
// All active SnapZones. SnapInteractable iterates this instead of FindObjectsByType.
static readonly List<SnapZone> All;
```

---

#### SnapInteractable

**Attach to:** the object that should snap. Requires `ObjectManipulator`.

##### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `_snapSpeed` | `float` | `10` | Lerp speed toward the snap position |
| `OnSnapped` | `UnityEvent` | — | Fired when the object snaps into a zone |
| `OnUnsnapped` | `UnityEvent` | — | Fired when the object is picked up from a snapped state |

##### Public state

```csharp
bool     IsSnapped    { get; }   // true while snapped to a zone
SnapZone CurrentZone  { get; }   // the zone currently holding this object, or null
```

##### Public method (called by SnapZone)

```csharp
// Starts the lerp-to-snap and fires OnSnapped. Do not call directly.
void SnapTo(SnapZone zone);
```

##### Behaviour summary

- While being dragged: the nearest compatible `SnapZone` within its radius shows a highlight.
- On drag release: the object lerps to the nearest zone (if any).
- On grab while snapped: immediately unsnaps, re-enables `canDrag`, fires `OnUnsnapped`.

#### SnapZone + SnapInteractable setup example

```
Scene hierarchy:
  BuretteNeck (empty GO)          ← SnapZone, acceptTag = "Stopper"
  Stopper (mesh + Collider)       ← ObjectManipulator + SnapInteractable, tag = "Stopper"
```

1. Set the Stopper's tag to `"Stopper"`.
2. Set `SnapZone.acceptTag` to `"Stopper"`.
3. Tune `_snapRadius` using the yellow Gizmo sphere.
4. Assign a ghost mesh (translucent stopper) to `_highlightVisual`.

---

### AutoRotateZone

**File:** `Scripts/InteractionLogic/AutoRotateZone.cs`  
**Attach to:** a GameObject with a **trigger** Collider.

Rotates any `ObjectManipulator` that enters the trigger. Suppresses manual rotation while inside (restores original `canRotate` on exit).

#### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `rotationAxis` | `Vector3` | `(0,1,0)` | Axis to rotate around, in the chosen space |
| `rotationSpeed` | `float` | `90` | Degrees per second |
| `rotationSpace` | `Space` | `World` | `World`: axis is a fixed world direction. `Self`: axis is the object's local direction. |
| `acceptTag` | `string` | `""` | Only affect matching tags. Empty = any. |
| `suppressManualRotate` | `bool` | `true` | Set `canRotate = false` while inside the zone |

#### Public events

```csharp
// Fired when an ObjectManipulator enters the zone.
event Action<ObjectManipulator> OnObjectEntered;

// Fired when an ObjectManipulator exits (or the zone is disabled).
event Action<ObjectManipulator> OnObjectExited;
```

#### Notes

- Supports multiple simultaneous occupants.
- If the zone is **disabled** while objects are inside, all objects have their original `canRotate` restored and `OnObjectExited` is fired for each.
- If an object is **destroyed** while inside, it is silently removed from the occupant list.
- `suppressManualRotate` records each object's original `canRotate` value. If an object had `canRotate = false` before entering, it will remain `false` after exiting.

#### Setup

1. Add any Collider to the zone GameObject → enable **Is Trigger**.
2. Add **`AutoRotateZone`**.
3. If the trigger Collider is missing, `OnValidate` logs a warning.
4. The Gizmo shows a light-blue circle (rotation plane) and an axis arrow.

---

### CameraZoomController

**File:** `Scripts/InteractionLogic/CameraZoomController.cs`  
**Attach to:** the same GameObject as `GestureController` (e.g. `Managers`).

Zooms the view when the user pinches on **empty space**. Pinching on a focused object still scales that object — the camera is unaffected.

#### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `mode` | `ZoomMode` | `ContentScale` | See table below |
| `contentRoot` | `Transform` | — | *(ContentScale only)* Transform to scale — typically the child of your Vuforia ImageTarget |
| `scaleRange` | `Vector2` | `(0.4, 2.5)` | Min/max absolute local scale (ContentScale mode) |
| `fovRange` | `Vector2` | `(20, 70)` | Min/max camera FOV in degrees (FieldOfView mode) |
| `_smoothSpeed` | `float` | `8` | Lerp speed toward the zoom target |

#### ZoomMode enum

| Value | What changes | AR-safe? |
|---|---|---|
| `FieldOfView` | `Camera.main.fieldOfView` | **No** — Vuforia controls FOV to match the device lens; overriding it breaks AR alignment |
| `ContentScale` | `contentRoot.localScale` (uniform) | **Yes** |

#### Notes

- The baseline (FOV or scale) is re-captured on every `OnPinchBegin`, so each new pinch starts from the current zoom level — no drift across gestures.
- The zoom target persists after the pinch ends; the lerp settles the camera smoothly.
- `OnValidate` warns if `mode == FieldOfView` (Vuforia concern) or if `contentRoot` is unassigned in `ContentScale` mode.

---

### SwirlGestureDetector

**File:** `Scripts/InteractionLogic/SwirlGestureDetector.cs`  
**Attach to:** the Erlenmeyer (or any object that responds to circular mixing gestures).  
**Reads:** `EnhancedTouch` directly — independent of `GestureController`.

#### Algorithm

Rather than tracking position around a fixed center, `SwirlGestureDetector` measures how much the finger's **direction of travel** rotates. When the cumulative rotation (signed, in one direction) reaches `_completionAngle`, a swirl is detected.

```
Straight swipe    → velocity direction rotates ~15°   → no swirl
Large arc         → velocity direction rotates ~150°  → no swirl
Full circle       → velocity direction rotates 360°   → OnSwirl fires ✓
```

This approach works regardless of circle size, starting position, or hand size.

#### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `_completionAngle` | `float` | `360` | Cumulative velocity-angle rotation needed (degrees) |
| `_timeWindow` | `float` | `2.5` | Max seconds allowed to complete a swirl; resets on timeout |
| `_minSpeedPixels` | `float` | `80` | Min finger speed (px/sec) to register movement. Filters drift. |
| `_reverseResetDegrees` | `float` | `45` | If the travel direction reverses by this many degrees, progress resets |
| `OnSwirl` | `UnityEvent` | — | Fired each time a swirl completes. Inspector-wirable. |
| `_showDebugGizmo` | `bool` | `false` | *(Editor only)* Overlay showing % completion and direction |

#### Public event (code-only)

```csharp
// Fired each time a swirl completes.
// Arg: seconds the swirl took — use to scale animation speed.
event Action<float> OnSwirlDetected;
```

#### Continuous stirring

After each completion, progress resets but tracking continues — the detector does **not** require a new touch-down between stirs. Every revolution fires `OnSwirl` independently.

#### Usage example

```csharp
void Start()
{
    GetComponent<SwirlGestureDetector>().OnSwirlDetected += OnMix;
}

void OnMix(float duration)
{
    _animator.SetFloat("MixSpeed", 1f / duration);
    _animator.SetTrigger("Mix");
}
```

---

### DragDetach

**File:** `Scripts/InteractionLogic/DragDetach.cs`  
**Attach to:** the detachable object (e.g. burette stopper, pipette). Requires `ObjectManipulator`.

Detects when the object is pulled far enough from its `anchor` and fires a detach event. Useful for removing stoppers, separating connected equipment, or breaking a seal.

#### Inspector fields

| Field | Type | Default | Description |
|---|---|---|---|
| `anchor` | `Transform` | — | **Required.** The fixed attachment point (e.g. the burette neck). Component disables itself if null. |
| `_detachDistance` | `float` | `0.15` | World-space pull distance at which detach fires (red Gizmo sphere on anchor) |
| `snapBackOnRelease` | `bool` | `true` | Lerp back to anchor if released before detaching |
| `_snapBackSpeed` | `float` | `6` | Lerp speed of the snap-back |
| `_tensionLine` | `LineRenderer` | — | Optional tether visual. Component owns its positions and colour; you only set width and material. |
| `_colorRelaxed` | `Color` | Green | Tether colour at zero pull |
| `_colorTense` | `Color` | Red | Tether colour at full pull (detach threshold) |
| `OnDetach` | `UnityEvent` | — | Fired the moment the object detaches. Inspector-wirable. |

#### Public events (code-only)

```csharp
// Fired when detach occurs.
event Action OnDetached;

// Fired every frame while grabbed.
// Arg: normalised tension [0, 1]  — 0 = at anchor, 1 = at detach threshold.
// Use to drive audio pitch, particles, or haptics.
event Action<float> OnTensionChanged;
```

#### Public state

```csharp
bool  IsDetached     { get; }   // true once detached; never resets automatically
bool  IsSnappingBack { get; }   // true while lerping back to anchor
float CurrentTension { get; }   // normalised pull distance, updated each frame
```

#### Public method

```csharp
// Returns the object to anchor.position and resets all state.
// Call from TahapanInteractionController when resetting a lab step.
void Reattach();
```

#### Behaviour by state

| State | Grab | 2-finger drag | Release |
|---|---|---|---|
| Attached, not grabbed | `_isGrabbed = true` | builds tension → fires `OnTensionChanged` | snap-back (if enabled) |
| Detached | ignored | free movement via `ObjectManipulator` | nothing |
| Snapping back | interrupts snap-back | builds tension from current position | new snap-back |

#### Tension visual

Assign a `LineRenderer` with at least 2 positions. The component:
- Sets `positions[0]` = `anchor.position` every frame.
- Sets `positions[1]` = `transform.position` every frame.
- Lerps `startColor`/`endColor` from `_colorRelaxed` to `_colorTense` based on pull fraction.

---

## Component Combinations

### 1. Stopper on a burette (SnapInteractable + DragDetach)

The stopper sits in the burette neck. The user must pull hard enough to remove it.

```
BuretteNeck (empty GO)
  └─ SnapZone
       acceptTag = "Stopper"
       snapRotation = true

Stopper (mesh + Collider)
  └─ ObjectManipulator
       canRotate = true, canDrag = true, canScale = false
  └─ SnapInteractable
       snapBackOnRelease not relevant (DragDetach handles return)
  └─ DragDetach
       anchor → BuretteNeck transform
       detachDistance = 0.12
       snapBackOnRelease = true
       OnDetach → EnableBuretteFlow()
```

**Interaction flow:**

```
1. Object starts snapped  (SnapInteractable)
2. User 2-finger drags
   → OnManipulatorGrabbed
   → SnapInteractable: unsnaps, canDrag = true
   → DragDetach: _isGrabbed = true
3. Object moves away; tension builds
4. Distance ≥ detachDistance
   → DragDetach.ExecuteDetach()
   → OnDetach fires → EnableBuretteFlow()
5. If released before threshold
   → DragDetach: snap-back to anchor
   → SnapInteractable: TrySnapToNearest() → re-snaps
```

---

### 2. Erlenmeyer on hotplate (SnapInteractable + AutoRotateZone)

Drop the Erlenmeyer onto the hotplate snap zone; it auto-rotates to simulate mixing on heat.

```
Hotplate surface (mesh + Collider trigger)
  └─ AutoRotateZone
       rotationAxis = (0, 1, 0)
       rotationSpeed = 60
       acceptTag = "Erlenmeyer"

SnapZone (empty GO, child of Hotplate)
  └─ SnapZone
       acceptTag = "Erlenmeyer"
       snapRadius = 0.08

Erlenmeyer (mesh + Collider)
  └─ ObjectManipulator
  └─ SnapInteractable
       OnSnapped → PlayHeatingAudio()
```

---

### 3. Erlenmeyer mixing (SwirlGestureDetector)

After placing the Erlenmeyer, the user stirs by making circles on screen.

```csharp
// On TahapanInteractionController or a dedicated MixingStep script:
void OnEnterMixingStep()
{
    erlenmeyerGO.GetComponent<SwirlGestureDetector>().enabled = true;
    erlenmeyerGO.GetComponent<SwirlGestureDetector>().OnSwirlDetected += HandleMix;
}

void HandleMix(float duration)
{
    _mixCount++;
    if (_mixCount >= _requiredMixes)
        CompleteStep();
}
```

---

### 4. Camera zoom while inspecting the burette (CameraZoomController)

No setup on the burette itself — `CameraZoomController` activates automatically when the user pinches on empty space.

```
Managers (GO)
  └─ GestureController
  └─ CameraZoomController
       mode = ContentScale
       contentRoot → ARContent (child of ImageTarget)
       scaleRange = (0.5, 2.0)
```

---

## Migration from Legacy Scripts

The five old scripts in `InteractionLogic/` are superseded and should be deleted.

| Old script | Reason to delete |
|---|---|
| `DragObject.cs` | Uses `OnMouseDrag` + Legacy Input; broken 2-finger logic |
| `Rotate.cs` | Uses Legacy `Input.GetTouch` directly |
| `RotateObject.cs` | Same; conflicts with `Rotate.cs` |
| `CSharpScaling.cs` | Global mutable `static Transform ScaleTransform`; breaks multi-object |
| `OnClickForScaling.cs` | Writes to `CSharpScaling.ScaleTransform` — no longer needed |

### Steps to migrate a prefab

1. Open the prefab in the Editor.
2. Remove old components: `DragObject`, `Rotate`, `RotateObject`, `CSharpScaling`, `OnClickForScaling`.
3. Verify a `Collider` is present (add one if missing).
4. Add **`ObjectManipulator`** and configure in Inspector.
5. Add any optional components (`SnapInteractable`, `DragDetach`, `SwirlGestureDetector`) as needed.
6. Ensure a single **`GestureController`** exists in the scene (add to `Managers` if absent).

### Behaviour equivalents

| Legacy behaviour | New equivalent |
|---|---|
| `OnMouseDrag` 2-finger drag | `ObjectManipulator.canDrag = true` + 2-finger gesture |
| `Rotate.cs` 1-finger Y rotation | `ObjectManipulator.canRotate = true`, `rotateAxis = (0,1,0)` |
| `RotateObject.cs` 1-finger X rotation | `ObjectManipulator.rotateAxis = (1,0,0)` |
| `CSharpScaling` 2-finger scale | `ObjectManipulator.canScale = true` |
