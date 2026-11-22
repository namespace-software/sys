using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Core.Player;
using UnityEngine;

public class CustomPlayerInputs : HVRPlayerInputs
{
    public bool SmoothMovementEnabled = false;
    public bool TeleportEnabled = true;

    private void Start()
    {
        HVRPlayerController player = GetComponent<HVRPlayerController>();
        if (player == null)
            return;

        // Префаб лобби держит оба флага включёнными. Не включаем скольжение,
        // пока сессия явно не выбрала только Smooth.
        if (SmoothMovementEnabled && TeleportEnabled)
            player.MovementEnabled = false;
        else
            player.MovementEnabled = SmoothMovementEnabled;
    }

    protected override Vector2 GetMovementAxis()
    {
        if (!SmoothMovementEnabled)
            return Vector2.zero;

        return base.GetMovementAxis();
    }

    protected override bool GetTeleportActivated()
    {
        if (!TeleportEnabled)
            return false;

        return base.GetTeleportActivated();
    }

    protected override bool GetTeleportDeactivated()
    {
        if (!TeleportEnabled)
            return true;

        return base.GetTeleportDeactivated();
    }

    protected override void AfterInputUpdate()
    {
        if (!SmoothMovementEnabled)
            MovementAxis = Vector2.zero;
    }
}
