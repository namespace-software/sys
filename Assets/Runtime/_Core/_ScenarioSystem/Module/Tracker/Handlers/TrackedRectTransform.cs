using _Core._ScenarioSystem.Component.Step;
using _Core._ScenarioSystem.Module.Tracker.Core;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Tracker.Handlers
{
    /// <summary>
    /// Верхне-уровневая реализация истории под rect transform
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TrackedRectTransform : TrackedData<Vector3[]>
    {
        /// <summary>
        /// Ссылка на рект трансформ
        /// </summary>
        private RectTransform _rt;

        private void Start()
        {
            _rt = GetComponent<RectTransform>();
        }
        
        /// <summary>
        /// Реализация сохранения новых данных
        /// </summary>
        protected override StructTrackedData<Vector3[]> SetNewData(StepBase step)
        {
            return new StructTrackedData<Vector3[]>()
            {
                Step = step,
                Data = new Vector3[] { _rt.localPosition, _rt.localEulerAngles, _rt.localScale }
            };
        }

        /// <summary>
        /// Реализация загрузки старых данных
        /// </summary>
        protected override void LoadData(StructTrackedData<Vector3[]> data, StepBase step)
        {
            _rt.localPosition = data.Data[0];
            _rt.localEulerAngles = data.Data[1];
            _rt.localScale = data.Data[2];
        }
    }
}
