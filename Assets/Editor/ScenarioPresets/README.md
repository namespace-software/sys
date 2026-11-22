# Scenario Presets

Scenario Presets capture configured scene hierarchies and let you reuse them. Composite presets group existing presets into reusable assemblies.

## Create a regular preset

1. In the Hierarchy, select exactly one root GameObject. All descendants are included.
2. Choose GameObject > Scenario > Save Selection as Preset.
3. Choose the .asset location and save.
4. The tool creates a *_Prefab.prefab next to the preset asset and a ScenarioPreset ScriptableObject.

The source scene hierarchy is not modified. A temporary copy is used to create the prefab.

## Create a composite preset

1. In the Project window, select two or more ScenarioPreset assets.
2. Choose Assets > Create > Scenario Presets > Composite.
3. Save the new composite .asset.
4. Open the composite asset in the Inspector and edit its Parts list. Each part references another preset and has a local position, rotation and scale.

Composite presets can contain regular presets or other composite presets. They instantiate as a container GameObject with each part instantiated as a separate prefab instance beneath it. Nested composite transforms are preserved.

The instantiator detects circular dependencies and rolls back the current Undo group on failure.

## Use a preset

Open Tools > Scenario Presets > Library, then click Add to Scene. Regular presets restore external scene references after instantiation. Composite presets instantiate each leaf preset and restore its external references in the destination scene.

Internal references between objects inside a captured hierarchy are retained by the prefab. References to scene objects outside the captured hierarchy are stored as bindings and restored when the preset is instantiated.

## Limitations

- Capture currently supports one selected root GameObject and its complete child hierarchy.
- External references to scene GameObjects and Components are captured. References to project assets remain normal prefab references.
- References stored only in runtime state or unsupported custom serialization may not be discoverable by SerializedObject.
- External references are restored only when the target can be found in the destination scene.
- Composite part transforms are relative to their composite container.
- The library is editor-only; no runtime dependency is added.
