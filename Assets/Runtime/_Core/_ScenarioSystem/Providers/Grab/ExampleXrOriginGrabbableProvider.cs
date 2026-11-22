// using PromVR.ScenarioSystem._Core._ScenarioSystem.Providers;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactables;
/// <summary>
/// Пример использования Grab Provider Base для взаимодействия с захватом рига XR Origin
/// </summary>
// public class ExampleXrOriginGrabbableProvider : GrabProviderBase
// {
//     [SerializeField] private XRGrabInteractable _xrInteractable;
//
//     private void Awake()
//     {
//         if (_xrInteractable == null)
//             _xrInteractable = GetComponent<XRGrabInteractable>();
//     }
//
//     protected override void Initialize()
//     {
//         _xrInteractable.activated.AddListener((ActivateEventArgs args) =>
//             OnGrabbed?.Invoke(args.interactorObject.transform));
//         _xrInteractable.deactivated.AddListener((DeactivateEventArgs args) =>
//             OnReleased?.Invoke(args.interactorObject.transform));
//         _xrInteractable.hoverEntered.AddListener((HoverEnterEventArgs args) =>
//             OnHoverEnter?.Invoke(args.interactorObject.transform));
//         _xrInteractable.hoverExited.AddListener((HoverExitEventArgs args) =>
//             OnHoverExit?.Invoke(args.interactorObject.transform));
//     }
//
//     protected override void Dispose()
//     {
//         _xrInteractable.activated.RemoveListener((ActivateEventArgs args) =>
//             OnGrabbed?.Invoke(args.interactorObject.transform));
//         _xrInteractable.deactivated.RemoveListener((DeactivateEventArgs args) =>
//             OnReleased?.Invoke(args.interactorObject.transform));
//         _xrInteractable.hoverEntered.RemoveListener((HoverEnterEventArgs args) =>
//             OnHoverEnter?.Invoke(args.interactorObject.transform));
//         _xrInteractable.hoverExited.RemoveListener((HoverExitEventArgs args) =>
//             OnHoverExit?.Invoke(args.interactorObject.transform));
//     }
//
//     public override void ForceRelease()
//     {
//
//     }
// }
