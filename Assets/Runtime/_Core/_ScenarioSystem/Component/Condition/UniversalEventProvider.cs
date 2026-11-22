using _Alex.Utils;
using UnityEngine;

namespace _Core._ScenarioSystem.Quest
{
    public class UniversalEventProvider : MonoBehaviour, IBooleanStateProvider
    {
        private StateWrapper<bool> _conditionReached = new StateWrapper<bool>(false);

        public StateWrapper<bool> GetStateWrapper()
        {
            return _conditionReached;
        }

        protected void SetValue(bool isSuccess)
        {
            _conditionReached.SetValue(isSuccess);
        }
    }
}
