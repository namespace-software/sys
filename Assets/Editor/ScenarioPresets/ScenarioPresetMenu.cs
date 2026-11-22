#if UNITY_EDITOR
using UnityEditor;

namespace ScenarioValidator.Presets
{
    public static class ScenarioPresetMenu
    {
        [MenuItem("GameObject/Scenario/Save Selection as Preset", false, 49)]
        private static void SaveSelectionAsPreset() => ScenarioPresetCapture.CaptureSelection();

        [MenuItem("GameObject/Scenario/Save Selection as Preset", true)]
        private static bool ValidateSaveSelectionAsPreset() => ScenarioPresetCapture.CanCaptureSelection();
    }
}
#endif
