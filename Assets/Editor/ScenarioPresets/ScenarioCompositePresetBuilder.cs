#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScenarioTools.Presets
{
    public static class ScenarioCompositePresetBuilder
    {
        [MenuItem("Assets/Create/Scenario Presets/Composite", false, 220)]
        private static void CreateComposite()
        {
            ScenarioPreset[] selected = Selection.objects
                .OfType<ScenarioPreset>()
                .Distinct()
                .ToArray();

            if (selected.Length < 2)
            {
                EditorUtility.DisplayDialog("Composite Scenario Preset",
                    "Select at least two ScenarioPreset assets in the Project window.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Create Composite Scenario Preset", "CompositeScenarioPreset", "asset",
                "Choose where to save the composite preset.");
            if (string.IsNullOrEmpty(path))
                return;

            var parts = new List<ScenarioPresetPart>();
            foreach (ScenarioPreset preset in selected)
                parts.Add(new ScenarioPresetPart(preset, Vector3.zero, Vector3.zero, Vector3.one));

            ScenarioPreset composite = ScriptableObject.CreateInstance<ScenarioPreset>();
            composite.InitializeComposite(Path.GetFileNameWithoutExtension(path), string.Empty, parts);
            AssetDatabase.CreateAsset(composite, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(composite);
            Selection.activeObject = composite;
            Debug.Log($"[Scenario Presets] Created composite '{composite.DisplayName}' with {parts.Count} parts.", composite);
        }

        [MenuItem("Assets/Create/Scenario Presets/Composite", true)]
        private static bool ValidateCreateComposite()
        {
            return Selection.objects.OfType<ScenarioPreset>().Distinct().Count() >= 2;
        }
    }
}
#endif
