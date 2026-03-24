using UnityEngine;

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

    private PlayerInventory playerInventory;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    // Chamado pelo sistema de highlight (opcional, se quiser highlight visual)
    public void SetHighlight(bool state)
    {
        // Aqui você pode ativar/desativar highlight visual se quiser
        Debug.Log("[ItemPickup] Highlight " + (state ? "ativado" : "desativado") + " para: " + displayName);
    }

    public void Collect()
    {
        // Busca o inventário do player na cena
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
                break;

            case ItemType.Key:
                if (!string.IsNullOrEmpty(keyId))
                {
                    playerInventory.AddKey(keyId);
                    Debug.Log("[ItemPickup] Pegou chave: " + keyId);
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