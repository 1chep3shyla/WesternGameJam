using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;

    private int currentHealth;
    private Material defaultMaterial;
    private Coroutine damageFlashRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            defaultMaterial = targetRenderer.material;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        TriggerDamageFlash();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void TriggerDamageFlash()
    {
        if (targetRenderer == null || damageMaterial == null || damageFlashDuration <= 0f)
        {
            return;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlash());
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        targetRenderer.material = damageMaterial;
        yield return new WaitForSeconds(damageFlashDuration);
        if (defaultMaterial != null)
        {
            targetRenderer.material = defaultMaterial;
        }
        damageFlashRoutine = null;
    }
}
