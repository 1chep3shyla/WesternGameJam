using UnityEngine;

public class ProjectileImpact : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject impactPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int collisionLayer = collision.collider.gameObject.layer;
        bool hitGround = (groundMask.value & (1 << collisionLayer)) != 0;
        bool hitEnemy = (enemyMask.value & (1 << collisionLayer)) != 0;

        if (!hitGround && !hitEnemy)
        {
            return;
        }

        if (hitGround)
        {
            Vector3 spawnPosition = collision.GetContact(0).point;
            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, spawnPosition, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}
