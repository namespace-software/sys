#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScenarioTools.Presets
{
    public static class ScenarioPresetCapture
    {
        public static bool CanCaptureSelection()
        {
            return Selection.gameObjects != null
                   && Selection.gameObjects.Length == 1
                   && Selection.activeGameObject != null
                   && Selection.activeGameObject.scene.IsValid()
                   && !EditorUtility.IsPersistent(Selection.activeGameObject);
        }

        public static void CaptureSelection()
        {
            if (!CanCaptureSelection())
            {
                EditorUtility.DisplayDialog("Scenario Preset",
                    "Select exactly one scene GameObject. Its complete child hierarchy will be captured.", "OK");
                return;
            }

            GameObject source = Selection.activeGameObject;
            string assetPath = EditorUtility.SaveFilePanelInProject(
                "Save Scenario Preset", MakeSafeFileName(source.name), "asset",
                "Choose where to save the Scenario Preset.");
            if (string.IsNullOrEmpty(assetPath))
                return;

            string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directory))
                directory = "Assets";
            EnsureFolder(directory);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(directory, Path.GetFileNameWithoutExtension(assetPath) + "_Prefab.prefab").Replace('\\', '/'));

            GameObject temporaryCopy = null;
            try
            {
                List<ScenarioPresetReference> externalReferences = CaptureExternalReferences(source);
                temporaryCopy = UnityEngine.Object.Instantiate(source);
                temporaryCopy.name = source.name;
                temporaryCopy.hideFlags = HideFlags.HideAndDontSave;

                ClearExternalSceneReferences(temporaryCopy, externalReferences);
                temporaryCopy.hideFlags = HideFlags.None;
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temporaryCopy, prefabPath);
                if (prefab == null)
                    throw new InvalidOperationException("Unity failed to save the hierarchy as a prefab.");

                ScenarioPreset preset = ScriptableObject.CreateInstance<ScenarioPreset>();
                preset.Initialize(source.name, string.Empty, prefab, externalReferences);
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(preset);
                Selection.activeObject = preset;
                Debug.Log($"[Scenario Presets] Saved '{source.name}' to {assetPath}. External references: {externalReferences.Count}.", preset);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Scenario Preset", "Preset could not be saved. See the Console for details.", "OK");
                if (AssetDatabase.LoadMainAssetAtPath(prefabPath) != null)
                    AssetDatabase.DeleteAsset(prefabPath);
                if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
                    AssetDatabase.DeleteAsset(assetPath);
            }
            finally
            {
                if (temporaryCopy != null)
                    UnityEngine.Object.DestroyImmediate(temporaryCopy);
            }
        }

        private static List<ScenarioPresetReference> CaptureExternalReferences(GameObject root)
        {
            var result = new List<ScenarioPresetReference>();
            foreach (Component owner in root.GetComponentsInChildren<Component>(true))
            {
                if (owner == null)
                    continue;
                var serializedObject = new SerializedObject(owner);
                SerializedProperty iterator = serializedObject.GetIterator();
                bool enterChildren = true;
                while (iterator.Next(enterChildren))
                {
                    enterChildren = true;
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                        continue;
                    UnityEngine.Object target = iterator.objectReferenceValue;
                    if (target == null || EditorUtility.IsPersistent(target))
                        continue;
                    GameObject targetGameObject = GetGameObject(target);
                    if (targetGameObject == null || IsInsideHierarchy(targetGameObject.transform, root.transform))
                        continue;
                    result.Add(CreateReference(root.transform, owner, iterator.propertyPath, target, targetGameObject));
                }
            }
            return result;
        }

        private static ScenarioPresetReference CreateReference(Transform root, Component owner,
            string propertyPath, UnityEngine.Object target, GameObject targetGameObject)
        {
            string ownerPath = BuildIndexPath(owner.transform, root);
            Component targetComponent = target as Component;
            bool targetIsGameObject = target is GameObject;
            string targetType = targetComponent != null ? targetComponent.GetType().AssemblyQualifiedName : string.Empty;
            int targetIndex = targetComponent != null ? GetComponentIndex(targetComponent) : -1;
            Transform sceneRoot = GetSceneRoot(targetGameObject.transform);
            string targetPath = BuildScenePath(targetGameObject.transform, sceneRoot);
            string globalId = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();

            return new ScenarioPresetReference(ownerPath, owner.GetType().AssemblyQualifiedName,
                GetComponentIndex(owner), propertyPath, globalId, targetGameObject.scene.path,
                targetPath, targetType, targetIndex, targetIsGameObject);
        }

        private static void ClearExternalSceneReferences(GameObject clone, List<ScenarioPresetReference> references)
        {
            foreach (ScenarioPresetReference reference in references)
            {
                Component owner = FindComponent(clone.transform, reference.OwnerPath,
                    reference.OwnerComponentType, reference.OwnerComponentIndex);
                if (owner == null)
                    continue;
                var serializedObject = new SerializedObject(owner);
                SerializedProperty property = serializedObject.FindProperty(reference.PropertyPath);
                if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;
                property.objectReferenceValue = null;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        internal static Component FindComponent(Transform root, string path, string typeName, int componentIndex)
        {
            Transform target = ResolveIndexPath(root, path);
            Type type = Type.GetType(typeName);
            if (target == null || type == null || !typeof(Component).IsAssignableFrom(type))
                return null;
            Component[] components = target.GetComponents(type);
            return componentIndex >= 0 && componentIndex < components.Length ? components[componentIndex] : null;
        }



        internal static string BuildScenePath(Transform target, Transform sceneRoot)
        {
            return sceneRoot.GetSiblingIndex() + "/" + BuildIndexPath(target, sceneRoot);
        }

        internal static string BuildIndexPath(Transform target, Transform root)
        {
            var indices = new System.Collections.Generic.Stack<int>();
            Transform current = target;
            while (current != null && current != root)
            {
                indices.Push(current.GetSiblingIndex());
                current = current.parent;
            }
            if (current != root)
                return string.Empty;
            return string.Join("/", indices);
        }

        internal static Transform ResolveIndexPath(Transform root, string path)
        {
            if (root == null)
                return null;
            if (string.IsNullOrEmpty(path))
                return root;
            Transform current = root;
            string[] segments = path.Split('/');
            foreach (string segment in segments)
            {
                if (!int.TryParse(segment, out int index) || index < 0 || index >= current.childCount)
                    return null;
                current = current.GetChild(index);
            }
            return current;
        }

        private static int GetComponentIndex(Component component)
        {
            Component[] components = component.gameObject.GetComponents(component.GetType());
            for (int i = 0; i < components.Length; i++)
                if (components[i] == component)
                    return i;
            return -1;
        }

        private static GameObject GetGameObject(UnityEngine.Object target)
        {
            if (target is GameObject gameObject) return gameObject;
            if (target is Component component) return component.gameObject;
            return null;
        }

        private static bool IsInsideHierarchy(Transform candidate, Transform root)
        {
            return candidate == root || candidate.IsChildOf(root);
        }

        private static Transform GetSceneRoot(Transform target)
        {
            Transform current = target;
            while (current.parent != null) current = current.parent;
            return current;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string name = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent)) parent = "Assets";
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "ScenarioPreset" : value;
        }
    }
}
#endif
