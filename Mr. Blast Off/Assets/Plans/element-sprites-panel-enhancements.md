# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A strategic 2D/3D hybrid sci-fi engineering game where players act as a nuclear engineer for the Interstellar Orbital Cleanup Authority (IOCA), building a planetary destruction device to vaporize celestial hazards in a single controlled detonation.
- **Players:** Single player
- **Inspiration / Reference Games:** Retro sci-fi games, nuclear engineering strategy simulators.
- **Tone / Art Direction:** Light-hearted sci-fi retro-futurism with bright neon-holographic interfaces, electric cyan, and hot molten colors.
- **Target Platform:** PC (StandaloneWindows64)
- **Screen Orientation / Resolution:** Landscape (1920x1080)
- **Render Pipeline:** Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player selects exactly 10 radioactive fuel elements, customizes their planetary detonator upgrades, launches in their shuttle, lands on the designated hazard planet, navigates to the terminal, and triggers the nuclear detonation. Once triggered, a 10-second adrenaline escape countdown begins, during which the player must run back to the shuttle and pilot it to a safe space boundary.

## Controls and Input Methods
- **Mouse Hover (IPointerEnter / IPointerExit):** Hovering over an element slot smoothly scales up the slot and displays high-fidelity thermodynamic detail text.
- **Left-Click (IPointerClick):** Clicking a slot toggles selection, triggers a bouncy scale transition, and activates/deactivates a bold selection outline.
- **Start Button:** Validates the selection and transitions to the Kickoff gameplay level.

# UI
- **Element Selection Panel:** Replaces the direct dynamic file reads with an optimized preloaded sprite cache.
- **Juicy Interactive Scaling:** Replaces instant scale toggles with smooth, responsive spring-based scale lerps.
- **Thematic Info HUD:** Dynamically updates the selection panel's status text with rich, formatted scientific/engineering descriptions for whichever element is currently hovered, restoring selection count metrics on mouse-out.

# Key Asset & Context
- `Assets/Resources/elements/*.png`: 30 pre-imported high-quality PNG sprite icons.
- `Assets/Scripts/ElementSelectionManager.cs`: Controls slot generation, layouts, start button validations, and will house the static preloaded sprite dictionary and element chemical classification database.
- `Assets/Scripts/ElementSlot.cs`: Manages slot interactions. Will be refactored to support smooth hover scaling, hover pointer callbacks, outline colors, and dynamic tooltip updates.
- `Assets/Scripts/InventoryBarManager.cs`: Handles playtime select bar icons. Will be refactored to draw from the static preloaded sprite cache rather than repeated file reads.

# Implementation Steps

### Step 1: Establish Element Scientific Database & Preload Cache in ElementSelectionManager
- **Description**: 
  - Add a static dictionary `public static readonly Dictionary<string, Sprite> ElementSprites = new Dictionary<string, Sprite>();` in `ElementSelectionManager`.
  - In `Awake()`, loop through the 30 element names and load them into the dictionary using `Resources.Load<Sprite>("elements/" + name)`.
  - Add a chemical classification struct `ElementThematicData` mapping element names to chemical categories (Actinide Fuel, Fusion Isotope, Radiation Source, Shielding/Absorber, Transition core) and custom physics descriptions.
  - Implement a public retrieval method `public string GetElementThematicDescription(string name)` that formats detailed rich-text string (e.g., colorized element category, yield index, and a descriptive blurb).
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Refactor ElementSlot.cs with Smooth Lerps & Hover Tooltips
- **Description**: 
  - Implement `IPointerEnterHandler` and `IPointerExitHandler` on `ElementSlot` to track mouse hover states.
  - In `Init()`, retrieve the sprite directly from `ElementSelectionManager.ElementSprites[name]` to skip disk assets access.
  - In `OnPointerEnter`, smoothly animate scale to `1.08f` and invoke `ElementSelectionManager` to update the Status Text with the element's scientific description.
  - In `OnPointerExit`, smoothly restore scale back to baseline (`1.0f` if deselected, `0.85f` if selected), restoring the original selection status text.
  - Replace the instant selection scale jump with a smooth spring transition coroutine that interpolates from `1.0f` down to `0.85f` (selected state).
  - Enhance visual outline: set outline color to electric cyan on hover, and red on selection.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Refactor InventoryBarManager.cs to Use Preloaded Sprites
- **Description**: 
  - In `InventoryBarManager.InitializeInventory()`, replace the direct `Resources.Load<Sprite>` call with the static dictionary retrieval `ElementSelectionManager.ElementSprites[name]` to maximize garbage collection optimization and asset reuse.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Validate PlayMode & Save Scene
- **Description**: Run a verification script that enters playmode, iterates over slots, and triggers mouse-hover and clicks to ensure zero runtime exceptions.
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Preload Asset Verification**:
   - Enter Play Mode in the Preparation scene.
   - Verify that all 30 elements load successfully on startup without console warnings or file load lag.
2. **Hover Tooltip Verification**:
   - Hover the mouse cursor over Uranium, Tritium, and Boron.
   - Verify that the bottom status text instantly displays their custom rich-text classification (e.g., Actinide, Isotope) and description.
   - Move the cursor out and verify the text smoothly returns to `"Select nuclear fuels & elements: X / 10"`.
3. **Smooth Interactive Scaling**:
   - Hover over a slot. Verify that the slot scales up smoothly to `1.08x` with a responsive lerp rather than snapping.
   - Click a slot. Verify that it scales down to `0.85x` with a quick spring-bounce visual effect, and the outline glows red.
4. **Playtime HUD Consistency**:
   - Select 10 elements and click Start.
   - Verify that the bottom Inventory Bar renders the selected elements with their correct preloaded icons.
