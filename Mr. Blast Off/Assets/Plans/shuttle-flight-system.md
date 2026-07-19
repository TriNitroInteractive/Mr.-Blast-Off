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
4. **Shuttle Flight**: Players can interact with the Shuttle, get inside, and fly around the planetary system freely in 3D.

## Controls and Input Methods
- **WASD / Arrow Keys**: Move Mr. Blast around the spherical surface.
- **F Key**: Interact with the Control Terminal or enter/exit the Shuttle.
- **W / S**: Move Shuttle forward / backward while piloting.
- **A / D**: Yaw Shuttle left / right (turn left/right) while piloting.
- **Mouse / LookAction**: Orbit or steer the camera while piloting.
- **Space / Left Shift**: Fly Shuttle upward / downward (lift / drop) while piloting.
- **R Key**: Restart the scene after Game Over/Vaporization.

# UI
- **Interact Prompt**: Displays "Press [F] to Enter Shuttle" when Mr. Blast is in range of the Shuttle.
- **Piloting Controls HUD**: Displays flying controls on the screen when inside the Shuttle:
  - "W/S - Forward/Backward"
  - "A/D - Turn Left/Right"
  - "Space/Shift - Fly Up/Down"
  - "Press [F] to Exit Shuttle"

# Key Asset & Context
1. **Assets/Scripts/ShuttleController.cs** (New):
   - Handles the player interaction (F key to enter/exit).
   - Contains the 3D flying movement physics/translation.
   - Manages the Piloting UI and smooth camera follow target switching.
2. **Assets/Scripts/ShuttlePowerEffect.cs** (New):
   - Procedurally creates and animates 2-3 glowing green rings moving from top to bottom over the shuttle using LineRenderers and the `"Universal Render Pipeline/Particles/Unlit"` shader.
   - Configures the Shuttle's mesh material to look like elegant "grey glassy / black glass" transparent material.
3. **Assets/Scripts/SphericalCameraFollow.cs** (Modified):
   - Integrates with the Shuttle system to allow the camera follow target to switch dynamically or pause tracking during shuttle piloting.

# Implementation Steps
## Step 1: Implement `ShuttlePowerEffect.cs`
- **Description**: Create the shader-driven glassy look and procedural green powering-up rings on the Shuttle mesh.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Implement `ShuttleController.cs`
- **Description**: Implement interaction checking, prompt UI, entering/exiting the Shuttle, HUD displays, and the 3D space flight controller.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Integrate Camera Transition and Follow
- **Description**: Add support in `SphericalCameraFollow` to handle the target transition, or implement a specialized smooth follow camera state in `ShuttleController.cs` that overrides the main camera's transform while piloting.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Verify and Save Scene
- **Description**: Save the scene changes and verify that the layout and components compile correctly with no errors.
- **Assigned role**: developer
- **Dependencies**: All steps
- **Parallelizable**: No

# Verification & Testing
1. **Glassy Look & Rings Visual Test**:
   - Verify that the Shuttle is transparent grey glassy (black glass) and green energy rings slide from top to bottom repeatedly.
2. **Interaction & Pilot Entrance Test**:
   - Walk Mr. Blast up to the Shuttle. Verify that "Press [F] to Enter Shuttle" prompt appears.
   - Press [F]. Verify that Mr. Blast disappears, the flight HUD is displayed, and the camera switches to the 3D third-person flight view.
3. **Flight Mechanics Test**:
   - Use WASD, Space, and Shift to fly the Shuttle around Planet M and the other planetary hazards.
   - Verify that flight is smooth, satisfying, and responsive.
4. **Exit Flight Test**:
   - Land or hover, and press [F] to exit. Mr. Blast should reappear on the closest planet surface and camera follow should restore.
