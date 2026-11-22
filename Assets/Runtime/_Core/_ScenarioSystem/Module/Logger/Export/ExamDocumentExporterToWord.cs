using System;
using System.IO;
using System.Linq;
using UnityEngine;
// Основной namespace для работы с OpenXML
// Для работы с частями документа

// Для работы с элементами Word документа

namespace _Core._ScenarioSystem.Module.Logger.Export
{
    public class ExamDocumentExporterToWord
    {
        private ExamDocument examDocument; // Хранит данные экзамена

        public ExamDocumentExporterToWord(ExamDocument document)
        {
            this.examDocument = document; // Инициализация данных экзамена
        }

        public void ExportToWord(string path, string fileOutputName)
        {
            //int totalPenaltyPoints = examDocument.ViolationList.Sum(v => v.IsViolated ? v.PenaltyPoints : 0); // Подсчет общего количества штрафных баллов
            //int totalViolations = examDocument.ViolationList.Sum(v => v.ViolationCount); // Подсчет общего количества нарушений
            //int finalScore = Math.Max(100 - totalPenaltyPoints, 0); // Расчет итогового балла (минимум 0)
            //string grade = GetGrade(finalScore); // Получение оценки на основе итогового балла

            //// string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Получение пути к рабочему столу
            //string desktopPath = path;
            //string filePath = Path.Combine(desktopPath, fileOutputName); // Формирование полного пути к файлу

            //using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document)) // Создание нового документа Word
            //{
            //    MainDocumentPart mainPart = doc.AddMainDocumentPart(); // Добавление основной части документа
            //    mainPart.Document = new Document(); // Создание нового документа
            //    Body body = mainPart.Document.AppendChild(new Body()); // Добавление тела документа

            //    AddTitle(body); // Добавление заголовка
            //    AddSubtitle(body); // Добавление подзаголовка
            
            //    body.AppendChild(new Paragraph()); // Добавление пустой строки после заголовка
            
            //    AddExamineeInfo(body); // Добавление информации об экзаменуемом
            //    AddViolationsTable(body); // Добавление таблицы нарушений
            //    AddFinalResults(body, totalViolations, totalPenaltyPoints, finalScore, grade); // Добавление итоговых результатов
            //}

            //Debug.Log($"Документ сохранен на рабочем столе: {filePath}"); // Вывод сообщения о сохранении документа
        }
    }
}

