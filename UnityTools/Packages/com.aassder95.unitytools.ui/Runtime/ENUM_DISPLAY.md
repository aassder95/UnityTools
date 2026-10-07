# Enum display names

Put `[EnumDisplayName("Localized label")]` on enum members. Put `[EnumDropdown]` on a serialized enum field to show those names in the Inspector. The drawer uses `enumValueIndex`, preserving sparse/negative numeric values and multi-object Undo/prefab editing. Arrays and List enum elements are supported. Flags enums are not supported by the drawer. Undefined values are not silently replaced until a user selects a defined value.

`EnumDisplay<T>.TryGetName(value, out name)` and `TryParseName(name, out value)` cache reflection metadata once per enum type. Unattributed members use their field name. Duplicate display names for different values cannot be parsed. Numeric aliases use the first declared member for display; all unambiguous alias names can be parsed. Undefined values/composite flag combinations return false unless explicitly declared.

Display names are for presentation, not persistent keys; keep numeric enum values and stable code names unchanged. Reflection-only enum fields must be preserved by the consuming game's linker configuration for IL2CPP/high stripping. This batch does not claim IL2CPP validation.
