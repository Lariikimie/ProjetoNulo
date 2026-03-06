using UnityEngine;

public class MapPanelController : MonoBehaviour
{
    public static bool IsMapOpen { get; private set; } = false;

    [Header("Referências de UI")]
    [SerializeField] private GameObject mapPanelRoot;        // MapPanel (raiz)
    [SerializeField] private RectTransform mapRect;          // MapImage
    [SerializeField] private RectTransform playerIcon;       // PlayerIcon

    [Header("Pause (opcional)")]
    [SerializeField] private GameObject pauseMenuPanel;      // PauseMenuPanel que deve sumir quando o mapa abrir

    [Header("Referência do Player no mundo")]
    [SerializeField] private Transform playerTransform;

    [Header("Limites do mundo (X/Z)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Inputs")]
    [SerializeField] private KeyCode closeKey1 = KeyCode.Escape;
    [SerializeField] private KeyCode closeKey2 = KeyCode.Q;

    [Header("Comportamento")]
    [Tooltip("Tempo (s) para ignorar as teclas de fechar após abrir (evita fechar no mesmo frame).")]
    [SerializeField] private float ignoreCloseInputSeconds = 0.15f;
    [Tooltip("Rotaciona o ícone conforme a rotação Y do player.")]
    [SerializeField] private bool rotateIconWithPlayer = false;

    // Controle de debounce
    private float _canCloseAtTime = 0f;

    // Controle de Canvas/sorting
    private Canvas _mapCanvas;                 // Canvas dedicado ao MapPanel
    private bool _hadCanvasBefore = false;   // Se já existia um Canvas no MapPanel
    private bool _prevOverrideSorting;
    private int _prevSortingOrder;

    private void Awake()
    {
        if (mapPanelRoot == null) mapPanelRoot = gameObject;

        // Auto-descoberta (se esqueceu de arrastar)
        if (mapRect == null)
        {
            var t = mapPanelRoot.transform.Find("MapImage");
            if (t) mapRect = t.GetComponent<RectTransform>();
        }
        if (playerIcon == null && mapRect != null)
        {
            var t = mapRect.transform.Find("PlayerIcon");
            if (t) playerIcon = t.GetComponent<RectTransform>();
        }

        // Canvas do MapPanel
        _mapCanvas = mapPanelRoot.GetComponent<Canvas>();
        if (_mapCanvas != null)
        {
            _hadCanvasBefore = true;
            _prevOverrideSorting = _mapCanvas.overrideSorting;
            _prevSortingOrder = _mapCanvas.sortingOrder;
        }
    }

    private void Start()
    {
        if (mapPanelRoot) mapPanelRoot.SetActive(false);
        IsMapOpen = false;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        // <<< NOVO: se entrou em Diálogo ou Pause, fecha imediatamente
        if (IsMapOpen && (GameStateManager.IsDialogue || GameStateManager.IsPaused))
        {
            HideMap();
            return;
        }

        if (!IsMapOpen) return;

        if (playerTransform && mapRect && playerIcon)
        {
            UpdatePlayerIconPosition();

            if (rotateIconWithPlayer)
            {
                float yaw = playerTransform.eulerAngles.y;
                playerIcon.localRotation = Quaternion.Euler(0f, 0f, -yaw);
            }
        }

        if (Time.unscaledTime >= _canCloseAtTime)
        {
            if (Input.GetKeyDown(closeKey1) || Input.GetKeyDown(closeKey2))
                HideMap();
        }
    }

    public void ShowMap()
    {
        // <<< NOVO: não abrir durante Diálogo ou Pause
        if (GameStateManager.IsDialogue || GameStateManager.IsPaused)
        {
            Debug.Log("[MAPA] Bloqueado: diálogo ou pause ativos.");
            return;
        }

        // Player de fallback
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerTransform = p.transform;
        }

        // (1) Garante que o Pause não apareça por baixo
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);

        // (2) Garante Canvas próprio com Sorting alto
        if (_mapCanvas == null)
        {
            _mapCanvas = mapPanelRoot.AddComponent<Canvas>();
            _hadCanvasBefore = false; // foi criado agora
        }
        // Se houver outro Canvas no mesmo GO, também precisamos de um GraphicRaycaster
        if (mapPanelRoot.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            mapPanelRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Guarda estado se já existia
        if (_hadCanvasBefore)
        {
            _prevOverrideSorting = _mapCanvas.overrideSorting;
            _prevSortingOrder = _mapCanvas.sortingOrder;
        }

        // Força ficar por cima de QUALQUER outro canvas
        _mapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _mapCanvas.overrideSorting = true;
        _mapCanvas.sortingOrder = 1000;   // bem alto para sobrepor o Pause

        // (3) Liga painel e filhos críticos
        if (mapPanelRoot) mapPanelRoot.SetActive(true);
        if (mapRect) mapRect.gameObject.SetActive(true);
        if (playerIcon) playerIcon.gameObject.SetActive(true);

        // (4) Debounce
        IsMapOpen = true;
        _canCloseAtTime = Time.unscaledTime + ignoreCloseInputSeconds;

        if (playerTransform && mapRect && playerIcon)
            UpdatePlayerIconPosition();

        Debug.Log("[MAPA] ABERTO (Canvas próprio, overrideSorting=TRUE, order=1000, Pause oculto).");
    }

    public void HideMap()
    {
        IsMapOpen = false;

        // (1) Desliga painel
        if (mapPanelRoot) mapPanelRoot.SetActive(false);

        // (2) Restaura Pause
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);

        // (3) Se o Canvas já existia antes, restaura estado; se foi criado só para o mapa, pode manter (não atrapalha)
        if (_mapCanvas != null && _hadCanvasBefore)
        {
            _mapCanvas.overrideSorting = _prevOverrideSorting;
            _mapCanvas.sortingOrder = _prevSortingOrder;
        }

        Debug.Log("[MAPA] FECHADO (Pause reexibido, MapPanel por baixo).");
    }

    private void UpdatePlayerIconPosition()
    {
        Vector3 p = playerTransform.position;

        float tX = Mathf.InverseLerp(worldMin.x, worldMax.x, p.x);
        float tZ = Mathf.InverseLerp(worldMin.y, worldMax.y, p.z);

        tX = Mathf.Clamp01(tX);
        tZ = Mathf.Clamp01(tZ);

        Vector2 size = mapRect.rect.size;
        float x = (tX - 0.5f) * size.x;
        float y = (tZ - 0.5f) * size.y;

        playerIcon.anchoredPosition = new Vector2(x, y);
    }
}