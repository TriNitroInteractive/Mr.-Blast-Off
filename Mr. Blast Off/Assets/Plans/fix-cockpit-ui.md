# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: "Mr. Blast Off" is a 2D sci-fi engineering/strategy game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards (moons, asteroids, rogue planets) in a single controlled detonation.
- Players: Single player
- Target Platform: PC (StandaloneWindows64)
- Render Pipeline: PC_RPAsset (URP / custom)

# Diagnostic & Root Cause Analysis
During our analysis of the `"Preparation"` scene structure, we identified the following critical issues causing the Cockpit UI to appear blank, unresponsive, or non-functional:
1. **Missing GameManager in Preparation Scene**: `GameManager` is implemented as a `DontDestroyOnLoad` singleton. It is present in `"Kickoff.unity"`, but it is completely missing from `"Preparation.unity"`. When starting play mode from `"Preparation"`, `GameManager.Instance` is `null`.
2. **Early Returns in Cockpit UI Refresh**: Because `GameManager.Instance` is null, methods like `CockpitManager.RefreshUI()` return early and do not update any text fields. As a result, texts default to placeholder string values (like "New Text") and Solacs/Highscores/Profile labels remain empty.
3. **Upgrades Evaluation Failure**: The buy actions and unlock check functions rely on `GameManager.Instance` to store the persistent state of upgrades. Without it, upgrades cannot be queried or unlocked.

# Game Mechanics
## Cockpit Integration
- When starting from `"Preparation"`, a persistent `_GameManager` GameObject will be present right from the beginning.
- This `GameManager` initializes variables (such as starting Solacs of `150` and profile names).
- The `CockpitManager` is then able to retrieve these properties during `Start()`, rendering the planets, dynamic upgrades, and stats beautifully.

# Key Asset & Context
1. **Assets/Scenes/Preparation.unity** (Modified):
   - Add a `_GameManager` GameObject with the `GameManager` component attached, configured identically to `"Kickoff.unity"`.
2. **Assets/Scripts/GameManager.cs** (Modified):
   - Ensure the singleton setup safely tolerates and destroys duplicates when transitioning between scenes.

# Implementation Steps
## Step 1: Add GameManager to Preparation Scene
- **Description**: Add a `_GameManager` GameObject with the `GameManager` component to `"Preparation.unity"`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Verify Scene Synchronization and Save
- **Description**: Save `"Preparation.unity"` and verify that `GameManager.Instance` initializes successfully upon startup.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

# Verification & Testing
1. **Cockpit UI Rendering Test**:
   - Play from `"Preparation.unity"`.
   - Verify that Name, Level, Solacs count, and Score labels display correct real-time data instead of placeholder text.
   - Verify that 3D Shuttle Preview renders and rotates continuously in the UI.
2. **Interactive Upgrades Test**:
   - Click on the Catalyst Pre-heater upgrade (cost: 50 Solacs).
   - Verify that the slot color changes to green and states `ACTIVE`, and your Solacs total is reduced by 50.
3. **Multi-World Detonation Flow Test**:
   - Select Planet A.
   - Proceed to Element Selection and select 10 elements.
   - Verify that the game loads `"Kickoff"` scene, spawning the Shuttle directly above Planet A.
