# Scenario Presets (MVP)

Scenario Presets capture an already configured scene hierarchy and let you reuse it.

## Create a preset

1. In the Hierarchy, select exactly one root GameObject. All descendants are included.
2. Choose GameObject > Scenario > Save Selection as Preset.
3. Choose the .asset location and save.
4. The tool creates a *_Prefab.prefab next to the preset asset and a ScenarioPreset ScriptableObject.

The source scene hierarchy is not modified. A temporary copy is used to create the prefab.

## Use a preset

Open Tools > Scenario Presets > Library, then click Add to Scene.

Internal references between objects and components inside the captured hierarchy are retained by the prefab. References to scene objects outside the captured hierarchy are stored as bindings and restored when the preset is instantiated.

External references are resolved using GlobalObjectId first, then a scene hierarchy path fallback. If the referenced scene object was moved, deleted, or the scene changed, the reference remains unset and a warning is written to the Console.

## MVP limitations

- Capture currently supports one selected root GameObject and its complete child hierarchy.
- External references to scene GameObjects and Components are captured. References to project assets remain normal prefab references.
- References stored only in runtime state or unsupported custom serialization may not be discoverable by SerializedObject.
- External references are restored only when the target can be found in the active scene.
- The library is editor-only; no runtime dependency is added.
