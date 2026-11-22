#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScenarioTools.Presets
{
    public sealed class ScenarioPresetLibraryWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _search = string.Empty;

        [MenuItem("Tools/Scenario Presets/Library", false, 30)]
        public static void Open()
        {
            GetWindow<ScenarioPresetLibraryWindow>("Scenario Presets");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Scenario Preset Library", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh", GUILayout.Width(70))) Repaint();
            }

            EditorGUILayout.HelpBox(
                "Select one configured root GameObject in the Hierarchy and use " +
                "GameObject > Scenario > Save Selection as Preset to create a preset.",
                MessageType.Info);
            _search = EditorGUILayout.TextField("Search", _search);
            EditorGUILayout.Space(4);

            string[] guids = AssetDatabase.FindAssets("t:ScenarioPreset");
            ScenarioPreset[] presets = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<ScenarioPreset>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(preset => preset != null)
                .Where(preset => string.IsNullOrEmpty(_search) ||
                    preset.DisplayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(preset => preset.DisplayName)
                .ToArray();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (ScenarioPreset preset in presets) DrawPreset(preset);
            EditorGUILayout.EndScrollView();
            if (presets.Length == 0)
                EditorGUILayout.HelpBox("No Scenario Presets found. Create one from a Hierarchy selection.", MessageType.None);
        }

        private void DrawPreset(ScenarioPreset preset)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(preset, typeof(ScenarioPreset), false,
                        GUILayout.Width(70), GUILayout.Height(64));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(preset.DisplayName, EditorStyles.boldLabel);
                        if (!string.IsNullOrWhiteSpace(preset.Description))
                            EditorGUILayout.LabelField(preset.Description, EditorStyles.wordWrappedMiniLabel);
                        EditorGUILayout.LabelField("External references: " + preset.ExternalReferences.Count, EditorStyles.miniLabel);
                        if (preset.Prefab == null) EditorGUILayout.HelpBox("Prefab asset is missing.", MessageType.Error);
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add to Scene", GUILayout.Height(24)) && preset.Prefab != null)
                        ScenarioPresetInstantiator.Instantiate(preset);
                    if (GUILayout.Button("Select Asset", GUILayout.Width(95), GUILayout.Height(24)))
                    {
                        Selection.activeObject = preset;
                        EditorGUIUtility.PingObject(preset);
                    }
                }
            }
        }
    }
}
#endif
