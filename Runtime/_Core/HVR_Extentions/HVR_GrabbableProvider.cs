using System;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using PromVR.ScenarioSystem._Core._ScenarioSystem.Providers;
using UnityEngine;

namespace PromVR.ScenarioSystem._Core.HVR_Extentions
{
    [Obsolete("Этот класс будет вынесен в отдельный пакет для HVR. Будет удалён ориентировочно в 1.2.0")]
    public class Hvr_GrabbableProvider : GrabProviderBase
    {
        [SerializeField] private HVRGrabbable _hvrGrabbable;

        public override bool IsLeftHandGrabbed => 
            _hvrGrabbable != null && _hvrGrabbable.IsLeftHandGrabbed;

        public override bool IsRightHandGrabbed => 
            _hvrGrabbable != null && _hvrGrabbable.IsRightHandGrabbed;

        private void Awake()
        {
            if (_hvrGrabbable == null)
                _hvrGrabbable = GetComponent<HVRGrabbable>();
        }

        protected override void Initialize()
        {
            _hvrGrabbable.Grabbed.AddListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnGrabbed?.Invoke(grabber.transform));
            _hvrGrabbable.Released.AddListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnReleased?.Invoke(grabber.transform));
            _hvrGrabbable.HoverEnter.AddListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnHoverEnter?.Invoke(grabber.transform));
            _hvrGrabbable.HoverExit.AddListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnHoverExit?.Invoke(grabber.transform));
        }

        protected override void Dispose()
        {
            _hvrGrabbable.Grabbed.RemoveListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnGrabbed?.Invoke(grabber.transform));
            _hvrGrabbable.Released.RemoveListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnReleased?.Invoke(grabber.transform));
            _hvrGrabbable.HoverEnter.RemoveListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnHoverEnter?.Invoke(grabber.transform));
            _hvrGrabbable.HoverExit.RemoveListener((HVRGrabberBase grabber, HVRGrabbable grabbable) => 
                OnHoverExit?.Invoke(grabber.transform));
        }

        public override void ForceRelease()
        {
            _hvrGrabbable.ForceRelease();
        }
    }
}
