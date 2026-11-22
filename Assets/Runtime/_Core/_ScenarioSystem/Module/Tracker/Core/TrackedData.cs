using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Step;

namespace _Core._ScenarioSystem.Module.Tracker.Core
{
    /// <summary>
    /// Класс реализующий тип данных и добавляющий 2 абстрактных протектед метода для детей
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public abstract class TrackedData<T> : TrackedBase
    {
        /// <summary>
        /// Список данных текущего объекта по шагам
        /// </summary>
        private readonly List<StructTrackedData<T>> _data = new List<StructTrackedData<T>>();
        
        public override void UpdatedData(StepBase step)
        {
            // Проверяем есть ли данный шаг уже в данных
            bool containsStep = false;
            for (int x = 0; x < _data.Count; x++)
            {
                if (_data[x].Step == step)
                {
                    containsStep = true;
                    break;
                }
            }
            
            // Если есть сносим всё что было после до этого шага, выгружаем данные и сносим этот шаг 
            if (containsStep)
            {
                for (int x = _data.Count - 1; x >= 0; x--)
                {
                    if (_data[x].Step == step)
                    {
                        StructTrackedData<T> data = _data[x];
                        LoadData(data, step);
                        break;
                    }
                    
                    _data.RemoveAt(x);
                }
            }
            // Если нет, то в конце добавляем новые данные 
            else
            {
                StructTrackedData<T> newData = SetNewData(step);
                _data.Add(newData);
            }
        }
        
        /// <summary>
        /// Метод добавления новых данных для объекта
        /// </summary>
        /// <param name="step">Ссылка на шаг для генерации структуры истории</param>
        /// <returns>Возвращаем структуру, которая хранит в себе данные текущего шага</returns>
        protected abstract StructTrackedData<T> SetNewData(StepBase step);

        /// <summary>
        /// Загружаем ранее сохранённые данные
        /// </summary>
        /// <param name="data">Ранее сохранённые данные</param>
        /// <param name="step"></param>
        protected abstract void LoadData(StructTrackedData<T> data, StepBase step);
    }
}
