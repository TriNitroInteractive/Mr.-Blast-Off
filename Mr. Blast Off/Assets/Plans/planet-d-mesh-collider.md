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
- Spherical character movement on planetoids using New Input System.
- Flight-based piloting for traveling between planets in the space shuttle.
- Keyboard interaction keys (e.g. `[F]` to land/enter/detonate).

# UI
No UI changes are required for this adjustment. Standard prompts and flight HUD are retained.

# Key Asset & Context
- `Assets/Scenes/Kickoff.unity`: The gameplay scene containing planet GameObjects and terminals.
- `Assets/Scripts/PlanetBlaster.cs`: Computes planet radius and handles detonation particle/debris spawning.
- `Assets/Scripts/ShuttleController.cs`: Handles shuttle-to-planet orbit spawning, landing, and player exit placement.
- **Planet D GameObject** in `Kickoff.unity`:
  - Currently has a `SphereCollider` (center `(0,0,0)`, radius `2.5f`, scale `10.0`).
  - Contains 7 child shards: `shard5`, `shard10`, `shard12`, `shard24`, `shard25`, `shard28`, and `shard29` (each has a `MeshFilter` and `MeshRenderer`).
- **Terminals** in `Kickoff.unity`:
  - `Terminal D`: Positioned at `(55.50, 27.49, -20.56)`, perfectly sits on the top surface.

# Implementation Steps

## Step 1: Implement General Planet Radius Helper
- **Description**: Implement a robust static helper method `GetPlanetRadius(GameObject planetGo)` in both `PlanetBlaster.cs` and `ShuttleController.cs` to accurately measure a planet's physical radius from its mesh visual boundaries, supporting any combination of `SphereCollider`, `MeshCollider`, or renderers.
  - The algorithm:
    1. Check for `SphereCollider` on the root. If present, return `col.radius * transform.localScale.x`.
    2. Otherwise, check for `MeshFilter` components in children.
    3. If found, transform the 8 corners of the `sharedMesh.bounds` of each child mesh into world space, and calculate the maximum distance to the planet root's position. This returns the true world-space physical visual radius of the non-spherical planet.
    4. Fall back to `transform.localScale.x * 0.5f` if no mesh filters are found.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Update `PlanetBlaster.cs` and `ShuttleController.cs` to Use the Helper
- **Description**: Replace direct `SphereCollider` dependencies and outdated radius calculations with `GetPlanetRadius(gameObject)`.
  - In `PlanetBlaster.cs` (Awake):
    ```csharp
    planetRadius = GetPlanetRadius(gameObject);
    ```
  - In `ShuttleController.cs` (Start & FindNearestPlanet):
    ```csharp
    float planetRadius = GetPlanetRadius(planetGo);
    ...
    radius = GetPlanetRadius(obj);
    ```
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Scene Editor Script to Replace Colliders on Planet D
- **Description**: Write and execute a temporary Editor script to:
  1. Open `Kickoff.unity`.
  2. Find `Planet D` GameObject.
  3. Remove the `SphereCollider` from `Planet D`.
  4. Find all child objects of `Planet D` that contain a `MeshFilter`.
  5. Add a `MeshCollider` to each of these children, setting its `sharedMesh` to the child's `MeshFilter.sharedMesh`. Keep `convex = false`.
  6. Save and commit scene changes.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

# Verification & Testing
## Automated Verification
- Run a C# query script after changes to assert:
  - `Planet D` does not have a `SphereCollider` component on the root.
  - Each of the 7 child shards on `Planet D` has a `MeshCollider` component configured with the correct `sharedMesh`.
  - C# scripts compile perfectly without error.

## Playtest Verification
1. **Shuttle Spawn**: Start the gameplay in `Kickoff` scene. Ensure the shuttle spawns outside the irregular visual boundary of Planet D.
2. **Landing on Planet D**: Pilot the shuttle to Planet D. Trigger a landing.
   - Verify that the shuttle docks/lands correctly on the irregular surface.
   - Verify that the player exits and lands perfectly on the collision boundary of the closest shard.
3. **Spherical Surface Walking**: Walk around the entire surface of Planet D.
   - Verify that Mr. Blast can walk smoothly over the jagged surfaces of the shards.
   - Verify that gravity continuously pulls the character towards the center of Planet D.
   - Verify that Mr. Blast does not fall through, float above, or sink into the mesh shards.
4. **Terminal Interaction & Detonation**: Walk up to `Terminal D` on the surface of Planet D. Interact with it and trigger the detonation. Verify that the planet explodes and all child meshes/colliders are successfully disabled upon blast.
