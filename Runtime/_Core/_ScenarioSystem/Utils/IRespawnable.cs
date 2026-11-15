using UnityEngine;

public interface IRespawnable
{
    void SavePoint();
    void RespawnToPrevious();
    void RespawnToPoint(Transform pointTransform);
    void Hold();
    void UnHold();
}
