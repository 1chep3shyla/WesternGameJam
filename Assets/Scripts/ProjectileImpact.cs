using System.Collections.Generic;
using UnityEngine;

public class ProjectileImpact : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private float damageInterval = 0.25f;
    [SerializeField] private AudioClip impactEnemyClip;
    [SerializeField] private AudioClip impactGroundClip;

    private readonly Dictionary<int, float> nextDamageTimeByTarget = new Dictionary<int, float>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.collider, collision.GetContact(0).point);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleImpact(collision.collider, collision.GetContact(0).point);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other, other.ClosestPoint(transform.position));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleImpact(other, other.ClosestPoint(transform.position));
    }

    private void HandleImpact(Collider2D collider, Vector2 hitPoint)
    {
        int collisionLayer = collider.gameObject.layer;
        bool hitGround = (groundMask.value & (1 << collisionLayer)) != 0;
        bool hitEnemy = (enemyMask.value & (1 << collisionLayer)) != 0;

        if (!hitGround && !hitEnemy)
        {
            return;
        }

        if (hitEnemy)
        {
            if (CanDealDamage(collider))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    PlayClip(impactEnemyClip);
                }
            }
        }

        if (hitGround)
        {
            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, hitPoint, Quaternion.identity);
            }

            PlayClip(impactGroundClip);
        }

        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (AudioManager.Instance == null || clip == null)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(clip);
    }

    private bool CanDealDamage(Collider2D collider)
    {
        if (damageInterval <= 0f)
        {
            return true;
        }

        int id = collider.GetInstanceID();
        if (nextDamageTimeByTarget.TryGetValue(id, out float nextTime) && Time.time < nextTime)
        {
            return false;
        }

        nextDamageTimeByTarget[id] = Time.time + damageInterval;
        return true;
    }
}
