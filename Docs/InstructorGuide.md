# TitraSims — Instructor Guide

For lecturers and lab staff running TitraSims with a class: what it covers, how
to get it onto devices, how to prepare the markers, and what to expect in the
room.

For the student-facing walkthrough, hand out
[`UserGuide.md`](UserGuide.md). For the codebase, see
[`Architecture.md`](Architecture.md).

---

## Contents

1. [What the app covers](#what-the-app-covers)
2. [Device requirements](#device-requirements)
3. [Installing](#installing)
4. [Preparing the markers](#preparing-the-markers)
5. [Room setup](#room-setup)
6. [How progression works](#how-progression-works)
7. [Running a session](#running-a-session)
8. [Between students](#between-students)
9. [Reporting problems](#reporting-problems)

---

## What the app covers

Two procedures, ten stages (*tahapan*) each, worked through in order:

| | Titrasi Bebas Air (TBA) | Titrasi Kompleksometri (TK) |
|---|---|---|
| Sample | Caffeine, 400 mg | CaCO₃, 200 mg |
| Titrant | Perchloric acid standard | EDTA standard, 0.05 M |
| Primary standard | Potassium biphthalate, 700 mg | CaCO₃, 200 mg |
| Indicator | Crystal violet | Hydroxynaphthol blue |
| Solvents | Glacial acetic acid, acetic anhydride, benzene | Distilled water, HCl, NaOH |

Both follow the same shape: preparation of equipment and materials, burette
preparation, primary standard, blank, weighing, sample solution, indicator, then
standardisation (triplicate), blank titration, and sample titration
(triplicate).

**Scope.** The app teaches *procedure and technique* — the order of operations,
what goes into what, reading the burette. It does **not** do the calculations.
Students record start and end burette volumes in the activity book for stages 8,
9 and 10 and work out the titre and assay themselves. Plan your worksheet
accordingly.

---

## Device requirements

| | |
|---|---|
| OS | Android 10 (API 29) or newer |
| AR | Google ARCore support required |
| Architecture | ARM64 |
| Camera | Working rear camera, camera permission granted |
| Network | Not required — the app runs fully offline |

Devices are ARM64 only; older 32-bit hardware will not install the build. If you
are provisioning a device cart, verify ARCore support per model before buying —
Google publishes a supported-device list, and a device that lacks it will
install the app but never track a marker.

---

## Installing

The app is distributed as an Android APK (package id `com.Unpad.TitraSims`).

1. On each device, allow installation from your chosen source (a file manager,
   MDM, or your institution's distribution channel).
2. Install the APK.
3. Open the app once and **grant camera permission**. Doing this during
   provisioning saves a class-wide interruption later.
4. Confirm it works: open any tahapan and scan its marker. If equipment appears,
   the device is ready.

If you are managing a device cart, install and verify on one device first, then
clone or repeat.

---

## Preparing the markers

The app uses **twenty distinct image markers** — one per tahapan, ten per
procedure. They are not interchangeable: tahapan 3 of TBA only responds to the
TBA 3 marker.

Source images live in the project at
`Assets/_TitraSims/Art/Markers/V2/`, named `tba1.png`–`tba10.png` and
`tk1.png`–`tk10.png`. These are bound into the app as instant image targets, so
there is no database file to distribute — only the printed pages matter.

**Printing**

- Print onto **matte** paper. Glossy stock reflects overhead lighting and
  defeats tracking.
- Print each marker at a consistent size — the app is configured for a target
  around **20 cm square**. Much smaller and students must hold the phone
  uncomfortably close; much larger and the whole marker may not fit in frame.
- Keep the full marker on one flat page with a clear margin around it. Do not
  crop, overlap, or print across a fold.
- Label each page clearly with its procedure and tahapan number so students can
  find the right one.

These pages are what the app calls the **activity book**. The same book should
carry the tables where students record their burette readings.

**Replacing worn pages.** Creased, stained or faded markers track badly. Keep a
spare set and swap pages out rather than letting a class fight with a bad one.

---

## Room setup

- **Flat surfaces.** Books must lie flat on a desk. A curled page will not
  track.
- **Even, diffuse light.** Bright but indirect is ideal.
- **No direct glare.** A window or downlight reflecting off the page is the most
  common failure in practice. Move tables rather than turning lights off — dim
  rooms fail too.
- **Room to work.** Students hold the phone 20–30 cm above the page and gesture
  with the other hand, so they need desk space and a stable position.

---

## How progression works

Stages unlock **sequentially**: a student can only enter a tahapan if they have
completed the one before it. Stage 1 is always available. Locked stages appear
dimmed and do not respond.

A stage counts as completed only when the student reaches the last step and taps
**Selesai**. Leaving with the back arrow discards progress within that stage but
keeps everything already completed.

**Progress is stored on the device, not per student.** There are no accounts or
logins. Two students sharing a tablet share one progress record, per procedure.
TBA and TK progress are tracked separately.

The in-app **Reset** button clears progress for **both** procedures at once and
cannot be undone.

---

## Running a session

A workable shape for a single lab period:

1. **Before class** — devices charged, camera permission already granted,
   activity books printed and sorted, tables positioned away from glare.
2. **Brief the gestures** — one finger moves, two fingers rotate, pinch resizes,
   circular motion stirs. The in-app **Cara Bermain** screen covers this; five
   minutes here saves a lot of individual troubleshooting.
3. **Set the expectation about recording** — stages 8, 9 and 10 produce the
   numbers, and the app will not remember them. Volumes go in the book, before
   and after each titration, three times over for the triplicate stages.
4. **Let students work in order.** The sequential lock means you cannot send a
   group straight to stage 8 for a demonstration on a device that has not
   completed 1–7.
5. **Budget time for stages 8–10.** Each of those stages runs in triplicate and
   plays a full titration animation every time, so they take considerably longer
   than the preparation stages.

If you want to demonstrate a late stage on your own device, either work through
the earlier stages once beforehand, or ask the development team about a build
with the unlock override enabled.

---

## Between students

Progress lives on the device, so before handing a device to a new student:

- Tap **Reset** on the home screen to clear both procedures, **or**
- Accept that the next student continues from where the last one stopped.

For assessment, decide which of these you want *before* the session — there is
no per-student record to fall back on, and no way to recover progress once Reset
is used.

---

## Reporting problems

When something goes wrong, the useful details are:

- Which procedure (TBA or TK), which tahapan number, and which step.
- What you expected versus what happened.
- Device model and Android version.
- Whether it reproduces on another device.

Send these to the development team along with a photo or screen recording where
possible.
