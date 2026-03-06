using UnityEngine;

// Tipo de item que este pickup representa
public enum ItemType
{
    Battery,
    Key,
    Note
}

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
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

    [SerializeField] private bool usarControle = false;
    [SerializeField] private string interactButton = "Submit"; // se quiser usar botão do controle depois

    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    private void Reset()
    {
        // Garante que o collider é gatilho
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory == null)
            {
                Debug.LogWarning("[ItemPickup] Player não tem PlayerInventory!");
            }
            else
            {
                Debug.Log("[ItemPickup] Player entrou na área de " + displayName);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }

    private void Update()
    {
        if (!playerInRange || playerInventory == null)
            return;

        // Teclado: tecla E
        bool interactKey = Input.GetKeyDown(KeyCode.E);

        // Controle: botão configurado no Input Manager (Submit), se quiser ativar
        bool interactButtonPressed = usarControle && Input.GetButtonDown(interactButton);

        if (interactKey || interactButtonPressed)
        {
            Pickup();
        }
    }

    private void Pickup()
    {
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

        // Depois de pegar, some da cena
        Destroy(gameObject);
    }
}