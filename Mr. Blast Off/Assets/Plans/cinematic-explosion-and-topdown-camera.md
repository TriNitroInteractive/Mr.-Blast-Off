# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards in a single controlled detonation.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player prepares a hazardous planet for a controlled demolition. When they trigger the detonation terminal, a cinematic multi-stage reaction initiates, ending in a spectacular explosion that vaporizes the planet and launches the player into orbit.

## Controls and Input Methods
- **Top-Down Perspective Camera:** The camera is positioned directly above the player (relative to gravity Up), looking down. Look-around input (mouse delta / right stick) orbits the camera horizontally around the gravity normal while keeping this top-down perspective.
- **Cinematic Explosion Sequence:** The detonation is no longer sudden. It has a build-up phase where the planet rumbles, light rays poke through, and then the final big bang occurs.

# UI
- **Interaction Prompt:** Displayed when near the terminal.

# Key Asset & Context
### Modified Assets
1. **`Assets/Scripts/SphericalCameraFollow.cs`**: Needs to be updated to lock the camera to a high-angle top-down perspective and orbit horizontally around the player.
2. **`Assets/Scripts/PlanetBlaster.cs`**: Needs to be rewritten to support a multi-stage delayed explosion sequence.

# Implementation Steps

## Step 1: Implement Top-Down Orbit Camera in SphericalCameraFollow
- **Description:** 
  Modify `Assets/Scripts/SphericalCameraFollow.cs` to force a top-down perspective.
  - Set the camera's default pitch to a high angle looking down (e.g. 70° to 80°).
  - Clamp the vertical viewing angle (pitch) strictly between 60° and 85° (or lock it to a fixed top-down angle like 75° if requested, but clamping it to high angles keeps it top-down while allowing vertical look adjustments).
  - Horizontal movement (Mouse X) rotates the camera's yaw around the player's gravity Up normal, letting the player orbit their view from a birds-eye perspective.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 2: Implement Multi-Phase Cinematic Detonation in PlanetBlaster
- **Description:** 
  Rewrite the detonation logic in `Assets/Scripts/PlanetBlaster.cs` to split the sequence into distinct cinematic stages:
  - **Stage 1: Pre-Explosion Build-up (Duration: 3.0s)**
    - Planet does **not** disappear yet.
    - Planet starts to vibrate/shake violently (applying randomized positional offsets to represent pressure build-up and cracking).
    - An internal light source is spawned and slowly swells in intensity (`0` to `maxLightIntensity * 0.2f`).
    - The volumetric light rays are spawned immediately but at a small scale, slowly growing and rotating, looking like they are piercing through the cracks of the planet.
  - **Stage 2: The Big Boom (At 3.0s)**
    - Disable the planet mesh renderer and collider.
    - Instantly flash the internal light to full `maxLightIntensity`.
    - Spawn the expanding fireball core and scale it up rapidly.
    - Spawn the 96 debris pieces (outer and inner core) and launch them outwards with full `explosionForce` and torque.
    - Apply the sudden explosive impulse to `Mr.Blast` to launch him into space.
  - **Stage 3: Settle & Clean-up**
    - Fade the rays, fireball, and light.
    - Apply damping to the debris and smoothly shrink them to zero before destruction.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No

# Verification & Testing
- **Top-Down Camera Test:** Enter Play mode. Verify that the camera is high above the player looking down at them. Move the mouse. Yaw look-around should rotate around the player smoothly, and vertical pitch should be restricted to high top-down angles (60° to 85°).
- **Cinematic Detonation Sequence Test:** Press F near the terminal.
  - Verify that the planet remains visible initially, shaking violently.
  - Verify that glowing light rays and a gentle central glow start emerging from the core before the planet explodes.
  - At 3.0 seconds, verify the "BOOM": the planet vanishes, a massive fireball swells, debris are blasted away, and the player is launched into deep space.
  - Verify that all debris and rays cleanly fade and are destroyed, leaving a clean console.
