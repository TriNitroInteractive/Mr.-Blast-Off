# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build a planetary destruction device to vaporize celestial hazards in a single controlled detonation ("Kickoff") as an Interstellar Orbital Cleanup Authority (IOCA) engineer.
- **Players**: Single player
- **Inspiration / Reference Games**: Spacechem, Kerbal Space Program (UI / humor), retro-sci-fi terminal interfaces.
- **Tone / Art Direction**: Slightly comedic, retro-futuristic, scientific/nuclear, vibrant neon colors (glowing plasma, hot orange/reds, electric yellow).
- **Target Platform**: PC (StandaloneWindows64)
- **Screen Orientation / Resolution**: Landscape (1920x1080 or responsive)
- **Render Pipeline**: Universal Render Pipeline (URP - Active Asset: `PC_RPAsset`)

# Game Mechanics
## Core Gameplay Loop
Players select nuclear fuel and active elements in the **Preparation Scene** to stabilize and optimize a planetary destruction reactor. In the **Kickoff Scene**, they navigate space, interact with terminal panels, and detonate the reactor, attempting to exceed the critical threshold for complete cleanup without backfiring.

## Controls and Input Methods
- **Keyboard / Mouse**: Click button inputs on screens, pilot shuttle with W/A/S/D and Up/Down arrows, walk with WASD, interact with F.
- **New Input System**: Actions mapped via `InputSystem_Actions.inputactions`.

# UI
The UI screens use uGUI with ScreenSpaceOverlay rendering.
- **MainMenu Scene**: Canvas contains a Menu Panel with a "Play" button and a "Quit" button.
- **Kickoff Scene**: Canvas contains a "Pause Button" and a "Pause Menu" panel. The "Pause Menu" panel contains a "Resume" button and a "Main Menu" button.

# Key Asset & Context
### Existing Assets & Scenes
- `Assets/Scenes/MainMenu.unity`: Needs a Main Menu manager to handle play/quit actions, link the "Play" button to a holographic fade-out effect, and transition to the `Preparation` scene.
- `Assets/Scenes/Kickoff.unity`: Contains a Canvas with Pause Button, Pause Menu Panel (which contains Resume and Main Menu buttons). Needs a manager to handle pausing, unpausing, and scene transitioning.
- `Assets/Sprites/Button Background.png`: Background sprite used for UI buttons.

### New Scripts
1. `Assets/Scripts/UIButtonJuice.cs`: Reusable component attached to buttons to provide animated hover scaling, pointer-down squish, and pointer-up spring-back. Works under unscaled time.
2. `Assets/Scripts/MainMenuController.cs`: Dynamically hooks up `Play` and `Quit` buttons. Manages the holographic fade-out transition for the `Play` button before loading `Preparation`.
3. `Assets/Scripts/PauseMenuController.cs`: Dynamically hooks up `Pause Button`, `Resume` button, and `Main Menu` button in the `Kickoff` scene. Manages pause panel visibility and time scaling.
4. `Assets/Scripts/UIHolographicEffect.cs`: Procedural C#-based holographic projection effect attached to the `Play` button. When clicked, it changes the button's visual elements to a neon holographic style (tinted cyan/teal), adds subtle scanlines/flicker glitches, and slowly dissolves/fades it out over 3-5 seconds.

---

# Implementation Steps

### Step 1: Create Reusable UI Button Juice Script
- **Description**: Implement `UIButtonJuice.cs` which inherits from `MonoBehaviour` and implements pointer event interfaces (`IPointerEnterHandler`, `IPointerExitHandler`, `IPointerDownHandler`, `IPointerUpHandler`). It will smoothly scale the button on hover (up to `1.08f`) and click compression (down to `0.92f`) using an unscaled time Coroutine so it works perfectly in pause menus.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Create Procedural C# UI Holographic Effect
- **Description**: Implement `UIHolographicEffect.cs`. When triggered, it will:
  - Transition the button's background image and text color to a sci-fi holographic tint (e.g. electric cyan or neon green).
  - Simulate a holographic projection: apply high-frequency flickering (random subtle alpha fluctuations) and rapid positional micro-jitters to mimic a sci-fi holographic transmission.
  - Implement rising horizontal scanlines (animated overlay image or overlay lines) and slow fade-out of the button elements (alpha decreases from 1.0 to 0.0) over a configurable duration (e.g., 3 seconds).
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Implement Main Menu Controller
- **Description**: Implement `MainMenuController.cs` which dynamically finds the `Play` and `Quit` button components in `MainMenu` scene, registers listeners:
  - `Quit` button calls `Application.Quit()`.
  - `Play` button disables further clicks, triggers the `UIHolographicEffect`, and after the effect completes, calls `SceneManager.LoadScene("Preparation")`.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Implement Pause Menu Controller
- **Description**: Implement `PauseMenuController.cs`. It finds the `Pause Button`, `Resume` button, `Main Menu` button, and the `Pause Menu` panel. On Awake, it registers listeners:
  - `Pause Button` -> Pauses gameplay (`Time.timeScale = 0f`), activates the `Pause Menu` Panel, hides the Pause Button.
  - `Resume` button -> Resumes gameplay (`Time.timeScale = 1f`), deactivates the `Pause Menu` Panel, shows the Pause Button.
  - `Main Menu` button -> Sets `Time.timeScale = 1f`, loads `MainMenu` scene.
  - Adds a hotkey option (e.g. `Escape` or `P` key) using the new Input System to toggle pausing/resuming.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 5: Configure the Scenes & Apply Components
- **Description**: Write an editor script or use a run command to:
  - Load `MainMenu` scene, attach `MainMenuController` to the Canvas or Panel, attach `UIButtonJuice` to `Play` and `Quit` buttons, save scene.
  - Load `Kickoff` scene, attach `PauseMenuController` to the Canvas or Panel (and link references like the Pause Menu Panel which should start disabled), attach `UIButtonJuice` to `Pause Button`, `Resume`, and `Main Menu` buttons, save scene.
- **Assigned role**: developer
- **Dependencies**: Steps 1, 3, 4
- **Parallelizable**: No

---

# Verification & Testing
1. **Juice Verification**:
   - Hover and press all buttons to verify smooth scaling transitions.
   - Verify scaling works while the game is paused in the `Kickoff` scene.
2. **Pause Menu Verification**:
   - In `Kickoff` scene, click the Pause Button. Verify `Pause Menu` opens and gameplay (Time) freezes.
   - Verify `ShuttleController` and `TerminalController` cannot be interacted with or rotated while paused.
   - Click Resume. Verify gameplay resumes and the menu closes.
   - Click Main Menu. Verify the game returns to `MainMenu` scene.
   - Press Escape / P key to verify keyboard toggle of the pause menu.
3. **Holographic Effect Verification**:
   - In `MainMenu` scene, click Play.
   - Verify button becomes unclickable and transforms into a neon cyan/teal holographic look.
   - Verify subtle flicker/glitch movements and alpha oscillations occur, simulating a sci-fi projector.
   - Verify scanlines/dissolve fades the button to 100% transparency over the transition duration.
   - Verify scene transition to `Preparation` happens smoothly after the button disappears.
