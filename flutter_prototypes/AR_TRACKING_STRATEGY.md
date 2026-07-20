# AR Tracking Strategy

> **Document status:** Phase 1 baseline — created before Task 1.0 implementation.
> Update this document after each AR validation milestone (arcoreimg score, emulator test, physical device test).

---

## 1. Overview

The app overlays 30+ POI markers onto a 23-metre panoramic tile panel at the Museu de Lisboa.
AR tracking must reliably detect and stabilise the panorama image within 2–3 seconds of the user
pointing their device at it.

This document records:
- The tracking approach chosen and why
- The arcoreimg quality score for the reference image
- Known constraints and mitigations
- The validation plan and current status

---

## 2. Tracking Approach

**Chosen: ARCore/ARKit Augmented Images (single reference image)**

The panorama panel is a fixed, large, high-contrast printed artwork — ideal for image-based
tracking. The user is always in the same physical location (museum floor) relative to the panel.

**Why not SLAM / feature-point tracking?**
SLAM tracks arbitrary environments but drifts over time without a fixed anchor. A 23m panel 10m
from the viewer would accumulate unacceptable positional error. Image tracking re-anchors on every
frame, giving deterministic POI placement.

**Why not QR / fiducial markers?**
The museum panel cannot be modified. No physical markers can be added. Image tracking on the
existing artwork is the only viable approach.

**Fallback: section-based tracking (if full panel score < 75)**
Split the panorama into 3–5 horizontal sections with distinct architectural detail. Each section
is a separate ARCore reference image. The app activates the nearest-section anchor based on device
orientation. This increases implementation complexity but recovers tracking if the full-width image
scores poorly due to featureless sky or uniform tile areas.

---

## 3. arcoreimg Quality Score

> **Status: PENDING — run the tool before starting Task 1.0 code.**

```
arcoreimg eval-img --input_image_path=assets/images/panorama/<panorama_filename.jpg>
```

Download the tool: https://github.com/google-ar/arcoreimg (Windows binary included)

| Image                       | Score | Notes                          |
| --------------------------- | ----- | ------------------------------ |
| Full panorama (placeholder) | TBD   | Run before any AR session code |

**Acceptance threshold: >= 75**

If score < 75:
1. Try a higher-contrast crop of the most architecturally rich section.
2. If still < 75, implement section-based fallback (see Section 2).
3. Document final score and chosen image in this table before proceeding.

---

## 4. Physical Dimensions

ARCore tracking accuracy improves significantly when `widthInMeters` is passed to the image
database. For large images (> 75 cm) it is not optional — without it, pose stabilisation requires
the user to move the device before the anchor locks, which is unacceptable UX.

| Context                | Width (m) | Height (m) | Source                |
| ---------------------- | --------- | ---------- | --------------------- |
| Museum panel (actual)  | ~23       | TBD        | Confirm with museum   |
| A3 printed proxy (dev) | 0.420     | TBD        | Measure printed image |

**Action required:** Measure the printed A3 proxy before emulator testing. Enter the values in
`lib/domains/panorama/ar/config/panorama_reference_images.dart` as `kPanelPhysicalWidthMeters`.
Confirm museum dimensions with the institution for the production build.

---

## 5. Reference Image Location

The ARCore reference image is registered in:

```
lib/domains/panorama/ar/config/panorama_reference_images.dart
```

The compile-time flag `kUseRealAR` in `ar_infrastructure_providers.dart` controls whether the real
ARCore session or the mock tracker is used. The debug overlay flag `kShowARDebugOverlay` adds a
real-time overlay showing current tracking state, pose quality, and marker counts.

---

## 6. Validation Plan

### Layer 4a — Android Emulator (priority 1)

Validates the full ARCore pipeline without a physical device trip.

**Setup:**
1. AVD: Pixel 6 equivalent, x86_64, API 27+, Google APIs, Camera Back = VirtualScene
2. Install ARCore APK: `adb install -r Google_Play_Services_for_AR_<version>_x86.apk`
3. Extended Controls > Camera > add panorama JPEG as virtual scene image
4. Build with `--dart-define=USE_REAL_AR=true --dart-define=SHOW_AR_DEBUG=true`

**Confirm x86 ABI support in `android/app/build.gradle`:**
Debug builds must include `"x86"` in `abiFilters` for NDK components to load in the emulator.

**Pass criteria:**
- ARCore session initialises without crash
- Reference image detected within 5 seconds of pointing virtual camera at it
- At least one POI marker appears at the correct relative position on screen
- Tracking state transitions: INITIALIZING → TRACKING → TRACKING_NORMAL

### Layer 4b — Physical Android (priority 2)

Run on the wirelessly-connected Android device with the printed A3 proxy image.

**Pass criteria:**
- Detection within 2–3 seconds in normal indoor lighting
- Markers remain stable when device moves slowly
- Reopening the panorama page re-detects within 2 seconds

### Layer 4c — Physical iPhone (optional, later)

iOS Simulator has no camera and no ARKit. Physical iPhone required.
Confirm `IPHONEOS_DEPLOYMENT_TARGET = '15.0'` in Podfile before test.

---

## 7. Known Constraints

| Constraint                   | Impact                               | Mitigation                            |
| ---------------------------- | ------------------------------------ | ------------------------------------- |
| 23m panel — very large image | Pose stabilises slower               | Pass exact widthInMeters to ARCore    |
| Museum lighting variable     | Detection quality varies by time/day | Test at different lighting conditions |
| Tile surface may reflect     | Glare reduces feature quality        | Test with and without polarised glass |
| iOS Simulator has no ARKit   | No emulator shortcut for iOS         | Physical iPhone required for iOS      |
| x86 emulator ABI             | App must include x86 ABI for NDK     | Add x86 to abiFilters in debug build  |

---

## 8. Validation Status Log

| Date       | Test                        | Result  | Notes                       |
| ---------- | --------------------------- | ------- | --------------------------- |
| 2026-03-09 | Document created            | —       | Pre-code baseline           |
| TBD        | arcoreimg score             | PENDING | Run before Task 1.0 code    |
| TBD        | Android Emulator pipeline   | PENDING | Layer 4a validation         |
| TBD        | Physical Android (A3 proxy) | PENDING | Layer 4b validation         |
| TBD        | Physical museum panel       | PENDING | Final real-world validation |

---

*Next update:* After arcoreimg score is confirmed. Do not proceed to Task 1.0 code without that score.
