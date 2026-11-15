using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    public float wheelRadius = 0.35f; // Радиус колеса в метрах
    private float wheelCircumference; // Периметр колеса
    private Vector3 lastPosition; // Последняя мировая позиция родителя колеса

    [SerializeField] private Transform parentTransform; // Трансформ родительского объекта (например, шасси)

    void Start()
    {
        wheelCircumference = 2 * Mathf.PI * wheelRadius; // Вычисляем периметр колеса
        if (parentTransform == null) parentTransform = transform.parent; // Получаем родителя (шасси)
        lastPosition = parentTransform.position; // Запоминаем начальную позицию родителя
    }

    void Update()
    {
        // Расчитываем пройденное расстояние
        Vector3 currentPosition = parentTransform.position;
        float distanceTravelled = Vector3.Distance(currentPosition, lastPosition);

        // Определяем направление движения
        Vector3 movementDirection = currentPosition - lastPosition;
        float directionMultiplier = Vector3.Dot(movementDirection, parentTransform.forward) > 0 ? 1 : -1;

        // Обновляем угол поворота колеса
        float angle = directionMultiplier * (distanceTravelled / wheelCircumference) * 360.0f;
        transform.Rotate(Vector3.right, angle, Space.Self); // Вращаем колесо в локальной системе координат

        // Обновляем последнюю позицию родителя
        lastPosition = currentPosition;
    }
}
