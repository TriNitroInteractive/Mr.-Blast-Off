# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** Strategic sci-fi nuclear reactor engineering game with spherical gravity and flight mechanics.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Camera Flight Follow
- **Currently:** When Mr. Blast enters the Shuttle, the camera is locked and does not follow the shuttle during flight.
- **Goal:** Enable smooth orbital camera follow behind the Shuttle during flight.
- **Safety:** Prevent camera fight or override during the planetary detonation cinematic by bypassing follow behaviors only when the explosion sequence is active.

## Detonation Yield HUD
- **Currently:** Players select 10 elements without knowing if they have enough yield to satisfy the planet's detonation threshold.
- **Goal:** Display real-time reactive points yield (current total selection / planet required threshold) directly inside the Element Selection HUD status label.
- **Thematics:** Incorporate ship upgrades (Moderator multiplier +10%, Catalyst Pre-heater +10 pts, Super-critical core requirements lowered by -10 pts) into the calculation dynamically.

# UI
- **Element Selection HUD Status:** Format label as:
  `Selected: X/10 | Yield: CurrentYield / TargetRequired Required (PlanetName)`
  Colorizes `CurrentYield` as Green if threshold is satisfied, Yellow if unsatisfied, or Red on warning limits.

# Key Asset & Context
- `Assets/Scripts/SphericalCameraFollow.cs`: Camera orbital follow controller for walking on foot.
- `Assets/Scripts/ShuttleController.cs`: Flight movement and cockpit follow camera.
- `Assets/Scripts/GameManager.cs`: Detonation countdown, explosion timeline, and persistent reactor upgrade states.
- `Assets/Scripts/ElementSelectionManager.cs`: Grid slot controller and selection text formatter.

# Implementation Steps

### Step 1: Implement Dynamic Reactor Yield Calculations in ElementSelectionManager
- **Description**:
  - Add a static dictionary mapping all 30 elements to their core reaction points yield values (matching GameManager's values).
  - In `UpdateStatusDisplay()`, sum selected elements' yield points.
  - Dynamically query active persistent upgrades from `GameManager.Instance` (Moderator multiplier, Catalyst additives, and Supercritical thresholds) to compute the final expected reactivity score.
  - Query target required points dynamically based on the selected planet.
  - Format the status label in rich colorized text: `Selected: X/10 | Yield: CurrentYield / Required Required (Planet)`
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Implement Inactive Target Check in SphericalCameraFollow
- **Description**:
  - In `SphericalCameraFollow.LateUpdate()`, add a defensive check: `if (!target.gameObject.activeInHierarchy) return;`.
  - When the player enters the shuttle (where Mr.Blast is deactivated), this instantly halts the spherical camera without needing to toggle `isCinematicActive`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Implement IsExplosionCinematicActive in GameManager & ShuttleController
- **Description**:
  - In `GameManager.cs`, declare `public bool IsExplosionCinematicActive { get; set; } = false;`.
  - Set it to `true` at the start of `TransitionToSecondaryCamera()` and `false` at the end of the return transition.
  - In `ShuttleController.cs`'s `LateUpdate()`, bypass flight follow if `IsExplosionCinematicActive` is true.
  - Remove `cameraFollow.isCinematicActive = true` from `EnterShuttle()` and `ExitShuttle()` so that flight camera follow executes normally when not in a cinematic.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Verification and Clean Scene Saves
- **Description**: Enter playmode in the Preparation scene, select exactly 10 elements, launch into space, enter flight, trigger detonation, and verify that the camera handles transitions perfectly.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Dynamic Yield Status Display Verification**:
   - Hover and select elements on the Selection Grid.
   - Verify that the total yield increments based on element points.
   - Buy upgrades in the cockpit and verify that the calculated yield (and required points) adapt dynamically.
2. **Shuttle Flight Camera Follow Verification**:
   - In Kickoff scene, enter the Shuttle.
   - Fly the shuttle around in space.
   - Verify that the camera follows smoothly from a behind-the-ship cockpit perspective.
3. **Explosion Cinematic Camera Pivot Verification**:
   - Detonate the planet, board the shuttle, fly to orbit.
   - Wait for the countdown to hit zero.
   - Verify that the camera shifts seamlessly to the Scenic view side-angle showing the planet rumbles, ray expansions, and explosion.
   - Once the explosion completes, verify that the camera returns to smoothly following the flight position of the shuttle.
