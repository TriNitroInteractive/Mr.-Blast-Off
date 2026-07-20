# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Players**: Single player
- **Inspiration / Reference Games**: Retro sci-fi games, nuclear simulation games.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Target Platform**: PC (StandaloneWindows64)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: URP

# Game Mechanics
## Core Gameplay Loop
The player selects 10 radioactive fuel elements, customizes their planetary detonator, launches in the shuttle, lands on the designated hazard planet, navigates to the terminal, and triggers the nuclear detonation. Once triggered, a 10-second adrenaline escape countdown begins, during which the player must run back to the shuttle and pilot it to a safe space boundary.

## Controls and Input Methods
Standard keyboard and mouse controls for flying the shuttle and moving Mr. Blast on foot.

# UI
- **Element Selection Panel**: Replaces the generic colored template slots with high-quality buttons displaying both the element's custom color background AND its specific neon element sprite icon, with text labels resized and aligned cleanly underneath the icon.
- **Playtime Inventory Bar (HUD)**: Re-renders the slot layout inside the active gameplay HUD to display the same element icon sprites next to their text labels for visual consistency.

# Key Asset & Context
- `Assets/Sprites/elements/1.png` to `30.png`: The 30 custom element icons.
- `Assets/Resources/elements/`: The target runtime load folder.
- `Assets/Scripts/ElementSelectionManager.cs`: Generates the selection grid slots.
- `Assets/Scripts/ElementSlot.cs`: Displays details for each selection slot.
- `Assets/Scripts/InventoryBarManager.cs`: Renders playtime selected slots at the bottom HUD.
- `Assets/Scripts/ShuttleController.cs`: Handles flight movements and camera tracking.
- `Assets/Scripts/GameManager.cs`: Controls transitions and cinematic state pacing.

---

# Implementation Steps

### Step 1: Create Resources Directory, Move and Rename Sprites
- **Description**: Move the 30 individual sprites from `Assets/Sprites/elements/` into `Assets/Resources/elements/` and rename them from numbers (`1.png` to `30.png`) to their corresponding element names (`Uranium.png`, `Plutonium.png`, etc.) so they can be loaded dynamically at runtime in builds.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Refactor ElementSlot.cs to Render Dynamic Icon Sprites
- **Description**: Rewrite `ElementSlot.cs`'s `Init()` to:
  - Dynamically load the matched sprite from `Resources/elements/` using the element's name.
  - Create a child GameObject named `Icon` with an `Image` component.
  - Set the `Icon` image's sprite to the loaded element sprite and color to White (so it displays its native colors).
  - Constrain the `Icon` to the top 65% of the 50x50 slot.
  - Adjust the text label to fit the bottom 35% of the slot at a clean 8pt font to ensure perfect readability.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Refactor InventoryBarManager.cs to Display Icon Sprites in Gameplay HUD
- **Description**: Update `InventoryBarManager.cs`'s slot setup loop:
  - Inside the slot generation, instantiate the same child `Icon` structure with the element sprite matching the active selected element.
  - Position the `Icon` at the top 65% and the text label at the bottom 35% to match the grid's visual style perfectly.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Refactor ShuttleController.cs and GameManager.cs to Prevent Camera Overriding
- **Description**:
  - In `ShuttleController.LateUpdate()`, bypass camera follow snap if `cameraFollow.isCinematicActive` is true.
  - In `GameManager.cs`, rewrite `TransitionToSecondaryCamera` to accept the entire `PlanetBlaster blaster` instance.
  - After waiting for the total blast duration (`blaster.buildUpDuration + blaster.blastDuration + 1.5f`), smoothly interpolate the camera position and rotation from the secondary camera viewpoint back to the shuttle/player follow target position, and then set `cameraFollow.isCinematicActive = false` to return smooth gameplay control without snaps.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

---

# Verification & Testing
1. **Teletype Sprite Mapping & Selection Grid**:
   - Run the game and enter the Preparation Scene.
   - Verify that all 30 elements now show their corresponding custom sprite icons with names cleanly aligned underneath, with no visual overlapping.
2. **Playtime HUD Inventory Consistency**:
   - Select 10 elements and transition to Kickoff.
   - Verify that the bottom inventory bar renders the exact same icon sprites matching your selected elements inside their slots.
3. **Cinematic Explosion & Camera Follow Return**:
   - Land on Planet M, configure element terminal, trigger detonation, and board the shuttle.
   - Wait for the countdown to hit 0 and fly the shuttle into space.
   - Verify that the camera shifts smoothly to the scenic `SecondaryCamera` side view showing the planetary buildup and epic blast, staying focused on the explosion.
   - Once the explosion is fully complete, verify that the camera smoothly pans back to the shuttle's flight follow position and resumes manual flight control seamlessly.
