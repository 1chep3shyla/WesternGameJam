using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private string fireButton = "Fire1";
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float projectileRotationOffset;

    private float nextFireTime;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (projectilePrefab == null || firePoint == null || targetCamera == null)
        {
            return;
        }

        if (!Input.GetButton(fireButton))
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -targetCamera.transform.position.z;
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(mousePosition);
        Vector2 direction = (worldPosition - firePoint.position);
        direction.Normalize();

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle + projectileRotationOffset, Vector3.forward);

        nextFireTime = Time.time + fireRate;
    }
}
