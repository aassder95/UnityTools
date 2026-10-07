# UI Asset Catalog

Create a UI Asset Catalog via Assets > Create > UnityTools > UI > Asset Catalog. Assign keyed Prefab/Sprite/Atlas entries explicitly. Keys are ordinal and case-sensitive, unique per category. Inspector validation reports duplicates, empty keys and missing references. Inject the catalog into consumers; TryGetPrefab/TryGetSprite/TryGetAtlas never search Resources or use fallback paths. Duplicate lookup keys fail rather than choosing an arbitrary entry. Resolution is a linear scan with no cached stale Inspector state; resolve once during initialization for hot paths.
