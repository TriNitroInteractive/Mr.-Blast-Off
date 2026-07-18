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

In this specific feature, when the player reaches the Terminal on the planet surface and presses 'F', they trigger the final detonation. This causes the planet to explode violently from the inside, generating flying debris, glowing heat elements, a massive internal light swell, and blasting the player into deep space.

## Controls and Input Methods
- **Detonation Trigger:** While standing near the Terminal (Cylinder), press the **F** key (using the New Input System) to activate the blast sequence.
- **Movement (Before):** Standard WASD/Left Stick movement constrained to the spherical surface.
- **Movement (During/After):** The spherical constraint is disabled. The player is launched into deep space with full 3D physics and torque applied.

# UI
- **Interaction Prompt:** A screen-space TextMeshPro overlay that appears when the player is within interaction range of the Terminal. It displays: `"Press [F] to Detonate Planet_M"`.
- **Fading/Transition UI:** Optionally, a simple screen flash or text showing `"TACTICAL KICKOFF INITIATED"` during the blast.

# Visual & Shading Effects
- **Volumetric Light Rays:** Since a standard point light is flat, we will procedurally generate dramatic, glowing light rays that shoot outward from inside the planet's core during the blast.
- **Ray Shader (`Assets/Shaders/URPAdditiveRay.shader`):** A custom URP unlit additive shader that fades smoothly at the edges (`sin(uv.x * PI)`) and tapers off along the length (`pow(1.0 - uv.y, fadePower)`). This ensures the rays look like smooth, volumetric cones of energy without hard mesh edges.
- **Cross-Quad Procedural Mesh:** Each ray is represented by two intersecting quads (a cross-quad) generated dynamically in C# to ensure zero file dependencies and optimal performance.

# Key Asset & Context
- **`Assets/Scripts/TerminalController.cs`:** Handles distance detection between `Mr.Blast` and the Terminal, controls the visual prompt, and triggers the detonation when `F` is pressed.
- **`Assets/Scripts/PlanetBlaster.cs`:** Handles the planetary detonation sequence. It procedurally generates external debris pieces (rock/soil), internal glowing fragments, the expanding core fireball, and the spectacular volumetric light rays shooting outwards. It instantiates a fading point light at the center, applies outward explosion forces and torques, and implements the fade/shrink/freeze logic.
- **`Assets/Shaders/URPAdditiveRay.shader` (New):** Custom URP unlit additive shader for high-fidelity light shafts.
- **`Planet_M`:** The target planet GameObject in the active scene `Kickoff.unity`.
- **`Cylinder`:** The terminal GameObject in the active scene `Kickoff.unity`, which is renamed to `Terminal` for clarity.
- **`Earth.mat`:** Material used for the planet's outer surface debris.
- **`Blast.mat`:** Material used for the planet's internal glowing core and inner debris pieces.
- **`Ray.mat` (New):** Additive material using the custom ray shader to render light shafts.

# Implementation Steps

## Step 1: Implement the URP Additive Ray Shader & Material
- **Description:** 
  Create a custom URP-compatible additive shader `Assets/Shaders/URPAdditiveRay.shader` that performs edge-softening and length-fading. Create `Assets/Materials/Ray.mat` and assign this shader to it. Set its Tint to orange/yellow.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 2: Implement the Interactive Terminal script
- **Description:** 
  Create `Assets/Scripts/TerminalController.cs` which:
  - Finds `Mr.Blast` and monitors distance to it.
  - Activates/deactivates a screen-space UI prompt when within the specified range (e.g., 3 units).
  - Listens for the **F** key using the New Input System (`Keyboard.current.fKey.wasPressedThisFrame`).
  - Calls `PlanetBlaster.Instance.Detonate()` when pressed.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 3: Implement the Planet Blaster script with Volumetric Rays
- **Description:**
  Create `Assets/Scripts/PlanetBlaster.cs` which:
  - Exposes customizable parameters: `numDebris` (e.g., 64), `explosionForce`, `torqueForce`, `glowIntensity`, `blastDuration`, material references (`Earth`, `Blast`, and `Ray`).
  - Generates cross-quad meshes procedurally for the light rays.
  - Implements `Detonate()`:
    1. **Player Launch:** Disables `SphericalCharacterController` and `SphericalCameraFollow` relative constraints, and applies an outward impulse and torque to `Mr.Blast`'s `Rigidbody` to launch him into space.
    2. **Planet Deactivation:** Disables `MeshRenderer` and `SphereCollider` of `Planet_M`.
    3. **Internal Light:** Instantiates a Point Light at the planet's center with a customizable color and high intensity.
    4. **Fireball Expansion:** Instantiates a temporary sphere at the center using `Blast.mat` that rapidly scales up, then shrinks or fades.
    5. **Light Ray Generation:** Instantiates 16-24 procedurally scaled and randomly rotated cross-quad light ray GameObjects pointing outwards from the center. During the blast, they scale outward rapidly and rotate, while their color/alpha fades to zero.
    6. **Debris Generation:** Generates `numDebris` outer cubes (using `Earth.mat`) and `numDebris / 2` inner cubes (using `Blast.mat`) distributed on the sphere shell using Fibonacci sphere distribution.
    7. **Physics Application:** Dynamically adds `Rigidbody` to every debris piece and applies an outward explosion impulse + random spin torque.
    8. **Physics Removal ("After, there will be no physics"):** Over a coroutine of 4-5 seconds, slowly slows down debris (applying high linear/angular drag), and then freezes them in space by removing/disabling their `Rigidbody` components. Optionally, shrinks them smoothly to 0 before destruction to complete the vaporization effect.
- **Assigned role:** developer
- **Dependencies:** Step 1, Step 2
- **Parallelizable:** No

## Step 4: Scene Setup and Asset Hookup
- **Description:**
  In the active scene `Kickoff.unity`:
  1. Rename the `Cylinder` GameObject to `Terminal` for clarity.
  2. Add `TerminalController.cs` to the `Terminal` GameObject.
  3. Add `PlanetBlaster.cs` to the `Planet_M` GameObject.
  4. Create a UI Canvas with a TMPro Text component for the interaction prompt, and link it to the `TerminalController` component.
  5. Assign all required inspector fields (materials, player reference, planet reference) in both controllers, including `Ray.mat` to the `PlanetBlaster` component.
- **Assigned role:** developer
- **Dependencies:** Step 3
- **Parallelizable:** No

# Verification & Testing
- **Detonation Range Test:** Stand far away from the Terminal. The prompt should not be visible. Walk within 3 units of the Terminal. The prompt should appear.
- **Detonation Execution & Rays Test:** Press F when standing near the Terminal.
  - The main planet sphere should disappear immediately.
  - Dramatic, volumetric-looking glowing light rays/beams should burst outwards in all directions from the center, spinning and expanding rapidly before fading out.
  - A bright flash/light should swell from the center.
  - Debris pieces (both earth-like and magma-like) must fly outwards in all directions with spinning motions.
  - The player must be blasted off into space, spinning under physics.
- **Physics Freeze Test:** After 4-5 seconds, the debris pieces should stop moving and have their Rigidbodies disabled/destroyed (confirm in the hierarchy/inspector that no active Rigidbodies remain for the debris), verifying that there is zero physics overhead after the explosion.
- **Console Log Verification:** Ensure no script errors, null references, or Input System warnings are logged in the Console during the entire gameplay session.
