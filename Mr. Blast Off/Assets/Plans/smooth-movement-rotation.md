# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: 2D sci-fi engineering/strategy game where players build a planetary destruction device.
- Players: Single player
- Target Platform: PC (StandaloneWindows64)
- Render Pipeline: Universal Render Pipeline (URP)

# Game Mechanics
## Smooth Spherical Movement & Rotation
We will fix the player's movement and rotation to prevent sudden snapping, skipping distance, or too fast rotation.
- **Velocity Smoothing (Acceleration/Deceleration)**: Instead of instantly setting the horizontal velocity to the target speed, we will use a linear rate of acceleration and deceleration. This creates a natural build-up and slow-down, taking smooth, small steps.
- **Single-Pass Rotation Alignment**: Instead of compounding two separate slerps in a single frame (which causes erratic rotation speed and stuttering), we compute a single, unified target rotation each frame based on whether the player is moving or stationary, and perform a single smooth Slerp towards it.

# Key Asset & Context
### Modified Assets
- `Assets/Scripts/SphericalCharacterController.cs`: Refactor the physics and movement logic to include horizontal velocity acceleration/deceleration and a unified single-pass rotation Slerp.

# Implementation Steps

## Step 1: Update SphericalCharacterController.cs
- **Description**: Add `acceleration` and `deceleration` settings to the inspector. Update `FixedUpdate()` to project current velocity onto the gravity tangent plane, smoothly interpolate it using `Vector3.MoveTowards` based on player input, and unify the rotation logic into a single smooth Slerp.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

# Verification & Testing
- **Acceleration/Deceleration Test**: Run the game, tap a movement key, and observe that Mr.Blast smoothly accelerates to speed and smoothly glides to a stop rather than instantly snapping.
- **Rotation Smoothness Test**: Rotate in place and while moving. Verify that the rotation is perfectly smooth, does not skip large angles, and aligns correctly with gravity and movement.
- **Console Log Verification**: Ensure no warning or error logs are generated.
