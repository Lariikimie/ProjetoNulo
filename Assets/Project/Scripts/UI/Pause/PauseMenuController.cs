using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Gameplay.Dialogue; // usa DialogueUI do namespace

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public static bool IsPaused = false;   // compat com outros scripts

    [Header("Referências")]
    [SerializeField] private GameObject pauseMenuPanel;         // PauseMenuPanel (fundo + textos)
    [SerializeField] private CheckPointManager checkpointManager;
    [SerializeField] private MapPanelController mapPanelController;
    [SerializeField] private PauseOptionsController pauseOptionsController;
    [SerializeField] private DialogueUI dialogueUI;             // agora é o do namespace

    [Header("Inputs (fallback, se NÃO houver UINavigationInput)")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode upKey1 = KeyCode.A;
    [SerializeField] private KeyCode upKey2 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode downKey1 = KeyCode.D;
    [SerializeField] private KeyCode downKey2 = KeyCode.RightArrow;
    [SerializeField] private KeyCode confirmKey = KeyCode.Q;

    [Header("Itens do Menu (em ordem)")]
    [SerializeField] private TMP_Text[] menuItems;   // Text_Resume, Text_Checkpoint, etc.
    [SerializeField] private Button[] menuButtons;   // Button_Resume, Button_Checkpoint, etc. (MESMA ORDEM!)

    [Header("Configuração visual do highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float normalFontSize = 32f;
    [SerializeField] private float selectedFontSize = 40f;
    [SerializeField] private string selectedPrefix = "> ";   // prefixo do item selecionado

    private int currentIndex = 0;
    private string[] baseLabels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true); // pega o do namespace

        if (menuItems == null || menuItems.Length == 0)
        {
            Debug.LogWarning("[PAUSE] Nenhum menuItem configurado.");
            return;
        }

        baseLabels = new string[menuItems.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i] != null)
                baseLabels[i] = menuItems[i].text;
            else
                Debug.LogWarning($"[PAUSE] menuItems[{i}] está nulo no Inspector.");
        }

        if (menuButtons == null || menuButtons.Length != menuItems.Length)
        {
            Debug.LogWarning("[PAUSE] menuButtons não está configurado ou não tem o mesmo tamanho que menuItems.");
        }
    }

    private void Update()
    {
        // 🔒 Não abre o pause se o inventário estiver aberto
        if (InventoryUI.IsInventoryOpen)
            return;

        // 🔒 Se o mapa OU as opções do pause estão abertas, o PauseMenu não reage
        if (MapPanelController.IsMapOpen || PauseOptionsController.IsOptionsOpen)
            return;

        bool pauseKeyDown = Input.GetKeyDown(pauseKey);
        bool cancelPressed = UINavigationInput.Instance != null &&
                             UINavigationInput.Instance.CancelPressedThisFrame();

        // ============= FECHAR QUANDO JÁ ESTÁ EM PAUSE =============
        if (IsPaused)
        {
            if (pauseKeyDown || cancelPressed)
            {
                Debug.Log($"[PAUSE] Recebeu comando para FECHAR (ESC/Cancel). TimeScale atual: {Time.timeScale}");
                TogglePause();
                return;
            }

            HandleNavigation();
            HandleConfirm();
            return;
        }

        // ============= ABRIR QUANDO NÃO ESTÁ EM PAUSE =============
        if (pauseKeyDown || cancelPressed)
        {
            // Se existe UIPanelManager e já há outro painel aberto (Inventory, Map, Dialogue etc.),
            // simplesmente IGNORA a tentativa de abrir o Pause.
            if (UIPanelManager.Instance != null &&
                UIPanelManager.Instance.CurrentPanel != UIPanelType.None)
            {
                Debug.Log($"[PAUSE] Ignorando tentativa de abrir Pause. Outro painel ativo: {UIPanelManager.Instance.CurrentPanel}");
                return;
            }

            Debug.Log($"[PAUSE] Recebeu comando para ABRIR (ESC/Cancel). TimeScale atual: {Time.timeScale}");
            TogglePause();
        }
    }

    // =============== ABRIR / FECHAR ===============

    public void TogglePause()
    {
        if (IsPaused) ClosePause();
        else OpenPause();
    }

    private void OpenPause()
    {
        // Gate com UIPanelManager: não abre se outro painel bloqueante já estiver ativo
        if (UIPanelManager.Instance != null)
        {
            if (!UIPanelManager.Instance.TryOpen(UIPanelType.Pause))
            {
                Debug.Log($"[PAUSE] Não abriu: outro painel já está ativo ({UIPanelManager.Instance.CurrentPanel}).");
                return;
            }
        }

        Debug.Log($"[PAUSE] Abrindo Pause. Time.timeScale antes: {Time.timeScale}");

        IsPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Se houver GameStateManager, usa ele; senão, pausa via Time.timeScale
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetPaused(true);
            Debug.Log($"[PAUSE] GameStateManager.SetPaused(true) chamado. Time.timeScale agora: {Time.timeScale}");
        }
        else
        {
            Time.timeScale = 0f;
            Debug.Log($"[PAUSE] Sem GameStateManager. Time.timeScale forçado para 0.");
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentIndex = 0;
        UpdateHighlight();

        Debug.Log($"[PAUSE] ABERTO. IsPaused={IsPaused}, PanelAtivo={UIPanelManager.Instance?.CurrentPanel}, TimeScale={Time.timeScale}");
    }

    private void ClosePause()
    {
        Debug.Log($"[PAUSE] Fechando Pause. Time.timeScale antes: {Time.timeScale}");

        IsPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        bool dialogueAtivo = dialogueUI != null && dialogueUI.IsVisible;

        if (GameStateManager.Instance != null)
        {
            if (dialogueAtivo)
            {
                GameStateManager.Instance.EnterDialogue();
                Debug.Log($"[PAUSE] Retornando para diálogo. Time.timeScale após EnterDialogue: {Time.timeScale}");
            }
            else
            {
                GameStateManager.Instance.SetPaused(false);
                Debug.Log($"[PAUSE] GameStateManager.SetPaused(false) chamado. Time.timeScale agora: {Time.timeScale}");
            }
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log($"[PAUSE] Sem GameStateManager. Time.timeScale forçado para 1.");
        }

        // Safeguard: se por algum motivo ainda ficou 0, força para 1
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            Debug.LogWarning("[PAUSE] Safeguard: Time.timeScale ainda estava 0 após fechar o pause. Ajustado para 1.");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Informa ao UIPanelManager que o Pause foi fechado
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.Close(UIPanelType.Pause);
            Debug.Log($"[PAUSE] UIPanelManager.Close(Pause). Painel atual agora: {UIPanelManager.Instance.CurrentPanel}");
        }

        Debug.Log($"[PAUSE] FECHADO. IsPaused={IsPaused}, TimeScale={Time.timeScale}");
    }

    // Chamado pelo PauseOptionsController quando fecha o painel de opções
    public void ShowPauseMenuFromOptions()
    {
        if (!IsPaused)
            return;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        UpdateHighlight();
    }

    // Usado antes de abrir o painel de opções
    public void HidePauseMenuForOptions()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    // =============== NAVEGAÇÃO ===============

    private void HandleNavigation()
    {
        if (menuItems == null || menuItems.Length == 0)
            return;

        bool moved = false;

        // Se temos UINavigationInput, usamos ele (W/S, setas e controle)
        if (UINavigationInput.Instance != null)
        {
            if (UINavigationInput.Instance.UpPressedThisFrame())
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = menuItems.Length - 1;
                moved = true;
            }

            if (UINavigationInput.Instance.DownPressedThisFrame())
            {
                currentIndex++;
                if (currentIndex >= menuItems.Length)
                    currentIndex = 0;
                moved = true;
            }
        }
        else
        {
            // Fallback: teclas configuradas aqui no PauseMenuController
            if (Input.GetKeyDown(upKey1) || Input.GetKeyDown(upKey2))
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = menuItems.Length - 1;
                moved = true;
            }

            if (Input.GetKeyDown(downKey1) || Input.GetKeyDown(downKey2))
            {
                currentIndex++;
                if (currentIndex >= menuItems.Length)
                    currentIndex = 0;
                moved = true;
            }
        }

        if (moved)
        {
            Debug.Log("[PAUSE] Navegação index = " + currentIndex);
            UpdateHighlight();
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

            string originalText = baseLabels[i];

            if (i == currentIndex)
            {
                txt.text = selectedPrefix + originalText;
                txt.color = selectedColor;
                txt.fontSize = selectedFontSize;
            }
            else
            {
                txt.text = originalText;
                txt.color = normalColor;
                txt.fontSize = normalFontSize;
            }
        }
    }

    // =============== CONFIRMAR (Q / Confirm) ===============

    private void HandleConfirm()
    {
        bool confirmPressed = false;

        if (UINavigationInput.Instance != null)
            confirmPressed = UINavigationInput.Instance.ConfirmPressedThisFrame();
        else
            confirmPressed = Input.GetKeyDown(confirmKey);

        if (!confirmPressed)
            return;

        Debug.Log("[PAUSE] Confirmado index = " + currentIndex);

        // Se tiver Button configurado para este índice, dispara o onClick
        if (menuButtons != null &&
            currentIndex >= 0 &&
            currentIndex < menuButtons.Length &&
            menuButtons[currentIndex] != null)
        {
            menuButtons[currentIndex].onClick.Invoke();
            return;
        }

        // Fallback (caso esqueça de configurar algum botão)
        switch (currentIndex)
        {
            case 0: // Resume
                TogglePause();
                break;

            case 1: // Retornar ao checkpoint
                if (checkpointManager != null)
                    checkpointManager.ReturnToLastCheckpoint();
                else
                    Debug.LogWarning("[PAUSE] checkpointManager não atribuído.");
                TogglePause();
                break;

            case 2: // Mapa
                if (mapPanelController != null)
                    mapPanelController.ShowMap(); // MapPanel já bloqueia no Pause/Diálogo
                else
                    Debug.LogWarning("[PAUSE] MapPanelController não atribuído.");
                break;

            case 3: // Opções / Volume
                if (pauseOptionsController != null)
                {
                    HidePauseMenuForOptions();
                    pauseOptionsController.OpenOptions();
                }
                else
                {
                    Debug.LogWarning("[PAUSE] PauseOptionsController não atribuído.");
                }
                break;

            case 4: // Sair para o menu principal
                if (GameStateManager.Instance != null)
                    GameStateManager.Instance.SetPaused(false);
                else
                    Time.timeScale = 1f;

                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                break;
        }
    }

    // ==================== MÉTODOS PÚBLICOS (OnClick) ====================

    public void OnClick_Resume() => TogglePause();

    public void OnClick_ReturnCheckpoint()
    {
        if (checkpointManager != null)
            checkpointManager.ReturnToLastCheckpoint();
        else
            Debug.LogWarning("[PAUSE] checkpointManager não atribuído.");

        TogglePause();
    }

    public void OnClick_Map()
    {
        if (mapPanelController != null)
            mapPanelController.ShowMap();
        else
            Debug.LogWarning("[PAUSE] MapPanelController não atribuído.");
    }

    public void OnClick_Options()
    {
        if (pauseOptionsController != null)
        {
            HidePauseMenuForOptions();
            pauseOptionsController.OpenOptions();
        }
        else
        {
            Debug.LogWarning("[PAUSE] PauseOptionsController não atribuído.");
        }
    }

    public void OnClick_Quit()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetPaused(false);
        else
            Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}