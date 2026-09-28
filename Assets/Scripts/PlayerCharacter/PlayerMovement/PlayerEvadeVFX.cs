using UnityEngine;
using Unity.Cinemachine;

public class PlayerEvadeVFX : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip evadeWoosh;

    [Header("Dust")]
    [SerializeField] private ParticleSystem evadeDust;

    [Header("Camera")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Range(0f, 1f)]
    [SerializeField] private float evadeVolume = 1f;

    public void PlayEvadeStart()
    {
        if(audioSource != null && evadeWoosh != null)
        {
            audioSource.PlayOneShot(evadeWoosh, evadeVolume);
        }

        if(evadeDust != null)
        {
            evadeDust.Play();
        }

        if(impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        
    }
}
