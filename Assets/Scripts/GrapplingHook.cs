using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class GrapplingHook : MonoBehaviour
{
    [SerializeField] private KeyCode hookKey = KeyCode.E;
    [SerializeField] private LayerMask grappleMask;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float pullForce = 18f;
    [SerializeField] private float grapplingDrag = 2f;

    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    private bool isGrappling;
    private float defaultDrag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        defaultDrag = rb.drag;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(hookKey))
        {
            if (isGrappling)
            {
                StopGrapple();
            }
            else
            {
                TryStartGrapple();
            }
        }

        if (isGrappling && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, joint.connectedAnchor);
        }
    }

    private void FixedUpdate()
    {
        if (!isGrappling)
        {
            return;
        }

        float currentDistance = Vector2.Distance(rb.position, joint.connectedAnchor);
        if (currentDistance > maxDistance)
        {
            StopGrapple();
            return;
        }

        Vector2 direction = (joint.connectedAnchor - rb.position).normalized;
        rb.AddForce(direction * pullForce, ForceMode2D.Force);
    }

    private void TryStartGrapple()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -targetCamera.transform.position.z;
        Vector2 targetPoint = targetCamera.ScreenToWorldPoint(mousePosition);
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, grappleMask);
        if (!hit)
        {
            return;
        }

        joint.connectedAnchor = hit.point;
        joint.distance = Vector2.Distance(transform.position, hit.point);
        joint.enabled = true;
        isGrappling = true;
        rb.drag = grapplingDrag;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        joint.enabled = false;
        rb.drag = defaultDrag;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
