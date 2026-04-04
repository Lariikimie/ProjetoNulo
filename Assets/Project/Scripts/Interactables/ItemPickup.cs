using UnityEngine;
using TMPro;

public enum ItemType
{
    Battery,
    Key,
    Note
}

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour, IPickup
{
    [Header("Configuração do Item")]
    public ItemType itemType = ItemType.Battery;

    [Tooltip("Nome/ID da chave (ex: 'ChaveDiretoria', 'ChaveLaboratorio')")]
    public string keyId;

    [Tooltip("Quantidade de pilhas que este item dá (para Battery)")]
    public int batteryAmount = 1;

    [Tooltip("Referência à nota que será adicionada ao diário (para Note)")]
    public NoteData noteData;

    [Header("Interação")]
    [Tooltip("Mensagem só para debug")]
    public string displayName = "Item";

    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private string hintMessage = "Pressione TAB para abrir o inventário.";

    private PlayerInventory playerInventory;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void SetHighlight(bool state)
    {
        Debug.Log("[ItemPickup] Highlight " + (state ? "ativado" : "desativado") + " para: " + displayName);
    }

   public void Collect()
{
    if (playerInventory == null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInventory = player.GetComponent<PlayerInventory>();
    }

    if (playerInventory == null)
    {
        Debug.LogWarning("[ItemPickup] PlayerInventory não encontrado!");
        return;
    }


    switch (itemType)
    {
        case ItemType.Battery:
            playerInventory.AddBattery(batteryAmount);
            Debug.Log("[ItemPickup] Pegou pilha x" + batteryAmount);
            // Toca som de pegar pilha
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D("ColocarPilhasTaser");
            if (PlayerPrefs.GetInt("Hint_Inventory", 0) == 0 && HintManager.Instance != null)
            {
                HintManager.Instance.ShowHint("abrir_inventario"); // Ativa o painel da HUD
                PlayerPrefs.SetInt("ShowInventoryTutorialText", 1);
                PlayerPrefs.SetInt("Hint_Inventory", 1);
            }
            break;

        case ItemType.Key:
            if (!string.IsNullOrEmpty(keyId))
            {
                playerInventory.AddKey(keyId);
                Debug.Log("[ItemPickup] Pegou chave: " + keyId);
                // Toca som de pegar chave
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX2D("PegarChaves");
            }
            else
            {
                Debug.LogWarning("[ItemPickup] keyId está vazio para um item do tipo Key.");
            }
            break;

        case ItemType.Note:
            if (noteData != null)
            {
                playerInventory.AddNote(noteData);
                Debug.Log("[ItemPickup] Pegou nota: " + noteData.title);
                // Toca som de pegar nota
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX2D("PegarNota");
            }
            else
            {
                Debug.LogWarning("[ItemPickup] noteData não atribuído para um item do tipo Note.");
            }
            break;
    }

    Destroy(gameObject);
}

}