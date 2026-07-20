# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Environment & Space Navigation
The **Kickoff Scene** contains the main 3D space environment with 5 planets (`Planet M`, `Planet A`, `Planet B`, `Planet C`, `Planet D`) where the player pilots the shuttle and lands to interact with terminals. We want to blacken the skybox environment, add beautiful glowing space stars, and introduce realistic planetary rotations.

# UI
The background space environment will be blackened, and stars will render procedurally across both cameras (`Main Camera` and `SecondaryCamera`).

# Key Asset & Context
- `Assets/Scenes/Kickoff.unity`: The scene where the 3D outer space environment is set up.
- `Mr.Blast` and `Shuttle` GameObjects inside the scene.

### New Scripts
1. `Assets/Scripts/EnvironmentSetup.cs`: Replaces default skybox with pure black, sets cameras to Solid Color black clear mode, and configures ambient lighting.
2. `Assets/Scripts/StarfieldGenerator.cs`: Procedurally generates a glowing, twinkling 3D starfield particle system that locks onto and follows the active camera (giving the perfect illusion of an infinite star background).
3. `Assets/Scripts/PlanetRotator.cs`: Rotates planets around their local Y axis. Dynamically detects if the player is currently walking on that planet OR if the player is piloting the shuttle and this planet is the closest one (landing phase), in which case it pauses the planet's rotation so that gravity, movement, and flight alignments remain 100% stable.

---

# Implementation Steps

### Step 1: Create EnvironmentSetup.cs
- **Description**: Implement `EnvironmentSetup.cs`.
  - In `Start()`, locate all cameras in the scene (`Main Camera` and `SecondaryCamera`).
  - Configure each camera to clear to `SolidColor` and set the background color to pure black (`Color.black`).
  - Clear the skybox (`RenderSettings.skybox = null;`).
  - Configure Flat Ambient Source with a low-intensity, cool ambient color (`new Color(0.12f, 0.12f, 0.15f, 1f)`) so that shadowed parts of meshes are still nicely visible in the dark.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Create StarfieldGenerator.cs
- **Description**: Implement `StarfieldGenerator.cs`.
  - Dynamically spawns a child Particle System.
  - Setup particle system properties:
    - Material: Instantiates a material with `Shader.Find("Universal Render Pipeline/Particles/Unlit")` (or fallback `Shader.Find("Universal Render Pipeline/Unlit")`), colored neon white/yellow/blue.
    - Burst-emits 800-1200 static particles on start.
    - Shape: Massive sphere shell with a radius of `450f` centered around the camera.
    - Particle properties: Infinite lifetime, static local positions.
    - Enable random twinkle/flicker by mapping a subtle random noise or size pulsation to the particles.
  - In `LateUpdate()`, find the active camera (`Main Camera` or `SecondaryCamera`) and match the starfield transform's position to it.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Create PlanetRotator.cs
- **Description**: Implement `PlanetRotator.cs`.
  - Rotates the planet on its local axis: `transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);`.
  - Implements dynamic detection logic in `Update()`:
    - Finds player `"Mr.Blast"`. If player is on-foot on *this* planet (verified by checking `SphericalCharacterController.planet == transform`), pause rotation.
    - Finds `"Shuttle"`. If the shuttle is active and `ShuttleController.isPiloted` is true:
      - Determine which planet is closest to the shuttle's current position.
      - If *this* planet is the closest planet, pause rotation to prevent alignment issues during shuttle landing.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 4: Add Rotators and Environment Setup to Scene
- **Description**: Write and run an editor script to:
  - Open `Kickoff` scene.
  - Create a new empty GameObject `_EnvironmentSetup` and attach `EnvironmentSetup` and `StarfieldGenerator` to it.
  - Find all GameObjects starting with `"Planet"` and attach `PlanetRotator` to each of them.
  - Save and serialize the scene.
- **Assigned role**: developer
- **Dependencies**: Steps 1, 2, 3
- **Parallelizable**: No

---

# Verification & Testing
1. **Scene Layout Verification**:
   - Check that all 5 planets in the `Kickoff` scene successfully have the `PlanetRotator` component attached.
2. **Environment & Visual Verification**:
   - Run the game in `Kickoff` scene.
   - Verify that the background is completely, deep-space black with beautiful twinkling star fields scattered around.
   - Verify that as the player travels thousands of units, the star field moves with the camera, making stars look infinitely far away.
3. **Rotation Verification**:
   - Watch the planets from space while piloting the shuttle. Verify that the planets are rotating slowly.
   - Fly the shuttle close to a planet. Verify that as you get near and it becomes the closest planet, its rotation smoothly halts so you can land cleanly.
   - Exit the shuttle. Verify that the planet you are currently standing on does not rotate, while the other distant planets in space continue their majestic rotation.
