using _Core._ScenarioSystem.Component.Step;

namespace _Core._ScenarioSystem.Module.Tracker.Core
{
    /// <summary>
    /// Структура хранения данных объекта или скрипта и ссылка на шаг которому эти данные принадлежат
    /// </summary>
    /// <typeparam name="T">Данные</typeparam>
    public struct StructTrackedData<T>
    {
        public StepBase Step;
        public T Data;

        public StructTrackedData(StepBase step, T data)
        {
            Step = step;
            Data = data;
        }
    }
}
