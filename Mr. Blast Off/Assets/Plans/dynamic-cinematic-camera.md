# Project Overview
- **Game Title**: Mr. Blast Off
- **High-Level Concept**: A 2D sci-fi nuclear engineering/strategy game where players build planetary destruction devices to clean up stellar hazards.
- **Tone / Art Direction**: Sci-fi retro-futurism.
- **Render Pipeline**: URP

# Game Mechanics
## Cinematic Detonation Pacing
When the reactor criticality threshold is successfully exceeded, the game triggers a dramatic planetary detonation sequence. The camera smoothly transitions from the player's current view to a `SecondaryCamera` positioned at a scenic side angle, showcasing the planetary buildup, rumble, and eventual blast. 

Previously, `SecondaryCamera` was hardcoded to focus only on `Planet M`. We will modify this to dynamically reposition the cinematic camera to whichever planet Mr. Blast has landed on and is detonating.

# UI
None affected.

# Key Asset & Context
- `Assets/Scripts/GameManager.cs`: Handles reactor evaluation, detonation triggering, and manages the `TransitionToSecondaryCamera` coroutine.

---

# Implementation Steps

### Step 1: Modify GameManager.cs Transition Coroutine Signature and Repositioning Logic
- **Description**: Update `TransitionToSecondaryCamera` to accept a `Transform targetPlanet` parameter.
  - Right at the start of the coroutine, reposition the `SecondaryCamera` GameObject:
    - Compute the scenic distance: `float targetDistance = targetPlanet.localScale.x * 2.01f;` (maintaining the exact framing ratio used for Planet M's scale of 30 and distance of 60.3f).
    - Position the camera: `secCam.transform.position = targetPlanet.position + Vector3.back * targetDistance;`
    - Align the camera rotation to point straight at the planet: `secCam.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);`
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Pass Planet Reference on Detonation
- **Description**: In `TryDetonate()`, when starting the transition coroutine, pass the planet's transform:
  - `StartCoroutine(TransitionToSecondaryCamera(blaster.transform));`
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

---

# Verification & Testing
1. **Compilation Check**:
   - Verify that the updated scripts compile with zero errors or warnings.
2. **Visual Detonation Verification (Planet M)**:
   - Land on Planet M, trigger the detonation, and verify that the cinematic camera smoothly pans to the standard scenic side angle.
3. **Visual Detonation Verification (Other Planets)**:
   - Land on a smaller planet (e.g. Planet A or Planet B with scale 10).
   - Trigger the detonation, and verify that the cinematic camera is positioned at a proportional distance (distance of ~20 units instead of 60), framing the smaller planet perfectly inside the screen during the blast buildup.
