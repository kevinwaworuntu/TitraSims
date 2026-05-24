# Product Requirements Document — TitraSims

**Version:** 1.1  
**Date:** 2026-05-23  
**Author:** Kevin Andrew  
**Engine:** Unity 6000.4.3f1  
**Platform:** Android (AR)

---

## 1. Executive Summary

TitraSims is an augmented reality (AR) mobile application that teaches pharmaceutical titration procedures through interactive 3D simulations. Students scan physical AR markers to overlay virtual lab equipment onto a real surface, then interact with step-by-step guided procedures for two titration methods. The goal is to supplement or replace physical lab sessions with an accessible, reusable, and safe virtual alternative.

---

## 2. Problem Statement

Pharmacy and chemistry students must learn titration techniques — non-aqueous (Titrasi Bebas Air) and complexometric — that require expensive equipment, chemicals, and supervised lab access. Limited lab time, equipment shortages, and safety concerns reduce the quality and frequency of hands-on practice. Students lack a self-paced tool for learning correct procedures before or between physical lab sessions.

---

## 3. Goals & Success Metrics

| Goal | Metric |
|---|---|
| Students can complete both titration simulations without instructor guidance | ≥ 80% completion rate per mode |
| AR tracking is reliable across common lighting conditions | AR marker detection in < 3 seconds under normal indoor light |
| Sequential progress prevents skipping steps | Zero ability to unlock stage N without completing N-1 |
| App runs without crashes on target Android devices | Zero crash-on-launch on target device tier |
| Students understand procedure steps via narration | Narration plays correctly on every interaction |

---

## 4. Scope

### In Scope
- Two titration modes: **TBA** (Titrasi Bebas Air / Non-Aqueous) and **TK** (Titrasi Kompleksometri / Complexometric)
- 10 sequential stages per mode
- AR marker-triggered interactive 3D lab equipment
- Step-by-step interactions per stage (animations + audio narration)
- Progress saving and sequential stage unlocking
- Contextual UI buttons for user actions within interactions
- Measuring glass, burette, and analytical balance interaction mechanics
- Indicator dropper tap mechanic (1 tap = 1 drop)
- Stirring gesture (rotate) after liquid mixing steps
- Zoom-to-read burette scale before and after titration
- Funnel (corong) attach and detach interaction
- Liquid transfer (drag/pour) between containers
- In-app info/instruction panels per stage

### Out of Scope
- Multiplayer or networked features
- User accounts or cloud sync
- Real-time chemical formula computation
- iOS support (Android-only at launch)
- Assessment/grading system

---

## 5. Users

**Primary:** Pharmacy/chemistry undergraduate students practicing titration procedures.

**Secondary:** Instructors using the app for in-class demonstration.

**Assumed context:** Students have limited prior AR app experience. Instructions must be self-contained with no external reference material required.

---

## 6. Functional Requirements

### 6.1 Application Entry & Navigation

| ID | Requirement |
|---|---|
| F-01 | App opens to a main menu with options to select game mode or reset progress |
| F-02 | User can select TBA (Titrasi Bebas Air) or TK (Titrasi Kompleksometri) mode from a mode selection screen |
| F-03 | Navigation supports a back stack — back button returns to the previous panel in history |
| F-04 | User can reset all progress from the main menu (clears PlayerPrefs for both modes) |

### 6.2 Stage Selection & Unlocking

| ID | Requirement |
|---|---|
| F-05 | Each mode displays 10 stages in a stage selection list |
| F-06 | Stage 1 is unlocked by default; stage N unlocks only after stage N-1 is marked complete |
| F-07 | Completed stages show a distinct visual state (different alpha/color) |
| F-08 | Locked stages are visually disabled and non-interactable |
| F-09 | Progress is persisted between app sessions using PlayerPrefs |

### 6.3 AR Scanning & Marker Detection

| ID | Requirement |
|---|---|
| F-10 | On entering a stage, the app shows an AR scanning view instructing the user to point at the AR marker |
| F-11 | The correct AR marker(s) for the selected stage are activated; all others are deactivated |
| F-12 | When the marker is detected, the 3D lab content for that stage overlays the marker |
| F-13 | When the marker is lost, content remains but interaction buttons are hidden |
| F-14 | Stage-specific AR content (prefab) is positioned and scaled relative to the detected marker |

### 6.4 Stage Interactions & Procedure Flow

