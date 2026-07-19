# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: "Mr. Blast Off" is a 2D sci-fi engineering/strategy game where players build a planetary destruction device to vaporize celestial hazards in a single controlled detonation.
- Players: Single-player
- Tone / Art Direction: Slightly comedic sci-fi, accessible nuclear physics, strategic optimization.
- Target Platform: Standalone Windows PC
- Render Pipeline: Custom PC_RPAsset (Scriptable Render Pipeline / Universal Render Pipeline)

# Game Mechanics
## Core Gameplay Loop
The player (Mr. Blast) lands on small planets, interacts with terminals to customize chemical/nuclear element configurations, and triggers detonations once criticality is reached.

## Controls and Input Methods
- Spherical movement on planetoids using New Input System.
- Flight-based piloting for traveling between planets in the space shuttle.
- Keyboard interaction keys (e.g. `[F]` to land/enter/detonate).

# UI
No UI changes are required for this adjustment. Standard prompts and flight HUD are retained.

# Key Asset & Context
- `Assets/Scenes/Preparation.unity`: The setup scene containing planet GameObjects and terminals.
- `Assets/Scenes/Kickoff.unity`: The gameplay scene containing planet GameObjects and terminals.
- `Assets/Scripts/PlanetBlaster.cs`: Computes planet radius and handles detonation particle/debris spawning.
- `Assets/Scripts/ShuttleController.cs`: Handles shuttle-to-planet orbit spawning, landing, and player exit placement.
- **Planet GameObjects** in both scenes:
  - `Planet M` (Scale: 30, Collider Radius: 0.5, World Radius: 15.0) - Currently perfectly adjusted.
  - `Planet A` (Scale: 10, Collider Radius: 0.5, World Radius: 5.0) - To be adjusted to local radius 2.5 (World Radius: 25.0) to match visual surface.
  - `Planet B` (Scale: 10, Collider Radius: 0.5, World Radius: 5.0) - To be adjusted to local radius 2.5 (World Radius: 25.0) to match visual surface.
  - `Planet C` (Scale: 10, Collider Radius: 0.5, World Radius: 5.0) - To be adjusted to local radius 2.5 (World Radius: 25.0) to match visual surface.
  - `Planet D` (Scale: 10, Collider Radius: 2.5, Center: `(0, -0.33, 0)`) - To be adjusted to center `(0, 0, 0)` and radius 2.5 (World Radius: 25.0).
- **Terminal GameObjects** in both scenes:
  - `Terminal A`, `Terminal B`, `Terminal C`, `Terminal D` - To be moved from 5.1 units above planet center to 25.1 units above center so they sit correctly on the new adjusted surfaces.

# Implementation Steps

## Step 1: Update `PlanetBlaster.cs` for Dynamic Planet Radius
- **Description**: Modify `PlanetBlaster.cs` to read the true world-space planet radius from the `SphereCollider` component (if present) rather than assuming a hardcoded `localScale.x * 0.5f`.
  - In `Awake()`:
    ```csharp
    SphereCollider col = GetComponent<SphereCollider>();
    planetRadius = col != null ? (col.radius * transform.localScale.x) : (transform.localScale.x * 0.5f);
    ```
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Update `ShuttleController.cs` for Dynamic Planet Radius
- **Description**: Update `ShuttleController.cs` to dynamically query planet radius from `SphereCollider` components when spawning the shuttle at level start and when placing the player upon landing.
  - In `Start()`:
    ```csharp
    var col = planetGo.GetComponent<SphereCollider>();
    float planetRadius = col != null ? (col.radius * planetGo.transform.localScale.x) : (planetGo.transform.localScale.x * 0.5f);
    ```
  - In `FindNearestPlanet()`:
    ```csharp
    var col = obj.GetComponent<SphereCollider>();
    radius = col != null ? (col.radius * obj.transform.localScale.x) : (obj.transform.localScale.x * 0.5f);
    ```
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 3: Scene Editor Script to Adjust Colliders and Terminals
- **Description**: Write and execute a temporary/one-off Editor script to systematically modify `Preparation.unity` and `Kickoff.unity` scenes:
  1. Open each scene additively/individually.
  2. For `Planet A`, `Planet B`, `Planet C`, `Planet D`, find their `SphereCollider` and set `center = (0, 0, 0)` and `radius = 2.5f`.
  3. Locate `Terminal A`, `Terminal B`, `Terminal C`, `Terminal D` in the scene. Reposition them to sit on the new adjusted world-space surface:
     - `Terminal A`: `Planet A.position` + `(0, 25.1, 0)`
     - `Terminal B`: `Planet B.position` + `(0, 25.1, 0)`
     - `Terminal C`: `Planet C.position` + `(0, 25.1, 0)`
     - `Terminal D`: `Planet D.position` + `(0, 25.1, 0)`
  4. Save and close the modified scenes to commit changes.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: No

# Verification & Testing
## Automated Verification
- Run a C# query script after changes to assert:
  - Planet A, B, C, D all have SphereCollider center = `(0, 0, 0)` and radius = `2.5`.
  - Terminals A, B, C, D are located at Y coordinate `Planet.position.y + 25.1` (around 27.63 / 27.49).
  - C# scripts compile perfectly without error.

## Playtest Verification
1. **Prepare Scene**: Launch the game and enter cockpit mode. Choose Planet A/B/C/D and verify that preparation screen UI functions as expected.
2. **Shuttle Launch & Flight**: Transition to `Kickoff` scene. Verify that the Shuttle spawns safely outside the planet's outer visual surface, not inside the planet.
3. **Landing & Exploration**: Fly the shuttle to Planet A, B, C, and D. Land on each of them.
   - Verify that the player exits the shuttle and stands perfectly on the surface of the planet meshes without sinking in or floating.
   - Verify that the player can walk smoothly over the entire spherical surface of each planet.
   - Verify that Terminals A, B, C, D are visible, fully accessible, and positioned exactly at the surface.
4. **Detonation FX**: Trigger a detonation on any of the adjusted planets. Verify that debris spawns correctly at the visual surface boundaries and flies outward realistically.
