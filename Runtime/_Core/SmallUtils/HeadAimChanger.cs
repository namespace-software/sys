using UnityEngine;

public class HeadAimChanger : MonoBehaviour
{
    [SerializeField] private Transform aimConstraint;
    private float _height_Y;

    public void ChangeHeadAimTarget(Transform target, Vector3 pathForward)
    {
        BotPathPoint pathPoint = target.GetComponent<BotPathPoint>();
        _height_Y = 0;

        if (pathPoint)
        {
            switch (pathPoint.LookPoint)
            {
                case EnumLookPoint.NoPoint:
                    return;
                case EnumLookPoint.Point:
                    pathForward = Vector3.zero;
                    _height_Y = 0;
                    break;
                case EnumLookPoint.Forward:
                    pathForward *= pathPoint.ForwardDistance;
                    _height_Y = pathPoint.LookHeight;
                    break;
                default:
                    break;
            }
        }

        aimConstraint.SetParent(target);
        aimConstraint.localPosition = Vector3.zero;
        aimConstraint.localEulerAngles = Vector3.zero;

        aimConstraint.position += pathForward; //Меняем точку линии взгляда, добавляем вектор вперёд
        aimConstraint.localPosition = new Vector3(aimConstraint.localPosition.x, _height_Y, aimConstraint.localPosition.z); //Меняем высоту линии взгляда
    }
}
