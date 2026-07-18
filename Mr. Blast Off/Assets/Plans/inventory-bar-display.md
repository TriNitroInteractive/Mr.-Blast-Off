# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game with an initial "Element Selection" phase and subsequent gameplay phase.
- **Players:** Single player
- **Target Platform:** PC (StandaloneWindows64)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The game starts with an "Element Selection" panel where the player chooses exactly 10 nuclear/stabilizing elements. After selecting them and clicking "Start", the Element Selection panel deactivates, the game unpauses, and the chosen 10 elements are displayed on a HUD-level "Inventory Bar" during active playtime.

## Controls and Input Methods
- **Gameplay HUD display:** The Inventory Bar remains fully confined and displayed during playtime, showing the 10 chosen elements to reflect the reactor core configuration.

# UI
- **Inventory Bar:** Sized exactly at its native `750 x 75` at the bottom of the screen.
- **Inventory Slots:** 10 slots in total. Each slot preserves its native size of `50 x 50`.
- **Horizontal Layout Group:** Added programmatically to the `Inventory Bar` to position all 10 slots beautifully and evenly across the 750px width without modifying their size or scale. Spacing is configured to `20` with left/right padding of `25` to perfectly fit the slots within the bar.
- **Visual Feedback:** Each inventory slot is updated to use the color shade of its selected element and includes a text child showing its element name.

# Key Asset & Context
### New Scripts
1. **`Assets/Scripts/InventoryBarManager.cs`**: Handles overall inventory bar layout configuration (HorizontalLayoutGroup setup), dynamic slot duplication (from 1 template to 10), and populating element names/colors once playtime begins.

### Modified Scripts
2. **`Assets/Scripts/ElementSelectionManager.cs`**: Add a static registry of element color shades (`public static Dictionary<string, Color> ElementColors`) and trigger `InventoryBarManager` initialization when the "Start" button is clicked.

# Implementation Steps

## Step 1: Update ElementSelectionManager.cs with Color Registry and Event Trigger
- **Description:** 
  1. Add a public static `Dictionary<string, Color> ElementColors` to `ElementSelectionManager`.
  2. Populate this dictionary during `GenerateSlots()`.
  3. Locate the `Inventory Bar` and call its `InventoryBarManager` to initialize when `OnStartClicked()` is called.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

## Step 2: Implement InventoryBarManager.cs Script
- **Description:** 
  Create `Assets/Scripts/InventoryBarManager.cs` which:
  - Adds a `HorizontalLayoutGroup` programmatically to the `Inventory Bar` with parameters to enforce alignment while ignoring size alterations (`childControlWidth = false`, `childControlHeight = false`, `childForceExpandWidth = false`, `childForceExpandHeight = false`, `spacing = 20`, `padding = left:25, right:25, top:12, bottom:12`).
  - Hides the `Inventory Bar` initially in `Awake()`.
  - Defines a public `InitializeInventory(List<string> selected)` method that:
    1. Activates the Inventory Bar GameObject.
    2. Duplicates the template `Slot` GameObject into 10 slots total.
    3. Adds a `TextMeshProUGUI` label inside each slot showing the element's name with black outline readable borders.
    4. Sets each slot's image color based on `ElementSelectionManager.ElementColors[name]`.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No

## Step 3: Scene Configuration & Setup Script
- **Description:** Attach the `InventoryBarManager` script to the `Inventory Bar` GameObject in the `Kickoff` scene and map its references.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

# Verification & Testing
- **Editor Scene Inspection:** Enter Play mode. The `Inventory Bar` should be invisible initially during Element Selection.
- **Inventory Bar Setup Test:** Select 10 elements and click "Start". Verify that:
  - The `Inventory Bar` activates and becomes visible at the bottom of playtime.
  - Its size remains exactly `750 x 75`.
  - There are exactly 10 slots.
  - Each slot's size remains exactly `50 x 50`.
  - Each slot correctly displays its corresponding selected element name in a readable text format.
  - Each slot's background image matches the exact HSV color shade of the selected element.
- **Console Log Verification:** Ensure no errors or warnings are logged in the Console.
