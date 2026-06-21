# Changelog

All notable changes to TitraSims will be documented here.

---

## [Unreleased] — 2026-06-01

### New Files
- `Scripts/UI/TahapanInteractionUI.cs` — Presenter component that bridges `TahapanInteractionController` and UI. Owns narration panel, animation buttons, and button wiring. Removes all UI knowledge from the gameplay controller.
- `Scripts/Core/AudioManager.cs` — Singleton audio manager with separate SFX and narration channels, volume control, and `SoundBank` integration.
- `Scripts/Config/SoundBank.cs` — ScriptableObject registry for SFX clips. Key-based lookup with dictionary cache built on `OnEnable`/`OnValidate`.

### Changed

#### `Scripts/Animation/AnimNotify.cs`
- Namespace changed from `Gameplay` → `Animation` to match file location.
- Replaced `public Action OnNotifyHit` and `OnNotifyHitHandler()` with `public event Action<string> OnNotify` and `Notify(string notifyName)`. Callers now filter by name, enabling a single component to handle multiple notify types.

#### `Scripts/Gameplay/TahapanInteractionController.cs`
- Removed all `UIManager.Instance` references — controller no longer has any UI knowledge.
- Removed `AudioSource` serialized field — audio now routes through `AudioManager`.
- Added `event Action<TahapanInteractionData, bool> OnInteractionEnter` fired at the start of each interaction step.
- `RequestPlayAnimation()` / `RequestStopAnimation()` renamed to `public PlayAnimation()` / `StopAnimation()` for external access by `TahapanInteractionUI`.
- Removed dead `HasNarationText()` method.
- Removed `OnDisable` button listener cleanup (now owned by `TahapanInteractionUI`).

#### `Scripts/Gameplay/TahapanInteractionPlayer.cs`
- **Fixed shared mutable ScriptableObject**: instantiates a per-instance `AnimatorOverrideController` copy at `Initialize` time (`new AnimatorOverrideController(config.GenericAnimController)`). No longer mutates the shared asset at runtime.
- **Fixed stale coroutines on re-play**: added `animCoroutine` / `audioCoroutine` fields and `StopActiveCoroutines()` helper. `Play` and `Stop` both cancel running coroutines before starting new ones, preventing ghost callbacks.
- **Migrated narration audio to AudioManager**: removed `AudioSource` field and parameter. `Play` calls `AudioManager.Instance.PlayNarration()`, `Stop` calls `AudioManager.Instance.StopNarration()`.

#### `Scripts/Core/GameManager.cs`
- Removed direct `animationConfig.GenericAnimController[...] = null` mutations from `BackFromCurrentTahap` and `CompleteCurrentTahap`. Animation controller state is now owned by `TahapanInteractionPlayer`.

#### `Scripts/Core/AudioManager.cs`
- Added `SoundBank` serialized field.
- Added `PlaySFX(string key)` overload that resolves the clip from the bank.

#### `Scripts/Core/UIManager.cs`
- **Fixed Unity fake-null crash**: replaced `?.SetActive()` calls in `SetOnlyOnePanelActive` with explicit `if (kvp.Value != null)` checks. The C# `?.` operator bypasses Unity's overridden `==`, causing NRE on destroyed or missing GameObjects.
- `GetPanelByType` now uses `TryGetValue` instead of the dictionary indexer, preventing `KeyNotFoundException` for unregistered panel types.

#### `Scripts/Gameplay/GelasUkurFillInteractionManager.cs`
- **Fixed NRE in `OnDisable`**: `ResetLarutanContainer` now guards against null `config` before accessing `FillObjectInitialScale` and `MeniskusInitPos`.

#### `Scripts/Gameplay/TimbanganInteractionManager.cs`
- **Fixed NRE in `OnDisable`**: `ResetPowderFill` now guards against null `config` and `powderFillObject` before applying reset values.

### Known Issues / Follow-up
- `GelasUkurFillInteractionManager.PlayAnimation` and `TimbanganInteractionManager.PlayAnimation` still mutate the shared `animationConfig.GenericAnimController` directly — same issue fixed in `TahapanInteractionPlayer`, not yet addressed here.
- Animation completion in both managers above is still time-based (`WaitForSeconds(clip.length)`) rather than event-based via `AnimNotify`.

---

## [c34d0d4] — UI Refactor, Scene Structure, Data Handler

- UI refactor across panels
- Scene structure reorganisation
- Data handler updates

## [a596bfa] — Init

- Initial project scaffolding

## [27f146c] — Initial Commit