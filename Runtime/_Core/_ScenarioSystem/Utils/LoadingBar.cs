using System.Collections;
using HurricaneVR.Framework.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Core._ScenarioSystem.Utils
{
    /// <summary>
    /// Класс отвечающий за отображение статуса ожидания при выполнении какого-либо действия
    /// РЕФАКТОР Три эвента окончания события / Необходимость HVRGrabbable и ButtonHelper ???
    /// </summary>
    public class LoadingBar : MonoBehaviour
    {
        /// <summary>
        /// captainkryga Кнопка как я понял для закрытия кондишина в сценарии (чекал через щуп)
        /// </summary>
        [SerializeField] private Button _clickBtn;
        [SerializeField] private float _loadingTime;
        [SerializeField] private HVRGrabbable _hVRGrabbable;
        [SerializeField] private bool _grabbableClickIgnore;
        [SerializeField] private bool _isPause;
        [SerializeField] private bool _multipleActivating = true;
        private ButtonHelper _buttonHelper;
        [SerializeField] private Image _loadingbarImg;
        private float _currentLoadingTime;
        private bool _isCompleted;

        public UnityEvent OnCompleted;

        private void Awake()
        {
            _loadingbarImg = GetComponent<Image>();
            _hVRGrabbable = GetComponentInParent<HVRGrabbable>();
        }

        private void OnEnable()
        {
            if (_multipleActivating) ResetToDefault();
            if (_multipleActivating || !_isCompleted || _grabbableClickIgnore) StartCoroutine(LoadingbarHandler());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _loadingbarImg.fillAmount = 0;
            _currentLoadingTime = _loadingTime;
        }

        public void ActivateBar()
        {
            if (_multipleActivating) ResetToDefault();
            if (_multipleActivating || !_isCompleted || _grabbableClickIgnore) StartCoroutine(LoadingbarHandler());
        }

        public void DeactivateBar()
        {
            StopAllCoroutines();
            _loadingbarImg.fillAmount = 0;
            _currentLoadingTime = _loadingTime;
        }

        private IEnumerator LoadingbarHandler()
        {
            // Проверка все комплектующих для запуска прогрессбара
            if (_loadingbarImg == null) _loadingbarImg = GetComponent<Image>();
            if (_hVRGrabbable == null && !_grabbableClickIgnore) { Debug.LogError("_hVRGrabbable is null", transform); yield break; }
            if (!_grabbableClickIgnore) yield return new WaitUntil(() => _hVRGrabbable.IsHandGrabbed);

            _loadingbarImg.fillAmount = 0;
            _currentLoadingTime = _loadingTime;

            // Цикл прокрутки
            while (_currentLoadingTime > 0)
            {
                if (!_isPause)
                {
                    _currentLoadingTime -= Time.deltaTime;
                    _loadingbarImg.fillAmount = 1f - _currentLoadingTime / _loadingTime;
                }
                yield return null;
            }

            // Запуск эвентов окончания прокрутки
            if (_clickBtn)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                _clickBtn.OnPointerClick(pointerData);
                _buttonHelper = _clickBtn.GetComponent<ButtonHelper>();
                _buttonHelper?.Click();
            }

            _loadingbarImg.enabled = false;
            _isCompleted = true;

            OnCompleted?.Invoke();
            //Debug.LogError("Here");
        }

        public void PauseActive(bool active)
        {
            _isPause = active;
        }

        private void ResetToDefault()
        {
            StopAllCoroutines();
            if (_loadingbarImg == null) _loadingbarImg = GetComponent<Image>();
            _isCompleted = false;
            _loadingbarImg.enabled = true;
            _loadingbarImg.fillAmount = 0;
            _currentLoadingTime = _loadingTime;
        }
    }
}
