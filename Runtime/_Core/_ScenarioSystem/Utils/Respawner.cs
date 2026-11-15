using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;


public class Respawner : MonoBehaviour
{
    [Title("Параметры респавна")]
    [SerializeField] private Transform _defaultSpawnPoint;
    [SerializeField] private float _respawnDelay;
    [SerializeField] private bool _needToFreezeBeforeRespawn;
    [Space(20)]

    [Title("Проверка вываливания/переворота")]
    [SerializeField] private bool _checkExtremumPositions;

    [ShowIf(nameof(_checkExtremumPositions))]
    [SerializeField] private float _extremumCheckFriquency = 2;

    [ShowIf(nameof(_checkExtremumPositions))]
    [SerializeField] private float _fallOut_Y_PositionThreshold = -10;

    [ShowIf(nameof(_checkExtremumPositions))]
    [SerializeField] private float _rolloverAngleThreshold = 35;
    [Space(20)]

    public UnityEvent BeforeRespawn;
    public UnityEvent AfterRespawn;

    private IRespawnable _respawnable;
    private Coroutine _respawnCoroutine;

    private void Awake()
    {
        _respawnable = GetComponent<IRespawnable>();
        if (_respawnable == null) { Debug.LogError("IRespawnable interface not found!", this); enabled = false; }
    }

    private void OnEnable()
    {
        if (!_needToFreezeBeforeRespawn) return;

        BeforeRespawn.AddListener(_respawnable.Hold);
        AfterRespawn.AddListener(_respawnable.UnHold);
    }

    private void OnDisable()
    {
        if (!_needToFreezeBeforeRespawn) return;

        BeforeRespawn.RemoveListener(_respawnable.Hold);
        AfterRespawn.RemoveListener(_respawnable.UnHold);
    }

    private void Start()
    {
        SaveCurrentPoint();
        if (_checkExtremumPositions) StartCoroutine(ExtremumPositionDetectCoroutine());
    }

    public void SaveCurrentPoint()
    {
        _respawnable?.SavePoint();
    }

    public void RespawnPrevious()
    {
        Debug.Log("RespawnPrevious()");
        if (_respawnCoroutine != null) return;
        _respawnCoroutine = StartCoroutine(RespawnToSavedCoroutine());
    }

    public void RespawnToPoint(Transform point)
    {
        Debug.Log("RespawnToPoint()");
        if (!point) { Debug.LogError("RespawnToPoint has no argument!", transform); return; }

        if (_respawnCoroutine != null) return;
        _respawnCoroutine = StartCoroutine(RespawnToPointCoroutine(point));
    }

    //public void RespawnToPoint(Vector3 position, Quaternion rotation)
    //{
    //    Debug.Log("RespawnToPoint()");

    //    if (_respawnCoroutine != null) return;
    //    _respawnCoroutine = StartCoroutine(RespawnToPointCoroutine(point));
    //}

    public void RespawnToDefault()
    {
        Debug.Log("RespawnToDefault()");
        if (!_defaultSpawnPoint) { Debug.LogError("Respawner has no default spawn point!", transform); return; }

        if (_respawnCoroutine != null) return;
        _respawnCoroutine = StartCoroutine(RespawnToPointCoroutine(_defaultSpawnPoint));
    }

    private IEnumerator RespawnToPointCoroutine(Transform point)
    {
        BeforeRespawn?.Invoke();

        yield return new WaitForSeconds(_respawnDelay);
        _respawnable?.RespawnToPoint(point);

        AfterRespawn?.Invoke();
        _respawnCoroutine = null;
    }

    private IEnumerator RespawnToSavedCoroutine()
    {
        BeforeRespawn?.Invoke();

        yield return new WaitForSeconds(_respawnDelay);
        _respawnable?.RespawnToPrevious();

        AfterRespawn?.Invoke();
        _respawnCoroutine = null;
    }

    private IEnumerator ExtremumPositionDetectCoroutine()
    {
        var wait = new WaitForSeconds(_extremumCheckFriquency);

        while (true)
        {
            yield return wait;

            if (_respawnCoroutine != null)
                continue;

            bool fallen = transform.position.y <= _fallOut_Y_PositionThreshold;

            // Угол между "верхом" машины и мировым Vector3.up
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            bool rolledOver = tiltAngle >= _rolloverAngleThreshold;

            if (fallen || rolledOver)
            {
                RespawnPrevious();
            }
        }
    }
}
