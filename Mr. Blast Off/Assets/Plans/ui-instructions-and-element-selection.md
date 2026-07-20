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
- **Assets/Scripts/GameManager.cs**: Oversees game lifecycle, transitions, scene loading, and generates the dynamic `GameplayInstructionsHUD` banner.
- **Assets/Scripts/LoadingPlotController.cs**: Manages the introductory narrative and skip transitions.
- **Assets/Scripts/ElementSelectionManager.cs**: Generates the element grid procedurally on startup, tracks selected elements, and loads the Kickoff scene.
- **Assets/Scripts/CockpitManager.cs**: Manages the main cockpit interface, planet locking, upgrades, and triggers the Element Selection phase.

---

# Implementation Steps

### Step 1: Add Visibility and Scene-Correction Controls to GameManager
- **Description**: 
  - Update `GameManager.cs` to cache a reference to the dynamically generated `GameplayInstructionsHUD` under `private GameObject _instructionsHUD;`.
  - Add public method `public void SetInstructionsVisible(bool visible)` to show/hide the instructions HUD. If the cached reference is null, use a robust hierarchy search (`Canvas/GameplayInstructionsHUD`) to retrieve it (works even when inactive).
  - In `CreateInstructionsUI`, check if a `LoadingPlotController` exists in the current scene (`Object.FindAnyObjectByType<LoadingPlotController>()`). If it does, set the HUD GameObject to inactive initially.
  - In `OnSceneLoaded` for the `"Preparation"` scene, implement dynamic state correction: locate `LoadingPlotController`, `CockpitManager`, and `ElementSelectionManager` (including inactive instances) and set initial active states programmatically (`Loading Plot` -> Active, `Cockpit` -> Inactive, `Element Selection` -> Inactive). This ensures a clean game setup regardless of editor-time designer layout adjustments.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Show GameplayInstructionsHUD upon Transitioning to Cockpit
- **Description**:
  - Update `LoadingPlotController.cs` inside `TransitionToCockpit()` to activate the `GameplayInstructionsHUD` when transitioning to the Cockpit Workspace.
  - Simply call `GameManager.Instance.SetInstructionsVisible(true);` when the cockpit panel becomes active.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Enable Dynamic Runtime Customization of Element Selection Panel
- **Description**:
  - Refactor `ElementSelectionManager.cs` to expose a `generateLayoutProcedurally` boolean field (defaults to `true`).
  - Add optional serialized slot container fields (`customScrollView`, `customGridRect`, and `customStatusText`) to allow the user to drag and drop custom editor-designed UI hierarchy elements.
  - In `Awake()`, if `generateLayoutProcedurally` is false and custom references are assigned, use the custom UI components instead of procedurally instantiating `ScrollView`, `Grid`, and `StatusText`.
  - Expose all layout parameters (scroll view size/position, cell size/spacing, grid columns, status text position/size/font size/color, start button position/size) as serialized fields under a `[Header("Procedural Layout Customization")]` group.
  - Implement a `public void ApplyLayoutParameters()` method to apply these inspector properties onto the UI elements dynamically.
  - Call `ApplyLayoutParameters()` in `OnEnable()`.
  - Implement a runtime-only `Update()` loop (wrapped in `#if UNITY_EDITOR`) that automatically calls `ApplyLayoutParameters()` every frame in Play Mode when in the Editor, allowing the designer to interactively adjust and polish the UI layout live at runtime.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

---

# Verification & Testing

## Manual Verification
1. **Scene Transition Verification**:
   - Open `Preparation` scene. Ensure the `Element Selection` panel is active on design-time canvas (or whatever state), and enter Play Mode.
   - Verify the game automatically corrects states on scene load: `Loading Plot` should activate, while `Cockpit` and `Element Selection` panels deactivate.
   - Verify that the `GameplayInstructionsHUD` is **not** visible while the Loading Plot is displaying the narrative text.
   - Press any key (or mouse-click) to skip/complete the Loading Plot.
   - Verify that the `Loading Plot` fades out, the `Cockpit` panel becomes active, and the `GameplayInstructionsHUD` smoothly appears on the canvas.
2. **Runtime Layout Modification Verification**:
   - In Play Mode, select a planet from the Cockpit to transition to the `Element Selection` panel.
   - Select the `Element Selection` GameObject in the hierarchy.
   - Change values in the Inspector under `Procedural Layout Customization` (such as `cellSpacing`, `cellSize`, `gridColumnCount`, `statusTextFontSize`, `startButtonPosition`).
   - Verify that the layout adjusts **instantly and live** in the Game View during gameplay.
3. **Editor-Authored Custom UI Verification (Optional)**:
   - Create a custom ScrollView and custom StatusText under the `Element Selection` GameObject in the scene.
   - Disable `Generate Layout Procedurally` in the inspector, and drag-and-drop your custom UI elements into the respective slot fields.
   - Enter Play Mode and verify that the selection slots are correctly spawned directly inside your customized editor-designed ScrollView/Grid instead of the procedural one, and that the custom StatusText updates dynamically with selections.

## Automated Verification
1. Run Play Mode Tests in the Editor (`Window -> General -> Test Runner`).
2. Verify all existing tests, including `PlayModeTestRunner`'s Element Selection phase checking, pass successfully with no errors or regressions.
