# XRI AR-Specific Components

**Package:** `com.unity.xr.interaction.toolkit` v3.4.0  
**Scope:** Components under the `UnityEngine.XR.Interaction.Toolkit` namespace that are exclusively for mobile/screen-based Augmented Reality — not applicable to VR headsets.

---

## Architecture Overview

XRI AR works on top of AR Foundation. The core flow is:

```
Touch Input (screen)
    └── TouchscreenGestureInputController   ← virtual input device
            └── ScreenSpaceRayPoseDriver    ← converts touch to a 3D ray
                    └── XRRayInteractor     ← fires raycast into AR scene
                            └── ARRaycastManager (ARFoundation)
                                    └── Hits AR plane / trackable
                                            └── ARTransformer / ARInteractable
                                                    └── Manipulates target object
```

---

## 1. Gesture System

**Namespace:** `UnityEngine.XR.Interaction.Toolkit.AR.Gestures`  
**Location:** `Runtime/AR/Gestures/`

XRI ships its own gesture recognizer pipeline built for AR touch input. All gestures inherit from `Gesture<T>` and are produced by a corresponding `GestureRecognizer`.

### Base Class — `Gesture<T>`

| Member | Type | Description |
|---|---|---|
| `targetObject` | `GameObject` | The AR object this gesture is targeting |
| `isCanceled` | `bool` | Whether the gesture was canceled mid-motion |
| `onStart` | `event` | Fires when the gesture is first recognized |
| `onUpdated` | `event` | Fires each frame while the gesture is active |
| `onFinished` | `event` | Fires when the gesture ends or is canceled |

### Gesture Types

| Class | Fingers | Motion | Recognizer |
|---|---|---|---|
| `TapGesture` | 1 | Quick tap, no drag | `TapGestureRecognizer` |
| `DragGesture` | 1 | Sustained single-finger drag | `DragGestureRecognizer` |
| `PinchGesture` | 2 | Fingers moving toward/away from each other | `PinchGestureRecognizer` |
| `TwistGesture` | 2 | Fingers rotating around a center point | `TwistGestureRecognizer` |
| `TwoFingerDragGesture` | 2 | Both fingers dragging in the same direction | `TwoFingerDragGestureRecognizer` |

### Supporting Utilities

- **`GestureRecognizer`** — base class for all recognizers; manages touch lifecycle and gesture state machine.
- **`GestureTouchesUtility`** — helper for filtering touch events and determining if a touch is over a UI element (prevents AR gestures from firing through UI).

---

## 2. Screen-Space Input Components

**Namespace:** `UnityEngine.XR.Interaction.Toolkit.AR.Inputs`  
**Location:** `Runtime/AR/Inputs/`

These components bridge raw touch events to XRI's Input System pipeline so `XRRayInteractor` can work on a flat screen.

### `TouchscreenGestureInputController`

A virtual Input System device that exposes touch gestures as typed controls. Added automatically to the Input System device list at runtime.

| Control | Type | Description |
|---|---|---|
| `tapStartPosition` | `Vector2Control` | Screen position where a tap began |
| `dragStartPosition` | `Vector2Control` | Screen position where a drag began |
| `dragCurrentPosition` | `Vector2Control` | Current drag finger position |
| `dragDelta` | `Vector2Control` | Delta movement of the drag this frame |
| `pinchStartPosition1/2` | `Vector2Control` | Starting positions of the two pinch fingers |
| `pinchGap` | `AxisControl` | Current distance between pinch fingers (pixels) |
| `pinchGapDelta` | `AxisControl` | Change in pinch gap this frame |
| `twistStartPosition1/2` | `Vector2Control` | Starting positions of the two twist fingers |
| `twistDeltaRotation` | `AxisControl` | Rotation delta (degrees) this frame |
| `twoFingerDragStartPosition1/2` | `Vector2Control` | Start positions for two-finger drag |

**Usage:** This device is consumed automatically by `ScreenSpaceRayPoseDriver` and the screen-space input readers below. You do not need to read from it directly.

---

### `ScreenSpaceRayPoseDriver`

Drives the pose (position + rotation) of a Transform based on a touchscreen tap or drag position. Used to position an `XRRayInteractor`'s origin so it casts a ray from the camera through the touch point.

**Key behavior:**
- Reads `tapStartPosition` or `dragCurrentPosition` from `TouchscreenGestureInputController`.
- Converts the screen-space point to a world-space ray using the AR camera.
- Updates `transform.position` and `transform.rotation` to match that ray each frame.

**Usage:** Add to the same GameObject as `XRRayInteractor`. The interactor then automatically casts from the touched screen point into the AR scene.

---

