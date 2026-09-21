using UnityEngine;

public class CombatClashFeedback : MonoBehaviour
{
    [Header("Fallback Clash Position")]
    [SerializeField] private Transform fallbackClashPoint;

    [Header("Visual Effects")]
    [SerializeField] private GameObject blockVfxPrefab;
    [SerializeField] private GameObject deflectVfxPrefab;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blockClip;
    [SerializeField] private AudioClip deflectClip;

    public void PlayBlock()
    {
        Vector3 position = GetFallbackPosition();

        PlayBlock(position);
    }

    public void PlayDeflect()
    {
        Vector3 position = GetFallbackPosition();

        PlayDeflect(position);
    }

    public void PlayBlock(Vector3 position)
    {
        SpawnVfx(blockVfxPrefab, position);
        PlaySound(blockClip, position);
    }

    public void PlayDeflect(Vector3 position)
    {
        SpawnVfx(deflectVfxPrefab, position);
        PlaySound(deflectClip, position);
    }

    private Vector3 GetFallbackPosition()
    {
        if (fallbackClashPoint != null)
        {
            return fallbackClashPoint.position;
        }

        return transform.position;
    }

    private void SpawnVfx(GameObject vfxPrefab, Vector3 position)
    {
        if (vfxPrefab == null) return;

        GameObject effect = Instantiate(vfxPrefab, position, Quaternion.identity);

        Destroy(effect, vfxLifetime);
    }

    private void PlaySound(
        AudioClip clip,
        Vector3 position)
    {
        if (audioSource == null ||
            clip == null)
        {
            return;
        }

        // Move the audio emitter to the clash.
        audioSource.transform.position = position;

        audioSource.PlayOneShot(clip);
    }

    // Temporary testing tools.
    [ContextMenu("TEST - Block Clash")]
    private void TestBlockClash()
    {
        PlayBlock();
    }

    [ContextMenu("TEST - Deflect Clash")]
    private void TestDeflectClash()
    {
        PlayDeflect();
    }
}