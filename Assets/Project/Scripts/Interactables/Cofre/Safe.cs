using UnityEngine;
using TMPro;

public class Safe : MonoBehaviour
{
    [Header("Identifica��o do Cofre")]
    [SerializeField] private string safeName = "Cofre";

    [Header("Senha do Cofre (8 d�gitos)")]
    [SerializeField] private string correctCode = "12345678";

    [Header("UI de Dica (opcional)")]
    [SerializeField] private GameObject hintPanel;   // painel "Pressione E..."
    [SerializeField] private TMP_Text hintText;      // texto da dica

    [Header("Refer�ncia � UI do Cofre")]
    [SerializeField] private SafeUIController safeUIController;

    [Header("Itens dentro do Cofre")]
    [SerializeField] private GameObject[] itemsToActivate; // Items desativados que serao ativados

    [Header("Modelo do Cofre (opcional)")]
    [SerializeField] private GameObject safeModelGameObject; // Arraste o modelo visual do cofre aqui

    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        // DEBUG inicial
        Debug.Log($"[Safe:{safeName}] Start() chamado. Senha configurada: {correctCode}");

        // Verificar tamanho da senha
        if (correctCode.Length != 8)
        {
            Debug.LogWarning($"[Safe:{safeName}] A senha n�o tem 8 d�gitos. Valor atual: '{correctCode}'");
        }

        // Conferir se o collider � trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] N�O h� Collider neste GameObject. O OnTriggerEnter nunca ser� chamado.");
        }
        else
        {
            Debug.Log($"[Safe:{safeName}] Collider encontrado. IsTrigger = {col.isTrigger}");
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log($"[Safe:{safeName}] HintPanel atribu�do e desativado no Start.");
        }
        else
        {
            Debug.LogWarning($"[Safe:{safeName}] HintPanel N�O atribu�do no Inspector.");
        }

        if (hintText == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] HintText N�O atribu�do no Inspector.");
        }

        if (safeUIController == null)
        {
            Debug.LogWarning($"[Safe:{safeName}] SafeUIController N�O atribu�do no Inspector.");
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
            Debug.Log($"[Safe:{safeName}] OnTriggerEnter ignorado. Tag n�o � Player.");
            return;
        }

        playerInRange = true;
        Debug.Log($"[Safe:{safeName}] Player ENTROU na �rea do cofre. playerInRange = true");
        ShowHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Safe:{safeName}] OnTriggerExit com '{other.name}', tag = {other.tag}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Safe:{safeName}] OnTriggerExit ignorado. Tag n�o � Player.");
            return;
        }

        playerInRange = false;
        Debug.Log($"[Safe:{safeName}] Player SAIU da �rea do cofre. playerInRange = false");
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
            Debug.LogWarning($"[Safe:{safeName}] HintPanel � nulo. N�o foi poss�vel SetActive.");
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
            Debug.LogWarning($"[Safe:{safeName}] N�o foi poss�vel abrir UI do cofre. SafeUIController � nulo.");
            return;
        }

        Debug.Log($"[Safe:{safeName}] Chamando SafeUIController.OpenSafe(this).");
        safeUIController.OpenSafe(this);
        ShowHint(false);
    }

    // ===== M�todos usados pela UI =====

    public string GetCorrectCode()
    {
        Debug.Log($"[Safe:{safeName}] GetCorrectCode() chamado.");
        return correctCode;
    }

    public void OnCorrectCodeEntered()
    {
        isOpen = true;
        Debug.Log($"[Safe:{safeName}] C�DIGO CORRETO! Cofre aberto.");

        // Ativa os items desativados
        if (itemsToActivate != null && itemsToActivate.Length > 0)
        {
            foreach (GameObject item in itemsToActivate)
            {
                if (item != null)
                {
                    item.SetActive(true);
                    Debug.Log($"[Safe:{safeName}] Item ativado: {item.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Safe:{safeName}] Nenhum item configurado para ativar!");
        }

        // Aguarda um pouco antes de destruir (para permitir animações, sons, etc)
        Invoke(nameof(DestroyThis), 0.5f);
    }

    private void DestroyThis()
    {
        Debug.Log($"[Safe:{safeName}] Destruindo cofre.");

        // Destrói o modelo visual se estiver atribuído
        if (safeModelGameObject != null)
        {
            Debug.Log($"[Safe:{safeName}] Destruindo modelo: {safeModelGameObject.name}");
            Destroy(safeModelGameObject);
        }

        // Destrói o trigger/collider (este GameObject)
        Destroy(gameObject);
    }

    public void OnWrongCodeEntered()
    {
        Debug.Log($"[Safe:{safeName}] C�digo ERRADO. Cofre permanece fechado.");
    }
}