#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScenarioTools.Presets
{
    public static class ScenarioPresetInstantiator
    {
        public static GameObject Instantiate(ScenarioPreset preset, Scene targetScene = default)
        {
            if (preset == null || preset.Prefab == null)
            {
                Debug.LogError("[Scenario Presets] Preset or its prefab is missing.", preset);
                return null;
            }

            GameObject instance = targetScene.IsValid()
                ? PrefabUtility.InstantiatePrefab(preset.Prefab, targetScene) as GameObject
                : PrefabUtility.InstantiatePrefab(preset.Prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Scenario Presets] Failed to instantiate '{preset.DisplayName}'.", preset);
                return null;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Add Scenario Preset");
            int restored = 0;
            int unresolved = 0;
            foreach (ScenarioPresetReference reference in preset.ExternalReferences)
            {
                if (TryRestoreReference(instance, reference)) restored++;
                else unresolved++;
            }

            EditorSceneManager.MarkSceneDirty(instance.scene);
            Selection.activeGameObject = instance;
            if (unresolved == 0)
                Debug.Log($"[Scenario Presets] Added '{preset.DisplayName}'. Restored references: {restored}.", instance);
            else
                Debug.LogWarning($"[Scenario Presets] Added '{preset.DisplayName}', but {unresolved} external reference(s) could not be resolved.", instance);
            return instance;
        }

        private static bool TryRestoreReference(GameObject instance, ScenarioPresetReference reference)
        {
            Component owner = ScenarioPresetCapture.FindComponent(instance.transform, reference.OwnerPath,
                reference.OwnerComponentType, reference.OwnerComponentIndex);
            if (owner == null) return false;
            UnityEngine.Object target = ResolveTarget(reference, instance.scene);
            if (target == null) return false;

            var serializedObject = new SerializedObject(owner);
            SerializedProperty property = serializedObject.FindProperty(reference.PropertyPath);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                return false;
            property.objectReferenceValue = target;
            serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static UnityEngine.Object ResolveTarget(ScenarioPresetReference reference, Scene targetScene)
        {
            if (!string.IsNullOrEmpty(reference.TargetGlobalObjectId) &&
                GlobalObjectId.TryParse(reference.TargetGlobalObjectId, out GlobalObjectId globalId))
            {
                UnityEngine.Object target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
                if (IsCompatibleTarget(target, reference) && IsInExpectedScene(target, reference)) return target;
            }

            Scene scene = targetScene.IsValid() ? targetScene : SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(reference.TargetScenePath) && scene.path != reference.TargetScenePath)
                return null;
            GameObject targetObject = FindGameObjectByPath(scene, reference.TargetHierarchyPath);
            if (targetObject == null) return null;
            if (reference.TargetIsGameObject) return targetObject;

            Type type = Type.GetType(reference.TargetComponentType);
            if (type == null || !typeof(Component).IsAssignableFrom(type)) return null;
            Component[] components = targetObject.GetComponents(type);
            int index = reference.TargetComponentIndex;
            return index >= 0 && index < components.Length ? components[index] : null;
        }

        private static bool IsInExpectedScene(UnityEngine.Object target, ScenarioPresetReference reference)
        {
            GameObject go = target is GameObject gameObject ? gameObject : (target as Component)?.gameObject;
            return go != null && (string.IsNullOrEmpty(reference.TargetScenePath) || go.scene.path == reference.TargetScenePath);
        }

        private static bool IsCompatibleTarget(UnityEngine.Object target, ScenarioPresetReference reference)
        {
            if (target == null) return false;
            if (reference.TargetIsGameObject) return target is GameObject;
            Type expectedType = Type.GetType(reference.TargetComponentType);
            return expectedType != null && expectedType.IsInstanceOfType(target);
        }

        private static GameObject FindGameObjectByPath(Scene scene, string path)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;
            string[] segments = path.Split('/');
            if (segments.Length == 0 || !int.TryParse(segments[0], out int rootIndex))
                return null;
            GameObject[] roots = scene.GetRootGameObjects();
            if (rootIndex < 0 || rootIndex >= roots.Length)
                return null;
            string childPath = string.Join("/", segments, 1, segments.Length - 1);
            Transform found = ScenarioPresetCapture.ResolveIndexPath(roots[rootIndex].transform, childPath);
            return found != null ? found.gameObject : null;
        }
    }
}
#endif
