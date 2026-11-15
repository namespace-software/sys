using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HighlightPlus;

public class HandWatch : MonoBehaviour
{
    public Action OnHandCollision;

    [SerializeField] private GameObject _canvasGO;
    [SerializeField] private Transform _rightHandTransform;
    [SerializeField] private float _distanceToDetect;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private TextMeshProUGUI _additionalTimeField;
    [SerializeField] private HighlightEffect _effect;
    [SerializeField] private Image _backImage;

    private bool _isHandNearby;
    private bool _isProperDistance;
    private bool _isAlarmMode = false;
    private bool _isHaveAdditionalTimeField;

    public bool IsHandNearby { get => _isHandNearby; }
    public bool IsinteractionProhibited;

    private void Awake()
    {
        _isHaveAdditionalTimeField = _additionalTimeField != null;
    }

    //private void Start()
    //{
    //    StartCoroutine(WatchClockCoroutine());
    //}

    private void Update()
    {
        if (_rightHandTransform != null) _isProperDistance = Vector3.Distance(_canvasGO.transform.position, _rightHandTransform.position) <= _distanceToDetect;
        if (_effect != null) _effect.enabled = _isHandNearby;

        if (_isProperDistance && !_isHandNearby) OnHandCollision?.Invoke();
        _isHandNearby = _isProperDistance;

        _textMeshProUGUI.text = DateTime.Now.ToString("HH:mm");
        if (_isHaveAdditionalTimeField) _additionalTimeField.text = DateTime.Now.ToString("HH:mm:ss");
    }

    public void ChangeTimer(bool withAlarm, int time)
    {
        _isAlarmMode = true;
        if (withAlarm) _backImage.color = Color.red;
        _textMeshProUGUI.text = time.ToString();
    }

    //private IEnumerator WatchClockCoroutine()
    //{
    //    while (!_isAlarmMode)
    //    {
    //        _textMeshProUGUI.text = DateTime.Now.ToString("HH:mm");
    //        yield return null;
    //    }
    //}

    public void SwitchParent(Transform parent, Transform target)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        _rightHandTransform = target;
    }
}
