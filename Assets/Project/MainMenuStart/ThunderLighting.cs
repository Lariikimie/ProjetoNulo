using UnityEngine;

public class ThunderLightning : MonoBehaviour
{
    [Header("Luz do relâmpago")]
    public Light lightningLight;

    [Header("Som do trovão")]
    public AudioSource audioSource;
    public AudioClip thunderSound;

    [Header("Configuração")]
    public float thunderInterval = 7f; // intervalo entre trovões
    public int flashCount = 3;         // quantas piscadas a luz vai dar
    public float minFlashTime = 0.03f; // menor tempo da piscada
    public float maxFlashTime = 0.1f;  // maior tempo da piscada

    private void Start()
    {
        if (lightningLight != null)
            lightningLight.enabled = false; // garante que começa desligada

        StartCoroutine(ThunderRoutine());
    }

    private System.Collections.IEnumerator ThunderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(thunderInterval);

            // toca o som do trovão
            if (audioSource != null && thunderSound != null)
                audioSource.PlayOneShot(thunderSound);

            // relâmpago (piscadas)
            if (lightningLight != null)
                yield return StartCoroutine(LightFlash());
        }
    }

    private System.Collections.IEnumerator LightFlash()
    {
        for (int i = 0; i < flashCount; i++)
        {
            lightningLight.enabled = true;
            yield return new WaitForSeconds(Random.Range(minFlashTime, maxFlashTime));

            lightningLight.enabled = false;
            yield return new WaitForSeconds(Random.Range(minFlashTime, maxFlashTime));
        }
    }
}