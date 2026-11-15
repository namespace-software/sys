using UnityEngine;
using TMPro;

public class SpeedometerDisplay : MonoBehaviour
{
    private TextMeshProUGUI _meshProUGUI;

    private void Awake()
    {
        _meshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateSpeedDisplay(float _speed)
    {
        _meshProUGUI.text = _speed.ToString();
    }
}
