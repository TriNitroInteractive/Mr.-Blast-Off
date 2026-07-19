# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: "Mr. Blast Off" is a 2D sci-fi engineering/strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards (moons, asteroids, rogue planets) in a single controlled detonation. Gameplay centers on strategic preparation, resource balancing, and engineering optimization phase prior to initiating the reaction (the "Kickoff").
- Players: Single player
- Inspiration / Reference Games: Kerbal Space Program (for physical positioning/orbits), Incremental reactor games, sci-fi sandbox games
- Tone / Art Direction: Light-hearted, comedic, accessible nuclear physics, glowing sci-fi aesthetics
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape 1920x1080
- Render Pipeline: PC_RPAsset (URP / custom)

# Game Mechanics
## Core Gameplay Loop
1. **Preparation Scene**: The player opens the game in the `"Preparation"` scene, where they interact with the Element Selection UI to select exactly 10 radioactive or moderator elements.
2. **Scene Transition**: Upon pressing the "Start" button, the selected elements are stored in a persistent global list, and the game loads the `"Kickoff"` scene.
3. **Shuttle Start**: Mr. Blast immediately starts inside the Shuttle orbiting directly above Planet M, piloting it in 3D flight.
4. **Detonation & Vaporization**: Once the player lands or hovers near the terminal, they can trigger the detonation check. If the points exceed 80, the planet detonates and Mr. Blast is launched into space. If points are 80 or less, the reactor backfires and Mr. Blast is vaporized.

# UI
- **Element Selection Screen**: Displayed as a full overlay in the `"Preparation"` scene.
- **Flight HUD**: Displays flight controls and spaceship status in `"Kickoff"` scene when piloting the Shuttle.

# Key Asset & Context
1. **Assets/Scripts/ElementSelectionManager.cs** (Modified):
   - Refactored to load `"Kickoff"` scene when the Start button is pressed in the `"Preparation"` scene.
   - Preserves backward compatibility when testing directly inside `"Kickoff"` scene.
2. **Assets/Scripts/GameManager.cs** (Modified):
   - Initializes the game state on scene start if the active scene is `"Kickoff"`.
   - Provides a fallback of 10 high-reactivity elements if `SelectedElements` is empty, ensuring developer playtesting in the `"Kickoff"` scene is completely uninterrupted.
3. **Assets/Scripts/ShuttleController.cs** (Modified):
   - Features a `public bool startInShuttle = true;` option.
   - Automatically finds `Mr.Blast` and enters piloting mode immediately on `Start()`.
4. **Scenes Configuration**:
   - `"Preparation.unity"` set as Build Index 0.
   - `"Kickoff.unity"` set as Build Index 1.

# Implementation Steps
## Step 1: Update Build Settings Scenes List
- **Description**: Add `Preparation.unity` and `Kickoff.unity` to Unity Build Settings, making `Preparation` the primary entry scene.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Relocate Element Selection UI to Preparation Scene
- **Description**: Write an editor script to move the `Element Selection` GameObject from `Kickoff.unity` to `Preparation.unity`. Add a `Canvas` and `EventSystem` to `Preparation.unity`. Disable or destroy the old `Element Selection` UI in `Kickoff.unity` so they do not overlap.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Refactor ElementSelectionManager Scene Loading
- **Description**: Update `ElementSelectionManager.cs` to handle scene loading and transfer persistent static selections seamlessly.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: Yes

## Step 4: Add Shuttle Start Logic to ShuttleController
- **Description**: Implement `startInShuttle` setup in `ShuttleController.cs` to automatically seat player in Shuttle and configure camera follow during initialization.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 5: Refactor GameManager for Multi-Scene Initialization
- **Description**: Update `GameManager.cs`'s initialization sequence to load selected elements, feed them to `InventoryBarManager`, and provide playtesting default fallbacks.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Preparation to Kickoff Transition Test**:
   - Play from `Preparation` scene.
   - Select 10 elements and click Start.
   - Verify that the game loads `Kickoff` scene, Mr. Blast starts directly piloting the Shuttle above Planet M, and Selected Elements are populated correctly inside the play HUD's Inventory Bar.
2. **Developer Direct Playtesting Test**:
   - Play directly from `Kickoff` scene.
   - Verify that the game populates default elements, initializes points, and allows playtesting without any errors.
