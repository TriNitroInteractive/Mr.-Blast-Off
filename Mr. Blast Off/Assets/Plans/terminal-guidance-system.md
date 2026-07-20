# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Navigation Guidance (Terminal Pointer)
Because players walk on a spherical planet, finding the terminal can sometimes be disorienting. To assist navigation, we will introduce a **Dynamic Neon Terminal Guidance System**:
- When the player is active on-foot on a planet, a small neon-colored dotted line will lie on the planet's spherical surface, connecting the player directly to the planet's control terminal.
- The path is calculated dynamically using a **spherical geodesic projection** to perfectly follow the circular perimeter/hull of the planet.
- To make the guide look alive and show direction:
  - The static path uses small neon-cyan glowing dots.
  - Every 2.5 seconds, a group of larger, neon-yellow "runner dots" will spawn at the player's feet and **cross/travel along the line to the terminal**, making the travel direction immediately intuitive.
  - The system dynamically updates as the player runs, contracting the line. Once the player is close to the terminal, the line fades out cleanly to avoid cluttering the terminal interface.

# UI
None affected (entirely world-space 3D visuals).

# Key Asset & Context
- `Assets/Scenes/Kickoff.unity`: Gameplay scene where the guidance system will be instantiated.
- `Assets/Scripts/SphericalCharacterController.cs`: Holds the player's active planet reference.
- `Assets/Scripts/TerminalGuidanceSystem.cs`: New script implementing the dynamic path calculation, dot pooling, and neon-breathing/running animations.

---

# Implementation Steps

### Step 1: Create/Update TerminalGuidanceSystem.cs with EditMode Support and Proximity Fixes
- **Description**: Refactor `TerminalGuidanceSystem.cs` to add:
  - `[ExecuteAlways]` attribute so that the geodesic guidance line is immediately visible and testable in Edit Mode inside the Scene view of the Unity Editor.
  - Expose `dotSize` (default `0.35f`) and `runnerSize` (default `0.65f`) in the inspector to make the neon dots significantly larger and highly visible.
  - Expose `hideProximity` (default `1.8f`) in the inspector to replace the hardcoded `4.0f` threshold. This prevents the line from being prematurely hidden since the player spawns `4.44` units away from the terminal.
  - Implement **Dual-State Flight & Foot Projection**:
    - If the player is on foot, draw the line from the player's position on the planet's surface to the terminal.
    - If the player is inside the Shuttle piloting, project the Shuttle's current coordinates straight down onto the surface of the nearest planet and draw the geodesic line from that projection point to the terminal. This provides continuous guidance while flying in space!
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Configure and Serialize in Scene
- **Description**: Verify the scene configuration to ensure `_TerminalGuidanceSystem` is loaded, script is active, and settings are saved.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Edit-Mode Verification**:
   - Open `Kickoff` scene in the editor.
   - Verify that the neon dotted path is instantly drawn and visible on the surface of Planet M, pointing toward the terminal.
2. **Interactive Flight & Foot Tracking**:
   - Enter Play Mode.
   - Verify that while piloting the Shuttle, the guidance system projects your flight path on the planet's surface underneath the ship and draws the line directly to the terminal.
   - Exit the shuttle. Verify that the line now tracks your character's position perfectly.
   - Run close to the terminal. Verify that the line fades out cleanly once you are within `1.8` units of the terminal to let you interact easily.





