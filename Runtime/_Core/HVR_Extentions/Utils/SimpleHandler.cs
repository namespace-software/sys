using UnityEngine;

public class SimpleHandler : MonoBehaviour
{
    [SerializeField] private EnumControllerType _controllerType;

    private SignalsInterpreterBase _signalsInterpreter;
    private SoundComponent _soundComponent;

    private void Awake()
    {
        _signalsInterpreter = FindFirstObjectByType<SignalsInterpreterBase>();

        _soundComponent = GetComponent<SoundComponent>();
    }

    public void SendSignal(bool result)
    {
        int resultIndex = result ? 1 : 0;
        _signalsInterpreter.OnSignalResieved?.Invoke(_controllerType, resultIndex, Vector2.zero, false);

        if (_soundComponent != null) 
            _soundComponent.PlayShort();
    }
}
