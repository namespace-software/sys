using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.HintsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core.SoundSystem
{
    /// <summary>
    /// Прослойка для работы с потребностями системы сценариев по обработке озвучки
    /// </summary>
    public class ScenarioAudioMediator : MonoBehaviour
    {
        /// <summary>
        /// Аудиосистема системы сценариев TODO нужно на абстракцию перейти для кастомных реализаций
        /// </summary>
        [Tooltip("Ссылка на аудиосистему для воспроизведения")]
        [BoxGroup("Link's")][SerializeField] private _Alex.Sound.SoundSystem _soundSystem;

        /// <summary>
        /// TODO необходимо перенести всю логику непосредственно в хинтс систем или сделать
        /// прослойку на основе медиатора аудио
        /// </summary>
        private HintsSystem _hintsSystem;
        
        /// <summary>
        /// Текущая корутина с отложенной озвучкой и эвентом
        /// </summary>
        private Coroutine _currentRoutine;
        
        /// <summary>
        /// Переменная для остановки воспроизведения при групповой озвучке
        /// </summary>
        [Tooltip("Пауза для групповых аудиосообщений")]
        [BoxGroup("Debug")][SerializeReference, ReadOnly] private bool _isPaused;
        private bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;

                if (!_isPaused && _conditions.Count > 0 && _lastConditionSound != _conditions.First())
                {
                    _delay = 0;
                }
            }
        }
        
        /// <summary>
        /// Список кондишинов групповой озвучки
        /// </summary>
        [Tooltip("Список активных аудиосообщений из ConditionBase")]
        [BoxGroup("Debug")][SerializeReference, ReadOnly] private List<ConditionBase> _conditions = new List<ConditionBase>();

        /// <summary>
        /// Сервисная переменная для отслеживания откуда был последний запуск аудио
        /// </summary>
        private ConditionBase _lastConditionSound;

        private void Start()
        {
            _hintsSystem = FindFirstObjectByType<HintsSystem>();
        }

        /// <summary>
        /// Запуск озвучки кондишинов
        /// </summary>
        /// <param name="condition">Кондишн для отработки множества одновременных озвучек</param>
        /// <param name="isInit">Кондишн запускается сейчас или завершается</param>
        /// <param name="clip">Ссылка на клип</param>
        /// <param name="delay">Ожидание в секундах</param>
        /// <param name="action">Событие</param>
        public void Play(ConditionBase condition, bool isInit, AudioClip clip, float delay, Action action = null)
        {
            if (isInit)
            {
                if (_conditions.Count <= 0)
                {
                    _delay = 0;
                }
                _conditions.Add(condition);
            }
            else
            {
                if (_conditions.Contains(condition))
                    _conditions.Remove(condition);
                
                IsPaused = true;
                
                Play(clip, delay, action);
            }
        }

        /// <summary>
        /// Чистка списка воспроизводимых кондишинов
        /// Сделано для обнуления во время ошибок
        /// </summary>
        public void ClearList()
        {
            _conditions.Clear();
            StopCurrentRoutine();
        }

        /// <summary>
        /// Сервисная переменная для отсчёта времени
        /// </summary>
        private float _delay = 0;
        private void Update()
        {
            if (_conditions.Count > 0 && !IsPaused)
            {
                _delay -= Time.deltaTime;
                if (_delay < 0)
                {
                    ConditionBase condition = _conditions.First();
                    _lastConditionSound = condition;
                    Play(condition.startInstructions.instructionClip, 0);
                    _hintsSystem.FillInfoPanel(condition.startInstructions.instructionText);
                    _delay = _conditions.First().startInstructions.instructionRepeatDelay;
                }
            }
        }

        /// <summary>
        /// Запуск озвучки шагов
        /// </summary>
        /// <param name="clip">Ссылка на клип</param>
        /// <param name="delay">Ожидание в секундах</param>
        /// <param name="action">Событие</param>
        public void Play(AudioClip clip, float delay, Action action = null)
        {
            StartExclusiveRoutine(SingleDelayRoutine(clip, delay, action));
        }
        
        private void StartExclusiveRoutine(IEnumerator routine)
        {
            StopCurrentRoutine();
            _soundSystem.FullStop?.Invoke();
            _currentRoutine = StartCoroutine(WrapRoutine(routine));
        }
        
        /// <summary>
        /// Сброс ссылки корутины после её завершения
        /// </summary>
        /// <param name="routine">Корутина</param>
        /// <returns></returns>
        private IEnumerator WrapRoutine(IEnumerator routine)
        {
            yield return routine;
            _currentRoutine = null;
        }
        
        /// <summary>
        /// Остановить текущую корутину
        /// </summary>
        private void StopCurrentRoutine()
        {
            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
                _currentRoutine = null;
            }
        }
        
        /// <summary>
        /// Озвучка с задержкой в секундах
        /// </summary>
        /// <param name="clip">Ссылка на клип</param>
        /// <param name="delay">Ожидание в секундах</param>
        /// <param name="action">Событие</param>
        /// <returns></returns>
        private IEnumerator SingleDelayRoutine(AudioClip clip, float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            if (_conditions.Count <= 0)
                _delay = 0;
            
            _soundSystem.OnSingleSoundPlayWithClip?.Invoke(clip, action);
            
            IsPaused = false;
        }
    }
}
