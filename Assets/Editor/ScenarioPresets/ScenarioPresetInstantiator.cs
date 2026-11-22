#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScenarioTools.Presets
{
    public static class ScenarioPresetInstantiator
    {
        public static GameObject Instantiate(ScenarioPreset preset, Scene targetScene = default, Transform parent = null)
        {
            if (preset == null)
            {
                Debug.LogError("[Scenario Presets] Preset is missing.");
                return null;
            }

            Scene scene = parent != null ? parent.gameObject.scene : targetScene;
            if (!scene.IsValid())
                scene = SceneManager.GetActiveScene();

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add Scenario Preset");

            try
            {
                GameObject result = InstantiateRecursive(preset, scene, parent, new HashSet<ScenarioPreset>());
                if (result == null)
                    return null;

                Undo.CollapseUndoOperations(undoGroup);
                EditorSceneManager.MarkSceneDirty(result.scene);
                return result;
            }
            catch (Exception exception)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                Debug.LogException(exception);
                Debug.LogError($"[Scenario Presets] Could not instantiate '{preset.DisplayName}'.", preset);
                return null;
            }
        }

        private static GameObject InstantiateRecursive(ScenarioPreset preset, Scene scene, Transform parent,
            HashSet<ScenarioPreset> recursionStack)
        {
            if (preset == null)
                throw new InvalidOperationException("A composite preset contains an empty child reference.");

            if (!recursionStack.Add(preset))
                throw new InvalidOperationException($"Circular composite preset dependency detected at '{preset.DisplayName}'.");

            try
            {
                if (!preset.IsComposite)
                    return InstantiateLeaf(preset, scene, parent);

                GameObject container = new GameObject(preset.DisplayName);
                Undo.RegisterCreatedObjectUndo(container, "Add Composite Scenario Preset");
                if (parent != null)
                    Undo.SetTransformParent(container.transform, parent, "Parent Composite Scenario Preset");
                else if (scene.IsValid() && container.scene != scene)
                    SceneManager.MoveGameObjectToScene(container, scene);

                foreach (ScenarioPresetPart part in preset.Parts)
                {
                    if (part == null || part.Preset == null)
                        throw new InvalidOperationException($"Composite '{preset.DisplayName}' has an empty part.");

                    GameObject child = InstantiateRecursive(part.Preset, container.scene, container.transform, recursionStack);
                    if (child == null)
                        throw new InvalidOperationException($"Failed to instantiate part '{part.Preset.DisplayName}'.");

                    Undo.RecordObject(child.transform, "Set Composite Preset Part Transform");
                    child.transform.localPosition = part.LocalPosition;
                    child.transform.localRotation = Quaternion.Euler(part.LocalEulerAngles);
                    child.transform.localScale = part.LocalScale;
                }

                return container;
            }
            finally
            {
                recursionStack.Remove(preset);
            }
        }

        private static GameObject InstantiateLeaf(ScenarioPreset preset, Scene scene, Transform parent)
        {
            if (preset.Prefab == null)
                throw new InvalidOperationException($"Preset '{preset.DisplayName}' has no prefab.");

            GameObject instance = parent != null
                ? PrefabUtility.InstantiatePrefab(preset.Prefab, parent) as GameObject
                : scene.IsValid()
                    ? PrefabUtility.InstantiatePrefab(preset.Prefab, scene) as GameObject
                    : PrefabUtility.InstantiatePrefab(preset.Prefab) as GameObject;

            if (instance == null)
                throw new InvalidOperationException($"Unity failed to instantiate '{preset.DisplayName}'.");

            Undo.RegisterCreatedObjectUndo(instance, "Add Scenario Preset");
            int restored = 0;
            int unresolved = 0;
            foreach (ScenarioPresetReference reference in preset.ExternalReferences)
            {
                if (TryRestoreReference(instance, reference)) restored++;
                else unresolved++;
            }

            if (unresolved > 0)
                Debug.LogWarning($"[Scenario Presets] Added '{preset.DisplayName}', but {unresolved} external reference(s) could not be resolved.", instance);
            else
                Debug.Log($"[Scenario Presets] Added '{preset.DisplayName}'. Restored references: {restored}.", instance);

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
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(path)) return null;
            string[] segments = path.Split('/');
            if (segments.Length == 0 || !int.TryParse(segments[0], out int rootIndex)) return null;
            GameObject[] roots = scene.GetRootGameObjects();
            if (rootIndex < 0 || rootIndex >= roots.Length) return null;
            string childPath = string.Join("/", segments, 1, segments.Length - 1);
            Transform found = ScenarioPresetCapture.ResolveIndexPath(roots[rootIndex].transform, childPath);
            return found != null ? found.gameObject : null;
        }
    }
}
#endif
