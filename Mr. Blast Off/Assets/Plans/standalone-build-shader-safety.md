# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: "Mr. Blast Off" is a 2D strategic engineering game for a game jam themed "Kickoff". Players configure reactor cores with various nuclear elements to safely vaporize celestial hazards and pilot shuttle escapes.
- **Players**: Single player
- **Inspiration / Reference Games**: Space Chem, Reactor simulators, arcade space strategy games
- **Tone / Art Direction**: Sci-fi retro holographic UI, neon glows, slightly comedic but high-stakes tone
- **Target Platform**: Standalone Windows (PC)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player selects a target planet from the cockpit workspace, selects exactly 10 radioactive or structural elements to balance core reactor yield, and triggers the "Kickoff" reaction to play the arcade core-priming cleanup phase.

## Controls and Input Methods
- **Keyboard / Mouse**: Navigate UI, select planets/upgrades, and interact with terminals using event-driven bindings from the New Input System.

# UI
- **Preparation Workspace**: Displays locked planets, upgrades, 3D shuttle preview, and dynamic element selection grid panels.
- **GameplayInstructionsHUD**: Floating header banner guiding players on active mission objectives.

# Key Asset & Context
- **Assets/Scripts/CockpitManager.cs**: Manages planet slots, upgrades, and initializes the 3D spinning shuttle preview.
- **Assets/Scripts/ShuttlePowerEffect.cs**: Generates animated power rings and applies premium glass materials to the shuttle at runtime.
- **Assets/Scripts/GameManager.cs**: Oversees lifecycle transitions, explosions, and debris instantiation.
- **Assets/Scripts/TerminalGuidanceSystem.cs**: Builds path and runner visual tracks to prime core reactor terminals.
- **Assets/Scripts/StarfieldGenerator.cs**: Configures star backgrounds dynamically.

---

# Implementation Steps

### Step 1: Fix Unsafe Shader Loading and Exception Vulnerability in CockpitManager
- **Description**:
  - Update `SetupShuttle3DPreview` in `CockpitManager.cs` to safely query shaders.
  - Implement fallback checks (checking for `"Universal Render Pipeline/Lit"`, `"Standard"`, `"Sprites/Default"`, and `"Hidden/InternalErrorShader"`).
  - Wrap the `new Material(...)` creation with null-checks to prevent any `ArgumentException` or `NullReferenceException` from halting the `Start` sequence. This ensures the planet slots and reactor upgrades are always populated even if the premium URP shaders are missing or stripped in the built player.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Secure Shader Loading in ShuttlePowerEffect
- **Description**:
  - Refactor `ShuttlePowerEffect.cs`'s `ApplyGlassyMaterial()` and `CreatePowerRings()` to perform robust null-safety checks and shader fallbacks.
  - Ensure that missing shaders gracefully fallback without breaking the script's initialization sequence.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Secure Remaining Shader.Find Calls in GameManager, TerminalGuidanceSystem, and StarfieldGenerator
- **Description**:
  - Inspect and protect `GameManager.cs`'s debris particle generation, `TerminalGuidanceSystem.cs`'s path renderer initialization, and `StarfieldGenerator.cs`'s particle rendering configurations.
  - Ensure every `new Material(Shader.Find(...))` call across the entire project is protected with falling-back null-safety checks to guarantee build robustness.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

---

# Verification & Testing

## Manual Verification
1. **Editor Play Mode Verification**:
   - Open `Preparation` scene. Press Play and verify that the Cockpit panels (Planets, Upgrades, 3D Shuttle preview) function perfectly with no console errors or exceptions.
2. **Build Compilation and Log Verification**:
   - Run a standalone Windows compilation build.
   - Run the built application, navigate to the Preparation scene, and verify that the Cockpit UI is fully visible, fully functional, and perfectly responsive.

## Automated Verification
1. Run all Play Mode Tests to verify 100% compliance across all core mechanics.
