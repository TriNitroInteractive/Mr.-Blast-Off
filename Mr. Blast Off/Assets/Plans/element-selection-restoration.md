# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: "Mr. Blast Off" is a 2D sci-fi engineering/strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards (moons, asteroids, rogue planets) in a single controlled detonation.
- Players: Single player
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape 1920x1080
- Render Pipeline: PC_RPAsset (URP / custom)

# Game Mechanics
## Core Gameplay Loop
1. **Preparation Scene**: The player chooses exactly 10 scientific/nuclear elements from the Element Selection panel.
2. **Kickoff Scene**: After pressing Start, the game loads the Kickoff scene, spawning the player inside the Shuttle to fly around and execute planetary destruction.

## Controls and Input Methods
- **Mouse Left-Click**: Select/deselect elements inside the selection grid in the Preparation scene.
- **Button Click**: Press "Start" to transition to the Kickoff scene.

# UI
## Element Selection Panel Restoration
We will restore the original perfect layout, sizing, scaling, and positioning of the Element Selection UI inside the `Preparation` scene:
- **Canvas Scaler**: Matches the reference resolution `(800, 600)` with `ScaleWithScreenSize` and Match Width `(0)`.
- **Element Selection**: Resets localScale to `(1, 1, 1)`, centered on the Canvas with design size `400 x 250`.
- **Slot 1**: Resets size to `50 x 50`, top-left anchored.
- **Start Button**: Resets size to `160 x 30` centered near the bottom at `(0, -105)`.
- **EventSystem**: Configured with the modern `InputSystemUIInputModule` to restore complete interactivity and click registering in the New Input System environment.

# Key Asset & Context
1. **Assets/Scenes/Preparation.unity** (Modified):
   - Restores the Canvas, Element Selection, Slot 1, and Start Button RectTransforms to their exact original layout values.
   - Upgrades legacy `StandaloneInputModule` to `InputSystemUIInputModule`.
2. **Assets/Scripts/GameManager.cs** (Modified):
   - Stores upgrade states, selected planet name, and scores globally.
   - Adjusts reaction thresholds and visual scaling dynamically depending on active upgrades.
3. **Assets/Scripts/CockpitManager.cs** (New):
   - Custom controller attached to the `Cockpit` panel.
   - Programmatically handles layout, click actions, upgrade purchases, custom planet icons, and displays an automated real-time rotating 3D Shuttle preview!

# Implementation Steps
## Step 1: Execute Programmatic UI Property Restoration in Preparation Scene
- **Description**: Write and execute a script to open `Preparation.unity`, locate Canvas components, set the exact reference resolution to `(800, 600)`, reset the RectTransforms of `Element Selection` to `400x250` with scale `(1,1,1)`, `Slot 1` to `50x50` top-left anchored, and `Start` button to `160x30` bottom-centered, and swap `StandaloneInputModule` for `InputSystemUIInputModule` on the EventSystem.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Implement CockpitManager.cs and upgrade GameManager.cs
- **Description**: Add persistent upgrade states and planet variables inside `GameManager.cs`. Create the fully functional `CockpitManager.cs` script with reactive upgrade buy logic, target planet select handlers, name badges, and a gorgeous rotating 3D preview render-texture rig. Attach it to `Cockpit` in the `Preparation` scene.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Save and Verify
- **Description**: Save the scene, run verification checks to confirm all elements match the original recovery scene layouts exactly, and test interactivity.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

# Verification & Testing
1. **Scene Layout Inspection Test**:
   - Verify that the Element Selection panel is perfectly proportioned, centered, and matching the original layout.
   - Verify that slot cells are exactly `50 x 50` at runtime.
2. **Interactivity & Click Test**:
   - Click slots. Verify they react immediately (scale down to `0.85`, show red outline, update selection count).
   - Select exactly 10 slots and click Start. Verify the scene transition works instantly.
