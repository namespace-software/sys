using _Core._ScenarioSystem.Quest;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class SocketFilledProvider : UniversalEventProvider
{
    [SerializeField] private HVRSocket _socket;

    private void Awake()
    {
        _socket.Grabbed.AddListener(OnGrabbed);
        _socket.Released.AddListener(OnReleased);
        SetValue(_socket.GrabbedTarget != null);
    }

    private void OnDestroy()
    {
        _socket.Grabbed.RemoveListener(OnGrabbed);
        _socket.Released.RemoveListener(OnReleased);
    }

    private void OnGrabbed(HVRGrabberBase arg0, HVRGrabbable arg1)
    {
        SetValue(true);
    }

    private void OnReleased(HVRGrabberBase arg0, HVRGrabbable arg1)
    {
        SetValue(false);
    }
}