| ID | Requirement |
|---|---|
| F-15 | Each stage contains one or more sequential interactions (TahapanInteractionData) |
| F-16 | An interaction consists of: title text, description text, audio narration, and an animation clip |
| F-17 | On entering an interaction, the narration panel displays the title and description |
| F-18 | The "Play Animation" button triggers the animation and plays the audio narration simultaneously |
| F-19 | The next interaction button becomes active only after the current animation and audio finish |
| F-20 | After the last interaction in a stage, the "Complete" button appears to mark the stage done |
| F-21 | Completing a stage saves progress and returns the user to stage selection |

### 6.5 Burette Fill Interaction

| ID | Requirement |
|---|---|
| F-22 | Burette interaction presents three fill buttons: +0.1 ml, +1 ml, and +5 ml |
| F-23 | Each button tap increments the displayed volume and scales the fluid object accordingly |
| F-24 | The meniscus position updates visually as volume changes |
| F-25 | When the filled volume reaches the target range, fluid color transitions from initial color to target color |
| F-26 | The "Next" button activates only after the volume is within the valid target range |
| F-26a | Before titration begins, user zooms in on the burette scale to record the starting volume |
| F-26b | After titration is complete, user zooms in again to record the final volume |
| F-26c | Each titration stage supports multiple erlenmeyer flasks (blanko, sampel 1, sampel 2) as separate sub-interactions |

### 6.6 Measuring Glass Fill Interaction

| ID | Requirement |
|---|---|
| F-27 | Measuring glass interaction supports two separate liquid containers |
| F-28 | Each container has its own fill buttons (1 ml and 10 ml variants as configured) |
| F-29 | Fill animations and fluid scaling update independently per container |
| F-30 | Completion condition is met when both containers reach their respective target volumes |

### 6.7 Analytical Balance (Weighing) Interaction

| ID | Requirement |
|---|---|
| F-31 | Balance interaction displays a weight value on screen |
| F-31a | User first places the watch glass (kaca arloji) onto the balance pan, then taps Tare to zero the scale |
| F-32 | User increments the sample weight in 25 mg steps via a button |
| F-33 | The displayed value updates with each tap |
| F-34 | Interaction is considered complete when the weight reaches the target value |

### 6.8 Indicator Dropper Interaction

| ID | Requirement |
|---|---|
| F-42 | Dropper interaction presents the pipet tetes (dropper) over the target flask |
| F-43 | Each tap delivers one drop; the drop animation plays per tap |
| F-44 | Target is 2–3 drops per flask; "Next" activates after the minimum drop count is reached |
| F-45 | For Titrasi Kompleksometri, the flask liquid color changes visually on indicator addition (clear → reddish-purple) |

### 6.9 Stirring (Rotate) Gesture

| ID | Requirement |
|---|---|
| F-46 | After liquid addition steps, the user performs a rotate gesture on screen to stir the flask |
| F-47 | A stirring animation plays in response to the gesture |
| F-48 | The "Next" button activates only after the stirring gesture is completed |

### 6.10 Funnel (Corong) Attach / Detach

| ID | Requirement |
|---|---|
| F-49 | Certain stages require the user to tap to attach a funnel to the target container before filling |
| F-50 | After filling is complete, the user taps to detach the funnel |
| F-51 | The "Next" button is gated until both attach and detach steps are performed in order |

### 6.11 Liquid Transfer (Drag / Pour)

| ID | Requirement |
|---|---|
| F-52 | Some steps require the user to drag a source container toward the target container to pour liquid |
| F-53 | A pour animation plays when the drag gesture is completed |
| F-54 | The interaction registers as complete after the pour animation finishes |

### 6.12 Zoom-to-Read (Burette Scale)

| ID | Requirement |
|---|---|
| F-55 | At designated reading steps, the view zooms in on the burette scale automatically or via tap |
| F-56 | The current volume value is displayed as an overlay during the zoom |
| F-57 | User confirms the reading by tapping a button, then the view returns to normal |

### 6.13 Info & Narration UI

| ID | Requirement |
|---|---|
| F-35 | A narration panel shows the current interaction's title and description text during the interaction |
| F-36 | An info panel is available per stage, accessible via a dedicated button, showing the stage's instructional text |
| F-37 | Instructional text supports basic HTML-like inline tags (e.g., `<sub>` for subscripts in chemical formulas) |
| F-38 | Text styling (font, size, alignment, bold, spacing) is configurable per mode via InfoStyleConfig ScriptableObject |

