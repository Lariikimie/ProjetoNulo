using UnityEngine;

/// <summary>
/// Controla uma porta simples que pode estar trancada por uma chave.
/// - Usa um collider com IsTrigger para detectar o player.
/// - Aperta E (teclado) ou botão "Submit" (controle) para interagir.
/// - Se precisar de chave, verifica no PlayerInventory.
/// - Abre/fecha rotacionando o Transform da porta em Y.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Transform da porta que será rotacionado (pode ser o próprio objeto ou um filho com o modelo).")]
    [SerializeField] private Transform doorTransform;

    [Header("Configuração")]
    [Tooltip("A porta começa aberta?")]
    [SerializeField] private bool startOpen = false;

    [Tooltip("A porta precisa de chave para abrir pela primeira vez?")]
    [SerializeField] private bool requiresKey = true;

    [Tooltip("ID da chave necessária (deve combinar com o keyId do ItemPickup de chave).")]
    [SerializeField] private string requiredKeyId = "ChaveDiretoria";

    [Tooltip("Ângulo de abertura da porta em Y (em graus). Ex: 90 ou -90.")]
    [SerializeField] private float openAngle = 90f;

    [Header("Interação")]
    [SerializeField] private bool usarControle = false;

    [Tooltip("Nome do botão de interação no Input Manager (para controle).")]
    [SerializeField] private string interactButton = "Submit"; // botão A / Enter, se configurado

    private bool isOpen;
    private bool isLocked; // travada por chave ou não
    private bool playerInRange;
    private PlayerInventory playerInventory;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private void Reset()
    {
        // Garante que o collider é Trigger para detectar o player
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Se não foi setado no Inspector, assume que a porta é o próprio objeto
        if (doorTransform == null)
            doorTransform = transform;
    }

    private void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        closedRotation = doorTransform.localRotation;
        openedRotation = Quaternion.Euler(
            doorTransform.localEulerAngles.x,
            doorTransform.localEulerAngles.y + openAngle,
            doorTransform.localEulerAngles.z
        );
    }

    private void Start()
    {
        isOpen = startOpen;

        // Se a porta precisa de chave, começa travada. Senão, destravada.
        isLocked = requiresKey;

        // Aplica rotação inicial
        if (isOpen)
            doorTransform.localRotation = openedRotation;
        else
            doorTransform.localRotation = closedRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory == null)
            {
                Debug.LogWarning("[DoorController] Player não tem PlayerInventory.");
            }
            else
            {
                Debug.Log("[DoorController] Player aproximou da porta.");
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

        // Teclado
        bool interactKey = Input.GetKeyDown(KeyCode.E);

        // Controle (se habilitado)
        bool interactBtn = usarControle && Input.GetButtonDown(interactButton);

        if (interactKey || interactBtn)
        {
            Interact();
        }
    }

    private void Interact()
    {
        // Se a porta está fechada, tentamos abrir
        if (!isOpen)
        {
            TryOpen();
        }
        else
        {
            // Se já está aberta, só fechamos (não precisa de chave para fechar)
            CloseDoor();
        }
    }

    private void TryOpen()
    {
        if (isLocked)
        {
            if (!requiresKey)
            {
                // Nunca deveria acontecer, mas só por segurança
                isLocked = false;
            }
            else
            {
                // Verifica se o player tem a chave
                if (!playerInventory.HasKey(requiredKeyId))
                {
                    Debug.Log("[DoorController] Porta trancada. Precisa da chave: " + requiredKeyId);
                    return;
                }

                // Tem a chave, destranca
                isLocked = false;
                Debug.Log("[DoorController] Porta destrancada com chave: " + requiredKeyId);
            }
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        doorTransform.localRotation = openedRotation;
        Debug.Log("[DoorController] Porta aberta.");
    }

    private void CloseDoor()
    {
        isOpen = false;
        doorTransform.localRotation = closedRotation;
        Debug.Log("[DoorController] Porta fechada.");
    }
}
