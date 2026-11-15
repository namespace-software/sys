using System;

namespace _Core._ScenarioSystem.Module.Logger.Export
{
    [Serializable]
    public class ViolationRow
    {
        public int Number { get; set; } // Порядковый номер нарушения
        public string ViolationDescription; // Описание нарушения
        public int ViolationCount; // Количество совершенных нарушений данного типа
        public int PenaltyPoints; // Количество штрафных баллов за данное нарушение

        // Свойство для определения наличия нарушения (true, если количество нарушений больше 0)
        public bool IsViolated => ViolationCount > 0;
    }
}