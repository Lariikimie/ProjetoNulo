using UnityEngine;

/// <summary>
/// Controla o uso de pilhas do inventário para recarregar a lanterna.
/// Pode ser acionado:
/// - Pelo próprio script (tecla R / controle), se usarInputTeclaR = true
/// - Pelo InventoryUI, chamando TryUseBattery()
/// </summary>
public class BatteryRechargeController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private FlashlightController flashlight;

    [Header("Configuração de recarga")]
    [Tooltip("Quanto cada pilha recarrega da bateria da lanterna (em unidades, não em porcentagem).")]
    [SerializeField] private float rechargeAmountPerBattery = 50f;

    [Header("Inputs (opcional)")]
    [Tooltip("Se verdadeiro, permite recarregar com a tecla R / botão do controle.")]
    [SerializeField] private bool usarInputTeclaR = false;

    [SerializeField] private bool usarControle = false;
    [SerializeField] private string rechargeButton = "Recharge"; // Botão no Input Manager (opcional)

    private void Reset()
    {
        // Tenta achar referências automaticamente quando o script é adicionado
        playerInventory = GetComponent<PlayerInventory>();
        flashlight = GetComponentInChildren<FlashlightController>();
    }

    private void Update()
    {
        // Se você não quiser mais usar R, deixe usarInputTeclaR = false no Inspector
        if (!usarInputTeclaR)
            return;

        if (playerInventory == null || flashlight == null)
            return;

        // Teclado: tecla R para recarregar
        bool rechargeKey = Input.GetKeyDown(KeyCode.R);

        // Controle: botão configurado (se habilitado)
        bool rechargeButtonPressed = usarControle && Input.GetButtonDown(rechargeButton);

        if (rechargeKey || rechargeButtonPressed)
        {
            TryUseBattery();
        }
    }

    /// <summary>
    /// Tenta usar 1 pilha para recarregar a lanterna.
    /// Pode ser chamado pelo InventoryUI.
    /// Retorna true se recarregou, false se não tinha pilha ou lanterna já cheia.
    /// </summary>
    public bool TryUseBattery()
    {
        if (playerInventory == null || flashlight == null)
        {
            Debug.LogWarning("[Bateria] Falta referência ao PlayerInventory ou FlashlightController.");
            return false;
        }

        // Verifica se tem pilha no inventário
        if (playerInventory.GetBatteryCount() <= 0)
        {
            Debug.Log("[Bateria] Não há pilhas no inventário.");
            return false;
        }

        // Verifica se a bateria já não está cheia
        float currentPercent = flashlight.GetBatteryPercent();
        if (currentPercent >= 0.99f)
        {
            Debug.Log("[Bateria] Lanterna já está praticamente cheia.");
            return false;
        }

        // Usa 1 pilha do inventário
        bool used = playerInventory.UseBattery(1);
        if (!used)
        {
            Debug.Log("[Bateria] Falha ao usar pilha (inventário informou que não tem).");
            return false;
        }

        // Recarrega a lanterna
        flashlight.RechargeBattery(rechargeAmountPerBattery);
        Debug.Log("[Bateria] Usou 1 pilha. Lanterna recarregada em " + rechargeAmountPerBattery + " unidades.");

        return true;
    }
}