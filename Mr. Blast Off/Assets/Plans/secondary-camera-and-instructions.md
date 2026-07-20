# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** Strategic sci-fi nuclear reactor engineering game with spherical gravity and flight mechanics.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Secondary Camera Positioning
- **Problem:** The secondary camera is positioned using `localScale.x * 2.01f` during the planet's detonation cinematic, placing it inside the bounds of large planets (especially those with irregular, non-uniform shapes like Planet D).
- **Goal:** Position the secondary camera far outside the visual/physics boundaries of any selected planet from a side-view angle, with the planet centered in the camera's view.
- **Solution:** Dynamically compute the visual/physical radius of the target planet using `GetPlanetRadius(GameObject)`, and position the camera at `3.2x` the actual radius along `Vector3.back` looking directly along `Vector3.forward`.

## Dynamic Top-Screen HUD Instructions
- **Problem:** Players have no on-screen guidance on how to navigate the spaceship cockpit upgrades, destination selection, and the kickoff planet landing/detonation steps.
- **Goal:** Display real-time, highly polished neon-framed instruction bars at the top of the game view guiding the player in each scene phase.
- **Solution:** Create a robust, programmatic UI generator in `GameManager.cs` that instantiates a unified, elegant horizontal top-bar instruction HUD. Update the instruction text contextually in real-time as the gameplay state advances (mission setup, element grid selection, terminal locating, critical countdown escaping, orbital piloting).

# UI
- **Instruction Panel styling:**
  - Height: `32` units, Width: `620` units, anchored at Top-Center `(0.5, 1)`, with Y position `-15` (15 pixels offset from screen top).
  - Background: Dark glass panel (`Color(0.04f, 0.04f, 0.06f, 0.88f)`) with a glowing neon cyan border outline (`Color(0f, 0.85f, 1f, 0.5f)`).
  - Font size: `11.5f` with center-middle alignment and rich-text markup support.

# Key Asset & Context
- `Assets/Scripts/GameManager.cs`: Controls scene transition callbacks, countdowns, and explosive secondary camera placements.
- `Assets/Scripts/CockpitManager.cs`: Controls cockpit UI navigation.

# Implementation Steps

### Step 1: Implement CreateInstructionsUI and GetPlanetRadius inside GameManager.cs
- **Description**:
  - Add private `_instructionsText` field.
  - Implement private helper `GetPlanetRadius(GameObject planetGo)` which scans for root `SphereCollider` or child `MeshFilter` boundaries.
  - Implement `CreateInstructionsUI(string sceneName)` which programmatically generates a stylized instruction HUD bar at the top of the screen.
  - Call `CreateInstructionsUI` and `UpdateInstructions` inside `OnSceneLoaded` and during critical state transitions (countdown trigger).
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Refactor TransitionToSecondaryCamera inside GameManager.cs
- **Description**:
  - Update `TransitionToSecondaryCamera()` to query `GetPlanetRadius()` on the detonating planet.
  - Set the target secondary camera distance to `3.2x` the computed planet radius.
  - Offset the position along `Vector3.back` and orient looking along `Vector3.forward` to perfectly center the planet in view.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Integrate Contextual Element Selection Instructions in CockpitManager.cs
- **Description**:
  - Inside `OnPlanetSelected()`, invoke `GameManager.Instance.UpdateInstructions()` to update the HUD banner to guide players through selecting 10 elements on the selection panel.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Verification and Clean Scene Saves
- **Description**: Enter playmode, verify instructions update seamlessly in the cockpit and in Kickoff phases, trigger core detonation, and verify that the explosion view from the secondary camera displays the planet centered and beautifully far away.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2, Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Scenic Secondary Camera Positioning Verification**:
   - In Kickoff scene, trigger detonation and fly to orbit.
   - Wait for countdown to reach 0.
   - Verify that the camera zooms out cleanly to show the entire planet in the center of the viewport, with no visual elements clipped.
2. **Top-Screen HUD Instructions Verification**:
   - Load Preparation scene. Verify top-screen banner reads: `MISSION SETUP: Select Planet to scan coordinates...`.
   - Click a planet. Verify banner dynamically transitions to: `STABILIZE CORE: Select exactly 10 elements...`.
   - Load Kickoff scene. Verify banner dynamically transitions to: `MISSION HAZARD: Locate and interact with the Control Terminal [F]...`.
   - Detonate reactor. Verify banner dynamically transitions to: `CRITICAL CORE DETONATION: Return to the Shuttle [F] and escape immediately!`.
   - Enter Shuttle. Verify banner dynamically transitions to: `ORBITAL ESCAPE: Pilot the Shuttle away from the planet into safe space orbit!`.
