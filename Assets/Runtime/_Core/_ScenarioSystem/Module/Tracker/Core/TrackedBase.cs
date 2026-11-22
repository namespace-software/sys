using _Core._ScenarioSystem.Component.Step;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Tracker.Core
{
    /// <summary>
    /// Базовый класс обновления данных истории (обновляем локальные данные или сохраняем текущие)
    /// </summary>
    public abstract class TrackedBase : MonoBehaviour
    {
        public abstract void UpdatedData(StepBase step);
    }
}
