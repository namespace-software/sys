using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Alex.Utils;

public interface IBooleanStateProvider
{
    StateWrapper<bool> GetStateWrapper();
}
