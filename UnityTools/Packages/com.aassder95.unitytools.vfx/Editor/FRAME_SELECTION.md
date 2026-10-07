# Representative frame selection

Select a prefab in the browser and click **Select representative frame**. The Replay duration defines the search interval (maximum 30 seconds). 36 equally spaced samples are rendered at 64x64. Foreground contrast, coverage and center weighting determine the winning frame. A first pass unions renderer bounds for a common camera, avoiding per-frame auto-fit bias.

The preview pauses at the selected time and the browser's global thumbnail time is updated; this refreshes the thumbnail/color index for all displayed prefabs, as with the existing Frame slider. This is not a per-prefab persisted time. Selection uses a separate visual-only preview, never executes prefab scripts, and disposes all temporary textures/scenes. Empty/uniform frames return failure. Sampling can miss very brief effects, and edge-filling effects may be hard to distinguish from background; manual Time/Frame controls remain available.
