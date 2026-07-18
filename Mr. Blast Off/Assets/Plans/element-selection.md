# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game where players prepare and execute a planetary cleanup detonation.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The game begins with an "Element Selection" phase. The player must select exactly 10 nuclear/structural elements from a list of 30 available options (e.g. Uranium, Thorium, Steel, Copper, Plutonium, etc.) to fuel and stabilize their planetary cleanup device. While this panel is open, the game is paused. Once 10 elements are selected and the player clicks "Start", the selected elements are stored in a global data structure, the panel deactivates, and the game is unpaused.

## Controls and Input Methods
- **Slot Selection (Mouse Left-Click):** Click on any of the 30 element slots to select/deselect.
- **Scale Down & Outline Feedback:** Selected slots scale down to `0.85` scale and receive a Red Outline. Deselected slots scale back to `1.0` and disable their outline.
- **Click Start Button:** Initiates the game if exactly 10 elements are chosen. Displays status helper text if the count is incorrect.

# UI Design
The existing `Canvas/Element Selection` panel will be styled and populated dynamically:
- **Element Selection Panel:** Resized to `600 x 480` centered on screen.
- **Grid Container:** A dynamically created layout container of size `520 x 300` anchored at `(0, 40)` inside the panel, with a `GridLayoutGroup` (cellSize `75x55`, spacing `10x10`) to hold the 30 slots.
- **Element Slots:** Generated dynamically using `Slot 1` as a template. Each slot gets its name from our thematic list, a vibrant unique HSV-spectrum shade of color, and a `TextMeshProUGUI` text child showing its element name.
- **Status Text:** A dynamically created `TextMeshProUGUI` text below the grid to show current progress (e.g. `"Selected: 0 / 10"`).
- **Start Button:** Moved to anchoredPosition `(0, -200)` at the bottom of the panel.

# Key Asset & Context
### New Scripts
1. **`Assets/Scripts/ElementSlot.cs`**: Handles individual slot interaction, click callback, outline toggle, and scaling transitions.
2. **`Assets/Scripts/ElementSelectionManager.cs`**: Handles overall selection UI initialization (resizing, grid/slot generation, start button hookup), tracking selection count, storing selected elements in a globally accessible public list, and controlling pause/unpause state.

### Modified Scripts
3. **`Assets/Scripts/TerminalController.cs`**: Modified to ignore key input if `Time.timeScale == 0f` (defensive measure during pause).

# Implementation Steps

## Step 1: Implement ElementSlot.cs Script
- **Description:** Create `Assets/Scripts/ElementSlot.cs` to implement `IPointerClickHandler` and manage selection states, outlines, and scale changes.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 2: Implement ElementSelectionManager.cs Script
- **Description:** Create `Assets/Scripts/ElementSelectionManager.cs` to manage the dynamic slot generation, tracking of 10 selected elements, storing selected elements in a public list, updating status text, and resuming the game on "Start".
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No

## Step 3: Modify TerminalController.cs for Pause Safety
- **Description:** Update `Update()` in `TerminalController.cs` to immediately return if `Time.timeScale == 0f`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 4: Programmatically Bind and Set Up the UI components
- **Description:** Create a run command script that loads the active scene, locates `Canvas/Element Selection`, attaches the `ElementSelectionManager` script, assigns its fields (Start Button, Slot 1 template), and marks the scene as dirty/saved.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

# Verification & Testing
- **Pause on Load Test:** Enter Play mode. The game should start paused (no camera orbiting or player physics running), and the `Element Selection` panel should be active.
- **Color Shade & Slot Setup Test:** Verify 30 slots are generated inside the grid with 30 unique, distinct color shades (generated using HSV steps) and clear text labels.
- **Interactive Clicking Test:** Click slots. Verify:
  - Selected slot scales down slightly (`0.85` scale) and shows a red outline.
  - Clicking again reverts scale to `1.0` and disables the outline.
  - Attempting to select more than 10 is ignored or shows status updates.
- **Start Validation Test:** 
  - Click "Start" with fewer or more than 10 selected. Verify the status text warns the player.
  - Select exactly 10 elements and click "Start". Verify:
    - The panel deactivates.
    - The game resumes (`Time.timeScale` becomes 1.0).
    - The player and camera controllers activate normally.
    - The data structure contains the exactly selected elements.
- **Console Log Verification:** Ensure no exceptions or warnings are logged.