### 6.14 Contextual Action Buttons

| ID | Requirement |
|---|---|
| F-39 | Contextual buttons are generated dynamically at runtime based on the current interaction |
| F-40 | Each button has a text label and a registered callback action |
| F-41 | Buttons are enabled or disabled based on interaction state (e.g., disabled during animation playback) |

---

## 7. Non-Functional Requirements

| ID | Requirement |
|---|---|
| NF-01 | App targets Android API level compatible with ARCore |
| NF-02 | AR marker detection must work under standard indoor fluorescent lighting |
| NF-03 | Audio narration must play without noticeable delay after the play button is tapped |
| NF-04 | App must not require an internet connection at runtime |
| NF-05 | AR content must remain stable (no excessive jitter) while the marker is in frame |
| NF-06 | App must support two-finger drag for repositioning 3D content |

---

## 8. System Architecture

### 8.1 Scene Structure

| Scene | Purpose |
|---|---|
| `ARPharma.unity` | Main runtime scene (build index 0); contains all gameplay and AR |
| `Animasi.unity` | Animation preview and testing (editor only) |
| `UI.unity` | UI layout and testing (editor only) |

### 8.2 Core Systems

**GameManager (Singleton)**  
Global state. Tracks current mode (TBA / Kompleksometri), current stage index, stage completion, and AR marker activation. Persists progress via PlayerPrefs.

**UIManager (Singleton)**  
All panel visibility. Maintains a history stack for back navigation. Controls button enable/disable states in response to interaction events.

**TahapanInteractionController**  
Drives the sequential interaction flow within a stage. Fires events to synchronize UI state (narration display, button availability) with animation/audio playback.

**TahapanInteractionPlayer**  
Executes a single TahapanInteractionData: overrides the animation clip via AnimatorOverrideController, plays audio narration, signals completion when both finish.

**ARContentManager (base class)**  
Bridges Vuforia target tracking events to gameplay. Shows/hides contextual buttons based on marker found/lost state.

### 8.3 Data Architecture

```
TahapanInteractionData (ScriptableObject)
  ├── title: string
  ├── description: string
  ├── narrationClip: AudioClip
  └── animationClip: AnimationClip

TahapanData
  ├── markerName: string
  └── infoPanel: GameObject

AnimationConfig (ScriptableObject)
  ├── animatorOverrideController: AnimatorOverrideController
  ├── playTrigger: string ("PlayAnimation")
  ├── stopTrigger: string ("StopAnimation")
  └── clipEntryName: string

InfoStyleConfig (ScriptableObject)
  └── style: InfoStyleStruct (font, size, alignment, bold, spacing, margins)

BuretteStageConfig (ScriptableObject)
  ├── fillObjectScaleModifier: float
  ├── fillObjectInitialScale: float
  ├── meniskusPositionModifier: float
  ├── meniskusInitPos: float
  ├── initialColor: Color
  ├── targetColor: Color
  ├── weights[]: float
  ├── floorTargets[]: float
  └── ceilTargets[]: float

GelasUkurStageConfig (ScriptableObject)
  ├── fillObjectScaleModifier: float
  ├── fillObjectInitialScale: float
  ├── meniskusPositionModifier: float
  └── meniskusInitPos: float

TimbanganStageConfig (ScriptableObject)
  ├── targetWeight: float
  ├── stepAmountMg: float
  ├── scaleModifier: float
  └── initialScale: float
```

### 8.4 Progress Storage (PlayerPrefs Keys)

| Key | Type | Description |
|---|---|---|
| `LastCompletedTahapTBA` | int | Index of last completed TBA stage |
| `LastCompletedTahapKomp` | int | Index of last completed Kompleksometri stage |

### 8.5 Key Dependencies

| Package | Version | Role |
|---|---|---|
| com.ptc.vuforia.engine | 11.4.4 | AR marker detection |
| com.unity.xr.arfoundation | 6.4.2 | AR session management |
| com.unity.xr.arcore | 6.4.2 | Android AR plane/tracking |
| com.unity.xr.interaction.toolkit | 3.4.0 | XR interaction handling |
| com.unity.render-pipelines.universal | 17.4.0 | URP rendering (mobile profile) |
| com.unity.inputsystem | 1.19.0 | Touch/pointer input |
| TextMesh Pro | — | Advanced UI text rendering |

