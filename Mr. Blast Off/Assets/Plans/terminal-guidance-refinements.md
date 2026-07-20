# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Navigation Guidance Refinements
The user has requested some specific adjustments to the **Dynamic Neon Terminal Guidance System** to better fit their vision:
1. **Thinner Path**: Reduce the static cyan path dot scale to make it a bit thinner (`pathDotSize = 0.18f` instead of `0.35f`) to make it look sleek and precise.
2. **Deactivate in Shuttle**: When piloting the Shuttle, the geodesic guidance path must be deactivated. It will only reappear once the player exits the shuttle and lands on a planet surface.
3. **Deactivate on Detonation Command**: Once the reactor is triggered and the 10-second escape countdown begins, the guidance line must deactivate. This signifies that the navigation phase is complete and builds the adrenaline of the escape phase.

# UI
None affected.

# Key Asset & Context
- `Assets/Scripts/GameManager.cs`: Manage state variables for `IsCountdownActive`.
- `Assets/Scripts/TerminalGuidanceSystem.cs`: Manage dot sizing, shuttle-piloted checks, and countdown state checks.

---

# Implementation Steps

### Step 1: Add IsCountdownActive State to GameManager.cs
- **Description**: Expose `public bool IsCountdownActive { get; private set; } = false;` in `GameManager.cs`.
  - In `InitializeGameplayState()`, set `IsCountdownActive = false;`.
  - In `CountdownAndDetonateRoutine()`, set `IsCountdownActive = true;` on entry and `IsCountdownActive = false;` on exit.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Refactor TerminalGuidanceSystem.cs for Thinner Path and Dynamic States
- **Description**: 
  - Change default `pathDotSize` to `0.18f` for a sleeker profile.
  - In `LateUpdate()`, add a check:
    - If `GameManager.Instance != null && (GameManager.Instance.IsCountdownActive || GameManager.Instance.IsGameOver)`, call `HideAllDots()` and return.
    - If `_shuttleController != null && _shuttleController.isPiloted`, call `HideAllDots()` and return (disabling guidance in flight).
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Compilation Verification**:
   - Verify that the game compiles cleanly with zero errors.
2. **Path Thickness Check**:
   - Verify that the path dots are thinner and sleeker (0.18 units in size).
3. **Shuttle Flight Check**:
   - Enter Play Mode, board the Shuttle. Verify that the guidance line and all dots are completely deactivated while piloting.
4. **On-Foot Landing Check**:
   - Land on any planet and exit the Shuttle. Verify that the guidance line instantly reactivates and points from your position to the terminal.
5. **Countdown Deactivation Check**:
   - Interact with the terminal. Verify that the moment you command the blast (criticality is achieved and the T-MINUS 10 countdown starts), the cyan path and yellow runner dots are deactivated.
