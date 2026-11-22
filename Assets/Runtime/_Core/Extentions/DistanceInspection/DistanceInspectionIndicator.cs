using UnityEngine;
using UnityEngine.UI;

namespace ScenarioSystem.Additional
{
    /// <summary>
    /// Класс отвечающий за индикацию дистанционного осмотра
    /// </summary>
    public class DistanceInspectionIndicator : MonoBehaviour
    {
        [SerializeField] private Slider inspectionSlider;

        /// <summary>
        /// Активация индикации
        /// </summary>
        /// <param name="value">значение индикации</param>
        public void ActiveIndicator(float value)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            inspectionSlider.value = value;
        }

        /// <summary>
        /// Сброс индикации
        /// </summary>
        public void ResetIndicator()
        {
            gameObject.SetActive(false);
            inspectionSlider.value = 0f;
        }
    }
}