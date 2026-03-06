using UnityEngine;
using TMPro;

public class PauseOptionsController : MonoBehaviour
{
    public static bool IsOptionsOpen { get; private set; } = false;

    [Header("Referências")]
    [SerializeField] private GameObject optionsPanel;    // PauseOptionsPanel
    [SerializeField] private TMP_Text volumeValueText;   // Text_VolumePause

    [Header("Inputs")]
    [SerializeField] private KeyCode leftKey1 = KeyCode.A;
    [SerializeField] private KeyCode leftKey2 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey1 = KeyCode.D;
    [SerializeField] private KeyCode rightKey2 = KeyCode.RightArrow;
    [SerializeField] private KeyCode confirmKey = KeyCode.Q;
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Configuração de volume")]
    [SerializeField] private float volumeStep = 0.05f;   // quanto aumenta/diminui por clique

    private void Start()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        IsOptionsOpen = false;
        RefreshVolumeText();
    }

    private void Update()
    {
        if (!IsOptionsOpen)
            return;

        HandleVolumeInput();
        HandleCloseInput();
    }

    // ===== API chamada pelo PauseMenuController =====

    public void OpenOptions()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("[PauseOptions] optionsPanel não atribuído.");
            return;
        }

        optionsPanel.SetActive(true);
        IsOptionsOpen = true;

        RefreshVolumeText();
        Debug.Log("[PauseOptions] Opções do pause ABERTAS.");
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        IsOptionsOpen = false;

        if (PauseMenuController.Instance != null)
            PauseMenuController.Instance.ShowPauseMenuFromOptions();

        Debug.Log("[PauseOptions] Opções do pause FECHADAS.");
    }

    // ===== Lógica de input / volume =====

    private void HandleVolumeInput()
    {
        bool changed = false;

        if (Input.GetKeyDown(rightKey1) || Input.GetKeyDown(rightKey2))
        {
            ChangeVolume(+volumeStep);
            changed = true;
        }

        if (Input.GetKeyDown(leftKey1) || Input.GetKeyDown(leftKey2))
        {
            ChangeVolume(-volumeStep);
            changed = true;
        }

        if (changed)
            RefreshVolumeText();
    }

    private void HandleCloseInput()
    {
        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(backKey))
        {
            CloseOptions();
        }
    }

    private void ChangeVolume(float delta)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[PauseOptions] AudioManager.Instance não encontrado.");
            return;
        }

        float current = AudioManager.Instance.GetMasterVolume();
        float newVolume = Mathf.Clamp01(current + delta);
        AudioManager.Instance.SetMasterVolume(newVolume);

        Debug.Log($"[PauseOptions] MasterVolume alterado de {current} para {newVolume} (delta={delta}).");
    }

    private void RefreshVolumeText()
    {
        if (volumeValueText == null)
            return;

        float vol = 1f;
        if (AudioManager.Instance != null)
            vol = AudioManager.Instance.GetMasterVolume();

        int percent = Mathf.RoundToInt(vol * 100f);
        volumeValueText.text = "Volume: " + percent + "%";

        Debug.Log("[PauseOptions] RefreshVolumeText – " + volumeValueText.text);
    }
}