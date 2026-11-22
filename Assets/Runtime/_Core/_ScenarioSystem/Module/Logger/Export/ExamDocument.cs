using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Logger.Export
{
    [Serializable]
    public class ExamDocument
    {
        [Tooltip("Заголовок документа")]
        public string DocumentTitle = "Экзаменационный лист";

        [Tooltip("Подзаголовок документа")]
        public string DocumentSubtitle = "проведения экзамена в классе интерактивной подготовки";

        [Tooltip("Фамилия экзаменуемого")]
        public string LastName;

        [Tooltip("Имя экзаменуемого")]
        public string FirstName;

        [Tooltip("Отчество экзаменуемого")]
        public string MiddleName;

        [Tooltip("Табельный номер экзаменуемого")]
        public string PersonnelNumber;

        [Tooltip("Сценарий проводимого экзамена")]
        public string ExamScenario;

        [Tooltip("Список нарушений")]
        public List<ViolationRow> ViolationList = new List<ViolationRow>();

        public string ExamDate { get; private set; } // Дата проведения экзамена

        void Start()
        {
            RecalculateViolationNumbers(); // Пересчет номеров нарушений
        }

        public void RecalculateViolationNumbers()
        {
            // Присвоение порядковых номеров каждому нарушению
            for (int i = 0; i < ViolationList.Count; i++)
            {
                ViolationList[i].Number = i + 1;
            }
        }

        // Создание и добавление нового нарушения в список
        public void AddViolation(string violationDescription, int violationCount, int penaltyPoints)
        {
            ViolationRow newRow = new ViolationRow
            {
                ViolationDescription = violationDescription,
                ViolationCount = violationCount,
                PenaltyPoints = penaltyPoints
            };
            ViolationList.Add(newRow);
            RecalculateViolationNumbers(); 
        }

        // Редактирование существующего нарушения по его номеру
        public void EditViolation(int rowNumber, string violationDescription, int violationCount, int penaltyPoints)
        {
            if (rowNumber > 0 && rowNumber <= ViolationList.Count)
            {
                ViolationRow row = ViolationList[rowNumber - 1];
                row.ViolationDescription = violationDescription;
                row.ViolationCount = violationCount;
                row.PenaltyPoints = penaltyPoints;
            }
            RecalculateViolationNumbers(); 
        }

        // Удаление нарушения по его номеру
        public void RemoveViolation(int rowNumber)
        {
            if (rowNumber > 0 && rowNumber <= ViolationList.Count)
            {
                ViolationList.RemoveAt(rowNumber - 1);
                RecalculateViolationNumbers();
            }
        }

        //Сохранение результата и экспорт
        [ContextMenu("Рассчитать результаты и экспортировать")]
        public void CalculateAndExport(string path, string fileOutputName)
        {
            ExamDate = DateTime.Now.ToString("dd.MM.yyyy"); // Установка текущей даты при старте
            RecalculateViolationNumbers(); // Пересчет номеров перед экспортом
            ExamDocumentExporterToWord exporterToWord = new ExamDocumentExporterToWord(this);
            exporterToWord.ExportToWord(path, fileOutputName); // Экспорт документа в Word
        }

        //Отладка//_______________________________________________________________________________________________________
    
        [ContextMenu("Добавить нарушение")]
        public void AddViolationFromMenu()
        {
            AddViolation("Новое нарушение", 0, 0);
            Debug.Log("Добавлено новое нарушение");
        }
    
        [ContextMenu("Изменить последнее нарушение")]
        public void EditLastViolationFromMenu()
        {
            if (ViolationList.Count > 0)
            {
                EditViolation(ViolationList.Count, "Измененное нарушение", 1, 5);
                Debug.Log("Последнее нарушение изменено");
            }
            else
            {
                Debug.Log("Нет нарушений для изменения");
            }
        }
    }
}