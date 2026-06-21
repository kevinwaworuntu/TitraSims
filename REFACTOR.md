# TitraSims — Refactor Log

Living document tracking code quality work across the project.
Update the status column as items are addressed.

---

## How to Use

| Symbol | Meaning |
|--------|---------|
| ✅ | Done |
| ⏳ | Not started |
| 🚧 | In progress |
| ⏸ | Deferred / decision made to skip |
| 🔴 | Bug — broken behaviour |
| 🟡 | Design debt — works but causes maintenance pain |
| 🟢 | Polish — typos, dead code, minor cleanup |

---

## Open Items

### 🔴 Bugs

| Status | File | Issue |
|--------|------|-------|
| ⏸ | `Scripts/UI/TahapanControllerButton.cs` | `OnTahapClicked()` and `OnInfoButtonClicked()` both fire on the same button click — info panel opens automatically every time a stage is started. Separate into two distinct button handlers. |
| ⏳ | `Scripts/UI/UIManager.cs` | `GetPanelByType()` throws `KeyNotFoundException` if a `PanelType` key is missing from the dictionary — no safe fallback. Add a `TryGetPanel()` method or guard with `TryGetValue`. |
| ✅ | `Scripts/Gameplay/BuretteFillInteractionManager.cs` | `Awake()` and `OnEnable()` were missing `override` — shadowing base silently. Added `override` to both; added `base.Awake()` so `ARContentManager.Awake()` now runs and `tahapanInteractionController` is initialized before `OnEnable`. |

### 🟡 Design Debt

| Status | File | Issue |
|--------|------|-------|
| ✅ | `Scripts/Core/GameManager.cs` ↔ `Scripts/Core/UIManager.cs` | **Circular dependency — resolved.** Added `OnModeSet`, `OnTahapStarted`, `OnTahapCompleted`, `OnProgressReset` events to GameManager. UIManager subscribes in `Start()`, unsubscribes in `OnDestroy()`. GameManager has zero UIManager references. |
| ✅ | `Scripts/Core/UIManager.cs` ↔ `Scripts/UI/TahapanControllerButton.cs` | **Circular dependency — resolved.** Removed `UpdateTahapButtonStates()`, `tahapanButtons` list, and `FindObjectsByType` from UIManager. TahapanControllerButton now self-initialises in `Start()` and subscribes to `GameManager.OnModeSet` / `OnTahapCompleted` / `OnProgressReset` to keep its own visual state in sync. |
| ⏳ | `Scripts/Core/GameManager.cs` | `currentMode` is `public` — can be mutated externally, bypassing `SetMode()` logic. Make private with a read-only property. |
| ⏳ | `Scripts/GameManager.cs` | PlayerPrefs calls are still inline. Wrap in a `SaveSystem` + `SaveData` class when save data grows beyond 2 integers. See § Save System below. |
| ⏳ | `Scripts/UI/UIManager.cs` | `HideAllARPopups()` is marked `// ToDo` — the method only hides popups and removes listeners but does not reset any AR state. Needs a proper AR session teardown. |
| ⏳ | `Scripts/Gameplay/TahapanInteractionController.cs` | `StartInteraction()` and `ContinueInteraction()` are identical one-liners that both call `EnterInteractionState(currentInteractionIndex)`. Consolidate or document why two names are needed. |
| ⏳ | `Scripts/Data/InfoTextBank.cs` | All text content is hardcoded static strings. Should migrate to `ScriptableObject` data assets (one per mode/stage) so content can be edited without recompiling. |
| ⏳ | `Scripts/Gameplay/TahapanInteractionController.cs:189` | `HasNarationText()` accesses `interactionDataActionMappings[currentInteractionIndex].InteractionData` twice inline on one line — extract to a local variable for readability. |
| ⏳ | `Scripts/Gameplay/BuretteFillInteractionManager.cs`, `GelasUkurFillInteractionManager.cs`, `TimbanganInteractionManager.cs` | `isCheckWeightToContinue` field, `SetIsCheckWeightToContinue(bool)`, `SetButtonEnabledState(bool)`, `PlayAnimation(AnimationClip)`, and `RestartCurrentInteraction()` are copy-pasted across all three subclasses. Move shared members to `ARContentManager` base class. |
| ✅ | `Scripts/InteractionLogic/CSharpScaling.cs`, `DragObject.cs`, `Rotate.cs`, `RotateObject.cs` | Replaced by `GestureController` + `ObjectManipulator` (both use `UnityEngine.InputSystem.EnhancedTouch`). Old files can now be deleted. |
| ✅ | `Scripts/InteractionLogic/CSharpScaling.cs` | `public static Transform ScaleTransform` eliminated — `ObjectManipulator` receives pinch via `ReceivePinchDelta(float)` called directly by `GestureController`. |
| ✅ | `Scripts/InteractionLogic/DragObject.cs` | `Camera.main` cached in `ObjectManipulator.Awake()` and `GestureController.Start()`. |
| ⏳ | `Scripts/Core/ARContentManager.cs` | `GetComponent<DefaultObserverEventHandler>()` is called in both `OnEnable()` and `OnDisable()` — redundant. Cache the result in `Awake()`. |
| ⏳ | `Scripts/Animation/TimbanganObject.cs` | `LerpValue` coroutine has a hardcoded `0.1f` duration with two `// ToDo: Make duration listen to anim length` comments. Wire to `AnimationConfig` or an exposed field. |

