using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioResettable
{
    void SaveCurrentState();
    void ResetToPreviousState();
}
