using UnityEngine;

public class PlayerEvadeVFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip evadeWoosh;

    [Range(0f, 1f)]
    [SerializeField] private float evadeVolume = 1f;

    public void PlayEvadeStart()
    {
        if(audioSource == null || evadeWoosh == null)
        {
            return;
        }

        audioSource.PlayOneShot(evadeWoosh, evadeVolume);
    }
}
