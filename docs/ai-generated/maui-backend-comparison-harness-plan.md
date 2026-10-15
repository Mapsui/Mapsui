---
title: MAUI Backend Comparison Harness
description: Follow-up plan for a repeatable backend comparison with measured draw timing, paint cadence and a locally copyable report.
---

# MAUI backend comparison harness

Date: 2026-10-12
Status: Deferred; separate deliverable from the implemented manual backend selector.

This carries step 6 of the [surface selection plan](maui-android-surface-selection-plan.md). The existing sample already selects a backend, recreates its scene and preserves the camera. This follow-up adds repeatable measurements without changing library defaults or claiming a faster backend in advance.

## Implementation

- Add Run comparison, Cancel and Copy results alongside manual selection.
- Run the same timed Navigator sequence for each backend with identical viewport dimensions, tile source, style and renderer. Disable manual gestures and unrelated animations while measuring.
- Separate scene loading/preparation from drawing. Track required tile completion and failures explicitly with a timeout; an arbitrary delay is not proof of readiness.
- Recreate application caches for the loading phase, then warm the scene before measuring drawing. Record this policy; do not claim to clear SQLite or operating-system disk caches.
- Repeat runs in alternating backend order and record order and warm-up policy.
- Record renderer-call duration distributions and paint callback intervals/counts. Where practical, use sample-side wrappers for source-read and tile-preparation timing. Never label paint cadence as display FPS.
- Include device/Android version, app/build identity, debugger state, resolved Mapsui/SkiaSharp versions, ActiveBackend, renderer, viewport pixels/density, scenario, cache policy, errors and measurements in a human-readable report.
- Copy results locally only. Exclude credentials and absolute paths; distinguish failed, cancelled and incomplete runs. Do not automatically declare a winner.

## Validation

Use the repository validation assessment before implementing. Test cancellation, serialization, readiness timeout, failed tiles and report completeness. Verify the workflow in Release without a debugger on physical Android hardware. Broader performance claims require at least two devices/GPU families; any single-device conclusion must remain device-specific. Use Android presentation traces separately if paint timing cannot explain an observed difference.

Update the rendering-options page with the exact cache policy and measurement limits when this work lands.
