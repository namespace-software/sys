using UnityEngine.SceneManagement;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>
    /// Оркестратор проверки. Не зависит от EditorWindow и не изменяет сцену.
    /// </summary>
    public sealed class ScenarioValidator
    {
        private readonly SceneScanner _scanner = new SceneScanner();
        private readonly SceneValidator _sceneValidator = new SceneValidator();

        public ValidationReport Validate(Scene scene)
        {
            ValidationReport report = new ValidationReport();

            if (!scene.IsValid() || !scene.isLoaded)
            {
                report.Add(new ValidationResult(
                    "SCENE000",
                    "Нет активной загруженной сцены для проверки.",
                    ValidationSeverity.Error,
                    null));
                return report;
            }

            ValidationContext context = _scanner.Scan(scene, report);
            _sceneValidator.Validate(context);

            if (report.Results.Count == 0)
            {
                report.Add(new ValidationResult(
                    "SCENE000",
                    "Базовая проверка состава сцены пройдена. Дополнительные проверки шагов и условий будут добавлены на следующих этапах.",
                    ValidationSeverity.Info,
                    context.SceneTarget));
            }

            return report;
        }
    }
}