---

## 9. Content Specification

### 9.1 Titration Modes

**TBA — Titrasi Bebas Air / Non-Aqueous Titration (10 stages)**  
Reagents: perchloric acid (asam perklorat), acetone (aseton), glacial acetic acid (asam asetat glasial), acetic anhydride (anhidrida asetat), benzene (benzena).  
Covers: equipment preparation, burette preparation (perchloric acid), primary standard solution (potassium phthalate + glacial acetic acid + acetic anhydride + benzene), blank solution preparation, sample weighing (Tare + 25 mg steps), sample solution preparation, indicator addition (2–3 drops, 1 tap = 1 drop), calibration (3 trials), blank titration (zoom → 5/1/0.1 ml buttons → zoom), sample titrations ×2.

**TK — Titrasi Kompleksometri / Complexometric Titration (10 stages)**  
Reagents: EDTA, CaCO₃ (baku primer), aquades, HCl, NaOH, hydroxynaphtol blue indicator.  
Covers: equipment preparation, burette preparation (EDTA), primary standard solution (CaCO₃ + aquades + HCl + EDTA + NaOH), buffer preparation (aquades + HCl 3N + NaOH via gelas ukur), sample weighing (Tare + 25 mg steps), sample solution preparation, indicator addition (hydroxynaphtol blue, 2–3 drops, color change: clear → reddish-purple), calibration (3 trials), blank titration (zoom → 5/1/0.1 ml buttons → zoom), sample titrations ×2.

### 9.2 Per-Stage Assets

Each of the 20 stages (10 TBA + 10 TK) requires:
- One or more `TahapanInteractionData` ScriptableObjects (animation + audio + text)
- An AR marker image
- A stage-specific prefab (see 9.3)
- Instructional text entry in `InfoTextData` ScriptableObject

### 9.3 Stage Prefab List

Prefabs live under `Assets/_TitraSims/Prefabs/Stages/`. Each prefab is self-contained: 3D models, `ARContentManager` subclass, `TahapanInteractionController`, `Animator`, and references to its stage config ScriptableObjects.

Stages 08–10 in both modes share the same burette interaction structure — they differ only in their `BuretteStageConfig` asset and 3D content.

**TBA — Titrasi Bebas Air**

| # | Prefab Name | Procedure Summary |
|---|---|---|
| 01 | `Stage_TBA_01_PersiapanAlat.prefab` | Equipment identification & preparation |
| 02 | `Stage_TBA_02_PersiapanBuret.prefab` | Burette prep with perchloric acid (aseton rinse → corong → fill → attach to statif) |
| 03 | `Stage_TBA_03_PersiapanLarutanBaku.prefab` | Primary standard solution (kalium biftalat + asam asetat glasial → aduk) |
| 04 | `Stage_TBA_04_PenambahanPelarut.prefab` | Solvent addition to gelas ukur (anhidrida asetat + benzena → tuang → aduk) |
| 05 | `Stage_TBA_05_PenimbanganSampel.prefab` | Sample weighing (kaca arloji → Tare → +25 mg steps) |
| 06 | `Stage_TBA_06_PersiapanSampel.prefab` | Sample solution prep (corong → sampel → anhidrida asetat + benzena → aduk) |
| 07 | `Stage_TBA_07_PenambahanIndikator.prefab` | Indicator drops (pipet tetes → 2–3 tetes per Erlenmeyer) |
| 08 | `Stage_TBA_08_Kalibrasi.prefab` | Calibration — 3 trials (zoom → 5/1/0.1 ml buttons → zoom) |
| 09 | `Stage_TBA_09_TitrasiBlanko.prefab` | Blank titration (zoom → 5/1/0.1 ml buttons → zoom) |
| 10 | `Stage_TBA_10_TirasiSampel.prefab` | Sample titration × 2 (sampel 1 + sampel 2 as sub-interactions) |

**TK — Titrasi Kompleksometri**

