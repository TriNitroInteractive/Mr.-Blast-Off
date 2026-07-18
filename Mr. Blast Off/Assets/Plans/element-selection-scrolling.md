# Project Overview
- **Game Title:** Mr. Blast Off
- **High-Level Concept:** A 2D/3D hybrid sci-fi engineering and strategy game with an initial "Element Selection" phase.

# Game Mechanics
## Confined Scrollable Element Selection Panel
To keep the 30 dynamically generated element slots fully confined inside the design-time `400 x 250` dimensions of the `Element Selection` panel while maintaining their original `50 x 50` template size, we will implement a vertical Scroll View programmatically.
- **Scroll View container**: Dynamically created scroll viewport (`ScrollRect`) centered within the panel.
- **Scroll Masking**: A `RectMask2D` viewport that clips any overflow, keeping all 30 slots fully confined.
- **Vertical Scrolling**: Enables smooth touch/mouse drag navigation to view the entire list of 30 items.
- **Layout Adjustments**: Adjusts the positions of the status text and Start button to fit perfectly inside the `400 x 250` boundary without overflow:
  - Scroll View: anchored at `(0, 20)`, size `(360, 140)`.
  - Status Text: anchored at `(0, -65)`, size `(360, 25)`.
  - Start Button: anchored at `(0, -100)`, size `(160, 30)`.

# Key Asset & Context
### Modified Assets
- `Assets/Scripts/ElementSelectionManager.cs`: Update the programmatic layout code to wrap the `GridLayoutGroup` in a `ScrollRect` viewport and adjust item sizes/anchors to fit the 400x250 bounds.

# Implementation Steps

## Step 1: Update ElementSelectionManager.cs with Scroll View support
- **Description:** Update the `CreateGridContainer()` and layout methods to create a `ScrollRect` container, a `RectMask2D` viewport, and set up the `Content` grid with a `ContentSizeFitter`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

# Verification & Testing
- **Visual Overflow Test:** Enter Play mode. Verify that the `Element Selection` panel does not resize, and the slots are neatly masked/contained inside the panel.
- **Scrolling Test:** Scroll the grid vertically using the mouse drag. Verify all 30 elements are accessible and maintain their original `50 x 50` size.
- **Start Button Alignment Test:** Click "Start" with exactly 10 elements and confirm everything unpauses successfully.
