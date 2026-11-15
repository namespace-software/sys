using _Core._ScenarioSystem.Component.Step;
using _Core._ScenarioSystem.Module.Tracker.Core;
// using Leap.Unity;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Tracker.Handlers
{
    /// <summary>
    /// Верхне-уровневая реализация истории под transform
    /// </summary>
    public class TrackedTransform : TrackedData<Vector3[]>
    {
        [Tooltip("Условие. Если выбрано, происходит сохранение локальных позиций, если нет то глобальных")]
        [SerializeField] private bool _isLocal;
        
        /// <summary>
        /// Реализация сохранения новых данных
        /// </summary>
        protected override StructTrackedData<Vector3[]> SetNewData(StepBase step)
        {
            return new StructTrackedData<Vector3[]>()
            {
                Step = step,
                Data = _isLocal ? 
                    new Vector3[] { transform.localPosition, transform.localEulerAngles, transform.localScale } :
                    new Vector3[] { transform.position, transform.eulerAngles, transform.lossyScale }
            };
        }

        /// <summary>
        /// Реализация загрузки старых данных
        /// </summary>
        protected override void LoadData(StructTrackedData<Vector3[]> data, StepBase step)
        {
            if (_isLocal)
            {
                transform.localPosition = data.Data[0];
                transform.localEulerAngles = data.Data[1];
                transform.localScale = data.Data[2];
            }
            else
            {
                transform.position = data.Data[0];
                transform.eulerAngles = data.Data[1];
                // transform.SetLossyScale(data.Data[2]);
            }
        }
    }
}
