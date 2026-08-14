# TitraSims

Living documentation for this project. Add new entries under the relevant
subsystem heading as work happens; keep entries short and dated.

## Overview

Unity AR app (Vuforia — see `Packages/com.ptc.vuforia.engine`, `QCAR/`) for
simulating chemistry titration procedures. Scenes/prefabs are organized by
stage (e.g. `Stage_TBA_*`, `Stage_TK_*` — preparation, calibration, blank
titration, sample titration).

## Interaction Logic (`Assets/_TitraSims/Scripts/InteractionLogic`)

### `ObjectManipulator` — drag-axis lock

`lockDragX/Y/Z` are meant to freeze a world-space axis during a drag. Locking
an axis by clamping the coordinate *after* raycasting a camera-facing plane
doesn't work reliably in AR: the drag plane (`-cam.transform.forward`) is
tilted relative to world axes for almost any camera angle, so snapping one
coordinate back afterward couples into the other two and reads as diagonal
drift instead of clean single-axis movement.

Fix (2026-07-18): `ResolveDragPlaneNormal()` picks the raycast plane's normal
to match the single locked axis (e.g. `Vector3.forward` when only Z is
locked) so the axis is already fixed by the plane itself — no post-hoc
clamp needed, no coupling. Falls back to the camera-facing plane when zero or
more than one axis is locked. `lockToHorizontalPlane` still takes priority.

Fix (2026-08-07): the common case in this app is actually the reverse — two
axes locked, one free (e.g. a burette clamp that only slides on Y). A plane
can't represent pure single-axis movement without a lossy step: raycasting
any 2D plane and then discarding two of its three coordinates still leaves
the *remaining* coordinate's sensitivity and apparent direction dependent on
how tilted that plane was relative to the camera, i.e. it drifted/felt wrong
depending on where in the world you were standing in AR. Plane-based
raycasting is inherently the wrong tool once only one axis is free.

`ClosestPointOnAxis()` replaces the plane for that case: it finds the
closest point between the camera's screen ray and the free axis's line
(the same technique transform gizmos use for single-axis dragging). Every
point it returns lies exactly on that world-axis line through the drag-start
position, so movement is pure single-axis translation by construction, with
zero camera-angle coupling — not just clamped away after the fact.
`TryGetSingleFreeAxis()` (via `GetEffectiveLocks()`) decides which path
applies: exactly one free axis → `ClosestPointOnAxis`; zero or 2+ free axes →
the plane path above, unchanged.

If you add a new lock combination and see skewed or angle-dependent
movement again, check whether it's landing in the plane path when it should
be axis-constrained (or vice versa) — `GetEffectiveLocks()` is the single
source of truth both paths read from.

Fix (2026-08-07, later same day): everything above was still done in world
space and written to `transform.position`. These objects are parented under
AR image-target anchors, whose world transform is arbitrary (depends on
where/how the physical marker was placed and scanned) — so "world axis"
never actually meant a meaningful direction relative to the apparatus itself,
only relative to wherever tracking happened to start. `lockDragX/Y/Z` are
meant to describe the object's own sliding directions (e.g. "this clamp only
moves up"), which is a local-space, parent-relative concept, not a global one.

`ToLocalRay()` now converts the camera's screen ray into the object's
parent's local space up front (`parent.InverseTransformPoint`/
`InverseTransformDirection`), and every downstream calculation —
`ClosestPointOnAxis`, `ResolveDragPlaneNormal`, `RaycastDragPlane` — runs
entirely in that local frame, writing the result to `transform.localPosition`
instead of `transform.position`. A straight line stays straight under this
coordinate change even with non-uniform parent scale, so nothing downstream
needed to change beyond swapping which space it reads/writes. Objects with no
parent are unaffected (local space == world space when there's no parent).
