# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards in a single controlled detonation.
- **Players:** Single player
- **Inspiration / Reference Games:** Super Mario Galaxy, Outer Wilds, Kerbal Space Program
- **Tone / Art Direction:** Light-hearted, slightly comedic corporate-bureaucratic, accessible sci-fi
- **Target Platform:** PC (StandaloneWindows64)
- **Screen Orientation / Resolution:** Landscape 1920x1080
- **Render Pipeline:** Universal Render Pipeline (URP) using the active `PC_RPAsset`

# Game Mechanics
## Core Gameplay Loop
The player prepares a hazardous planet for a controlled demolition by walking on its surface, placing stabilization anchors/sensors, gathering nuclear resources, and assembling the reaction device. Once ready, they initiate the "Kickoff" reaction from orbit, viewing the planet's vaporization or partial failure.

## Controls and Input Methods
- **Movement:** Keyboard (WASD) or Gamepad (Left Stick) to move along the planet's spherical surface.
- **Camera:** Dynamic 3rd person chase camera following the player, automatically matching their spherical gravity orientation.
- **Input System:** Uses the Unity New Input System and the project-wide action maps (`InputSystem_Actions.inputactions` -> `Player/Move`).

# UI
- **HUD Panel:** Showing distance to surface, current gravity pull, and stabilization metrics.
- **Kickoff Trigger Button:** Prominent red button on screen to trigger detonation.

# Key Asset & Context
### Discovered Context
- **Planet_M:** Located at `(0.32, 0.22, 0.32)` with scale `(10, 10, 10)`. Radius is 5.0. Has a `SphereCollider`.
- **Mr.Blast:** Located at `(1.66, 3.78, 3.56)`. Has a `BoxCollider`, currently lacks a `Rigidbody`.
- **Main Camera:** Child of `Mr.Blast` with relative position `(0, 3, -1)` and rotation `(60, 0, 0)`.

### New Scripts
1. **`Assets/Scripts/SphericalCharacterController.cs`**: Handles gravity calculation towards `Planet_M`'s center, aligns player's Up vector with the planet normal, and translates inputs to spherical movement.
2. **`Assets/Scripts/SphericalCameraFollow.cs`**: Follows `Mr.Blast` from the original offset `(0, 3, -1)` and rotation `(60, 0, 0)`, smoothing out rotation and position tracking.

# Implementation Steps

## Step 1: Create the Scripts Directory and Spherical Movement Code
- **Description:** Create the directory `Assets/Scripts` if it doesn't exist, and write `SphericalCharacterController.cs` and `SphericalCameraFollow.cs`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** No

## Step 2: Configure Player and Planet GameObjects
- **Description:** 
  1. Add a `Rigidbody` to `Mr.Blast`.
  2. Set `useGravity = false` and `freezeRotation = true` on the `Rigidbody` to allow manual spherical alignment.
  3. Attach `SphericalCharacterController` to `Mr.Blast`. Configure the speed, alignment, and assign `Planet_M` as the gravity source.
  4. Attach `SphericalCameraFollow` to `Main Camera`. Assign `Mr.Blast` as the target and `Planet_M` as the planet.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No

## Step 3: Input Verification and Hookup
- **Description:** Ensure `SphericalCharacterController` successfully binds to the `Player/Move` action of the project-wide `InputSystem_Actions.inputactions` and reads inputs correctly.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

# Verification & Testing
## Unit and Functional Tests
- **Gravity Test:** Enter Play mode. The player should drop from their starting height until colliding with the spherical surface of `Planet_M`.
- **Alignment Test:** Move around the sphere. The player's feet must remain oriented towards the center of `Planet_M` at all times.
- **Movement Test:** Press W/A/S/D. The player must move smoothly on the sphere surface and rotate to face the direction of movement.
- **Camera Follow Test:** Ensure the camera maintains the initial custom offset (`0, 3, -1` local position and `60, 0, 0` rotation) and rotates smoothly without clipping or jitter.
- **Log Verification:** Check the console for any Input System or script errors.
