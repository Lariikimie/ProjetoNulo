using UnityEngine;

/// <summary>
/// Tipos de painéis de UI que NÃO podem ficar abertos ao mesmo tempo.
/// Adicione aqui conforme for criando novos sistemas de painel.
/// </summary>
public enum UIPanelType
{
    None = 0,
    Inventory = 1,
    Pause = 2,
    Map = 3,
    NoteViewer = 4,
    Dialogue = 5,
    MainMenu = 6
    // Você pode adicionar mais tipos depois, se precisar
}

/// <summary>
/// Gerenciador central para garantir que apenas UM painel bloqueante
/// (Inventory, Pause, Map, MainMenu, etc.) fique aberto por vez.
/// 
/// Uso típico em um painel:
/// if (!UIPanelManager.Instance.TryOpen(UIPanelType.Inventory)) return;
/// // abrir inventário
/// 
/// Ao fechar:
/// UIPanelManager.Instance.Close(UIPanelType.Inventory);
/// </summary>
public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }

    [Header("Estado atual (debug)")]
    [SerializeField] private UIPanelType currentPanel = UIPanelType.None;

    public UIPanelType CurrentPanel => currentPanel;
    public bool AnyPanelOpen => currentPanel != UIPanelType.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIPanelManager] Já existe uma instância, destruindo duplicata.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Se quiser que continue entre cenas de menu e jogo:
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Verifica se é permitido abrir o painel deste tipo, considerando o painel já aberto.
    /// </summary>
    public bool CanOpen(UIPanelType type)
    {
        if (type == UIPanelType.None)
            return true;

        // Nenhum painel aberto -> pode abrir
        if (currentPanel == UIPanelType.None)
            return true;

        // Já está aberto o MESMO tipo -> ok (idempotente)
        if (currentPanel == type)
            return true;

        // Outro painel já está aberto -> bloqueia
        Debug.Log($"[UIPanelManager] Bloqueado abrir {type}, já está aberto {currentPanel}.");
        return false;
    }

    /// <summary>
    /// Tenta abrir o painel deste tipo.
    /// Retorna true se conseguiu (ou se esse painel já está aberto),
    /// false se há outro painel bloqueando.
    /// </summary>
    public bool TryOpen(UIPanelType type)
    {
        if (!CanOpen(type))
            return false;

        currentPanel = type;
        Debug.Log($"[UIPanelManager] Painel aberto: {currentPanel}");
        return true;
    }

    /// <summary>
    /// Informa que o painel deste tipo foi fechado.
    /// Se ele for o painel atual, o estado volta para None.
    /// </summary>
    public void Close(UIPanelType type)
    {
        if (currentPanel != type)
        {
            // Não é o painel registrado como atual, ignora silenciosamente
            return;
        }

        currentPanel = UIPanelType.None;
        Debug.Log($"[UIPanelManager] Painel fechado: {type}");
    }

    /// <summary>
    /// Fecha qualquer painel atual (reseta o estado).
    /// </summary>
    public void ForceCloseAll()
    {
        Debug.Log($"[UIPanelManager] ForceCloseAll. Painel anterior: {currentPanel}");
        currentPanel = UIPanelType.None;
    }
}
