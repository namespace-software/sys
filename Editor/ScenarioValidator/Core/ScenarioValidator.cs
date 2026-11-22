using UnityEngine.SceneManagement;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class ScenarioValidator
    {
        private readonly SceneScanner _scanner = new SceneScanner();
        private readonly SceneValidator _sceneValidator = new SceneValidator();
        private readonly ConditionValidator _conditionValidator = new ConditionValidator();

        public ValidationReport Validate(Scene scene)
        {
            ValidationReport report = new ValidationReport();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                report.Add(new ValidationResult("SCENE000", "Нет активной загруженной сцены для проверки.", ValidationSeverity.Error, null));
                return report;
            }

            ValidationContext context = _scanner.Scan(scene, report);
            _sceneValidator.Validate(context);
            _conditionValidator.Validate(context);

            if (report.Results.Count == 0)
                report.Add(new ValidationResult("SCENE000", "Базовые проверки сцены, групп, шагов и условий пройдены.", ValidationSeverity.Info, context.SceneTarget));

            return report;
        }
    }
}