# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: A physics-based celestial exploration game where the player navigates small planetoids, prepares a planetary detonation, and launches themselves to the next celestial body.
- Players: Single-player
- Render Pipeline: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player (Mr. Blast) pilots a space shuttle to travel between planets, lands on them, interacts with terminals, and prepares planetary detonations to harvest resources or advance.
## Controls and Input Methods
Standard flight controls for shuttle navigation; spherical character movement when walking on planet surfaces.

# UI
Not directly modified in this task. Uses existing `TerminalPromptCanvas` for interaction.

# Key Asset & Context
- `Assets/Scenes/Kickoff.unity` (Active scene containing the planets and player)
- `Assets/Scripts/PlanetBlaster.cs` (To be modified to disable child renderers)
- `Assets/Scripts/GameManager.cs` (To be modified to target the player's current planet for detonation)
- `Planet A`, `Planet B`, `Planet C`, `Planet D` (Existing planet GameObjects in Kickoff scene)
- `Terminal` (Existing Terminal GameObject, to be duplicated/referenced)

# Implementation Steps

## Step 1: Update `PlanetBlaster.cs` to Support Child Meshes
- **Description**: Modify `PlanetBlaster.cs` so that when a planet is detonated, it disables all `MeshRenderer` and `Collider` components in its entire hierarchy (rather than just on the root). This ensures multi-mesh or grouped planets (like Planet A, B, C, D) disappear correctly upon explosion.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Update `GameManager.cs` to Detonate Current Planet
- **Description**: Update the detonation evaluation logic in `GameManager.cs` to find and trigger the `PlanetBlaster` belonging to the planet the player is currently standing on (via `SphericalCharacterController.planet`), falling back to a global search only if none is found.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Add SphereColliders to All Planets
- **Description**: Open `Kickoff.unity` and add a `SphereCollider` component to `Planet A`, `Planet B`, `Planet C`, and `Planet D` root GameObjects.
  - Set `center = (0, 0, 0)` and `radius = 0.5f` on each collider (corresponding to a physical world radius of 5.0 units since each planet has a scale of 10).
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 4: Add PlanetBlaster Components to All Planets
- **Description**: Add the `PlanetBlaster` script component to `Planet A`, `Planet B`, `Planet C`, and `Planet D` root GameObjects. Configure their fields:
  - `earthMaterial`: Assign `A.mat` for Planet A, `B.mat` for Planet B, `C.mat` for Planet C, and `D.mat` for Planet D.
  - `blastMaterial`: Assign `Assets/Materials/Blast.mat`.
  - `rayMaterial`: Assign `Assets/Materials/Ray.mat`.
  - Copy all other numeric explosion parameters from Planet M's blaster (e.g. `explosionForce = 18`, `torqueForce = 12`, etc.) to keep the visual payout consistent and satisfying.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 5: Instantiate and Configure Terminals on All Planets
- **Description**: Create and position a `Terminal` GameObject on `Planet A`, `Planet B`, `Planet C`, and `Planet D`.
  - For each planet, instantiate/duplicate the existing `Terminal` GameObject, name it `Terminal A`/`B`/`C`/`D`.
  - Position it on the top of the planet (surface normal vector `(0, 1, 0)` relative to planet center):
    - `Terminal A` position: `Planet A` center + `(0, 5.1, 0)`
    - `Terminal B` position: `Planet B` center + `(0, 5.1, 0)`
    - `Terminal C` position: `Planet C` center + `(0, 5.1, 0)`
    - `Terminal D` position: `Planet D` center + `(0, 5.1, 0)`
  - Align local rotation to identity `(0, 0, 0)`.
  - Ensure `TerminalController` on each new terminal has:
    - `player` field assigned to `Mr.Blast` GameObject.
    - `promptUI` field assigned to `TerminalPromptCanvas` GameObject in the scene.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

# Verification & Testing
## Automated Checks
- Verify that `PlanetBlaster.cs` and `GameManager.cs` compile with zero errors.
- Verify that Planet A, B, C, and D all have `SphereCollider` and `PlanetBlaster` components attached.
- Verify that the terminal references for `promptUI` and `player` are correctly assigned.

## Manual Play Tests
1. **Shuttle Landing**: Fly the shuttle to Planet A/B/C/D. Land near the terminal and exit the shuttle. Ensure Mr. Blast stands and walks perfectly on the planet's surface (the `SphereCollider` functions correctly).
2. **Terminal Interaction**: Walk up to the new terminal. Ensure the screen prompt "Press [F] to Detonate" is shown correctly.
3. **Detonation and Explosion**: Trigger the detonation. Ensure the specific planet's child meshes disappear, and the procedural debris pieces correctly use that planet's matching crust material!