### `ScreenSpaceSelectInput`

Provides the **select** action value for `XRRayInteractor` from touchscreen input. Without this, the ray interactor would not know when a tap constitutes a "select".

**Reads from:** tap position, drag position, pinch gap delta, twist delta.  
**Implements:** `IXRInputValueReader<float>` — returns `1.0` when selecting, `0.0` otherwise.

---

### `ScreenSpacePinchScaleInput`

Provides a **scale factor** derived from pinch gap delta for use with `XRRayInteractor`'s scale transformer.

| Property | Default | Description |
|---|---|---|
| `rotationThreshold` | — | Minimum twist delta above which pinch is blocked (prevents fighting between scale and rotate) |

**Implements:** `IXRInputValueReader<float>`

---

### `ScreenSpaceRotateInput`

Provides a **rotation value** derived from twist gesture delta or two-finger drag for use with `XRRayInteractor`'s rotation transformer.

**Implements:** `IXRInputValueReader<Vector2>`

---

## 3. AR Interactors

**Namespace:** `UnityEngine.XR.Interaction.Toolkit.AR`  
**Location:** `Runtime/AR/Interactors/`

### `IARInteractor` (Interface — Active)

Interface implemented by interactors that perform AR raycasting against AR Foundation trackables (planes, feature points, meshes).

| Member | Description |
|---|---|
| `TryGetCurrentARRaycastHit(out ARRaycastHit)` | Returns the first AR raycast hit this frame |
| `trackableType` | Which AR trackable types to hit-test against (planes, feature points, etc.) |
| `enableARRaycasting` | Toggle AR raycasting on/off |
| `occludeARHitsWith3DObjects` | Block AR hits if a 3D physics collider is in front of the AR plane |
| `occludeARHitsWith2DObjects` | Block AR hits if a 2D physics collider is in front |

### `ARGestureInteractor` (Deprecated → use `XRRayInteractor` + `IARInteractor`)

Legacy interactor that bundled gesture recognition and AR raycasting together. Replaced by pairing:
- `XRRayInteractor` (implements `IARInteractor`)
- `ScreenSpaceRayPoseDriver` (drives the ray from touch)
- `ScreenSpaceSelectInput` (triggers select from tap)

---

## 4. AR Interactables (Deprecated — Replaced by ARTransformer)

**Namespace:** `UnityEngine.XR.Interaction.Toolkit.AR`  
**Location:** `Runtime/AR/Interactables/`

All of these inherit from `ARBaseGestureInteractable`, which:
- Requires `XROrigin` and `ARRaycastManager` in the scene.
- Automatically excludes touch events that land over UI elements.
- Connects to the gesture pipeline from Section 1.

These still function in XRI 3.4.0 but are marked `[Obsolete]`. The replacement for all of them is adding an `ARTransformer` component to the interactable and configuring which axes to allow.

---

### `ARSelectionInteractable`

Handles **object selection** via `TapGesture`.

| Property | Description |
|---|---|
| `selectionVisualization` | A child GameObject shown when selected, hidden when deselected |

**Behavior:** On tap, sets the object as selected in the XRI interaction manager and activates `selectionVisualization`. Deselects on next tap elsewhere.

---

### `ARPlacementInteractable`

Handles **placing a prefab** at the tap point on an AR plane. This is the only interactable that spawns a new object rather than manipulating an existing one.

| Property | Description |
|---|---|
| `placementPrefab` | The prefab to instantiate on the AR plane |
| `placementIndicator` | Optional visual shown at the raycast hit point before placing |

**Behavior:** On `TapGesture`, performs an AR raycast via `ARRaycastManager`. If a plane is hit, instantiates `placementPrefab` at the hit position aligned to the plane normal.

**Modern replacement:** `ObjectSpawner` + `ARInteractorSpawnTrigger` (from AR Starter Assets sample, not currently imported).

---

### `ARTranslationInteractable`

Moves the object along AR planes via `DragGesture`.

| Property | Type | Description |
|---|---|---|
| `objectGestureTranslationMode` | `GestureTranslationMode` | `HorizontalOnly`, `VerticalOnly`, or `Any` |

**Behavior:** Raycasts on each drag frame. Moves the object to the new hit point, constrained by `objectGestureTranslationMode`.

**Modern replacement:** `ARTransformer` with translate axes configured.

---

### `ARRotationInteractable`

Rotates the object via `DragGesture` (Y-axis) or `TwistGesture`.

| Property | Type | Description |
|---|---|---|
| `rotationRateDegreesDrag` | `float` | Degrees/pixel for drag rotation |
| `rotationRateDegreesGesture` | `float` | Degrees/degree for twist rotation |

