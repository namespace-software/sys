using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Базовый класс данных инструкции: текст и озвучка.
/// </summary>
[System.Serializable]
public class InstructionDataBase
{
    [HorizontalGroup("InstructionAudio", Width = 0.7f)]
    [Tooltip("Аудио инструкции, что сделать")]
    public AudioClip instructionClip;

    [LabelText("Instruction Text")]
    [TextArea(2, 10)]
    [Tooltip("Текст инструкции, что сделать")]
    public string instructionText;
}

/// <summary>
/// Инструкция уровня шага с задержкой начала проигрывания.
/// </summary>
[System.Serializable]
public class InstructionDataStep : InstructionDataBase
{
    [HorizontalGroup("InstructionAudio", Width = 0.3f)]
    [LabelText("Play Delay")]
    [LabelWidth(80)]
    [Tooltip("Задержка перед началом проигрывания")]
    public float instructionPlayDelay = 0;
}

/// <summary>
/// Инструкция уровня condition с задержкой повтора озвучки.
/// </summary>
[System.Serializable]
public class InstructionDataCondition : InstructionDataBase
{
    [HorizontalGroup("InstructionAudio", Width = 0.3f)]
    [LabelText("Repeat Delay")]
    [LabelWidth(80)]
    [Tooltip("Задержка между повторами")]
    public float instructionRepeatDelay = 30;
}