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
3. **The Detonation (Kickoff)**: Pull the trigger at the terminal to initiate the reaction. If the elements are highly reactive (sum of reaction points > 80), the planet undergoes a spectacular cinematic explosion, completing the cleanup. If the reaction is insufficient (sum of reaction points <= 80), the reactor backfires and Mr. Blast is immediately vaporized (causing a Game Over).

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
1. **Assets/Scripts/GameManager.cs** (New):
   - Co-ordinates the reaction checking, player death execution, procedural particle disintegration, and UI overlay.
   - Contains the static mapping of scientific elements to their reaction points (1-10 scale).
2. **Assets/Scripts/ElementSelectionManager.cs** (Modified):
   - Integrates with `GameManager` to hand over selected element list and calculate total points before starting the game.
3. **Assets/Scripts/TerminalController.cs** (Modified):
   - Diverts the trigger detonation event to `GameManager.Instance.TryDetonate()` instead of directly detonating.

### Reactivity Point Mapping
To make the gameplay highly thematic and strategic for player discovery, the 30 scientific elements are assigned the following hidden reaction points:
- **Fuels/Reactive (8-10 points)**:
  - Uranium: 10, Plutonium: 10, Tritium: 10, Helium-3: 10, Deuterium: 9, Thorium: 9, Neptunium: 9, Radium: 8, Polonium: 8, Cesium: 8
- **Moderate/Conductive (5-7 points)**:
  - Platinum: 8, Cobalt: 7, Tungsten: 7, Gold: 7, Lithium: 6, Zirconium: 6, Nickel: 6, Carbon: 5, Sodium: 5, Titanium: 5
- **Moderators/Absorbers (1-4 points)**:
  - Beryllium: 4, Xenon: 4, Krypton: 4, Graphite: 3, Copper: 3, Iron: 3, Boron: 2, Steel: 2, Lead: 1, Cadmium: 1

# Implementation Steps
## Step 1: Create GameManager Script
- **Description**: Implement `Assets/Scripts/GameManager.cs` to manage points calculation, player death, and scene reloading.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Set Up GameManager GameObject in Scene
- **Description**: Add an empty GameObject named `_GameManager` to `Kickoff.unity` scene, and attach the `GameManager` component to it.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Integrate with ElementSelectionManager
- **Description**: Modify `OnStartClicked()` in `ElementSelectionManager.cs` to send selected elements to `GameManager.Instance.InitializeSelectedPoints()` and store the sum before unpausing.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 4: Integrate with TerminalController
- **Description**: Modify `TriggerDetonation()` in `TerminalController.cs` to call `GameManager.Instance.TryDetonate()` instead of detonating directly.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 5: Implement Player Death and Game Over UI
- **Description**: Implement `KillPlayer()` inside `GameManager.cs`. It will:
  1. Set `isGameOver = true`.
  2. Disable the player's `SphericalCharacterController` to lock movement.
  3. Hide the player's `MeshRenderer` to simulate body destruction.
  4. Spawn 20 small cube primitives as debris at player's position, applying random forces/torques and fading/shrinking them over 2 seconds.
  5. Programmatically build a "GAME OVER" screen on the scene's Canvas.
  6. Listen for `R` key inside `Update()` to reload the scene.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

# Verification & Testing
1. **Successful Detonation Test**:
   - Select 10 high-reactivity elements (e.g. Uranium, Plutonium, Tritium, Helium-3, Deuterium, Thorium, Neptunium, Radium, Polonium, Cesium).
   - Press Start. Verify that total point is > 80 (should be 91).
   - Navigate to Terminal, press [F].
   - Verify that planet detonates successfully and Mr. Blast is launched into space.
2. **Vaporization Failure Test**:
   - Select 10 low-reactivity elements (e.g. Lead, Cadmium, Boron, Steel, Graphite, Copper, Iron, Beryllium, Xenon, Carbon).
   - Press Start. Verify that total point is <= 80 (should be 28).
   - Navigate to Terminal, press [F].
   - Verify that planet does NOT detonate.
   - Verify that Mr. Blast is vaporized: movement freezes, player model turns into scattering debris, and a beautiful Game Over UI panel appears displaying: "CRITICAL FAILURE - Mr. Blast was vaporized! Total Reaction Points: 28/80".
3. **Restart Test**:
   - Press [R] on the Game Over screen.
   - Verify that scene reloads immediately, re-entering the Element Selection phase.
