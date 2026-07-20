# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: "Mr. Blast Off" is a 2D sci-fi strategic engineering game for a game jam themed "Kickoff". Players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), optimizing and configuring a reactor core for a single controlled detonation to vaporize celestial hazards (moons, asteroids, rogue planets).
- **Players**: Single player
- **Inspiration / Reference Games**: Space Chem, Reactor simulators, arcade space strategy games
- **Tone / Art Direction**: Sci-fi retro holographic UI, neon glows, slightly comedic but high-stakes tone
- **Target Platform**: Standalone Windows (PC)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics 
## Core Gameplay Loop
The player selects a target celestial body (planet/moon/asteroid) from the Cockpit workspace. Based on the selected planet's safety threshold, the player enters the Element Selection phase where they choose exactly 10 radioactive or structural elements (moderators, shields, actinides, thermonuclear fuels) to balance core reactor yield and reach/exceed the required reaction points. Once optimized, they trigger the "Kickoff" reaction, and play the arcade-style core-priming cleanup phase (using control terminals, avoiding hazards, and escaping in the Shuttle).

## Controls and Input Methods
- **Keyboard / Mouse**: Navigate UI, click to select planets, upgrade core modules, and click elements to slot them into the reactor matrix.
- **New Input System**: Key presses and pointer interaction events are fully integrated using Unity's modern input architecture.

# UI
- **Preparation Workspace**: Contains multiple sub-panels under the main UI `Canvas`:
  - **Loading Plot**: Initial introductory screen that types out the thematic context (with a pulsing "Press Any Key" prompt).
  - **Cockpit Panel**: Displays locked planet destinations, stats, upgrades, and shuttle preview.
  - **Element Selection Panel**: Displays 30 selectable nuclear elements with descriptions, yields, and a stabilization progress tracker.
  - **GameplayInstructionsHUD**: Floating header banner that guides the player on current objectives.

# Key Asset & Context
- **Assets/Scripts/MainMenuController.cs**: Manages menu operations, play kickoff triggering, and graceful game termination.
- **Assets/Scenes/MainMenu.unity**: Scene containing MainMenu and background graphics.
- **Assets/Scenes/Preparation.unity**: Scene containing the spaceship cockpit and elements reactor customization interface.
- **Assets/Scenes/Kickoff.unity**: Main gameplay scene.

# Implementation Steps

### Step 1: Create a Compilation Build Utility and Verify the Code Compiler
- **Description**: 
  - Create a temporary editor-only verification utility at `Assets/Editor/WindowsBuildVerification.cs`.
  - The script will define a MenuItem `"Build / Verify Standalone Windows Compilation"` that compiles the player dynamically using `BuildPipeline.BuildPlayer` into a temporary build output folder.
  - This checks and reports any compiler errors (such as un-guarded `UnityEditor` references or missing conditional checks) before actual build distribution.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Adapt MainMenu Canvas Scaler for High Resolution Adaptability
- **Description**:
  - In `MainMenu.unity`, modify the `Canvas` component's `CanvasScaler` configuration from `ConstantPixelSize` to `ScaleWithScreenSize` using a reference resolution of `1920x1080` and Match Width or Height set to `0.5f` (balanced).
  - Since the main menu elements are cleanly anchored to Right-Center (`(1.0, 0.5)`), this ensures the layout transitions seamlessly without stretching or appearing microscopic on ultra-wide or 4K/UHD screens.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 3: Run Compiler Validation and Clean Up Temporary Build Verification Scripts
- **Description**:
  - Run the `"Build / Verify Standalone Windows Compilation"` tool in the Editor.
  - Verify if any other compile-time or build warnings pop up.
  - Once compilation matches are successfully verified, cleanly delete the temporary build script to keep the repository pristine.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: No

# Verification & Testing

## Manual Verification
1. **Aspect Ratio Layout Testing**:
   - Open the Game View inside the Unity Editor.
   - Toggle through common aspect ratios: `16:9` (1920x1080), `16:10` (1920x1200), `4:3` (1024x768), and ultra-wide `21:9`.
   - Verify that the Main Menu's "Play" and "Quit" buttons remain in their correct proportional positions and scale beautifully without clipping.
2. **Build Scene Build Settings Order Check**:
   - Verify that the Build settings contains all three scenes in correct operational order:
     - `MainMenu.unity` (Build index 0)
     - `Preparation.unity` (Build index 1)
     - `Kickoff.unity` (Build index 2)

## Automated Verification
1. Run Play Mode Tests to ensure all state machine transitions and reactor calculations remain 100% compliant.
