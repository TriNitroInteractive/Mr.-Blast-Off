# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Preparation Scene - Plot Introduction
To set the narrative tone before the strategy phase starts, the game will present a **Loading Plot panel** at the start of the `Preparation` scene.
- The **Cockpit panel** starts inactive.
- The **Loading Plot panel** is active and displays:
  - A dramatic narrative teletype/typing reveal of the background story, appearing word-by-word at a slow, readable speed.
  - A breathing/glowing **"Press Any Key"** prompt that fades in and out continuously like a pulsing light.
- Once the user presses **any key** (or clicks/taps the screen), the `Loading Plot` panel is deactivated, and the main `Cockpit` panel is activated so they can select a planet and install upgrades.

# UI
- **`[Loading Plot]` (Panel)**: Canvas child panel containing `Plot` (TextMeshProUGUI) and `Press Any key` (TextMeshProUGUI).
- **`[Cockpit]` (Panel)**: Main preparation workspace panel containing planet selection and upgrade controls.

# Key Asset & Context
- `Assets/Scenes/Preparation.unity`: Scene where the `[Loading Plot]` and `[Cockpit]` panels exist as children of the `[Canvas]`.
- `Assets/Scripts/LoadingPlotController.cs`: New controller handling the typing effect, breathing text glow, input monitoring, and panel transition.

---

# Implementation Steps

### Step 1: Create LoadingPlotController.cs
- **Description**: Implement `LoadingPlotController.cs` under `Assets/Scripts/`.
  - Expose serialized fields:
    - `loadingPlotPanel`: GameObject reference for the loading plot panel.
    - `cockpitPanel`: GameObject reference for the cockpit panel.
    - `plotTextComp`: `TextMeshProUGUI` for the story text.
    - `pressAnyKeyText`: `TextMeshProUGUI` for the breathing prompt.
    - `wordsPerMinute` / `typingSpeed`: Typing delay/speed configurations.
    - `revealWordByWord`: Boolean option to toggle between word-by-word reveal vs. character-by-character teletype.
    - `twinkleSpeed` and `minAlpha`/`maxAlpha` for the breathing prompt.
  - In `Awake()`, ensure `cockpitPanel` is set inactive and `loadingPlotPanel` is active.
  - In `Start()`, cache the story text, clear the `plotTextComp`, and start the story typing coroutine and the prompt fade coroutine.
  - In `Update()`, monitor for keyboard anyKey or pointer press from the new Input System. When pressed, deactivate the plot panel, activate the cockpit panel, and disable this controller.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Configure the Scene Setup in Preparation.unity
- **Description**: Write and run a command script to:
  - Open `Preparation` scene.
  - Create a new empty GameObject `_LoadingPlotController` (or attach it to `[Loading Plot]` directly).
  - Bind the serialized panel and text component references.
  - Verify that `[Loading Plot]` and `[Cockpit]` are both linked correctly.
  - Save the scene.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Teletype Typing Effect Verification**:
   - Play the `Preparation` scene.
   - Verify that the story starts typing automatically either character-by-character or word-by-word with clean spacing.
2. **Breathing Light Prompt Verification**:
   - Verify that the `"Press Any Key"` text continuously and smoothly fades in and out (alpha transitions) like a glowing/pulsing light.
3. **Transition Verification**:
   - Press any keyboard key or click anywhere on the screen with the mouse.
   - Verify that the `[Loading Plot]` panel instantly disappears, and the `[Cockpit]` selection panel becomes fully visible and active with normal game flow.
