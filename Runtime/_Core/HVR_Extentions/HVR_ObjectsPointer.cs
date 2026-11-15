using UnityEngine;

public class HVR_ObjectsPointer : MonoBehaviour
{
    [SerializeField] private float rayLength = 2f;
    [SerializeField] private LayerMask layerMask;

    private LineRenderer _lineRenderer;
    [SerializeField] private HVR_InteractionHelper _interactionHelper;

    [SerializeField] private bool _isActive = false;

    public bool IsActive { get => _isActive; set => _isActive = value; }
    public HVR_InteractionHelper InteractionHelper { get => _interactionHelper; }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (!_isActive)
        {
            _lineRenderer.enabled = false;
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayLength, layerMask))
        {
            //Debug.LogError("Pointer! " + hit.transform.name + " | " + hit.transform.gameObject.layer);
            _lineRenderer.enabled = true;

            _lineRenderer.SetPosition(0, ray.origin);
            _lineRenderer.SetPosition(1, hit.point);

            HVR_InteractionHelper hitHelper = hit.collider.GetComponentInParent<HVR_InteractionHelper>();

            if (_interactionHelper != null && _interactionHelper != hitHelper && _interactionHelper._isPointered)
                _interactionHelper.PointerExit();

            _interactionHelper = hitHelper;
            if (_interactionHelper != null && !_interactionHelper._isPointered)
                _interactionHelper.PointerEnter();
        }
        else
        {
            _lineRenderer.enabled = false;
            _lineRenderer.SetPosition(0, ray.origin);
            _lineRenderer.SetPosition(1, ray.origin + ray.direction * rayLength);

            if (_interactionHelper != null && _interactionHelper._isPointered)
            {
                _interactionHelper.PointerExit();
                _interactionHelper = null;
            }
        }
    }
}
