using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Alex.Utils;
using _Alex.Sound;

public class BackMovingController : MonoBehaviour, IBooleanStateProvider
{
    [Header("������ �� ��������� ���������")]
    [SerializeField] private Transform _endPositionTarget;
    [SerializeField] private Transform _vehicleTransform;
    [Space(10)]

    [Header("��������� ��������")]
    [SerializeField] private float _connectionAngleThreshold;
    [SerializeField] private float _connectionDistanceThreshold_X;
    [SerializeField] private float _driveAwayDistance;
    [Space(10)]

    [Header("Debug")]
    [SerializeField] private bool _debugMode;
    //[SerializeField] private float _connectionDistance_Z;
    //[SerializeField] private float _connectionDistance_X;
    // [SerializeField] private EnumTurnSignalType _targetAngleSide;
    // [SerializeField] private EnumTurnSignalType _resultSide;
    // [SerializeField] private EnumTurnSignalType _prevAnimationSide;
    [Space(10)]
    [SerializeField] private bool _connectionFullComplete;
    [SerializeField] private bool _moveAwayProcess;

    //[SerializeField] private float difference;
    [SerializeField] private float angleDifference;

    public StateWrapper<bool> _connectionComplete;
    public bool ConnectionComplete => _connectionComplete.Value;

    private Vector3 _startPoint;
    //private AnimationSystem _animationSystem;

    private void Awake()
    {
        _connectionComplete = new StateWrapper<bool>(false);
        //_animationSystem = FindFirstObjectByType<AnimationSystem>();
    }

    private void Start()
    {
        if (_debugMode && Input.GetKeyDown(KeyCode.Q)) StartBackMovingProcessHandler();
    }

    private void Update()
    {
        if (!_moveAwayProcess && _debugMode && Input.GetKeyDown(KeyCode.Q)) StartBackMovingProcessHandler();
    }

    public void StartBackMovingProcessHandler()
    {
        _moveAwayProcess = true;
        StartCoroutine(DriveAwayLogicCoroutine());
        StartCoroutine(MoveBackProcessHandlerCoroutine());
    }

    public void StopBackMovingProcessHandler()
    {
        StopAllCoroutines();
        //_animationSystem.Cargo_OPRRAnimationFullReset();
        AnimationFullReset();
        //_animationSystem.ContainerBotStopGesture();
    }

    public void StartMovingToTZone()
    {
        AnimationFullReset();
        //_animationSystem.OPRR_Containers_Animator.SetBool("Right", true);
    }

    public void Ending()
    {
        AnimationFullReset();
        //_animationSystem.ContainerBotStopGesture();
    }

    public StateWrapper<bool> GetStateWrapper()
    {
        return _connectionComplete;
    }

    /// <summary>
    /// ������ �������� � ��. ����� �������� ������� ��������.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveBackProcessHandlerCoroutine()
    {
        while (!_connectionFullComplete)
        {
            if (_moveAwayProcess) yield return new WaitUntil(() => !_moveAwayProcess);

            CheckTargetAngleSide();
            AnimationsController();

            yield return null;
        }
    }

    private void CheckTargetAngleSide()
    {
        Vector3 forwardA = _vehicleTransform.forward;
        Vector3 forwardB = _endPositionTarget.position - _vehicleTransform.position;
        forwardB.y = 0;

        // ���������� ���� ����� ���������
        angleDifference = Vector3.Angle(forwardA, forwardB);

        // ����� ����������� ���������� (����� ��� ������)
        Vector3 cross = Vector3.Cross(forwardA, forwardB);

        // ���� ���� ������ ��� ����� 2 ��������, ������� ��� ������� ������� ���� �� �����
        if (Mathf.Abs(angleDifference) <= _connectionAngleThreshold)
        {
            // _targetAngleSide = EnumTurnSignalType.None;
            // _resultSide = EnumTurnSignalType.None;
        }
        else
        {
            // ����������� ����������� ��������
            if (cross.y < 0)
            {
                //Debug.LogError("����: " + angleDifference + " �������� ������");
                // _targetAngleSide = EnumTurnSignalType.Right;
                // _resultSide = EnumTurnSignalType.Right;
            }
            else if (cross.y > 0)
            {
                //Debug.LogError("����: " + angleDifference + " �������� �����");
                // _targetAngleSide = EnumTurnSignalType.Left;
                // _resultSide = EnumTurnSignalType.Left;
            }

            //// ����������� ����������� ��������
            //if (_1_AcasDistance < _3_AcasDistance)
            //{
            //    _targetAngleSide = EnumTurnSignalType.Right;
            //}
            //else
            //{
            //    _targetAngleSide = EnumTurnSignalType.Left;
            //}
        }
    }

    private IEnumerator DriveAwayLogicCoroutine()
    {
        _moveAwayProcess = true;
        _startPoint = _vehicleTransform.position;
        AnimationFullReset();
        //_animationSystem.OPRR_Containers_Animator.SetBool("MoveBackGesture", true);

        while (Vector3.Distance(_vehicleTransform.position, _startPoint) <= _driveAwayDistance)
        {
            yield return null;
        }

        _moveAwayProcess = false;
    }

    private void AnimationsController()
    {
        //if (_prevAnimationSide == _resultSide) return;

        //AnimationFullReset();
        //_prevAnimationSide = _resultSide;

        //if (_resultSide == EnumTurnSignalType.Right) _animationSystem.OPRR_Containers_Animator.SetBool("Right", true);
        //else if (_resultSide == EnumTurnSignalType.Left) _animationSystem.OPRR_Containers_Animator.SetBool("Left", true);
        //else _animationSystem.OPRR_Containers_Animator.SetBool("MoveBackGesture", true);
    }

    private void AnimationFullReset()
    {
        //_animationSystem.OPRR_Containers_Animator.SetBool("Right", false);
        //_animationSystem.OPRR_Containers_Animator.SetBool("Left", false);
        //_animationSystem.OPRR_Containers_Animator.SetBool("MoveBackGesture", false);
        //_animationSystem.OPRR_Containers_Animator.SetBool("Ok", false);
    }
}
