namespace _Core.Editor.ScenarioValidator
{
    /// <summary>
    /// Проверяет минимальные требования, предъявляемые Scenario.Awake().
    /// На этом этапе правила намеренно ограничены составом сцены.
    /// </summary>
    public sealed class SceneValidator
    {
        public void Validate(ValidationContext context)
        {
            ValidateRequired(context, "SCENE001", "Scenario", context.Scenarios.Count);
            ValidateRequired(context, "SCENE002", "ScenarioAudioMediator", context.AudioMediators.Count);
            ValidateRequired(context, "SCENE003", "ScenarioSystem", context.ScenarioSystems.Count);
            ValidateRequired(context, "SCENE004", "HintsSystem", context.HintsSystems.Count);
            ValidateRequired(context, "SCENE005", "MistakeSystemBase", context.MistakeSystems.Count);
            ValidateRequired(context, "SCENE006", "LogSystem", context.LogSystems.Count);
            ValidateRequired(context, "SCENE007", "TrackedManager", context.TrackedManagers.Count);
        }

        private static void ValidateRequired(ValidationContext context, string code, string componentName, int count)
        {
            if (count == 0)
            {
                context.Add(code, "В активной сцене отсутствует обязательный компонент " + componentName + ".",
                    ValidationSeverity.Error);
            }
            else if (count > 1)
            {
                context.Add(code, "В активной сцене найдено несколько компонентов " + componentName +
                                  " (" + count + "). Runtime-код использует FindFirstObjectByType, поэтому выбор экземпляра неоднозначен.",
                    ValidationSeverity.Warning);
            }
        }
    }
}