# UI Asset Report

Open **Tools > UnityTools > UI > Asset Report**, select an Assets/Packages prefab folder, then Scan.
Filter by Sprite, source Texture or SpriteAtlas to locate prefab users. Missing-only shows images with no effective sprite or no atlas packable membership.

Inactive images and serialized sprites are included. Runtime-only overrideSprite changes are not tracked. Nested prefabs are reported in each owning prefab. Exact sprite packables, texture packables and recursive folder packables are supported; several atlases can match one sprite. Atlas membership uses authoring settings, without forcing packing or modifying assets. Results are a snapshot: rescan after edits.

This is an asset-reference report, not draw-call, batch-break, runtime visibility or build inclusion measurement. A missing sprite can be intentional. No scenes, prefabs or config assets are created/changed by scanning.
