# Project Overview
- Game Title: Mr. Blast Off
- High-Level Concept: Strategic 3D spherical exploration and resource harvesting game.
- Players: Single player
- Inspiration / Reference Games: Outer Wilds, Super Mario Galaxy
- Tone / Art Direction: Sci-fi / cartoonish / low-poly / planetary
- Target Platform: Standalone Windows
- Screen Orientation / Resolution: Landscape 1920x1080
- Render Pipeline: URP (Custom PC_RPAsset)

# Game Mechanics
## Core Gameplay Loop
Players control a character (Mr. Blast) running around small spherical planets, collecting resources, and boarding a space shuttle to travel between planets.

## Controls and Input Methods
Standard mouse and keyboard or controllers, with spherical gravity physics.

# UI
None modified in this fix.

# Key Asset & Context
- **Script**: `Assets/Scripts/ShuttleController.cs`
- **Line 273**:
  `var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);`
- **Issue**: Under modern Unity (e.g., Unity 6000.3.10f1), `FindObjectsByType` has overloads:
  1. `FindObjectsByType<T>(FindObjectsSortMode sortMode)`
  2. `FindObjectsByType<T>(FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode)`
  Passing only `FindObjectsInactive.Include` attempts to resolve to the single-argument overload, resulting in `CS1503: Argument 1: cannot convert from 'UnityEngine.FindObjectsInactive' to 'UnityEngine.FindObjectsSortMode'`.

# Implementation Steps
## Step 1: Fix FindObjectsByType Call in ShuttleController.cs
- **Description**: Edit line 273 in `Assets/Scripts/ShuttleController.cs` to pass `FindObjectsSortMode.None` as the second argument:
  `var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);`
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

# Verification & Testing
- Call `Unity.GetConsoleLogs` after file compilation to verify that all compiler errors are resolved.
- Launch play mode or inspect logs to ensure that `FindNearestPlanet` works as expected.