**Modern replacement:** `ARTransformer` with rotate axes configured.

---

### `ARScaleInteractable`

Scales the object via `PinchGesture`.

| Property | Type | Description |
|---|---|---|
| `minScale` | `float` | Minimum allowed scale |
| `maxScale` | `float` | Maximum allowed scale |
| `elasticity` | `float` | Rubber-band resistance when at min/max bounds |
| `sensitivity` | `float` | Scale rate multiplier |

**Behavior:** Scale is computed as `initialScale * (pinchGap / initialPinchGap)`. When the scale would exceed `minScale`/`maxScale`, elasticity reduces the response for a rubber-band feel.

**Modern replacement:** `ARTransformer` with scale enabled.

---

### `ARAnnotationInteractable`

Displays **annotation labels** on objects when hovered. Labels are hidden when out of view or occluded.

| Property | Type | Description |
|---|---|---|
| `annotations` | `List<ARAnnotation>` | List of annotation items |
| `maxAnnotationDistance` | `float` | Labels hidden beyond this distance from camera |

**`ARAnnotation` fields:**

| Field | Description |
|---|---|
| `annotationVisualization` | GameObject to show/hide as the label |
| `minFOVAngle` | Minimum camera FOV angle for the label to appear |
| `maxFOVAngle` | Maximum camera FOV angle for the label to appear |

**Modern replacement:** `LazyFollow` component + hover/select events on `XRSimpleInteractable`.

---

## 5. Modern AR Replacement — `ARTransformer`

Available in XRI 3.x. A single component that replaces `ARTranslationInteractable`, `ARRotationInteractable`, and `ARScaleInteractable`.

Add it to any `XRGrabInteractable` or `XRSimpleInteractable` object:

```
GameObject (with Rigidbody + Collider)
├── XRGrabInteractable
└── ARTransformer
    ├── allowTranslation: true/false
    ├── allowRotation: true/false
    └── allowScale: true/false
```

The `XRRayInteractor` (with `IARInteractor`) feeds gesture input → `ARTransformer` applies the transform.

---

## 6. Minimum Scene Setup for AR Interactions

```
Scene
├── AR Session                      ← ARFoundation AR session
├── XR Origin
│   ├── Camera Offset
│   │   └── Main Camera
│   │       └── AR Camera Manager   ← ARFoundation camera feed
│   └── AR Interactor [new style]
│       ├── XRRayInteractor         ← implements IARInteractor
│       ├── ScreenSpaceRayPoseDriver← drives ray from touch position
│       ├── ScreenSpaceSelectInput  ← tap = select
│       ├── ScreenSpacePinchScaleInput
│       └── ScreenSpaceRotateInput
├── AR Plane Manager                ← detects horizontal/vertical planes
└── AR Raycast Manager              ← raycasts against detected planes
```

Any object placed in the scene with `XRGrabInteractable` + `ARTransformer` becomes manipulable via touch.

---

## 7. Deprecation Summary

| Deprecated | Replacement |
|---|---|
| `ARGestureInteractor` | `XRRayInteractor` + `ScreenSpaceRayPoseDriver` + screen-space inputs |
| `ARTranslationInteractable` | `ARTransformer` (translation enabled) |
| `ARRotationInteractable` | `ARTransformer` (rotation enabled) |
| `ARScaleInteractable` | `ARTransformer` (scale enabled) |
| `ARPlacementInteractable` | `ObjectSpawner` + `ARInteractorSpawnTrigger` |
| `ARAnnotationInteractable` | `LazyFollow` + hover/select events |
| `ARSelectionInteractable` | `XRSimpleInteractable` + select events |

All deprecated classes remain functional in XRI 3.4.0. No breaking removal is scheduled in the current version.

---

## 8. Relation to TitraSims Custom Scripts

The custom scripts in `Assets/_TitraSims/Scripts/InteractionLogic/` duplicate functionality that XRI AR already provides:

| Custom Script | XRI Equivalent |
|---|---|
| `DragObject.cs` — 2-finger pan | `ARTranslationInteractable` / `ARTransformer` (translation) |
| `RotateObject.cs` — 1-finger X-axis tilt | `ARRotationInteractable` / `ARTransformer` (rotation) |
| `Rotate.cs` — 1-finger Y-axis spin | `ARRotationInteractable` / `ARTransformer` (rotation) |
| `CSharpScaling.cs` — 2-finger pinch scale | `ARScaleInteractable` / `ARTransformer` (scale) |

The custom scripts use Unity's legacy `Input` API directly. Migrating to XRI AR components would provide: proper gesture conflict resolution, UI touch exclusion, AR plane constraint, and min/max bounds without custom code.