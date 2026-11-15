using _Core._ScenarioSystem.Component;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Logger
{
    public class PrintResults : MonoBehaviour
    {
        [SerializeField] private LogSystem _logSystem;

        public void Print()
        {
            string result = "";
            for (int x = 0; x < _logSystem.LogComponents.Count; x++)
            {
                result += $"{(_logSystem.LogComponents[x].Type == ComponentType.Group ? "" : (_logSystem.LogComponents[x].Type == ComponentType.Step ? "\t" : "\t\t"))}" +
                          $"<color=cyan>{GetType(_logSystem.LogComponents[x].Type)}</color>" +
                          $"<color=yellow>[{GetState(_logSystem.LogComponents[x].State)}]</color>" +
                          $"<color=green>{_logSystem.LogComponents[x].Info}</color>\n";

                if (x % 50 == 0)
                {
                    Debug.Log(result);
                    result = "";
                }
            }
            Debug.Log(result);
        }
        
        private string GetType(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Group:       return "Group\t\t";
                case ComponentType.Step:        return "Step\t\t";
                case ComponentType.Condition:   return "Condition\t";
                default:                        return "Permanent\t";
            }
        }

        private string GetState(State state)
        {
            switch (state)
            {
                case State.Init:    return "Init\t\t";
                case State.Success: return "Success\t";
                case State.Failure: return "Failure\t";
                case State.Skipped: return "Skipped\t";
                default:            return "None\t";
            }
        }
    }
}
