# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: 2D sci-fi engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism, neon HUD interfaces.
- **Render Pipeline**: URP

# Game Mechanics
## Core Gameplay Loop
Players prepare in the **Preparation Scene** by selecting their destination planet and optimizing reactor upgrades in the cockpit panel, then progress to the **Kickoff Scene** to execute planetary detonation.

# UI
The cockpit selection menu in `Preparation` contains a list of candidate hazard planets. The buttons currently only display plain text with standard tinted backgrounds. We will enhance these buttons with high-quality planet sprites from `Assets/Sprites/`.

# Key Asset & Context
- `Assets/Scenes/Preparation.unity`: Scene containing the `_Cockpit` GameObject with the `CockpitManager` component.
- `Assets/Sprites/Planet M.png`, `Planet A.png`, `Planet B.png`, `Planet C.png`, `Planet D.png`: Texture files containing the celestial hazard sprites.
- `Assets/Scripts/CockpitManager.cs`: Code handling UI slot generation for planets.

---

# Implementation Steps

### Step 1: Update CockpitManager.cs Data Structure & Generation
- **Description**: Modify `CockpitManager.cs` to support planet sprite mapping.
  - Define a serialized struct `PlanetSpriteConfig` to pair a planet's name with its `Sprite`.
  - Expose a `public List<PlanetSpriteConfig> planetSprites` list on the class.
  - In `GeneratePlanetSlots()`, create a child `GameObject` for the planet icon:
    - Add `UnityEngine.UI.Image` and set its `sprite` by looking up the corresponding entry in `planetSprites`.
    - Set its anchors to Left-Center (`anchorMin = new Vector2(0f, 0.5f); anchorMax = new Vector2(0f, 0.5f);`), sizeDelta to `36x36`, and position it neatly inside the left edge of the slot.
    - Reposition and realign the `PlanetText` component to be left-aligned and shifted right (`offsetMin = new Vector2(44f, 0f)`) to sit elegantly next to the planet sprite.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Configure & Serialize Sprites in Scene
- **Description**: Write and run an editor script to:
  - Open `Preparation` scene.
  - Locate the `_Cockpit` GameObject containing `CockpitManager`.
  - Automatically search for all `.png` files matching `"Planet *"` in `Assets/Sprites/`.
  - Populate the `planetSprites` serialized list on `CockpitManager` with the matching asset references.
  - Save and serialize the scene changes.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Scene Verification**:
   - Open `Preparation` scene, select `_Cockpit`, and verify in the inspector that `Planet Sprites` contains 5 elements with correct sprite references assigned.
2. **Visual Verification**:
   - Enter Play Mode.
   - Go to `Preparation` scene.
   - Verify that each planet button in the panel displays the authentic circular celestial body sprite on its left side.
   - Verify text labels are fully readable, neatly aligned, and do not overlap with the sprites.
   - Click on the planet slots to verify that the selection feedback, sound, and transitions work correctly.
