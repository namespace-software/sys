using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextHints
{
	[TextArea(2, 10)]
	[Tooltip("Подсказка. Вызывается при нажатии на клавишу подсказки или из игрового планшета")]
	public string hintText;
	[TextArea(2, 10)]
	[Tooltip("Пояснение что конкретно было нарушено. Вызывается при провале шага")]
	public string descriptionMistake;
}