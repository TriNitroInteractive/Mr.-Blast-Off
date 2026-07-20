# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Core Gameplay Loop
Players prepare in the **Preparation Scene** by selecting their destination planet and optimizing reactor upgrades, then progress to the **Kickoff Scene** to execute planetary detonation.

# UI
All scenes use TextMeshPro (TMP) for UI texts. We want to apply the custom `Blast` font (`Assets/Blast Font/Blast SDF.asset`) **everywhere** across all scenes, including inside **inactive** GameObjects (such as initially hidden menus or prompt popups).

# Key Asset & Context
- `Assets/Blast Font/Blast SDF.asset`: The custom SDF font asset.
- `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Preparation.unity`, `Assets/Scenes/Kickoff.unity`: All scenes that contain active or inactive UI text elements.

---

# Implementation Steps

### Step 1: Update BlastFontSetupUtility.cs to Sweep Inactive GameObjects
- **Description**: Rewrite `BlastFontSetupUtility.cs` to use `Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()` and filter by scene. This ensures that every text component—whether active, inactive, nested, or disabled—is correctly updated to use `Blast SDF`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Execute the Utility and Save All Scenes
- **Description**: Run the menu command `"Tools/Mr. Blast Off/Apply Blast Font Everywhere"` which will programmatically open each scene, discover every single active and inactive TextMeshPro component, set its font to `Blast SDF`, mark the scene dirty, and save it.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Verification Script**:
   - Run a validation script that additive-loads all three scenes, fetches all `TextMeshProUGUI` components via `Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()`, and confirms that 100% of the texts belonging to those scenes have `font.name == "Blast SDF"`.
