using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Этот класс устарел. Не используется. Будет удалён в 1.1.0")]
public class EventsHelper : EventsHelperBase
{
    public override void ConditionReached(bool isCorrect)
    {
        OnCompleteCondition?.Invoke(isCorrect);
    }
}
