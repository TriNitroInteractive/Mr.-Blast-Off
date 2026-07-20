# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: "Mr. Blast Off" is a 2D sci-fi strategic engineering game for a game jam themed "Kickoff". Players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), optimizing and configuring a reactor core for a single controlled detonation to vaporize celestial hazards (moons, asteroids, rogue planets).
- **Players**: Single player
- **Inspiration / Reference Games**: Space Chem, Reactor simulators, arcade space strategy games
- **Tone / Art Direction**: Sci-fi retro holographic UI, neon glows, slightly comedic but high-stakes tone
- **Target Platform**: Standalone Windows (PC)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics 
## Core Gameplay Loop
The player selects a target celestial body (planet/moon/asteroid) from the Cockpit workspace. Based on the selected planet's safety threshold, the player enters the Element Selection phase where they choose exactly 10 radioactive or structural elements (moderators, shields, actinides, thermonuclear fuels) to balance core reactor yield and reach/exceed the required reaction points. Once optimized, they trigger the "Kickoff" reaction, and play the arcade-style core-priming cleanup phase (using control terminals, avoiding hazards, and escaping in the Shuttle).

## Controls and Input Methods
- **Keyboard / Mouse**: Navigate UI, click to select planets, upgrade core modules, and click elements to slot them into the reactor matrix.
- **New Input System**: Key presses and pointer interaction events are fully integrated using Unity's modern input architecture.

# UI
- **Preparation Workspace**: Contains multiple sub-panels under the main UI `Canvas`:
  - **Loading Plot**: Initial introductory screen that types out the thematic context (with a pulsing "Press Any Key" prompt).
  - **Cockpit Panel**: Displays locked planet destinations, stats, upgrades, and shuttle preview.
  - **Element Selection Panel**: Displays 30 selectable nuclear elements with descriptions, yields, and a stabilization progress tracker.
  - **GameplayInstructionsHUD**: Floating header banner that guides the player on current objectives.

# Key Asset & Context
- **Assets/Scripts/GameManager.cs**: Oversees game lifecycle, transitions, scene loading, and generates the dynamic `GameplayInstructionsHUD` banner.
- **Assets/Scripts/LoadingPlotController.cs**: Manages the introductory narrative and skip transitions.

# Implementation Steps

### Step 1: Secure Initial HUD Inactivity and State Correction in GameManager
- **Description**: 
  - Refine `OnSceneLoaded` in `GameManager.cs` to explicitly toggle instruction HUD visibility on active loading plot detection.
  - Find `LoadingPlotController` in the scene using `FindObjectsInactive.Include`. If present, set `SetInstructionsVisible(false)` initially, so the HUD is completely hidden while the story is running.
  - If a loading plot controller is NOT found in the scene, ensure instructions are shown by default using `SetInstructionsVisible(true)`.
  - Refine `CreateInstructionsUI` to use `FindObjectsInactive.Include` when checking for the presence of `LoadingPlotController`, preventing any editor-time state activation race conditions.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Set Instruction HUD Active on Cockpit Transition
- **Description**:
  - In `LoadingPlotController.cs` inside `TransitionToCockpit()`, ensure that when transitioning to the Cockpit Panel, `GameManager.Instance.SetInstructionsVisible(true)` is successfully called to show the HUD correctly. (This is already set, but will be double-checked and verified).
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

# Verification & Testing

## Manual Verification
1. **Scene Transition Verification**:
   - Open `Preparation` scene. Ensure the `Element Selection` panel or `Cockpit` panel are inactive, and enter Play Mode.
   - Verify that the `GameplayInstructionsHUD` is **completely invisible** during the typing out of the Loading Plot story.
   - Press any key (or mouse-click) to complete/skip the Loading Plot.
   - Verify that the `Loading Plot` fades out/deactivates, the `Cockpit` panel becomes active, and the `GameplayInstructionsHUD` smoothly appears on the canvas.
2. **Editor-Time Robustness**:
   - Toggle the `Loading Plot` panel to inactive in the scene hierarchy while in the editor, and then enter Play Mode.
   - Verify that the GameManager automatically corrects states, turning `Loading Plot` on, keeping `GameplayInstructionsHUD` off until skipped.

## Automated Verification
1. Run Play Mode Tests in the Editor (`Window -> General -> Test Runner`).
2. Verify all existing tests, including `PlayModeTestRunner`'s Element Selection phase checking, pass successfully with no errors or regressions.
