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
1. **Element Selection**: Choose exactly 10 scientific/nuclear elements to load into the planet's core reactor.
2. **Exploration & Navigation**: Mr. Blast walks around the planet, prepares the reactor, and interacts with the control terminal.
3. **The Detonation (Kickoff)**: Pull the trigger at the terminal to initiate the reaction. If the elements are highly reactive (sum of reaction points > 80), the planet undergoes a spectacular cinematic explosion, completing the cleanup. If the reaction is insufficient (sum of reaction points <= 80), the reactor backfires and Mr. Blast is immediately vaporized (causing a Game Over). During the detonation build-up, the Main Camera smoothly swings and shifts to a secondary static side-view camera to view the explosion/event in its full 2D glory.

## Controls and Input Methods
- **WASD / Arrow Keys**: Move Mr. Blast around the spherical surface.
- **F Key**: Interact with the Control Terminal to cause the detonation.
- **R Key**: Restart the scene after Game Over/Vaporization.

# UI
## Game Over Overlay
Since there is no pre-existing game-over UI, we will programmatically create a modern semi-transparent UI panel attached to the main `Canvas`:
1. **Background**: Semi-transparent dark overlay covering the screen.
2. **Title**: "CRITICAL FAILURE - GAME OVER" in big, glowing red text.
3. **Sub-text**: Detailed description of Mr. Blast's vaporization and the reaction failure.
4. **Point Tally**: Displays the exact sum of selected elements points and why it failed, e.g., "Reaction Yield: 64 points (Required: >80 points)".
5. **Prompt**: Flashing yellow instruction "Press [R] to Restart Cleanup".

# Key Asset & Context
1. **Assets/Scripts/GameManager.cs** (Modified):
   - Co-ordinates the reaction checking, player death execution, procedural particle disintegration, and UI overlay.
   - Contains the static mapping of scientific elements to their reaction points (1-10 scale).
   - Manages the smooth interpolation of the Main Camera to the Secondary side-view Camera when detonation is triggered.
2. **Assets/Scripts/ElementSelectionManager.cs** (Modified):
   - Integrates with `GameManager` to hand over selected element list and calculate total points before starting the game.
3. **Assets/Scripts/TerminalController.cs** (Modified):
   - Diverts the trigger detonation event to `GameManager.Instance.TryDetonate()` instead of directly detonating.
4. **Assets/Scripts/SphericalCameraFollow.cs** (Modified):
   - Supports a `isCinematicActive` flag which, when true, stops standard player following so that the GameManager can smoothly animate the camera to the secondary camera.
5. **Secondary Camera GameObject** (New scene object):
   - A physical camera named `SecondaryCamera` positioned at `(0.32, 0.22, -45.0f)` with rotation `(0, 0, 0)`.
   - Perfectly frames the 30-unit diameter Planet M centered in its 2D side-view.

### Reactivity Point Mapping
To make the gameplay highly thematic and strategic for player discovery, the 30 scientific elements are assigned the following hidden reaction points:
- **Fuels/Reactive (8-10 points)**:
  - Uranium: 10, Plutonium: 10, Tritium: 10, Helium-3: 10, Deuterium: 9, Thorium: 9, Neptunium: 9, Radium: 8, Polonium: 8, Cesium: 8
- **Moderate/Conductive (5-7 points)**:
  - Platinum: 8, Cobalt: 7, Tungsten: 7, Gold: 7, Lithium: 6, Zirconium: 6, Nickel: 6, Carbon: 5, Sodium: 5, Titanium: 5
- **Moderators/Absorbers (1-4 points)**:
  - Beryllium: 4, Xenon: 4, Krypton: 4, Graphite: 3, Copper: 3, Iron: 3, Boron: 2, Steel: 2, Lead: 1, Cadmium: 1

# Implementation Steps
## Step 1: Create Secondary Camera in the Scene
- **Description**: Add a new Camera GameObject named `SecondaryCamera` to `Kickoff.unity` scene. Disable its Camera and AudioListener components so they don't render/conflict at runtime, but can be referenced. Position it at `(0.32, 0.22, -45.0f)` with rotation `(0, 0, 0)`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Add `isCinematicActive` to SphericalCameraFollow
- **Description**: Add a `public bool isCinematicActive = false;` flag to `SphericalCameraFollow.cs`. Return early in `LateUpdate()` when this flag is active.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 3: Implement Camera Swing in GameManager
- **Description**: In `GameManager.cs`, implement a smooth coroutine that lerps the Main Camera to the `SecondaryCamera` position and rotation over 2.0 seconds using smoothstep interpolation. Trigger this transition immediately inside `TryDetonate()`.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: No

## Step 4: Verify and Save Scene
- **Description**: Save the scene changes and verify that the layout and components compile correctly with no errors.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Camera Swing and Detonation Test**:
   - Start game, select high points, go to terminal and press F.
   - Verify that the main camera smoothly swings around to the side of the planet, centering Planet M, before/during the massive cinematic explosion.
2. **Camera Swing and Vaporization Test**:
   - Start game, select low points, go to terminal and press F.
   - Verify that the camera swings to the side view while Mr. Blast gets vaporized on the surface and the Game Over UI is displayed.

