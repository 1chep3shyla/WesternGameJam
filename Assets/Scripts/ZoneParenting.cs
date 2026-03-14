using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZoneParenting : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform parentWhenInside;

    private Transform originalParent;
    private Collider2D zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;

        if (target != null)
        {
            originalParent = target.parent;
        }

        if (parentWhenInside == null)
        {
            parentWhenInside = transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (target == null || other.transform != target)
        {
            return;
        }

        originalParent = target.parent;
        target.SetParent(parentWhenInside, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (target == null || other.transform != target)
        {
            return;
        }

        target.SetParent(originalParent, true);
    }
}
