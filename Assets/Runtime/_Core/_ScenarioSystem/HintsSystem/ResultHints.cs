using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ResultHints
{
    public AudioClip mistakeClip;

    [TextArea(2, 10)]
    [Tooltip("Что сделано с ошибкой")]
    public string mistakeText;
    //[Space(10)]

    //public AudioClip orderClip;

    //[TextArea(2, 10)]
    //[Tooltip("Нарушен порядок")]
    //public string orderText;
}
