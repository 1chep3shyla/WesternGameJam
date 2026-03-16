using UnityEngine;

public class ShootAtTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float attackRadius = 6f;
    [SerializeField] private string shootAnimationName = "animation_enemyCowboy_shoot";
    [SerializeField] private AudioClip shootClip;

    private float nextFireTime;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (target == null || firePoint == null || projectilePrefab == null)
        {
            return;
        }

        if (Vector2.Distance(target.position, firePoint.position) > attackRadius)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        Vector2 direction = (target.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (animator != null && !string.IsNullOrEmpty(shootAnimationName))
        {
            animator.Play(shootAnimationName);
        }

        PlayClip(shootClip);

        nextFireTime = Time.time + fireInterval;
    }

    private void PlayClip(AudioClip clip)
    {
        if (AudioManager.Instance == null || clip == null)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = firePoint != null ? firePoint.position : transform.position;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}
