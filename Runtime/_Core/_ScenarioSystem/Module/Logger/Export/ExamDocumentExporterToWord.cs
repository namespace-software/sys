using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
            int totalPenaltyPoints = examDocument.ViolationList.Sum(v => v.IsViolated ? v.PenaltyPoints : 0); // Подсчет общего количества штрафных баллов
            int totalViolations = examDocument.ViolationList.Sum(v => v.ViolationCount); // Подсчет общего количества нарушений
            int finalScore = Math.Max(100 - totalPenaltyPoints, 0); // Расчет итогового балла (минимум 0)
            string grade = GetGrade(finalScore); // Получение оценки на основе итогового балла

            // string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Получение пути к рабочему столу
            string desktopPath = path;
            string filePath = Path.Combine(desktopPath, fileOutputName); // Формирование полного пути к файлу

            using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document)) // Создание нового документа Word
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart(); // Добавление основной части документа
                mainPart.Document = new Document(); // Создание нового документа
                Body body = mainPart.Document.AppendChild(new Body()); // Добавление тела документа

                AddTitle(body); // Добавление заголовка
                AddSubtitle(body); // Добавление подзаголовка
            
                body.AppendChild(new Paragraph()); // Добавление пустой строки после заголовка
            
                AddExamineeInfo(body); // Добавление информации об экзаменуемом
                AddViolationsTable(body); // Добавление таблицы нарушений
                AddFinalResults(body, totalViolations, totalPenaltyPoints, finalScore, grade); // Добавление итоговых результатов
            }

            Debug.Log($"Документ сохранен на рабочем столе: {filePath}"); // Вывод сообщения о сохранении документа
        }

        private void AddTitle(Body body)
        {
            Paragraph titleParagraph = body.AppendChild(new Paragraph(
                new Run(new Text(examDocument.DocumentTitle))
            )); // Создание параграфа с заголовком
            titleParagraph.ParagraphProperties = new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center }, // Выравнивание по центру
                new SpacingBetweenLines() { After = "0" } // Убирает отступ после параграфа
            );
        }

        private void AddSubtitle(Body body)
        {
            Paragraph subtitleParagraph = body.AppendChild(new Paragraph(
                new Run(
                    new RunProperties(new Bold()), // Жирный шрифт
                    new Text(examDocument.DocumentSubtitle)
                )
            )); // Создание параграфа с подзаголовком
            subtitleParagraph.ParagraphProperties = new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center }, // Выравнивание по центру
                new SpacingBetweenLines() { After = "0" }, // Убирает отступ после параграфа
                new ParagraphBorders( // Добавление границ вокруг параграфа
                    new TopBorder() { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder() { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder() { Val = BorderValues.Single, Size = 4 },
                    new RightBorder() { Val = BorderValues.Single, Size = 4 }
                )
            );
        }

        private void AddExamineeInfo(Body body)
        {
            Table infoTable = new Table();
            TableProperties infoTableProperties = new TableProperties(
                new TableBorders( // Установка границ таблицы (все границы отсутствуют)
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) }
                ),
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct } // Ширина таблицы 100%
            );
            infoTable.AppendChild(infoTableProperties);

            // Добавление строк с информацией об экзаменуемом
            //infoTable.Append(CreateInfoTableRow("Фамилия:", examDocument.LastName));
            //infoTable.Append(CreateInfoTableRow("Имя:", examDocument.FirstName));
            //infoTable.Append(CreateInfoTableRow("Отчество:", examDocument.MiddleName));
            //infoTable.Append(CreateInfoTableRow("Табельный номер:", examDocument.PersonnelNumber));
            infoTable.Append(CreateInfoTableRow("Дата проведения:", examDocument.ExamDate));
            infoTable.Append(CreateInfoTableRow("Сценарий экзамена:", examDocument.ExamScenario));

            body.Append(infoTable);

            body.Append(new Paragraph(new Run(new Text(" ")))); // Добавление отступа между таблицами
        }

        private TableRow CreateInfoTableRow(string label, string value)
        {
            TableRow row = new TableRow();
        
            TableCell labelCell = new TableCell(
                new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines() { After = "0" } // Убирает отступ после параграфа
                    ),
                    new Run(new Text(label))
                )
            );
            labelCell.Append(new TableCellProperties(
                new TableCellWidth() { Width = "1500", Type = TableWidthUnitValues.Pct }, // Ширина ячейки 30%
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Bottom } // Выравнивание текста по нижнему краю
            ));
        
            TableCell valueCell = new TableCell(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center }, // Выравнивание по центру
                        new SpacingBetweenLines() { After = "0" } // Убирает отступ после параграфа
                    ),
                    new Run(new Text(value))
                )
            );
            valueCell.Append(new TableCellProperties(
                new TableCellWidth() { Width = "3500", Type = TableWidthUnitValues.Pct }, // Ширина ячейки 70%
                new TableCellBorders(
                    new BottomBorder() { Val = BorderValues.Single, Size = 6 } // Добавление нижней границы
                ),
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Bottom } // Выравнивание текста по нижнему краю
            ));
        
            row.Append(labelCell);
            row.Append(valueCell);
        
            return row;
        }

        private void AddViolationsTable(Body body)
        {
            Table table = new Table();
            TableProperties tblProp = new TableProperties(
                new TableBorders( // Установка границ таблицы
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 }
                ),
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }, // Ширина таблицы 100%
                new TableLayout() { Type = TableLayoutValues.Fixed } // Фиксированная ширина столбцов
            );
            table.AppendChild(tblProp);

            TableRow headerRow = new TableRow();
            headerRow.Append(CreateTableCell("№", true, 600, true)); // Создание ячейки заголовка
            headerRow.Append(CreateTableCell("Перечень нарушений", true, 3500, true));  
            headerRow.Append(CreateTableCell("Отметка о нарушении", true, 1200, true)); 
            headerRow.Append(CreateTableCell("Количество нарушений", true, 1300, true));
            headerRow.Append(CreateTableCell("Шкала штрафных баллов за нарушение", true, 2700, true));

            table.Append(headerRow);

            foreach (var violation in examDocument.ViolationList)
            {
                TableRow dataRow = new TableRow();
                dataRow.Append(CreateTableCell(violation.Number.ToString(), true, 600));
                dataRow.Append(CreateTableCell(violation.ViolationDescription, false, 3800));
                dataRow.Append(CreateTableCell(violation.IsViolated ? "да" : "нет", true, 1500));
                dataRow.Append(CreateTableCell(violation.ViolationCount.ToString(), true, 1500));
                dataRow.Append(CreateTableCell(violation.PenaltyPoints.ToString(), true, 1900));
                table.Append(dataRow);
            }

            body.Append(table);
        
            body.Append(new Paragraph(new Run(new Text(" ")))); // Добавление отступа после таблицы
        }

        private TableCell CreateTableCell(string text, bool center, int width, bool bold = false)
        {
            TableCell cell = new TableCell();
            Paragraph paragraph = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = center ? JustificationValues.Center : JustificationValues.Left } // Выравнивание текста
                ),
                new Run(
                    new RunProperties(bold ? new Bold() : null), // Применение жирного шрифта, если требуется
                    new Text(text)
                )
            );
        
            cell.Append(paragraph);
            cell.Append(new TableCellProperties(
                new TableCellWidth() { Width = width.ToString(), Type = TableWidthUnitValues.Dxa }, // Установка ширины ячейки
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center } // Вертикальное выравнивание по центру
            ));
            return cell;
        }

        private void AddFinalResults(Body body, int totalViolations, int totalPenaltyPoints, int finalScore, string grade)
        {
            Table table = new Table();
            TableProperties tblProp = new TableProperties(
                new TableBorders( // Установка границ таблицы (все границы отсутствуют)
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) }
                ),
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }, // Ширина таблицы 100%
                new TableLayout() { Type = TableLayoutValues.Fixed } // Фиксированная ширина столбцов
            );
            table.AppendChild(tblProp);

            TableRow row = new TableRow();

            // Первый столбец
            TableCell cell1 = CreateTableCell("Итоговый результат:", true, 2000, true);
            cell1.Append(new TableCellProperties(
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center } // Вертикальное выравнивание по центру
            ));
            row.Append(cell1);

            // Второй столбец
            TableCell cell2 = new TableCell();
            cell2.Append(new TableCellProperties(
                new TableCellWidth() { Width = "1500", Type = TableWidthUnitValues.Dxa } // Установка ширины ячейки
            ));
            cell2.Append(CreateParagraphWithAlignment("Оценка:", JustificationValues.Left)); // Добавление параграфа с выравниванием по левому краю
            cell2.Append(CreateParagraphWithAlignment("Количество нарушений:", JustificationValues.Left));
            cell2.Append(CreateParagraphWithAlignment("Штрафные баллы:", JustificationValues.Left));
            row.Append(cell2);

            // Третий столбец
            TableCell cell3 = new TableCell();
            cell3.Append(new TableCellProperties(
                new TableCellWidth() { Width = "1500", Type = TableWidthUnitValues.Dxa } // Установка ширины ячейки
            ));
            cell3.Append(CreateParagraphWithAlignment(grade, JustificationValues.Left, true)); // Добавление параграфа с жирным шрифтом
            cell3.Append(CreateParagraphWithAlignment(totalViolations.ToString(), JustificationValues.Left, true));
            cell3.Append(CreateParagraphWithAlignment(totalPenaltyPoints.ToString(), JustificationValues.Left, true));
            row.Append(cell3);

            table.Append(row);
            body.Append(table);
        }

        private Paragraph CreateParagraphWithAlignment(string text, JustificationValues alignment, bool bold = false)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = alignment }, // Установка выравнивания текста
                    new SpacingBetweenLines() { After = "0" } // Убирает отступ после параграфа
                ),
                new Run(
                    new RunProperties(bold ? new Bold() : null), // Применение жирного шрифта, если требуется
                    new Text(text)
                )
            );
        }

        private string GetGrade(int score)
        {
            if (score < 90) return "Неудовлетворительно";
            if (score < 93) return "Удовлетворительно";
            if (score < 97) return "Хорошо";
            return "Отлично";
        }
    }
}

