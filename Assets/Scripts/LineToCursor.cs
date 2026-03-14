using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineToCursor : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool hideCursor = true;
    [SerializeField] private Transform cursorFollower;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        Cursor.visible = !hideCursor;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (origin == null || targetCamera == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -targetCamera.transform.position.z;
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = origin.position.z;

        if (cursorFollower != null)
        {
            cursorFollower.position = worldPosition;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, origin.position);
        lineRenderer.SetPosition(1, worldPosition);
    }
}
