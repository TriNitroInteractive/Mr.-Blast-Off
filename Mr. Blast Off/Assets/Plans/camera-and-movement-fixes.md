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
The player walks around the surface of a spherical planet using a custom gravity system, approaches the Detonation Terminal, and presses 'F' to kickoff a high-energy detonation. The kickoff triggers an explosive sequence where the planet is vaporized into floating debris, light rays, and fireballs, launching the player into orbit.

## Controls and Input Methods
- **Movement (WASD / Left Stick):** Moves smoothly on the sphere surface. Calculated relative to the camera's view projected on the planet's tangent surface normal (instead of the player's own rotating local axes) to avoid positive-feedback spinning loops.
- **Look Around (Mouse Delta / Right Stick):** Smooth third-person orbit look around the player. Aligns camera's Up with gravity, rotates camera yaw (horizontal) and pitch (vertical, clamped between -15° and 80°) based on input, and smoothly interpolates position and rotation.
- **Detonation (F Key):** Activates kickoff sequence at the Terminal.

# UI
- **Terminal Prompt:** Canvas with "Press [F] to Detonate Planet_M" when player is in range of the terminal.

# Key Asset & Context
### Modified Assets
1. **`Assets/Shaders/URPAdditiveRay.shader`**: Custom URP additive shader for volumetric light rays. Needs to be modified to remove unused vertex color attributes.
2. **`Assets/Scripts/SphericalCharacterController.cs`**: Player movement and gravity. Needs to be modified to support camera-relative movement and enable Rigidbody interpolation.
3. **`Assets/Scripts/SphericalCameraFollow.cs`**: Camera follow. Needs to be rewritten into a smooth, gravity-aligned orbital look-around controller that reads the New Input System's `Look` action.

# Implementation Steps

## Step 1: Fix Pink Effect on Exploding Rays
- **Description:** 
  Modify `Assets/Shaders/URPAdditiveRay.shader` to remove the vertex input color attribute (`float4 color : COLOR` in `Attributes` and `Varyings`) and its multiplication in the fragment shader. Because the procedural ray mesh generated in `PlanetBlaster.cs` does not contain vertex colors, requesting this attribute causes shader binding failures and magenta (pink) fallbacks on certain GPU configurations. The fading is already fully controlled via the `_Color` material property, so removing vertex colors is safe and correct.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 2: Smooth Player Movement in SphericalCharacterController
- **Description:** 
  Modify `Assets/Scripts/SphericalCharacterController.cs` to:
  1. Set `rb.interpolation = RigidbodyInterpolation.Interpolate;` in `Awake()` to sync physics and rendering frames.
  2. Change movement calculations: retrieve the camera's forward and right vectors, project them onto the planet's tangent plane (using the player's current `gravityUp` normal), and combine them with WASD input to calculate the target movement direction. This decouples the player's movement direction from their own rotating transform, completely fixing the positive-feedback spinning bug and sudden skips of distance.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 3: Implement Smooth Gravity-Aligned Orbit Looking in SphericalCameraFollow
- **Description:** 
  Modify `Assets/Scripts/SphericalCameraFollow.cs` to:
  1. Retrieve the New Input System's `Look` action (`Look` vector2) during `Start()`.
  2. Maintain `yaw` and `pitch` values based on mouse delta.
  3. Every frame in `LateUpdate()`:
     - Keep the camera's Up aligned with the player's `gravityUp` normal vector.
     - Apply mouse `yaw` (horizontal orbit around gravity Up) and `pitch` (vertical pitch, clamped between -15° and 80°).
     - Calculate target camera position behind its forward vector at a fixed distance from a target focus point slightly above the player (e.g. `target.position + gravityUp * 1.5f`).
     - Smoothly interpolate (`Lerp` and `Slerp`) the camera's position and rotation towards this target.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

# Verification & Testing
- **Visual Ray Test:** Detonate the planet. Ensure the procedural light rays have their correct orange/gold additive glow and do not display any magenta/pink artifacts on screen.
- **Movement Stability Test:** Move in circles and change directions sharply on the sphere. Confirm the player moves smoothly in response to WASD keys and no longer spins, stutters, or skips large chunks of distance.
- **Look Around Test:** Move the mouse in all directions. Confirm the camera rotates smoothly around the player, doesn't jitter, maintains gravity orientation correctly across the entire planet surface (including poles), and does not glitch or flip upside down when looking straight up/down.
- **Console Log Verification:** Ensure no errors or warnings are logged in the Unity Console.
