using HurricaneVR.Framework.Core.Player;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    [Header("Hurricane VR")]
    [SerializeField] private HVRTeleporter teleporter;

    [Header("Teleport Target")]
    [SerializeField] private Transform targetPoint;

    [Header("Options")]
    [SerializeField] private bool useTargetForward = true;

    public void TeleportToTarget()
    {
        if (teleporter == null)
        {
            Debug.LogError("HVRTeleporter не назначен.", this);
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogError("TargetPoint не назначен.", this);
            return;
        }

        Vector3 position = targetPoint.position;

        Vector3 direction = useTargetForward
            ? targetPoint.forward
            : teleporter.transform.forward;

        direction.y = 0f;
        direction.Normalize();

        teleporter.Teleport(position, direction);
    }

    public void TeleportTo(Transform point)
    {
        if (point == null)
            return;

        Vector3 direction = point.forward;
        direction.y = 0f;
        direction.Normalize();

        teleporter.Teleport(point.position, direction);
    }

    public void TeleportToPosition(Vector3 position)
    {
        Vector3 direction = teleporter.transform.forward;
        direction.y = 0f;
        direction.Normalize();

        teleporter.Teleport(position, direction);
    }
}
