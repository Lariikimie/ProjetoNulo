using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundEntry
{
    [Tooltip("ID usado nos scripts. Ex: \"Gunshot\", \"Footstep_Concrete\"")]
    public string id;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.5f, 2f)]
    public float pitch = 1f;

    [Tooltip("Se verdadeiro, esse som foi pensado para loop (música/ambiente).")]
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ==================== MASTER VOLUME (já existia) ====================

    [Header("Volume Master (0 a 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    private const string MASTER_VOLUME_KEY = "MasterVolume";

    // ==================== NOVO: VOLUME SFX ====================

    [Header("Volume SFX (0 a 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // ==================== Fontes / Banco ====================

    [Header("Fontes 2D")]
    [SerializeField] private AudioSource musicSource;   // Ambiente / música
    [SerializeField] private AudioSource sfx2DSource;   // UI / SFX gerais

    [Header("Configuração padrão para SFX 3D")]
    [SerializeField] private float sfx3DMinDistance = 1f;
    [SerializeField] private float sfx3DMaxDistance = 20f;

    [Header("Banco de Sons")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    private Dictionary<string, SoundEntry> soundDict = new Dictionary<string, SoundEntry>();

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Carrega volumes
        if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);

        if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        ApplyMasterVolume();

        // Monta dicionário de sons
        soundDict.Clear();
        foreach (var s in sounds)
        {
            if (s == null || string.IsNullOrEmpty(s.id) || s.clip == null)
                continue;

            if (soundDict.ContainsKey(s.id))
            {
                Debug.LogWarning("[AudioManager] ID de som duplicado: " + s.id);
                continue;
            }

            soundDict.Add(s.id, s);
        }

        // Garante fontes
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
        }

        if (sfx2DSource == null)
        {
            sfx2DSource = gameObject.AddComponent<AudioSource>();
            sfx2DSource.loop = false;
            sfx2DSource.playOnAwake = false;
            sfx2DSource.spatialBlend = 0f;
        }

        Debug.Log("[AudioManager] Inicializado. Sons cadastrados: " + soundDict.Count);
    }

    // ==================== MASTER (igual antes) ====================

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyMasterVolume();
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.Save();
        Debug.Log("[AudioManager] Master volume definido para: " + masterVolume);
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    private void ApplyMasterVolume()
    {
        AudioListener.volume = masterVolume;
    }

    // ==================== NOVO: SFX VOLUME ====================

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
        Debug.Log("[AudioManager] SFX volume definido para: " + sfxVolume);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    // ==================== PLAY 2D / 3D / MUSIC ====================

    public void PlaySFX2D(string id)
    {
        if (!TryGetSound(id, out SoundEntry s))
            return;

        sfx2DSource.volume = s.volume * sfxVolume; // master já é aplicado no AudioListener
        sfx2DSource.pitch = s.pitch;
        sfx2DSource.loop = false;
        sfx2DSource.PlayOneShot(s.clip);

        Debug.Log("[AudioManager] PlaySFX2D: " + id);
    }

    public void PlaySFX3D(string id, Vector3 position)
    {
        if (!TryGetSound(id, out SoundEntry s))
            return;

        GameObject temp = new GameObject("SFX3D_" + id);
        temp.transform.position = position;

        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = s.clip;
        a.volume = s.volume * sfxVolume;
        a.pitch = s.pitch;
        a.spatialBlend = 1f;
        a.minDistance = sfx3DMinDistance;
        a.maxDistance = sfx3DMaxDistance;
        a.rolloffMode = AudioRolloffMode.Linear;

        a.Play();
        Destroy(temp, s.clip.length + 0.1f);

        Debug.Log("[AudioManager] PlaySFX3D: " + id + " @ " + position);
    }

    public void PlayMusic(string id)
    {
        if (!TryGetSound(id, out SoundEntry s))
            return;

        musicSource.clip = s.clip;
        musicSource.volume = s.volume; // se quiser controlar volume de música depois, criamos outro slider
        musicSource.pitch = s.pitch;
        musicSource.loop = s.loop;
        musicSource.spatialBlend = 0f;
        musicSource.Play();

        Debug.Log("[AudioManager] PlayMusic: " + id);
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // ==================== UTIL ====================

    private bool TryGetSound(string id, out SoundEntry s)
    {
        if (!soundDict.TryGetValue(id, out s))
        {
            Debug.LogWarning("[AudioManager] Som não encontrado: " + id);
            return false;
        }
        return true;
    }
}