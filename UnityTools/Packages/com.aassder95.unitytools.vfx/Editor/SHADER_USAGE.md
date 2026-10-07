# Shader Usage

Open **Tools > UnityTools > VFX > Shader Usage**. Enter a material folder and optional semicolon-separated exclusion folders. Scan groups materials by Shader; use the Shader object field to filter a group and click material object fields to locate assets.

Subasset materials are included. Exclusion folders match directory boundaries (excluding `Assets/Test` does not exclude `Assets/TestMore`). Invalid folders reject the entire scan. Scanning does not generate ShaderProfile assets or modify any materials. Results include unused materials and do not measure scene use, variants, stripping or build inclusion. Rescan after asset edits.
