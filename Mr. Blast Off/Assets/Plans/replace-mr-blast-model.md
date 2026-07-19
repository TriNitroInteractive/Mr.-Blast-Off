# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: A physics-based celestial exploration game where the player navigates small planetoids, prepares a planetary detonation, and launches themselves to the next celestial body.
- Players: Single-player
- Inspiration / Reference Games: Super Mario Galaxy (spherical gravity, planetoids)
- Tone / Art Direction: Stylized, sci-fi, slightly comedic, bright and colorful.
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape (1920x1080)
- Render Pipeline: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player (Mr. Blast) explores a spherical planet, interacts with items/objects, manages resources, and triggers a planetary detonation. Upon detonation, Mr. Blast is launched into deep space with high-velocity physics.
## Controls and Input Methods
The player moves around the planet using keyboard/mouse or gamepad controls (WSAD/arrow keys) via the New Input System. The camera follows the player in a spherical orbit aligned with gravity.

# UI
Not directly modified in this task. We have an inventory bar and elements HUD.

# Key Asset & Context
- `Assets/Characters/Mr.Blast/Idle.fbx` (Visual model, generic rig)
- `Assets/Characters/Mr.Blast/Run_InPlace.fbx` (Run animation, generic rig, already in-place)
- `Assets/Characters/Mr.Blast/Texture/MrBlast/` (Original textures)
- `Assets/Scripts/SphericalCharacterController.cs` (Character controller, to be modified)
- `Assets/Scenes/Kickoff.unity` (The gameplay scene containing Mr.Blast)

# Implementation Steps

## Step 1: Configure Texture Import Settings
- **Description**: Set up import settings for textures in `Assets/Characters/Mr.Blast/Texture/MrBlast/`:
  - Set `MrBlast1_Normal.jpg` texture type to **Normal Map** and apply.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Create Metallic-Smoothness Packed Texture
- **Description**: Programmatically pack `MrBlast1_Metallic.jpg` (R channel) and `MrBlast1_Roughness.jpg` (inverted for smoothness, Alpha channel) into a single texture `MrBlast1_MetallicSmoothness.png` inside `Assets/Characters/Mr.Blast/Texture/MrBlast/`. This is standard for URP Lit material metallic-smoothness workflow.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Create and Configure URP Lit Material
- **Description**: Create a new material `Assets/Characters/Mr.Blast/MrBlast_Material.mat` using the `Universal Render Pipeline/Lit` shader. Assign textures:
  - Base Map: `MrBlast1_Base_color.jpg`
  - Normal Map: `MrBlast1_Normal.jpg` (Bump scale = 1.0)
  - Metallic Map: `MrBlast1_MetallicSmoothness.png` (Smoothness Source: Metallic Alpha, Smoothness factor = 1.0)
  - Occlusion Map: `MrBlast1_Mixed_AO.jpg` (Occlusion factor = 1.0)
  - Emission Map: `MrBlast1_Emissive.jpg` (Enable emission, set `_EmissionColor` to Warm/Orange, e.g. RGB(1.5, 1.2, 0.8) to make the model glow beautifully).
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Configure Animation Clips
- **Description**: Set import settings on `Assets/Characters/Mr.Blast/Run_InPlace.fbx` so it loops perfectly:
  - Enable **Loop Time** (`loopTime = true`) for `Take 001` clip inside `Run_InPlace.fbx` so the animation loops smoothly and continuously without getting stuck.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 5: Programmatically Create 1-Frame Idle Animation
