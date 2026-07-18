# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards in a single controlled detonation.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Controls and Input Methods
- **True Top-Down Orbit Camera:** The camera is positioned high above the player, looking down. Look-around input (mouse delta / right stick) orbits the camera horizontally around the gravity normal while keeping this top-down perspective (angle clamped between 135° and 175° relative to gravity Up).

# Key Asset & Context
### Modified Assets
1. **`Assets/Scripts/SphericalCameraFollow.cs`**: Needs to be updated to adjust the pitch initialization and limits to restrict looking to a true top-down perspective (e.g., pitch angle of 135° to 175° relative to gravity Up, initializing at 160°).

# Implementation Steps

## Step 1: Adjust Pitch Angles for True Top-Down Perspective in SphericalCameraFollow
- **Description:** 
  Modify `Assets/Scripts/SphericalCameraFollow.cs` to set top-down pitch values:
  - `minPitchAngle` = 135f (which is 45 degrees off straight down)
  - `maxPitchAngle` = 175f (which is 5 degrees off straight down)
  - In `Start()`, initialize `virtualRotation` with `Quaternion.Euler(20f, 0f, 0f)` (giving an initial angle of 160 degrees) instead of `Quaternion.Euler(75f, 0f, 0f)`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

# Verification & Testing
- **Top-Down Camera Test:** Enter Play mode. Verify that the camera is positioned high above the player looking directly down. Move the mouse horizontally to orbit around the player in a top-down view, and vertically to tilt the view within top-down boundaries (clamp between 135° and 175°).
- **Console Log Verification:** Ensure no errors or warnings are logged in the Unity Console.
