using UnityEditor;
using UnityEditor.SceneManagement;
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
            window.minSize = new Vector2(520, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scenario Validator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Проверяется только текущая активная сцена, включая неактивные GameObject. " +
                "Проверка не запускает сценарий и не изменяет объекты сцены.",
                MessageType.Info);

            Scene activeScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("Активная сцена", activeScene.IsValid() ? activeScene.name : "Нет активной сцены");
            using (new EditorGUI.DisabledScope(!activeScene.IsValid() || !activeScene.isLoaded))
            {
                if (GUILayout.Button("Validate Scene", GUILayout.Height(30)))
                {
                    _report = _validator.Validate(SceneManager.GetActiveScene());
                    _validatedScenePath = SceneManager.GetActiveScene().path;
                }
            }

            if (_report == null)
                return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Отчёт", EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(_validatedScenePath))
                EditorGUILayout.LabelField("Проверена", _validatedScenePath);

            EditorGUILayout.BeginHorizontal();
            _showErrors = GUILayout.Toggle(_showErrors, "Errors (" + _report.ErrorCount + ")", "Button");
            _showWarnings = GUILayout.Toggle(_showWarnings, "Warnings (" + _report.WarningCount + ")", "Button");
            _showInfo = GUILayout.Toggle(_showInfo, "Info (" + _report.InfoCount + ")", "Button");
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            bool anyVisible = false;
            for (int i = 0; i < _report.Results.Count; i++)
            {
                ValidationResult result = _report.Results[i];
                if (!IsVisible(result.Severity))
                    continue;

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
                : result.Severity == ValidationSeverity.Warning
                    ? MessageType.Warning
                    : MessageType.Info;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(result.Code, EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label(result.Severity.ToString(), GUILayout.Width(65));
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