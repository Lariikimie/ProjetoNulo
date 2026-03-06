using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Referências de Painéis")]
    [SerializeField] private GameObject mainMenuPanel;   // painel com Play / Options / Quit
    [SerializeField] private GameObject optionsPanel;    // painel de opções (volume)

    [Header("Itens do Menu Principal (ordem)")]
    [SerializeField] private TMP_Text[] menuItems;       // Text_Play, Text_Options, Text_Quit

    [Header("Config visual do highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float normalFontSize = 32f;
    [SerializeField] private float selectedFontSize = 40f;
    [SerializeField] private string selectedPrefix = "> ";   // prefixo do item selecionado

    [Header("Inputs")]
    [SerializeField] private KeyCode leftKey1 = KeyCode.A;
    [SerializeField] private KeyCode leftKey2 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey1 = KeyCode.D;
    [SerializeField] private KeyCode rightKey2 = KeyCode.RightArrow;
    [SerializeField] private KeyCode confirmKey = KeyCode.Q;
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Cena do jogo")]
    [SerializeField] private string gameSceneName = "Main";

    [Header("Opções / Volume")]
    [SerializeField] private TMP_Text volumeValueText;   // Text_Volume (no OptionsPanel)
    [SerializeField] private float volumeStep = 0.05f;

    // ==================== ESTADO INTERNO ====================

    private int currentIndex = 0;        // índice do menu principal
    private string[] baseLabels;         // rótulos originais
    private bool isInOptions = false;    // estamos no optionsPanel?

    private float currentSfxVolume = 1f;

    private void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Cache dos textos
        if (menuItems == null || menuItems.Length == 0)
        {
            Debug.LogWarning("[MAIN MENU] Nenhum menuItem configurado.");
        }
        else
        {
            baseLabels = new string[menuItems.Length];
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (menuItems[i] != null)
                    baseLabels[i] = menuItems[i].text;
                else
                    Debug.LogWarning($"[MAIN MENU] menuItems[{i}] está nulo no Inspector.");
            }
        }

        // Volume inicial
        if (AudioManager.Instance != null)
        {
            currentSfxVolume = AudioManager.Instance.GetSFXVolume();
            Debug.Log("[MAIN MENU] Start – SFX inicial = " + currentSfxVolume);
        }
        else
        {
            Debug.LogWarning("[MAIN MENU] Start – AudioManager.Instance é null.");
            currentSfxVolume = 1f;
        }

        UpdateHighlight();
        UpdateSfxVolumeText();
    }

    private void Update()
    {
        if (isInOptions)
        {
            HandleOptionsInput();
            return;
        }

        HandleMainMenuNavigation();
        HandleMainMenuConfirm();
    }

    // ==================== MENU PRINCIPAL ====================

    private void HandleMainMenuNavigation()
    {
        if (menuItems == null || menuItems.Length == 0)
            return;

        bool moved = false;

        if (UINavigationInput.Instance != null)
        {
            if (UINavigationInput.Instance.LeftPressedThisFrame())
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = menuItems.Length - 1;
                moved = true;
            }

            if (UINavigationInput.Instance.RightPressedThisFrame())
            {
                currentIndex++;
                if (currentIndex >= menuItems.Length)
                    currentIndex = 0;
                moved = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(leftKey1) || Input.GetKeyDown(leftKey2))
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = menuItems.Length - 1;
                moved = true;
            }

            if (Input.GetKeyDown(rightKey1) || Input.GetKeyDown(rightKey2))
            {
                currentIndex++;
                if (currentIndex >= menuItems.Length)
                    currentIndex = 0;
                moved = true;
            }
        }

        if (moved)
        {
            Debug.Log("[MAIN MENU] Navegação index = " + currentIndex);
            UpdateHighlight();
        }
    }

    private void HandleMainMenuConfirm()
    {
        bool confirmPressed = false;

        if (UINavigationInput.Instance != null)
            confirmPressed = UINavigationInput.Instance.ConfirmPressedThisFrame();
        else
            confirmPressed = Input.GetKeyDown(confirmKey);

        if (!confirmPressed)
            return;

        Debug.Log("[MAIN MENU] Confirmado index = " + currentIndex);

        switch (currentIndex)
        {
            case 0: // Play
                OnPlaySelected();
                break;

            case 1: // Options
                OnOptionsSelected();
                break;

            case 2: // Quit
                OnQuitSelected();
                break;
        }
    }

    private void UpdateHighlight()
    {
        if (menuItems == null || baseLabels == null)
            return;

        for (int i = 0; i < menuItems.Length; i++)
        {
            TMP_Text txt = menuItems[i];
            if (txt == null)
                continue;

            string original = baseLabels[i];

            if (i == currentIndex)
            {
                txt.text = selectedPrefix + original;
                txt.color = selectedColor;
                txt.fontSize = selectedFontSize;
            }
            else
            {
                txt.text = original;
                txt.color = normalColor;
                txt.fontSize = normalFontSize;
            }
        }
    }

    // ==================== AÇÕES DO MENU ====================

    private void OnPlaySelected()
    {
        Debug.Log("[MAIN MENU] Play selecionado. Carregando cena: " + gameSceneName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnOptionsSelected()
    {
        Debug.Log("[MAIN MENU] Options selecionado.");

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            currentSfxVolume = AudioManager.Instance.GetSFXVolume();
            Debug.Log("[MAIN MENU] OnOptionsSelected – SFX atual = " + currentSfxVolume);
        }
        else
        {
            Debug.LogWarning("[MAIN MENU] OnOptionsSelected – AudioManager.Instance é null.");
            currentSfxVolume = 1f;
        }

        UpdateSfxVolumeText();
        isInOptions = true;
    }

    private void OnQuitSelected()
    {
        Debug.Log("[MAIN MENU] Quit selecionado.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ==================== OPÇÕES (VOLUME SFX) ====================

    private void HandleOptionsInput()
    {
        bool left = false;
        bool right = false;
        bool backOrConfirm = false;

        if (UINavigationInput.Instance != null)
        {
            left = UINavigationInput.Instance.LeftPressedThisFrame();
            right = UINavigationInput.Instance.RightPressedThisFrame();
            backOrConfirm = UINavigationInput.Instance.CancelPressedThisFrame() ||
                            UINavigationInput.Instance.ConfirmPressedThisFrame();
        }
        else
        {
            left = Input.GetKeyDown(leftKey1) || Input.GetKeyDown(leftKey2);
            right = Input.GetKeyDown(rightKey1) || Input.GetKeyDown(rightKey2);
            backOrConfirm = Input.GetKeyDown(backKey) || Input.GetKeyDown(confirmKey);
        }

        if (left)
        {
            Debug.Log("[MAIN MENU] OptionsInput – LEFT detectado.");
            ChangeSfxVolume(-1);
        }

        if (right)
        {
            Debug.Log("[MAIN MENU] OptionsInput – RIGHT detectado.");
            ChangeSfxVolume(+1);
        }

        if (backOrConfirm)
        {
            Debug.Log("[MAIN MENU] OptionsInput – Back/Confirm detectado, fechando opções.");
            CloseOptions();
        }
    }

    private void ChangeSfxVolume(int direction)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[MAIN MENU] ChangeSfxVolume – AudioManager.Instance é null.");
            return;
        }

        float old = currentSfxVolume;
        currentSfxVolume += direction * volumeStep;
        currentSfxVolume = Mathf.Clamp01(currentSfxVolume);

        AudioManager.Instance.SetSFXVolume(currentSfxVolume);
        UpdateSfxVolumeText();

        Debug.Log($"[MAIN MENU] SFX Volume alterado de {old} para {currentSfxVolume} (direction={direction}).");
    }

    private void UpdateSfxVolumeText()
    {
        if (volumeValueText == null)
        {
            Debug.LogWarning("[MAIN MENU] volumeValueText não atribuído.");
            return;
        }

        int percent = Mathf.RoundToInt(currentSfxVolume * 100f);
        volumeValueText.text = "Efeitos: " + percent + "%";

        Debug.Log("[MAIN MENU] UpdateSfxVolumeText – exibindo: " + volumeValueText.text);
    }

    private void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        isInOptions = false;
        Debug.Log("[MAIN MENU] Options FECHADO. Voltando ao menu principal.");
    }
}