### 🟢 Polish

| Status | File | Issue |
|--------|------|-------|
| ⏳ | `Scripts/UI/TahapanControllerButton.cs` | Unused `private void OnInfoButtonClicked()` method — remove after resolving the P1 bug above. |
| ✅ | `Scripts/InteractionLogic/ObjectRotator.cs` | Empty class — deleted (file + meta). |
| ✅ | `Scripts/InteractionLogic/DragObject.cs`, `CSharpScaling.cs`, `Rotate.cs`, `RotateObject.cs`, `OnClickForScaling.cs` | Empty `Start()` / `Update()` stubs removed. Unused `System.Collections` / `System.Collections.Generic` imports removed. |
| ✅ | `Scripts/Gameplay/GelasUkurFillInteractionManager.cs` | `Larutan2Container` → `larutan2Container`. Added `[FormerlySerializedAs("Larutan2Container")]` to preserve Inspector assignment. |

---

## Completed

### Session — 2026-06-07

#### Interaction System — Snap

| File(s) | Change |
|---------|--------|
| `Scripts/InteractionLogic/GestureController.cs` | Added `OnManipulatorGrabbed(ObjectManipulator)` and `OnManipulatorReleased(ObjectManipulator)` events. Grabbed fires on first contact (Single from Idle, or Two from Idle for simultaneous touches, mouse down). Released fires in `ResetState()` when all contact ends. Two→Single transitions do not re-fire Grabbed (object stays continuously focused). |
| `Scripts/InteractionLogic/SnapZone.cs` | New. World-space snap destination. Maintains a static `All` list (OnEnable/OnDisable) so SnapInteractable can poll cheaply. Exposes `IsInRange`, `TryAccept`, `Release`, `SetHighlight`. Yellow/green Gizmo sphere shows snap radius. |
| `Scripts/InteractionLogic/SnapInteractable.cs` | New. Requires ObjectManipulator. Subscribes to GestureController grab/release events. Highlights nearest zone during drag; lerps to nearest zone on release. Sets `canDrag = false` while snapped; re-enables on unsnap. |
| `Scripts/InteractionLogic/AutoRotateZone.cs` | New. Trigger zone that auto-rotates any ObjectManipulator that enters. Preserves and restores each object's original `canRotate` on exit. Supports multiple simultaneous occupants. OnDisable restores all. C# events: `OnObjectEntered` / `OnObjectExited`. |
| `Scripts/InteractionLogic/CameraZoomController.cs` | New. Subscribes to `GestureController.OnPinchBegin/Update`. Two modes: `FieldOfView` (non-AR) and `ContentScale` (AR-safe, scales a content root transform). Pinching on a focused object still scales that object — camera is unaffected. |
| `Scripts/InteractionLogic/SwirlGestureDetector.cs` | New. Reads EnhancedTouch independently (ref-counted Enable/Disable). Velocity-angle accumulation algorithm — detects a full revolution of the finger's travel direction. Fires per revolution for continuous stirring. Inspector: `OnSwirl` (UnityEvent). Code: `OnSwirlDetected(float duration)`. |
| `Scripts/InteractionLogic/DragDetach.cs` | New. Requires ObjectManipulator. Subscribes to GestureController grab/release events. Monitors world-space pull distance from `anchor` each frame while grabbed; fires `OnDetach` at threshold. Optional LineRenderer tension visual (green→red). Snap-back on release if not detached. `Reattach()` for experiment reset. |

---

### Session — 2026-05-25

#### Architecture

