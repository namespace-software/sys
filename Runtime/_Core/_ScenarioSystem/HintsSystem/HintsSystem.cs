using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Core._ScenarioSystem.HintsSystem
{
    /// <summary>
    /// Модуль отвечающий за вывод текстовых сообщений в определённые TMP
    /// </summary>
    public class HintsSystem : MonoBehaviour
    {
        [Title("Info Outputs")]
        [Tooltip("Список TMP компонентов в которые автоматически будет отправляться информация о текущем шаге/кондишене/нарушении")]
        [SerializeField] private List<TextMeshProUGUI> hintTMPs;

        // [Title("Violation Message")]
        // [Tooltip("При наличии плашки для сообщении о типе нарушения")]
        // [SerializeField] private bool _hasViolationMessage;
        //
        // [Tooltip("Ссылка на GO родителя плашки")]
        // [ShowIf(nameof(_hasViolationMessage))]
        // [SerializeField] private GameObject _violationGO;
        //
        // [Tooltip("Ссылка на TMP плашки")]
        // [ShowIf(nameof(_hasViolationMessage))]
        // [SerializeField] private TextMeshProUGUI _violationText;
        //
        // [Tooltip("Время в течении которого плашка будет висеть. После окончания выключается автоматически")]
        // [ShowIf(nameof(_hasViolationMessage))]
        // [SerializeField] private float _violationMessageTime;

        // public UnityEvent OnHintsDataUpdated;
        // public UnityEvent OnHintsOpened;

        /// <summary>
        /// Метод обновление текстовой информации
        /// </summary>
        /// <param name="hints"></param>
        public void FillInfoPanel(string hints)
        {
            foreach (var item in hintTMPs)
            {
                if (item == null)
                    continue;
                
                item.text = hints;
            }
        }

        // public void ShowViolationMessage(string message)
        // {
        //     if (!_hasViolationMessage) return;
        //     StartCoroutine(ViolationMessageCoroutine(message));
        // }
        //
        // private IEnumerator ViolationMessageCoroutine(string message)
        // {
        //     _violationGO.SetActive(true);
        //     _violationText.text = message;
        //
        //     yield return new WaitForSeconds(_violationMessageTime);
        //     _violationGO.SetActive(false);
        // }
    }
}
