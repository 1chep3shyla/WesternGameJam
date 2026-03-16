using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ZoneSceneLoader : MonoBehaviour
{
    [SerializeField] private float delay = 1f;
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private string sceneName;
    [SerializeField] private bool loadNextBuildIndex = true;
    [SerializeField] private LayerMask playerMask;

    private bool isTriggered;

    private void Start()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered)
        {
            return;
        }

        if ((playerMask.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        isTriggered = true;
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
        }

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        Time.timeScale = 1f;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (loadNextBuildIndex)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
