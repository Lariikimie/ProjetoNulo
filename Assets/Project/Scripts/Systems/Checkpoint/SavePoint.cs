using UnityEngine;
using TMPro;

public class SavePoint : MonoBehaviour
{
    [Header("Nome do Save (opcional, só para texto)")]
    [SerializeField] private string saveName = "Checkpoint";

    [Header("Posição exata de respawn")]
    [SerializeField] private Transform respawnPoint;
    // Se não for preenchido, usa a posição deste GameObject

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI de dica (opcional)")]
    [SerializeField] private GameObject hintPanel;   // painel com fundo
    [SerializeField] private TMP_Text hintText;      // texto "Pressione E..."

    private bool playerInRange = false;

    private void Start()
    {
        // Garante que o painel de dica começa desligado
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        // Aperto da tecla E dentro da área do save
        if (Input.GetKeyDown(interactKey))
        {
            TrySaveCheckpoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        ShowHint(false);
    }

    private void ShowHint(bool show)
    {
        if (hintPanel != null)
            hintPanel.SetActive(show);

        if (show && hintText != null)
        {
            // Exemplo: "Pressione E para salvar no Diário da Diretora"
            hintText.text = $"Pressione {interactKey} para salvar em {saveName}";
        }
    }

    private void TrySaveCheckpoint()
    {
        // Tenta pegar o CheckPointManager pela Instance primeiro
        CheckPointManager manager = CheckPointManager.Instance;

        if (manager == null)
        {
            // Fallback, caso Instance não esteja configurada
            manager = FindObjectOfType<CheckPointManager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("[SavePoint] Nenhum CheckPointManager encontrado na cena.");
            return;
        }

        // Posição de respawn: se tiver um Transform, usa ele; senão, usa a própria posição do SavePoint
        Vector3 pos = transform.position;
        if (respawnPoint != null)
            pos = respawnPoint.position;

        manager.SetCheckpoint(pos);
        Debug.Log("[SavePoint] Checkpoint salvo em: " + pos + " (" + saveName + ")");
    }
}