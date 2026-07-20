# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Core Gameplay Loop
Players prepare in the **Preparation Scene** by selecting their destination planet and optimizing reactor upgrades, then progress to the **Kickoff Scene** to execute planetary detonation.

# UI
All scenes use TextMeshPro (TMP) for UI texts. Currently, the text assets default to the basic `LiberationSans SDF` font asset. We will generate a high-quality TextMeshPro SDF Font Asset for the custom `Blast` font (`Assets/Blast Font/Blast.ttf`) and apply it globally to all text elements in the game. In the `Preparation` scene, the text colors will be made darker and bold, with custom parameters exposed in the Inspector for complete designer control.

# Key Asset & Context
- `Assets/Blast Font/Blast.ttf`: Source TrueType font file.
- `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Preparation.unity`, `Assets/Scenes/Kickoff.unity`: All scenes that need global font application.
- `Assets/Scripts/CockpitManager.cs`: Class handling Cockpit UI. We will add custom font, styling, and icon scaling fields here.

---

# Implementation Steps

### Step 1: Create Programmatic Font Asset Generator & Global Scene Font Updater
- **Description**: Implement a robust editor script `Assets/Editor/BlastFontSetupUtility.cs` that:
  - Generates a fully compatible TextMeshPro SDF Font Asset at `Assets/Blast Font/Blast SDF.asset` with its required atlas texture and default material attached as sub-assets.
  - Adds a menu item `"Tools/Mr. Blast Off/Apply Blast Font Everywhere"` which:
    - Scans and loads `MainMenu`, `Preparation`, and `Kickoff` scenes in the editor.
    - Resolves every single `TextMeshProUGUI` component.
    - Replaces its active font with the newly generated `Blast SDF` asset.
    - Marks modified scenes dirty and saves them cleanly.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Update CockpitManager.cs with Customizable TMPro & Icon Scaling Fields
- **Description**: Add customizable inspector fields to `CockpitManager.cs` to give the user absolute control:
  - `customFont`: Custom TMP Font Asset reference.
  - `forceBold`: Toggle bold mode.
  - `textBaseColor`: Base color for general texts (defaults to dark charcoal/black `new Color(0.12f, 0.12f, 0.15f)`).
  - `planetIconScale`: Vector2 size delta of the planet icon (defaults to `36x36`).
  - `planetIconPosition`: Vector2 local position of the planet icon (defaults to `22x0`).
  - `textLeftOffset`: Offset spacing inside the slot when the planet icon is present (defaults to `44f`).
  - Create a method `ApplyCustomTextSettings(TextMeshProUGUI textComp)` to style individual texts, and a method `ApplySettingsToAllTexts()` to sweep across the cockpit Canvas.
  - In `GeneratePlanetSlots()`, use the exposed `planetIconScale`, `planetIconPosition`, and `textLeftOffset` variables to scale and position the planet sprites properly.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Configure scene, generate font, and apply settings
- **Description**: Execute the font generator script to produce `Blast SDF.asset`, apply the font to all scenes, locate the cockpit manager, assign the new font asset, customize text colors and sizes in the inspector, and save the scenes.
- **Assigned role**: developer
- **Dependencies**: Steps 1, 2
- **Parallelizable**: No

---

# Verification & Testing
1. **Font Asset Verification**:
   - Check that `Assets/Blast Font/Blast SDF.asset` has been successfully created and contains a valid texture atlas sub-asset.
2. **Visual & UI Verification**:
   - Load `MainMenu`, `Preparation`, and `Kickoff` scenes and verify that all buttons and HUD elements use the new custom **Blast** font.
   - In `Preparation` scene, verify that the text elements (such as Engineer Profile, Max Score, Solacs, Shuttle Level, and Button labels) are bold and colored darker for optimum contrast.
   - Modify the exposed properties on `_Cockpit` (on `CockpitManager` component) in the inspector, such as changing `textBaseColor` to another tint, altering icon scale, or toggling bold, and verify that the UI updates dynamically to match.
