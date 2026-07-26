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

If you add a new lock combination (e.g. locking two axes at once) and see
skewed movement again, check `ResolveDragPlaneNormal()` first — it only
special-cases the single-axis-locked case.
