# Scenario Validator

Open the window with **Tools → Scenario Validator** and run **Validate Scene**.

The validator scans only `SceneManager.GetActiveScene()`, including inactive GameObjects. It does not scan other loaded scenes, prefab assets, or project assets. Validation is read-only: it does not call scenario/condition initialization methods, UnityEvents, or runtime setup methods.

## MVP checks

- Required scene systems used by `Scenario.Awake()`: Scenario, ScenarioAudioMediator, ScenarioSystem, HintsSystem, MistakeSystemBase, LogSystem, TrackedManager.
- Duplicate required systems.
- GroupSteps direct-child layout (the runtime fills `Steps` from immediate children in `Start()`), nested steps, duplicate step indexes, null end-condition array.
- Step name, service type, instruction data and conditions collection.
- Managed-reference conditions in StepBase, GroupSteps and Scenario permanent conditions.
- Common condition instruction data, service/mistake enum values, dependency configuration, self/duplicate references and dependency cycles.
- Known condition-specific references for ButtonCondition, AwaitCondition, GrabCondition, TriggerCondition, PositionCondition and EventCondition.

## Severity

- **Error**: likely null reference, invalid reference, or a configuration that cannot work as intended.
- **Warning**: suspicious configuration that may be intentional.
- **Info**: non-blocking suggestion.

Some checks deliberately remain warnings because the runtime system permits project-specific usage. The validator is static and cannot prove that external providers, UnityEvent listeners, physics interactions, or runtime state transitions will behave correctly.

## Adding rules

Add scene/group/step rules to `Core/SceneValidator.cs`. Add shared and condition-specific checks to `Core/ConditionValidator.cs`. Condition objects are plain managed references; the report target is the owning Unity object so the user can select it in the Hierarchy/Inspector.