| # | Prefab Name | Procedure Summary |
|---|---|---|
| 01 | `Stage_TK_01_PersiapanAlat.prefab` | Equipment identification & preparation |
| 02 | `Stage_TK_02_PersiapanBuretEDTA.prefab` | Burette prep with EDTA (corong → bilas → fill → attach to statif) |
| 03 | `Stage_TK_03_PersiapanLarutanBakuCaCO3.prefab` | Primary standard CaCO₃ (corong → CaCO₃ → aquades → HCl → EDTA → NaOH → aduk) |
| 04 | `Stage_TK_04_PersiapanBuffer.prefab` | Buffer prep (aquades + HCl 3N drops + NaOH via gelas ukur → aduk) |
| 05 | `Stage_TK_05_PenimbanganSampel.prefab` | Sample weighing (kaca arloji → Tare → +25 mg steps) |
| 06 | `Stage_TK_06_PersiapanSampel.prefab` | Sample solution prep (corong → sampel → aquades + NaOH → aduk) |
| 07 | `Stage_TK_07_PenambahanIndikator.prefab` | Indicator drops (biru hidroksinaftol → 2–3 tetes, color: clear → reddish-purple) |
| 08 | `Stage_TK_08_Kalibrasi.prefab` | Calibration — 3 trials (zoom → 5/1/0.1 ml buttons → zoom) |
| 09 | `Stage_TK_09_TitrasiBlanko.prefab` | Blank titration (zoom → 5/1/0.1 ml buttons → zoom) |
| 10 | `Stage_TK_10_TirasiSampel.prefab` | Sample titration × 2 (sampel 1 + sampel 2 as sub-interactions) |

### 9.3 3D Lab Equipment Models

| Model | Used In |
|---|---|
| Analytical Balance | TBA, TK (weighing stages) |
| Watch Glass (kaca arloji) | TBA, TK (placed on balance before Tare) |
| Burette + fluid | TBA, TK (titration stages) |
| Burette Stand (statif) | TBA, TK (burette preparation) |
| Funnel (corong) | TBA, TK (burette filling, Erlenmeyer filling) |
| Measuring Glass + fluid | TBA (pelarut stage), TK (buffer stage) |
| Beaker + fluid | Multiple stages |
| Erlenmeyer Flask (×3) | Multiple stages (blanko + sampel 1 + sampel 2 per titration) |
| Dropper / Pipet Tetes | TBA, TK (indicator addition stages) |
| Bottle with Dropper | Indicator addition stages |

---

## 10. Interaction Design

### Stage Flow

```
Main Menu
  └── Mode Selection (TBA / TK)
        └── Stage List (10 stages, sequential unlock)
              └── AR Scan View (detect marker)
                    └── Stage Content (3D overlay on marker)
                          └── Interaction 1 → Interaction N → Complete
                                └── Return to Stage List
```

### Button States During Interaction

| State | Play Animation | Next Interaction | Complete Stage |
|---|---|---|---|
| Before play | Enabled | Disabled | Hidden |
| During animation | Disabled | Disabled | Hidden |
| After animation (mid-stage) | Enabled | Enabled | Hidden |
| After last animation | Enabled | Hidden | Enabled |

---

## 11. Out-of-Scope / Future Considerations

- iOS (ARKit) support
- Cloud-synced progress per student account
- Instructor dashboard for tracking class progress
- Assessment/quiz mode after stage completion
- Localization beyond Bahasa Indonesia
- Additional titration types (redox, precipitation)
- Offline audio narration caching (narrations currently loaded from assets)

---

## 12. Glossary

| Term | Definition |
|---|---|
| TBA | Titrasi Bebas Air — Non-Aqueous Titration |
| TK | Titrasi Kompleksometri — Complexometric Titration |
| Tahapan | Stage/step in the procedure |
| Burette | Graduated glass tube used to deliver precise liquid volumes during titration |
| Meniscus | Curved upper surface of liquid in a graduated container |
| Corong | Funnel; used to fill burettes and flasks without spillage |
| Kaca Arloji | Watch glass; placed on the balance pan before Tare for sample weighing |
| Pipet Tetes | Dropper pipette; used to add indicator drops to flasks |
| Statif | Laboratory stand; holds the burette vertically during titration |
| Blanko | Blank solution; a control sample run before the actual sample titration |
| Gestur Putar | Rotate gesture; the swirl gesture used to stir/mix liquid in a flask |
| Tare | Balance function that zeros the displayed weight with the container on the pan |
| AR Marker | Printed image used as a reference target for augmented reality overlay |
| ScriptableObject | Unity data container asset, used here for interaction and config data |
| AnimatorOverrideController | Unity mechanism for swapping animation clips at runtime without changing the Animator graph |
