using UnityEngine;
using TMPro;

public class Safe : MonoBehaviour
{
    [Header("Identificação do Cofre")]
    [SerializeField] private string safeName = "Cofre";

    [Header("Senha do Cofre (8 dígitos)")]
    [SerializeField] private string correctCode = "12345678";

    [Header("UI de Dica (opcional)")]
    [SerializeField] private GameObject hintPanel;   // painel "Pressione E..."
    [SerializeField] private TMP_Text hintText;      // texto da dica

    [Header("Referência à UI do Cofre")]
    [SerializeField] private SafeUIController safeUIController;

    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        // DEBUG inicial
        Debug.Log($"[Safe:{safeName}] Start() chamado. Senha configurada: {correctCode}");

        // Verificar tamanho da senha
        if (correctCode.Length != 8)
        {
            Debug.LogWarning($"[Safe:{safeName}] A senha não tem 8 dígitos. Valor atual: '{correctCode}'");
        }

        // Conferir se o collider é trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] NÃO há Collider neste GameObject. O OnTriggerEnter nunca será chamado.");
        }
        else
        {
            Debug.Log($"[Safe:{safeName}] Collider encontrado. IsTrigger = {col.isTrigger}");
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log($"[Safe:{safeName}] HintPanel atribuído e desativado no Start.");
        }
        else
        {
            Debug.LogWarning($"[Safe:{safeName}] HintPanel NÃO atribuído no Inspector.");
        }

        if (hintText == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] HintText NÃO atribuído no Inspector.");
        }

        if (safeUIController == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] SafeUIController NÃO atribuído no Inspector.");
        }
    }

    private void Update()
    {
        // DEBUG de estado
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[Safe:{safeName}] Tecla E pressionada com playerInRange = true, isOpen = false. Tentando abrir UI.");
            TryOpenSafeUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Safe:{safeName}] OnTriggerEnter com '{other.name}', tag = {other.tag}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Safe:{safeName}] OnTriggerEnter ignorado. Tag não é Player.");
            return;
        }

        playerInRange = true;
        Debug.Log($"[Safe:{safeName}] Player ENTROU na área do cofre. playerInRange = true");
        ShowHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Safe:{safeName}] OnTriggerExit com '{other.name}', tag = {other.tag}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Safe:{safeName}] OnTriggerExit ignorado. Tag não é Player.");
            return;
        }

        playerInRange = false;
        Debug.Log($"[Safe:{safeName}] Player SAIU da área do cofre. playerInRange = false");
        ShowHint(false);
    }

    private void ShowHint(bool show)
    {
        Debug.Log($"[Safe:{safeName}] ShowHint({show}) chamado.");

        if (hintPanel != null)
        {
            hintPanel.SetActive(show);
            Debug.Log($"[Safe:{safeName}] HintPanel.SetActive({show})");
        }
        else
        {
            Debug.LogWarning($"[Safe:{safeName}] HintPanel é nulo. Não foi possível SetActive.");
        }

        if (show && hintText != null)
        {
            hintText.text = $"Pressione E para interagir com {safeName}";
            Debug.Log($"[Safe:{safeName}] HintText atualizado.");
        }
    }

    private void TryOpenSafeUI()
    {
        if (safeUIController == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] Não foi possível abrir UI do cofre. SafeUIController é nulo.");
            return;
        }

        Debug.Log($"[Safe:{safeName}] Chamando SafeUIController.OpenSafe(this).");
        safeUIController.OpenSafe(this);
        ShowHint(false);
    }

    // ===== Métodos usados pela UI =====

    public string GetCorrectCode()
    {
        Debug.Log($"[Safe:{safeName}] GetCorrectCode() chamado.");
        return correctCode;
    }

    public void OnCorrectCodeEntered()
    {
        isOpen = true;
        Debug.Log($"[Safe:{safeName}] CÓDIGO CORRETO! Cofre aberto.");

        // Desativa o collider para não disparar mais trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log($"[Safe:{safeName}] Collider desativado após abrir cofre.");
        }

        // Aqui você pode colocar animação, spawn de item, etc.
    }

    public void OnWrongCodeEntered()
    {
        Debug.Log($"[Safe:{safeName}] Código ERRADO. Cofre permanece fechado.");
    }
}