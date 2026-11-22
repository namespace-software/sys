using UnityEngine;
using UnityEngine.Events;

namespace ScenarioSystems
{
    public class ManualScenarioController : MonoBehaviour
    {
        [SerializeField] private bool _debugMode;
        //[SerializeField] private bool _prohibitSpeeding;
        //[SerializeField] private bool _prohibitOutZone;
        //[SerializeField] private bool _prohibitIntersection;
        //[SerializeField] private bool _prohibitOncomingLane;
        [Space(10)]
        [SerializeField] private _Core._ScenarioSystem.ScenarioSystem _scenarioSystem; 
         
        public UnityEvent OnDebugAwake;
        public UnityEvent OnDebugStart;

        private void Awake()
        {
            if (!_debugMode) return;

            OnDebugAwake?.Invoke();
        }

        private void Start()
        {
            if (!_debugMode) return;

            //_scenarioSystem._testSpeedingDisable = _prohibitSpeeding;
            //_scenarioSystem._testOutZoneDisable = _prohibitOutZone;
            //_scenarioSystem._testIntersectionDisable = _prohibitIntersection;
            //_scenarioSystem._testOncomingLane = _prohibitOncomingLane;

            OnDebugStart?.Invoke();
        }

#if UNITY_EDITOR

        private void Update()
        {
            InputListener();
        }

        private void InputListener()
        {
            if (Input.GetKeyDown(KeyCode.N)) _scenarioSystem.SkipStage();
        }
#endif
    }
}
