#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ScenarioEditorMenu
{
    private const string PrefabPath = "Packages/Scenario System/Runtime/_Core/Prefabs/ScenarioSystemRoot.prefab";

    [MenuItem("Tools/Scenario System/Install Scenario Core", false, 0)]
    private static void CreatePrefabUnpackAndLiftChildren()
    {
        // 1. Находим префаб по имени
        string prefabPath = FindPrefabPath();
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[ERROR] Prefab 'ScenarioSystemRoot' не найден в проекте.");
            return;
        }

        // 2. Загружаем префаб
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[ERROR] Не удалось загрузить префаб по пути: {prefabPath}");
            return;
        }

        // 3. Инстанцируем в сцену
        GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (prefabInstance == null)
        {
            Debug.LogError("[ERROR] Не удалось создать instance prefab");
            return;
        }

        // 4. Распаковываем
        PrefabUtility.UnpackPrefabInstance(
            prefabInstance,
            PrefabUnpackMode.OutermostRoot,
            InteractionMode.UserAction
        );

        Transform root = prefabInstance.transform;
        Transform parent = root.parent;
        int siblingIndex = root.GetSiblingIndex();
        int movedCount = 0;

        // 5. Переносим детей наверх
        while (root.childCount > 0)
        {
            Transform child = root.GetChild(0);
            Undo.SetTransformParent(child, parent, "Lift Prefab Child");
            child.SetSiblingIndex(siblingIndex);
            siblingIndex++;
            movedCount++;
        }

        // 6. Удаляем root
        Undo.DestroyObjectImmediate(prefabInstance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

// Вспомогательный метод для поиска префаба
    private static string FindPrefabPath()
    {
        // Ищем все ассеты с именем "ScenarioSystemRoot" типа Prefab
        string[] guids = AssetDatabase.FindAssets("ScenarioSystemRoot t:Prefab", new[] { "Packages" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Дополнительная проверка на точное совпадение имени файла (на случай, если найдётся несколько)
            if (System.IO.Path.GetFileName(path) == "ScenarioSystemRoot.prefab")
                return path;
        }

        return null;
    }

    [MenuItem("Tools/Scenario System/Install Scenario Core", true)]
    private static bool Validate()
    {
        return true;
    }
}
#endif