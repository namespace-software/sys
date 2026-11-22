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
- Known condition-specific checks for ButtonCondition, AwaitCondition, GrabCondition, TriggerCondition, PositionCondition, EventCondition, ButtonPressCondition, WaitCondition and DistanceInspectCondition.

## Severity

- **Error**: likely null reference, invalid reference, or a configuration that cannot work as intended.
- **Warning**: suspicious configuration that may be intentional.
- **Info**: non-blocking suggestion.

Some checks deliberately remain warnings because the runtime system permits project-specific usage. The validator is static and cannot prove that external providers, UnityEvent listeners, physics interactions, or runtime state transitions will behave correctly.

## Adding rules

Add scene/group/step rules to `Core/SceneValidator.cs`. Add shared checks and dependency-graph rules to `Core/ConditionValidator.cs`. Specific condition checks belong in `Core/ConditionValidators/` and implement `IConditionValidator`. Register the validator in `ConditionValidatorCatalog` using the exact condition type. The catalog also exposes `Register<TCondition>()`, `Unregister<TCondition>()` and `HasValidator<TCondition>()` for editor extensions. The default catalog is initialized with built-in validators. A type without a registered validator is skipped for type-specific checks; common checks still run. Condition objects are plain managed references; the report target is the owning Unity object so the user can select it in the Hierarchy/Inspector.


### Дополнительные типы условий и провайдеры

- `ButtonPressCondition`: назначен `HVR_InteractionHelper`, разрешён хотя бы один контроллер, корректный тип кнопки, неотрицательное время удержания.
- `WaitCondition`: задан корректный режим; для Seconds — неотрицательное время, для Audio — AudioClip, для Animation — Animator, имя состояния и RuntimeAnimatorController.
- `DistanceInspectCondition`: назначены `HVR_InteractionHelper` и `DistanceInspectionIndicator`, время осмотра больше нуля.
- `SocketFilledProvider` (компонент-провайдер, не `ConditionBase`): назначен `HVRSocket`.
