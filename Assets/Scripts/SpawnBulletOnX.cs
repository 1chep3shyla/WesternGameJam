using UnityEngine;

public class SpawnBulletOnX : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float[] bulletSpeeds;
    [SerializeField] private float fireInterval = 0.5f;

    private float nextFireTime;

    private void Update()
    {
        if (bulletPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];
        if (spawnPoint == null)
        {
            return;
        }

        float speed = (bulletSpeeds != null && bulletSpeeds.Length > index) ? bulletSpeeds[index] : 0f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity, transform);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = new Vector2(speed, 0f);
        }

        nextFireTime = Time.time + fireInterval;
    }
}
