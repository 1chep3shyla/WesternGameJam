using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Bounds")]
    [SerializeField] private Transform boundsCenter;
    [SerializeField] private Vector2 boundsSize = new Vector2(20f, 12f);

    [Header("Target Physics")]
    [SerializeField] private bool forceInterpolation = true;

    private Rigidbody2D targetRigidbody;
    private Vector3 velocity;

    private void Awake()
    {
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody2D>();
            if (forceInterpolation && targetRigidbody != null && targetRigidbody.interpolation == RigidbodyInterpolation2D.None)
            {
                targetRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = (targetRigidbody != null ? (Vector3)targetRigidbody.position : target.position) + offset;
        targetPosition = ClampToBounds(targetPosition);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (boundsCenter == null)
        {
            return position;
        }

        Vector3 center = boundsCenter.position;
        float halfX = boundsSize.x * 0.5f;
        float halfY = boundsSize.y * 0.5f;

        position.x = Mathf.Clamp(position.x, center.x - halfX, center.x + halfX);
        position.y = Mathf.Clamp(position.y, center.y - halfY, center.y + halfY);
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (boundsCenter == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boundsCenter.position, new Vector3(boundsSize.x, boundsSize.y, 0f));
    }
}
