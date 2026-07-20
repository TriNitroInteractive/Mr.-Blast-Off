# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Escaping Before Planetary Detonation
To add high-stakes strategic tension, once a player successfully configures a reactor and triggers detonation, the game will initiate a **10-second countdown**. 
During these 10 seconds, the reactor core builds up critical pressure. The player is not locked; they must run back to their parked shuttle, board it, and pilot it into space to escape the blast.
- **Success (Survived)**: If the countdown reaches 0 and the player is safely inside the shuttle (`isPiloted == true`), the planet explodes, the cinematic camera executes, and they survive.
- **Failure (Vaporized)**: If the countdown reaches 0 and the player is still on-foot on the planet, they are vaporized immediately by the explosion, leading to a custom critical failure screen.

# UI
- **`[Count Down]` (TMPro Text)**: Pre-existing text element in the `Kickoff` scene canvas. It will be hidden at start and display a dramatic, flashing, bold red/orange count-down sequence (e.g. `T-MINUS 10`) once detonation is triggered.

# Key Asset & Context
- `Assets/Scenes/Kickoff.unity`: Gameplay scene containing the canvas with `[Count Down]` and the `Shuttle` / `Mr.Blast` GameObjects.
- `Assets/Scripts/GameManager.cs`: Handles detonation criticality check, game state, camera transitions, and player death.

---

# Implementation Steps

### Step 1: Add Countdown Reference and Dynamic Death Support in GameManager.cs
- **Description**: Add fields and modify `GameManager.cs` to handle the countdown and death screen:
  - Add `private TextMeshProUGUI _countdownText;` and `private string _deathReasonMessage = "";`.
  - In `OnSceneLoaded()` (or in `Start()`), dynamically search the scene for the `[Count Down]` GameObject, fetch its `TextMeshProUGUI` component, and disable/clear its text initially so it doesn't clutter the screen during normal gameplay.
  - Modify `KillPlayer(string customReason = null)` so it can accept a custom reason string. Set `_deathReasonMessage` accordingly.
  - Update `CreateGameOverUI()` to display `_deathReasonMessage` in the description label instead of the hardcoded default text.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Implement Countdown Coroutine and Escape Checking
- **Description**: Implement the 10-second countdown coroutine and integration in `GameManager.cs`:
  - Create `private IEnumerator CountdownAndDetonateRoutine(PlanetBlaster blaster)`:
    - If `_countdownText` is found, activate its GameObject.
    - Loop 10 seconds:
      - Set text to `$"<color=red>T-MINUS {i}</color>"` (bold/red style).
      - Wait 1.0 seconds (unscaled time or regular time scale).
    - When countdown finishes:
      - Deactivate/hide `_countdownText`.
      - Find `ShuttleController` in the scene.
      - If `shuttleController != null && shuttleController.isPiloted`:
        - **Success!** Trigger the scenic cinematic camera transition: `StartCoroutine(TransitionToSecondaryCamera(blaster.transform));`
        - Trigger actual detonation: `blaster.Detonate();`
      - Else:
        - **Failure!** The player is still on-foot. Trigger player vaporization: `KillPlayer("T-MINUS ZERO REACHED!\nYou failed to board the shuttle and escape the planet before detonation!");`
  - In `TryDetonate()`, if reaction criticality is met:
    - Record and calculate high score.
    - Start the `CountdownAndDetonateRoutine(blaster)` coroutine instead of immediately detonating.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Compilation Check**:
   - Ensure the modified code compiles with zero errors or warnings.
2. **Normal Gameplay Verification**:
   - Start the Kickoff scene. Verify that the `[Count Down]` text is hidden/cleared on start and does not clutter the HUD.
3. **Detonation Countdown Verification**:
   - Detonate the reactor on Planet M.
   - Verify that the countdown begins immediately, counting down from 10 to 1 in bold red text.
   - Verify that the player maintains full movement control and can run around or board the shuttle.
4. **Escape Failure Verification**:
   - Do not board the shuttle. Let the countdown hit 0.
   - Verify that the player is vaporized into procedural molten debris, and the game over screen states: *"T-MINUS ZERO REACHED! You failed to board the shuttle and escape..."*
5. **Escape Success Verification**:
   - Trigger detonation.
   - Run to the shuttle, board it, and fly it into space.
   - Let the countdown hit 0.
   - Verify that the planet explodes, the camera pans to show the epic planetary vaporization, and the player survives inside the shuttle safely.
