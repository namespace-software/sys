using System;
using _Core._ScenarioSystem.Component.Step;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Tracker.Core
{
    /// <summary>
    /// TODO Менеджер который надо вытащить из кора сценариев
    /// Сейчас вызывается по синглтону из кода, если в сцене нет то крах всей системы
    /// </summary>
    public class TrackedManager : MonoBehaviour
    {
        public static TrackedManager Instance;

        /// <summary>
        /// Евент обновление шага, новый шаг
        /// </summary>
        public Action<StepBase> OnUpdatedHistory;
        
        /// <summary>
        /// Ссылки на все компоненты истории
        /// </summary>
        private TrackedBase[] _trackedObjects;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _trackedObjects = FindObjectsByType<TrackedBase>(FindObjectsSortMode.None);
        }

        private void OnEnable()
        {
            OnUpdatedHistory += UpdatedHistory;
        }

        private void OnDisable()
        {
            OnUpdatedHistory -= UpdatedHistory;
        }

        /// <summary>
        /// Обновляем все объекты завязанные на истории
        /// </summary>
        /// <param name="step">Текущий шаг</param>
        private void UpdatedHistory(StepBase step)
        {
            for (int x = 0; x < _trackedObjects.Length; x++)
            {
                _trackedObjects[x].UpdatedData(step);
            }
        }
    }
}
