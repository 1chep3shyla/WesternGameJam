using UnityEngine;
using UnityEngine.UI;

public class PlayerEntity : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Color fullHeartColor = Color.white;
    [SerializeField] private Color emptyHeartColor = Color.black;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private GameObject onDeathObject;

    private int currentHealth;
    private Material defaultMaterial;
    private Coroutine damageFlashRoutine;
    private bool isDead;

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

        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHearts();
        TriggerDamageFlash();

        if (currentHealth <= 0)
        {
            isDead = true;
            if (onDeathObject != null)
            {
                onDeathObject.SetActive(true);
            }

            Time.timeScale = 0f;
        }
    }

    private void UpdateHearts()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }

            heartImages[i].color = i < currentHealth ? fullHeartColor : emptyHeartColor;
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
