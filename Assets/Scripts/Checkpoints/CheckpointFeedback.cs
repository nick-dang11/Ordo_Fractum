using UnityEngine;

public class CheckpointFeedback : MonoBehaviour
{
    [Header("Visual State")]
    [SerializeField] private GameObject activatedIndicator;
    [SerializeField] private GameObject currentIndicator;
    [Header("Activation Effect")]
    [SerializeField] private ParticleSystem activationParticles;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip respawnClip;
    private void Awake()
    {
        if (activatedIndicator != null)
        {
            activatedIndicator.SetActive(false);
        }
        if (currentIndicator != null)
        {
            currentIndicator.SetActive(false);
        }
    }
    public void PlayFirstActivation()
    {
        if (activatedIndicator != null)
        {
            activatedIndicator.SetActive(true);
        }
        if (activationParticles != null)
        {
            activationParticles.Play(true);
        }
        if (audioSource != null && checkpointClip != null)
        {
            audioSource.PlayOneShot(checkpointClip);
        }
        Debug.Log("[CheckpointFeedback] First activation feedback played");
    }
    public void PlayRespawn()
    {
        if (audioSource != null && respawnClip != null)
        {
            audioSource.PlayOneShot(respawnClip);
        }
        Debug.Log("[CheckpointFeedback] Respawn sound played.");
    }
    public void SetCurrent(bool isCurrent)
    {
        if (currentIndicator != null)
        {
            currentIndicator.SetActive(isCurrent);
        }
    }
    public void SetActivated(bool activated)
    {
        if (activatedIndicator != null)
        {
            activatedIndicator.SetActive(activated);
        }
    }
}
