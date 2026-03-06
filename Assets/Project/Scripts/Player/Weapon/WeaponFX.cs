using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform muzzlePoint;       // ponto onde aparece o flash
    [SerializeField] private ParticleSystem muzzleFlash;  // opcional, se você tiver partícula
    [SerializeField] private AudioSource audioSource;     // som do tiro

    [Header("Som")]
    [SerializeField] private AudioClip gunshotClip;

    public void PlayShotEffects()
    {
        // Partícula na boca da arma
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Som de tiro
        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }
    }
}
