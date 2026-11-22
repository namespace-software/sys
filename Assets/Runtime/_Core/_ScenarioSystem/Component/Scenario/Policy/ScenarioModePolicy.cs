using UnityEngine;

namespace ScenarioSystems
{
    /// <summary>
    /// Хранит настройки режима прохождения сценария.
    /// Определяет правила обработки входящих данных: использование подсказок, озвучки,
    /// накопление штрафов и реакцию на ошибки разных типов.
    /// Для каждого режима создаётся отдельный экземпляр этого класса.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioModePolicy", menuName = "ScenarioSystem/ScenarioModePolicy")]
    public class ScenarioModePolicy : ScriptableObject
    {
       
    }
}
