using UnityEngine;
using TMPro;

public class HintTrigger : MonoBehaviour
{
    [Header("Configuração da Dica")]
    [Tooltip("Texto que será exibido quando o player entrar neste trigger.")]
    [TextArea(2, 5)]
    [SerializeField] private string hintMessage = "Texto da dica aqui";

    [Header("Referências de UI (mesmo painel para tudo)")]
    [Tooltip("Painel de dica (HintPanel) que já existe no Canvas_HUD.")]
    [SerializeField] private GameObject hintPanel;

    [Tooltip("Texto da dica (Text_Hint - TextMeshProUGUI).")]
    [SerializeField] private TMP_Text hintText;

    private void Reset()
    {
        // Tenta achar automaticamente (só funciona se o painel estiver ATIVO na cena)
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
        // Garante que o painel COMEÇA desativado
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ShowHint();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        HideHint(); // 🔥 sempre desativa o painel ao sair
    }

    private void ShowHint()
    {
        if (hintPanel == null || hintText == null)
        {
            Debug.LogWarning("[HintTrigger] HintPanel ou HintText não configurados no Inspector.");
            return;
        }

        hintText.text = hintMessage;
        hintPanel.SetActive(true);   // 🔥 ativa, mesmo se estava desligado na cena

        Debug.Log("[HintTrigger] Mostrando dica: " + hintMessage);
    }

    private void HideHint()
    {
        if (hintPanel == null)
            return;

        hintPanel.SetActive(false);  // 🔥 desativa sempre que sair da área
        Debug.Log("[HintTrigger] Escondendo dica.");
    }
}