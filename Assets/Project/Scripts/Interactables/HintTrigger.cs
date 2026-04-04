using UnityEngine;
using TMPro;

public class HintTrigger : MonoBehaviour
{
    [Header("Configuração da Dica")]
    [Tooltip("ID da dica para o HintManager (ex: 'Hint_Moviment', 'Hint_Lanterna')")]
    [SerializeField] private string hintId = "Hint_Moviment";

    [Tooltip("Texto que será exibido quando o player entrar neste trigger.")]
    [TextArea(2, 5)]
    [SerializeField] private string hintMessage = "Texto da dica aqui";

    [Header("Referências de UI (opcional, se quiser mostrar texto localmente também)")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;

    private int playerCollidersInside = 0;

    private void Reset()
    {
        if (hintPanel == null)
        {
            GameObject panel = GameObject.Find("HintPanel");
            if (panel != null)
                hintPanel = panel;
        }

        if (hintText == null && hintPanel != null)
        {
            hintText = hintPanel.GetComponentInChildren<TMP_Text>();
        }
    }

    private void Start()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Chama o HintManager para mostrar a dica (apenas se ainda não foi mostrada)
        if (HintManager.Instance != null)
            HintManager.Instance.ShowHint(hintId);

        playerCollidersInside++;
        if (playerCollidersInside == 1)
        {
            ShowHintLocal();
        }
    }

    private void OnTriggerExit(Collider other)
{
    if (!other.CompareTag("Player"))
        return;

    playerCollidersInside--;
    if (playerCollidersInside <= 0)
    {
        playerCollidersInside = 0;
        HideHintLocal();

        // Marca como mostrado e destrói o objeto associado
        if (HintManager.Instance != null)
            HintManager.Instance.MarkHintAsShown(hintId);
    }
}

    // Exibe o painel local (opcional, se quiser mostrar texto na tela além do HintManager)
    private void ShowHintLocal()
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = hintMessage;
            hintPanel.SetActive(true);
            Debug.Log("[HintTrigger] Mostrando dica local: " + hintMessage);
        }
    }

    private void HideHintLocal()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log("[HintTrigger] Escondendo dica local.");
        }
    }
}