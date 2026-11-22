#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using _Core._ScenarioSystem.Component.Group;

namespace Editor
{
    [InitializeOnLoad]
    public static class GroupStepsHierarchyHighlighter
    {
        private static readonly Color WhiteGroupColor = new Color(1f, 1f, 1f, 0.10f);
        private static readonly Color BlueGroupColor = new Color(0.1f, 0.45f, 1f, 0.28f);

        private static readonly Dictionary<int, Color> ObjectColors = new();

        static GroupStepsHierarchyHighlighter()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += Rebuild;
            Rebuild();
        }

        private static void Rebuild()
        {
            ObjectColors.Clear(); 

            GroupSteps[] allGroups = Object.FindObjectsByType<GroupSteps>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            HashSet<Transform> processedParents = new();

            foreach (var group in allGroups)
            {
                if (group == null) continue;
                if (!group.gameObject.scene.IsValid()) continue;

                Transform parent = group.transform.parent;
                if (parent == null) continue;

                if (processedParents.Add(parent))
                {
                    ProcessParent(parent);
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        private static void ProcessParent(Transform parent)
        {
            int colorIndex = 0;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                GroupSteps group = child.GetComponent<GroupSteps>();

                if (group == null)
                    continue;

                Color color = colorIndex % 2 == 0 ? WhiteGroupColor : BlueGroupColor;

                ApplyColorRecursive(child, color);

                if (HasConditions(group))
                {
                    colorIndex++;
                }
            }
        }

        private static bool HasConditions(GroupSteps group)
        {
            return group.Conditions != null && group.Conditions.Length > 0;
        }

        private static void ApplyColorRecursive(Transform root, Color color)
        {
            ObjectColors[root.gameObject.GetInstanceID()] = color;

            for (int i = 0; i < root.childCount; i++)
            {
                ApplyColorRecursive(root.GetChild(i), color);
            }
        }

        private static void OnHierarchyGUI(int instanceId, Rect selectionRect)
        {
            if (!ObjectColors.TryGetValue(instanceId, out Color color))
                return;

            EditorGUI.DrawRect(selectionRect, color);
        }
    }
}
#endif