| File(s) | Change |
|---------|--------|
| `Scripts/UI/UIManager.cs` | Added `[Header("Info Style Config")]` + `styleConfigDefault/TBA/TK` serialized fields. Added `ApplyInfoTextStyle()` private method. `ShowInfoPanel()` now applies per-mode text style. ⚠️ Reassign the 3 style config assets on the UIManager prefab in the Editor. |
| `Scripts/GameManager.cs` | Removed `styleConfigDefault/TBA/TK` fields (moved to UIManager). Removed dead commented-out `RetrieveTextStyle` block. |
| `Scripts/Config/AnimationConfig.cs` | Added `InteractionEndDelay` (`float`, default `2f`) under `[Header("Interaction Timing")]`. |
| `Scripts/Gameplay/TahapanInteractionController.cs` | Hardcoded `WaitForSeconds(2f)` replaced with `AnimationConfig.InteractionEndDelay`; falls back to `0f` if config is null. Coroutine renamed `EndTahapanDelay`. |
| `Scripts/GameManager.cs` | Extracted triplicated `switch(currentMode)` marker blocks in `StartTahap`, `BackFromCurrentTahap`, `CompleteCurrentTahap` → single private helper `SetCurrentMarkerActive(int index, bool active)`. |

#### Decoupling

| File(s) | Change |
|---------|--------|
| `Scripts/Gameplay/TahapanInteractionPlayer.cs` | Removed all `UIManager` access from `Play()`. Removed `using UI;`. The class is now a pure audio/animation playback helper with no UI dependency. |
| `Scripts/Gameplay/TahapanInteractionController.cs` | Narration content-setting (`SetTitleText` / `SetDescriptionText`) moved into `ExecuteInteractionState()`, before `interactionPlayer.Play()` is called. Completion callback now uses pre-captured `mapping` variable instead of re-indexing. |

#### Bug Fixes

| File | Fix |
|------|-----|
| `Scripts/UI/UIManager.cs:75` | History stack was pushing `panelType` (destination) instead of `currentActivePanelType` (origin) — `GoBack()` was navigating to the same panel. Fixed to push origin. |
| `Scripts/UI/UIManager.cs` | `SetButtonAnimationVisibility` lacked null checks on both buttons. Added guards; renamed param to `isVisible` to avoid shadowing `Behaviour.enabled`. |
| `Scripts/UI/MainMenu.cs` | `HowToPlayButton()` was navigating to `PanelModeSelection` (same as Start). Fixed to `PanelHowToPlay`. |
| `Scripts/InfoTextBank.cs` | All `\n` escape sequences inside verbatim (`@""`) strings rendered as literal `\n` characters in TMP. Replaced with actual blank lines. |
| `Scripts/UI/ContextualButtonController.cs` | `RegisterAction` / `RegisterTextToButton` had no index validation — would throw on out-of-range index. Added bounds guard. |

#### Polish

| File | Fix |
|------|-----|
| `Scripts/GameManager.cs` | `BackFrromCurrentTahap` → `BackFromCurrentTahap` (typo). Updated call site in `BackButton.cs`. |
| `Scripts/GameManager.cs` | `currentAttemptingTahapIndex` changed from `public` to `private`. No external readers remain. |
| `Scripts/UI/UIManager.cs` | Removed `[Header("Tahapan Controllers Button")]` on a private non-serialized field (header had no effect in Inspector). |
| `Scripts/UI/UIManager.cs` | Removed dead commented-out `ShowARPopup` block. |
| `Scripts/InfoTextBank.cs` | Removed dead `InfoStyle` struct, empty `TBAStyle[]`, and `TKStyle[]` arrays. Removed leading space in TBA "Preparasi Larutan Blanko" entry. |
| `Scripts/UI/TahapanControllerButton.cs` | `buttonActiveAplhaValue` → `buttonActiveAlphaValue` (typo). |

---

## Notes

### Save System

Current state: two `PlayerPrefs` integer keys in `GameManager` (`LastCompletedTahapTBA`, `LastCompletedTahapKomp`).

Upgrade path if save data grows:
1. Create `SaveData.cs` — plain class with all persistent fields
2. Create `SaveSystem.cs` (static, `Scripts/Utility/`) — wraps `PlayerPrefs` reads/writes behind `Load()` / `Save(SaveData)` / `Reset()`
3. `GameManager` holds a `SaveData _saveData` field, loaded in `Awake()`

Only worth doing when a new field needs saving (score, timestamps, per-stage attempts). Do not refactor until then.

### Navigation History

`UIManager` uses a `Stack<PanelType>` for back-navigation. Rules:
- PanelHomePage is never pushed (it's the implicit floor)
- PanelInfo is never pushed (it overlays without adding history)
- All other panels push the **origin** panel when navigating away

If a new panel type should behave like PanelInfo (overlay, no history), add it to the guard condition in `ShowPanelAndAddToHistory()`.