- **Description**: Since `Idle.fbx` does not contain an animation clip, write a temporary Editor script to traverse the bone hierarchy of `Idle.fbx`, capture the default joint positions and rotations (the bind/standing pose), and create a high-quality 1-frame looping `Idle.anim` clip in `Assets/Characters/Mr.Blast/`. This ensures the character stays in a natural standing pose when stationary, rather than freezing or T-posing.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 6: Create Animator Controller
- **Description**: Create `Assets/Characters/Mr.Blast/MrBlast_AnimatorController.controller`:
  - Add a float parameter `Speed`.
  - Add two states: `Idle` (plays `Idle.anim`) and `Run` (plays the actual `Take 001` clip from `Run_InPlace.fbx` and NOT any `__preview__Take 001` clip to prevent freezing).
  - Create bidirectional transitions with `Transition Duration = 0.15s` and `Has Exit Time = false`:
    - `Idle` -> `Run` when `Speed > 0.1`
    - `Run` -> `Idle` when `Speed < 0.1`
- **Assigned role**: developer
- **Dependencies**: Step 4, Step 5
- **Parallelizable**: No

## Step 7: Update `SphericalCharacterController.cs` to Drive Animator
- **Description**: Edit `Assets/Scripts/SphericalCharacterController.cs` to cache and update the Animator component:
  - Add a private reference `private Animator animator;` cached via `GetComponentInChildren<Animator>()` in `Awake()`.
  - In `FixedUpdate()`, set the `Speed` parameter of the animator based on the input magnitude: `animator.SetFloat("Speed", input.magnitude);`.
  - Add `OnDisable()` and `OnDestroy()` safety logic to set the animator's speed to 0 so the character transitions back to Idle when disabled (such as during planetary launch).
- **Assigned role**: developer
- **Dependencies**: Step 6
- **Parallelizable**: No

## Step 8: Replace Generic Cube with Mr. Blast 3D Model in Scene
- **Description**: Open `Kickoff.unity`, modify the `Mr.Blast` GameObject:
  - Remove `MeshFilter` and `MeshRenderer` (the orange cube visuals).
  - Instantiate `Idle.fbx` as a child GameObject named `Visuals`.
  - Set its scale to `(25, 25, 25)` so its height matches the visual scale of the game (approx. 1.5 units height).
  - Set its local position to `(0, -0.5, 0)` so its feet align with the bottom of the physics collider (local Y = -0.5 is the bottom of the original cube).
  - Set its local rotation to `(0, 0, 0)`.
  - Assign `MrBlast_Material.mat` to the child's `MrBlast` SkinnedMeshRenderer.
  - Add an `Animator` component to the child `Visuals` (or parent) and assign `MrBlast_AnimatorController.controller`. Since we are using the `Run_InPlace.fbx` animation which does not have any forward translation, we do not need Root Motion. Set **Apply Root Motion** to **false** (disabled).
  - Modify the parent's `BoxCollider`:
    - Set Center to `(0, 0.25, 0)` and Size to `(0.8, 1.5, 0.8)` to fit the new character model perfectly.
- **Assigned role**: developer
- **Dependencies**: Step 3, Step 6, Step 7
- **Parallelizable**: No

# Verification & Testing
## Automated Checks
- Verify that `MrBlast_Material.mat` is successfully created with correct texture maps assigned.
- Verify that `Idle.anim` is created and has valid curves.
- Verify that `Run_InPlace.fbx` has its `Take 001` clip with loopTime set to true.
- Verify that the Animator Controller's "Run" state references the actual `Take 001` clip from `Run_InPlace.fbx` and NOT the `__preview__Take 001` clip.
- Verify that the Animator's `applyRootMotion` is disabled in the scene.

## Manual Play Tests
1. **Idle/Movement Transition**: Press WSAD to move Mr. Blast. Ensure he transitions smoothly to the running animation without popping. Stop moving and verify he transitions back to a natural standing idle pose.
2. **Animation Speed/Timing**: Check that the run animation speed matches the movement speed. It should feel smooth and well-grounded (no sliding or weird pacing).
3. **Materials/Vibe**: Verify the metallic and emissive parts of the model look excellent under the directional lighting.
4. **Planetary Detonation**: Trigger the planetary explosion. Verify that `SphericalCharacterController` is disabled, and Mr. Blast is launched into space while maintaining a clean, non-glitched pose.
