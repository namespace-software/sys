using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class ScenarioValidatorWindow : EditorWindow
    {
        private readonly ScenarioValidator _validator = new ScenarioValidator();
        private ValidationReport _report;
        private string _validatedScenePath;
        private Vector2 _scroll;
        private bool _showErrors = true;
        private bool _showWarnings = true;
        private bool _showInfo = true;

        [MenuItem("Tools/Scenario Validator")]
        public static void Open()
        {
            ScenarioValidatorWindow window = GetWindow<ScenarioValidatorWindow>();
            window.titleContent = new GUIContent("Scenario Validator");
            window.minSize = new Vector2(560, 340);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scenario Validator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Проверяется только текущая активная сцена, включая неактивные GameObject. " +
                "Валидатор не запускает сценарий, не вызывает Runtime-методы условий и не изменяет сцену.",
                MessageType.Info);

            Scene activeScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("Активная сцена", activeScene.IsValid() ? activeScene.name : "Нет активной сцены");
            using (new EditorGUI.DisabledScope(!activeScene.IsValid() || !activeScene.isLoaded))
            {
                if (GUILayout.Button("Validate Scene", GUILayout.Height(30)))
                {
                    _report = _validator.Validate(activeScene);
                    _validatedScenePath = activeScene.path;
                }
            }

            if (_report == null) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Результат", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Проверена сцена", string.IsNullOrEmpty(_validatedScenePath) ? "(без сохранённого пути)" : _validatedScenePath);

            EditorGUILayout.BeginHorizontal();
            _showErrors = GUILayout.Toggle(_showErrors, "Errors (" + _report.ErrorCount + ")", "Button");
            _showWarnings = GUILayout.Toggle(_showWarnings, "Warnings (" + _report.WarningCount + ")", "Button");
            _showInfo = GUILayout.Toggle(_showInfo, "Info (" + _report.InfoCount + ")", "Button");
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            bool anyVisible = false;
            foreach (ValidationResult result in _report.Results)
            {
                if (!IsVisible(result.Severity)) continue;
                anyVisible = true;
                DrawResult(result);
            }

            if (!anyVisible)
                EditorGUILayout.HelpBox("Нет результатов для выбранных фильтров.", MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private bool IsVisible(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error: return _showErrors;
                case ValidationSeverity.Warning: return _showWarnings;
                default: return _showInfo;
            }
        }

        private static void DrawResult(ValidationResult result)
        {
            MessageType messageType = result.Severity == ValidationSeverity.Error
                ? MessageType.Error
                : result.Severity == ValidationSeverity.Warning ? MessageType.Warning : MessageType.Info;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(result.Code, EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label(result.Severity.ToString(), GUILayout.Width(70));
            if (result.Target != null)
                GUILayout.Label(result.Target.name, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(result.Message, messageType);

            if (result.Target != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select", GUILayout.Width(80)))
                {
                    Selection.activeObject = result.Target;
                    EditorGUIUtility.PingObject(result.Target);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}