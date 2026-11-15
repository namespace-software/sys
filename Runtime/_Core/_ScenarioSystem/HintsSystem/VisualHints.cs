using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;
using Sirenix.OdinInspector;

[System.Serializable]
public class VisualHints
{
    [HorizontalGroup("Highlight Effect", Width = 0.6f)]
    public List<HighlightEffect> highlightEffects;

    [HorizontalGroup("Highlight Effect", Width = 0.4f)]
    [LabelText("Activate Anyway")]
    [LabelWidth(120)]
    public bool activateEffectsAnyway;

    [HorizontalGroup("Game Objects", Width = 0.6f)]
    public List<GameObject> gameObjects;

    [HorizontalGroup("Game Objects", Width = 0.4f)]
    [LabelText("Activate Anyway")]
    [LabelWidth(120)]
    public bool activateObjectsAnyway;
}
