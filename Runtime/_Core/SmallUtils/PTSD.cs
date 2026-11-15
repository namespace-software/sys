using System.Collections;
using System.Collections.Generic;
using _Alex.Utils;
using UnityEngine;

public enum EnumTSDTask
{
    Accept = 0,
    Close = 1,
    Departure = 2,
    Arrive = 3
}


public class PTSD : MonoBehaviour, IBooleanStateProvider
{
    public StateWrapper<bool> _taskReached;

    public EnumTSDTask _awaitedTask = EnumTSDTask.Accept;
    [SerializeField] private bool _availableAutoSwitch;
    [Space(15)]

    [SerializeField] private GameObject _acceptBtnGO;
    [SerializeField] private GameObject _completeBtnGO;
    [SerializeField] private GameObject _departureBtnGO;
    [SerializeField] private GameObject _arriveBtnGO;

    private void Awake()
    {
        _taskReached = new StateWrapper<bool>(false);
    }

    public void ChangeAwaitedTask(EnumTSDTask nextTask)
    {
        //_awaitedTask = nextTask;
    }

    public void AcceptTask()
    {
        SendResult(_awaitedTask == EnumTSDTask.Accept);
    }

    public void CloseTask()
    {
        SendResult(_awaitedTask == EnumTSDTask.Close);
    }

    public void Departure()
    {
        SendResult(_awaitedTask == EnumTSDTask.Departure);
    }

    public void Arrive()
    {
        SendResult(_awaitedTask == EnumTSDTask.Arrive);
    }

    private void SendResult(bool result)
    {
        _taskReached.SetValue(result);

        if (result) SwitchToNextTask();
        else Debug.LogError("Big PTSD mistake!");
    }

    public void SwitchNextTo(int nextIndex)
    {
        _awaitedTask = (EnumTSDTask)nextIndex;
    }

    private void SwitchToNextTask()
    {
        switch (_awaitedTask)
        {
            case EnumTSDTask.Accept:
                _acceptBtnGO.SetActive(false);
                _completeBtnGO.SetActive(true);
                _departureBtnGO.SetActive(true);
                _arriveBtnGO.SetActive(false);

                if (_availableAutoSwitch) _awaitedTask = EnumTSDTask.Departure;
                break;
            case EnumTSDTask.Close:
                _acceptBtnGO.SetActive(false);
                _departureBtnGO.SetActive(false);
                _arriveBtnGO.SetActive(false);


                break;
            case EnumTSDTask.Departure:
                _acceptBtnGO.SetActive(false);
                _completeBtnGO.SetActive(true);
                _departureBtnGO.SetActive(false);
                _arriveBtnGO.SetActive(true);

                if (_availableAutoSwitch) _awaitedTask = EnumTSDTask.Arrive;
                break;
            case EnumTSDTask.Arrive:
                _acceptBtnGO.SetActive(false);
                _completeBtnGO.SetActive(true);
                _departureBtnGO.SetActive(false);
                _arriveBtnGO.SetActive(false);

                if (_availableAutoSwitch) _awaitedTask = EnumTSDTask.Close;
                break;
            default:
                break;
        }
    }

    public StateWrapper<bool> GetStateWrapper()
    {
        return _taskReached;
    }
}
