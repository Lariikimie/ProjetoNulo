using UnityEngine;

/// <summary>
/// Coloque em qualquer objeto 3D inspecionável na cena.
/// Configure individualmente: som, distância, coleta, etc.
/// Layer deve ser "Inspectable" para o raycast funcionar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InspectableObject : MonoBehaviour
{
    [Header("Identificação")]
    [Tooltip("Nome exibido na UI (ex: 'Foto antiga', 'Chave estranha').")]
    public string displayName = "Objeto";

    [Header("Inspeção")]
    [Tooltip("Distância do objeto em relação à câmera durante a inspeção.")]
    public float inspectDistance = 0.5f;

    [Tooltip("Velocidade de rotação ao arrastar o mouse.")]
    public float rotationSpeed = 200f;

    [Header("Áudio — Pegar")]
    [Tooltip("Som tocado ao iniciar a inspeção (pegar o objeto). Deixe vazio para usar o ID do AudioManager.")]
    public AudioClip pickupSound;

    [Tooltip("Se pickupSound estiver vazio, usa este ID do AudioManager.")]
    public string pickupSoundId = "";

    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Header("Áudio — Soltar / Devolver")]
    [Tooltip("Som tocado ao soltar/devolver o objeto. Deixe vazio para usar o ID.")]
    public AudioClip releaseSound;

    [Tooltip("Se releaseSound estiver vazio, usa este ID do AudioManager.")]
    public string releaseSoundId = "";

    [Range(0f, 1f)]
    public float releaseVolume = 1f;

    [Header("Áudio — Girar (loop enquanto arrasta)")]
    [Tooltip("Som de manipulação ao girar o objeto (ex: madeira rangendo, metal raspando).")]
    public AudioClip handleSound;
    public string handleSoundId = "";

    [Range(0f, 1f)]
    public float handleVolume = 0.5f;

    [Header("Coleta (opcional)")]
    [Tooltip("Se true, o objeto é coletado ao fechar a inspeção.")]
    public bool collectAfterInspect = false;

    public ItemType collectItemType = ItemType.Key;
    public string collectKeyId;
    public int collectBatteryAmount = 1;
    public NoteData collectNoteData;

    // Estado interno (usado pelo ObjectInspector)
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
    [HideInInspector] public Transform originalParent;

    private AudioSource audioSource;

    private void Awake()
    {
        SaveOriginalTransform();

        // AudioSource próprio para sons 3D posicionais
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D
        audioSource.loop = false;
    }

    public void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    public void RestoreOriginalTransform()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    // ═══════════════ ÁUDIO ═══════════════

    /// <summary>Toca o som de pegar o objeto.</summary>
    public void PlayPickupSound()
    {
        PlaySound(pickupSound, pickupSoundId, pickupVolume, false);
    }

    /// <summary>Toca o som de soltar/devolver.</summary>
    public void PlayReleaseSound()
    {
        PlaySound(releaseSound, releaseSoundId, releaseVolume, false);
    }

    /// <summary>Inicia o som de manipulação (loop).</summary>
    public void StartHandleSound()
    {
        AudioClip clip = ResolveClip(handleSound, handleSoundId);
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.volume = handleVolume;
        audioSource.loop = true;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    /// <summary>Para o som de manipulação.</summary>
    public void StopHandleSound()
    {
        if (audioSource.isPlaying && audioSource.loop)
            audioSource.Stop();

        audioSource.loop = false;
    }

    private void PlaySound(AudioClip directClip, string soundId, float volume, bool loop)
    {
        AudioClip clip = ResolveClip(directClip, soundId);
        if (clip == null) return;

        audioSource.loop = loop;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip, volume);
    }

    /// <summary>Resolve o clip: prioriza o AudioClip direto, senão busca pelo ID no AudioManager.</summary>
    private AudioClip ResolveClip(AudioClip directClip, string soundId)
    {
        if (directClip != null)
            return directClip;

        if (!string.IsNullOrEmpty(soundId) && AudioManager.Instance != null)
        {
            // Usa PlaySFX2D do AudioManager se não tiver clip direto
            // Mas como queremos retornar o clip, tentamos pegar do banco
            // Fallback: toca via AudioManager e retorna null
            AudioManager.Instance.PlaySFX2D(soundId);
        }

        return null;
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}