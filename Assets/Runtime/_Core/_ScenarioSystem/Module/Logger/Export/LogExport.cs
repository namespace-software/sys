using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Core._ScenarioSystem.Component;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Logger.Export
{
    public class LogExport : MonoBehaviour
    {
        private LogSystem _logSystem;

        #region Path Export

        [BoxGroup("Настройки")]
        [Tooltip("Путь для сохранения результатов")]
        [EnableIf("_isCustomPath")] // поле становится недоступным для редактирования, когда _isPathLocked == true
        [SerializeField] private string _exportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Получение пути к рабочему столу;
        [BoxGroup("Настройки")]
        [Tooltip("При выключенной галке путь генерируются на старте из " +
                 "Environment.GetFolderPath(Environment.SpecialFolder.Desktop), " +
                 "при включенной из кастомного пути")]
        [SerializeField] 
        [OnValueChanged("OnUpdatedPathToExport")] // Вызывается при изменении значения в инспекторе
        private bool _isCustomPath = false; // При true - поле _exportPath только для чтения
        
        // Метод, который вызывается при переключении галочки
        private void OnUpdatedPathToExport()
        {
            // Если блокировка снята (переключили в false), устанавливаем путь "11111"
            if (!_isCustomPath)
            {
                _exportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                // Если хотите, чтобы изменение сразу сохранилось в инспекторе, можно вызвать:
                // UnityEditor.EditorUtility.SetDirty(this); // если в редакторе
            }
        }
        
        [Space(10)]
        #endregion


        // [BoxGroup("Настройки")]
        // [SerializeField] private bool _isPdf;
        [BoxGroup("Настройки")]
        [Tooltip("Создать документ word при завершении сценария?")]
        [SerializeField] private bool _isWord;
        [BoxGroup("Настройки")]
        [Tooltip("Создать документ excel при завершении сценария?")]
        [SerializeField] private bool _isExcel;

        [BoxGroup("Фильтры выгрузки логов в документ")]
        [Tooltip("Собирать одинаковые компоненты в одну сущность")]
        [SerializeReference] private bool _isCollapse;
        [BoxGroup("Фильтры выгрузки логов в документ")]
        // [SerializeReference] private Dictionary<State, string> result;
        [Tooltip("Переменная оператор:\n" +
                 "True для вывода должны совпасть состояние и тип ошибки.\n" +
                 "False выводим всё что подходит под состояние или под тип ошибки")]
        [SerializeField] private bool _isAnd;
        [BoxGroup("Фильтры выгрузки логов в документ")]
        [Tooltip("Фильтр по состоянию лога (None, Init, Success, Failure, Skipped)")]
        [SerializeField] private State[] _stateType;
        [BoxGroup("Фильтры выгрузки логов в документ")]
        [Tooltip("Фильтр по типу ошибки лога (Common, Critical, OrderRequired)")]
        [SerializeField] private EnumMistakeType[] _mistakeType;

        [FoldoutGroup("Данные пользователя"), HideLabel]
        [SerializeField] private ExamDocument _examDocument;

        private ScenarioSystem _scenarioSystem;

        private void Awake()
        {
            _logSystem = FindFirstObjectByType<LogSystem>();

            if (_scenarioSystem == null)
                _scenarioSystem = FindFirstObjectByType<ScenarioSystem>();

            // Пересоздание пути для устройств
            OnUpdatedPathToExport();
        }

        public void SetPathToExport(string path)
        {
            _exportPath = path;
        }

        public void Export()
        {
            if (_scenarioSystem.CurrentMode == EnumScenarioMode.Teaching || _scenarioSystem.CurrentMode == EnumScenarioMode.Training)
                return;

            if (!string.IsNullOrEmpty(ScenarioLaunchRequest.DisplayName))
                _examDocument.ExamScenario = ScenarioLaunchRequest.DisplayName;

            List<LogComponent> resultComponents = new List<LogComponent>();

            if (_isCollapse)
                RemoveDuplicatesAndCount(_logSystem.LogComponents);
            
            for (int x = 0; x < _logSystem.LogComponents.Count; x++)
            {
                //// Режим и тип стейта и тип ошибки
                //if (_isAnd && 
                //    _stateType.Contains(_logSystem.LogComponents[x].State) && 
                //    _mistakeType.Contains(_logSystem.LogComponents[x].MistakeType) ||
                //    // Режим или тип стейта, или тип ошибки
                //    !_isAnd && 
                //    (_stateType.Contains(_logSystem.LogComponents[x].State) || 
                //     _mistakeType.Contains(_logSystem.LogComponents[x].MistakeType)))
                //{
                //    resultComponents.Add(_logSystem.LogComponents[x]);
                //}


                LogComponent log = _logSystem.LogComponents[x];

                if (log == null ||
                    log.ExcludeFromReport ||
                    log.Type != ComponentType.Condition)
                {
                    continue;
                }

                if (log.State != State.Success &&
                    log.State != State.Failure &&
                    log.State != State.Skipped)
                {
                    continue;
                }

                resultComponents.Add(log);
            }

            if (_isWord)
            {
                foreach (LogComponent log in resultComponents)
                {
                    bool isViolation =
                        log.State == State.Failure ||
                        log.State == State.Skipped;

                    int violationCount = isViolation ? log.Count : 0;
                    int penaltyPoints = isViolation ? (int)log.PenaltyPoints : 0;

                    _examDocument.AddViolation(
                        log.Info,
                        violationCount,
                        penaltyPoints
                    );
                }

                _examDocument.CalculateAndExport(_exportPath, GetFileName("docx"));
            }

            if (_isExcel)
            {
                ExamDocumentExporterToExcel.Export(_exportPath, resultComponents, _examDocument, GetFileName("xlsx"));
            }
        }
        
        private void RemoveDuplicatesAndCount(List<LogComponent> logs)
        {
            List<LogComponent> result = new List<LogComponent>();
            result.Clear();

            Dictionary<(int GuidId, State State), LogComponent> unique = new();

            foreach (LogComponent log in logs)
            {
                if (log == null)
                    continue;

                bool isCountableState = log.State is State.Failure or State.Skipped;

                var key = (log.GuidId, log.State);

                if (unique.TryGetValue(key, out LogComponent existingLog))
                {
                    if (isCountableState)
                        existingLog.Count++;
                }
                else
                {
                    log.Count = isCountableState ? 1 : 0;

                    unique.Add(key, log);
                    result.Add(log);
                }
            }
            
            logs.Clear();
            logs.AddRange(result);
        }
        
        public string GetFileName(string extention = "docx")
        {
            string scenario = string.IsNullOrWhiteSpace(_examDocument.ExamScenario)
                ? "Результаты экзамена"
                : _examDocument.ExamScenario.Trim();

            foreach (char invalid in Path.GetInvalidFileNameChars())
                scenario = scenario.Replace(invalid, '_');

            return $"{scenario}.{extention}";
        }
    }
}
