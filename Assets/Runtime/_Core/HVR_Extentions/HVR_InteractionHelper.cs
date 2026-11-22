using System.Collections;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.ControllerInput;
using UnityEngine.Events;
using HighlightPlus;
using PromVR.ScenarioSystem._Core._ScenarioSystem.Providers;
using System.Linq;
using HurricaneVR.Framework.Shared;


public class HVR_InteractionHelper : MonoBehaviour
{
    //[SerializeField] private Outline _outline;
    [SerializeField] private HighlightEffect _highlight;
    [SerializeField] private bool _activateManual;
    [SerializeField] private bool _deactivateOutlineManual;
    [SerializeField] private bool _inactiveAfterUsing;
    [SerializeField] private bool _alwaysHighlited;
    [SerializeField] private bool _leapGrabbed = true;
    [SerializeField] private bool _autoSoundProhibited;
    [SerializeField] private bool _oneInteractionLeapRelease;

    //public UnityEvent<bool> OnAnyInteraction;
    public UnityEvent OnGrabbedEvent;
    public UnityEvent OnReleasedEvent;
    public UnityEvent OnPointerEnter;
    public UnityEvent OnPointerExit;

    //public UnityEvent OnGrab;
    //public UnityEvent OnRelease;

    public bool _isPointered;
    public bool _isGrabbed;
    public bool _isActive;

    //[SerializeField] private EnumInteractionType _interactionType;
    [SerializeField] private Transform _touchPivot;

    [SerializeField] private GrabProviderBase _grabbable;

    private HVRTrackedController[] _trackedControllers;

    private Transform _rightControllerTransform;
    private Transform _leftControllerTransform;

    private HVRControllerEvents _hVRControllerEvents;
    private SoundComponent _soundComponent;
    private Collider _collider;

    //public bool DeactivateOutlineManual { get => _deactivateOutlineManual; set => _deactivateOutlineManual = value; }

    public bool DeactivateOutlineManual
    {
        get => _deactivateOutlineManual;
        set
        {
            if (value == true)
            {
                _deactivateOutlineManual = value;
            }
            else
            {
                if (!_inactiveAfterUsing) _deactivateOutlineManual = false;
            }
        }
    }


    public Transform RightControllerTransform
    {
        get => _rightControllerTransform;
    }

    public Transform LeftControllerTransform
    {
        get => _leftControllerTransform;
    }

    public bool InactiveAfterUsing
    {
        get => _inactiveAfterUsing;
    }

    //public bool AlwaysLeapInteractable { get => _alwaysLeapInteractable; }
    //public Outline Outline { get => _highlight; }
    public Collider Collider => _collider;

    public bool LeapGrabbed
    {
        get => _leapGrabbed;
    }

    // private void Awake()
    // {
    //     _hVRControllerEvents = FindFirstObjectByType<HVRControllerEvents>();
    //
    //     InitComponents();
    // }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);

        _hVRControllerEvents = FindFirstObjectByType<HVRControllerEvents>();
        InitComponents();
    }

    public void HighlightActivate(bool active)
    {
        if (!_highlight)
        {
            Debug.LogError("Object has no Highlight component!", transform);
            return;
        }

        _highlight.highlighted = active;
    }

    //private void OnEnable()
    //{
    //    OnAnyInteraction.AddListener(OutlineDisable);
    //}

    //private void OnDisable()
    //{
    //    OnAnyInteraction.RemoveAllListeners();
    //}

    private void InitComponents()
    {
        _soundComponent = GetComponent<SoundComponent>();
        _collider = GetComponent<Collider>();

        if (_grabbable == null) 
            _grabbable = GetComponent<GrabProviderBase>();

        FindControllers();

        _isActive = !_activateManual;

        if (_grabbable != null)
        {
            _grabbable.OnGrabbed += OnGrabbed;
            _grabbable.OnReleased += OnReleased;
            _grabbable.OnHoverEnter += OnHVRHoverEnter;
            _grabbable.OnHoverExit += OnHVRHoverExit;
        }

        if (_highlight == null) _highlight = GetComponent<HighlightEffect>();
    }

    public void FindControllers()
    {
        _trackedControllers = FindObjectsOfType<HVRTrackedController>();

        var rightController =
            _trackedControllers.FirstOrDefault(controller => controller.HandSide == HVRHandSide.Right);
        if (rightController != null) _rightControllerTransform = rightController.transform;
        else Debug.LogError("Right controller not found");

        var leftController = _trackedControllers.FirstOrDefault(controller => controller.HandSide == HVRHandSide.Left);
        if (leftController != null) _leftControllerTransform = leftController.transform;
        else Debug.LogError("Left controller not found");
    }

    private void OnGrabbed(Transform grabber)
    {
        _isGrabbed = true;

        OnGrabbedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
        //Debug.LogError("OnGrabbed");
    }

    private void OnReleased(Transform grabber)
    {
        _isGrabbed = false;
        //if (_isActive)
        //{
        //    //foreach (var item in _controllerMoveHandlers)
        //    //{
        //    //    item.HandlingOff();
        //    //}
        //}


        OnReleasedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
    }

    public Transform _leapTransform;

    public void LeapGrab(Transform leapTransform)
    {
        _leapTransform = leapTransform;
        _isGrabbed = true;
        

        OnGrabbedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
    }

    public void LeapRelease()
    {
        _isGrabbed = false;


        OnReleasedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
    }

    public void LeapPointerTrigger()
    {
        //OnAnyInteraction?.Invoke(true);
        OnGrabbedEvent?.Invoke();
    }

    public void PointerEnter()
    {
        if (!_deactivateOutlineManual || _alwaysHighlited) HighlightActivate(true);
        _isPointered = true;
        OnPointerEnter?.Invoke();

        _hVRControllerEvents.RightTriggerActivated.AddListener(Grab);
        //if (transform.tag == "Debug") Debug.LogError("PointerEnter", transform);
    }

    public void PointerExit()
    {
        if (!_deactivateOutlineManual || _alwaysHighlited) HighlightActivate(false);
        _isPointered = false;
        _isGrabbed = false;
        OnPointerExit?.Invoke();

        _hVRControllerEvents.RightTriggerActivated.RemoveAllListeners();
        //if (transform.tag == "Debug") Debug.LogError("PointerExit", transform);
    }

    public void Grab()
    {
        _isGrabbed = true;
        OnGrabbedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
        //if (_soundComponent != null) _soundComponent.PlayShort();
    }

    public void Release()
    {
        _isGrabbed = false;
        OnReleasedEvent?.Invoke();
        //OnAnyInteraction?.Invoke(_isGrabbed);
    }

    public void OutlineDisable(bool value)
    {
        if (value && _inactiveAfterUsing)
        {
            if (_soundComponent != null && !_autoSoundProhibited) _soundComponent.PlayShort();
            HighlightActivate(false);
            _highlight = null;
            //if (_collider != null) _collider.enabled = false;
        }
    }

    private void OnHVRHoverEnter(Transform grabber)
    {
        if (!_deactivateOutlineManual) HighlightActivate(true);
        //Debug.LogError("OnHVRHoverEnter");
    }

    private void OnHVRHoverExit(Transform grabber)
    {
        if (!_deactivateOutlineManual) HighlightActivate(false);
        //Debug.LogError("OnHVRHoverExit");
    }

    public void GrabberBlock(bool value)
    {
        if (value && _isGrabbed && _grabbable != null)
        {
            _grabbable.ForceRelease();
        }

        _isActive = !value;
    }
}
