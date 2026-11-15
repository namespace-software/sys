using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniExcelLibs;
using UnityEngine;
using _Core._ScenarioSystem.Component;


namespace _Core._ScenarioSystem.Module.Logger.Export
{
    public static class ExamDocumentExporterToExcel
    {
        [Header("Template")] [SerializeField] private static string templateFileName = "ScenarioSystem/example.xlsx";

        [ContextMenu("Export Exam Excel")]
        public static void Export(string path, List<LogComponent> logComponents, ExamDocument examDocument,
            string fileOutputName)
        {
            string templatePath = Path.Combine(Application.streamingAssetsPath, templateFileName);

            string outputPath = Path.Combine(
                path,
                fileOutputName
            );

            List<ExamLogExcelRow> rows = BuildRows(logComponents);

            int totalViolations = rows.Sum(x => x.Count);
            int totalPenaltyPoints = rows.Sum(x => x.PenaltyPoints * x.Count);

            var value = new Dictionary<string, object>
            {
                ["LastName"] = examDocument.LastName,
                ["FirstName"] = examDocument.FirstName,
                ["MiddleName"] = examDocument.MiddleName,
                ["PersonnelNumber"] = examDocument.PersonnelNumber,
                ["ExamDate"] = DateTime.Now.ToString("dd.MM.yyyy"),
                ["ScenarioName"] = examDocument.ExamScenario,

                ["Items"] = rows,

                ["FinalGrade"] = GetGrade(totalPenaltyPoints),
                ["TotalViolationCount"] = totalViolations,
                ["TotalPenaltyPoints"] = totalPenaltyPoints
            };
            
            MiniExcel.SaveAsByTemplate(outputPath, templatePath, value);

            Debug.Log($"Excel создан: {outputPath}");
        }

        private static List<ExamLogExcelRow> BuildRows(List<LogComponent> logs)
        {
            List<LogComponent> uniqueLogs = MergeDuplicatesByGuid(logs);

            List<ExamLogExcelRow> result = new();

            for (int i = 0; i < uniqueLogs.Count; i++)
            {
                LogComponent log = uniqueLogs[i];

                result.Add(new ExamLogExcelRow
                {
                    Number = i + 1,
                    Info = string.IsNullOrWhiteSpace(log.Info) ? "EMPTY" : log.Info,
                    ViolationMark = log.Count > 0 ? "да" : "нет",
                    Count = log.State == State.Failure || log.State == State.Skipped
                    ? log.Count
                    : 0,
                    PenaltyPoints =
                    log.State == State.Failure || log.State == State.Skipped
                    ? (int)log.PenaltyPoints
                    : 0
                });
            }

            return result;
        }

        private static List<LogComponent> MergeDuplicatesByGuid(List<LogComponent> logs)
        {
            Dictionary<int, LogComponent> unique = new();
            List<LogComponent> result = new();

            foreach (LogComponent log in logs)
            {
                if (log == null)
                    continue;

                if (unique.TryGetValue(log.GuidId, out LogComponent existing))
                {
                    existing.Count++;
                }
                else
                {
                    log.Count = Mathf.Max(1, log.Count);
                    unique.Add(log.GuidId, log);
                    result.Add(log);
                }
            }

            return result;
        }

        private static string GetGrade(int totalPenaltyPoints)
        {
            if (totalPenaltyPoints == 0)
                return "Отлично";

            if (totalPenaltyPoints <= 5)
                return "Хорошо";

            if (totalPenaltyPoints <= 10)
                return "Удовлетворительно";

            return "Неудовлетворительно";
        }

        private class ExamLogExcelRow
        {
            public int Number { get; set; }
            public string Info { get; set; }
            public string ViolationMark { get; set; }
            public int Count { get; set; }
            public int PenaltyPoints { get; set; }
        }
    }
}
