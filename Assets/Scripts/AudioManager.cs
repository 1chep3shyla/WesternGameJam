using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        float pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip);
    }
}
