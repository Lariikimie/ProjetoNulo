using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public static CheckPointManager Instance { get; private set; }

    [Header("Referências do Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Estado do Checkpoint")]
    [SerializeField] private bool hasCheckpoint = false;
    [SerializeField] private Vector3 checkpointPosition;

    [Header("Inventário salvo no Checkpoint")]
    [SerializeField] private int checkpointBatteryCount = 0;
    [SerializeField] private List<string> checkpointKeys = new List<string>();
    [SerializeField] private List<NoteData> checkpointNotes = new List<NoteData>();

    private void Awake()
    {
        // Singleton simples (opcional, mas útil se quiser acessar por Instance)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tenta achar referências automaticamente se não estiverem setadas no Inspector
        if (playerTransform == null || playerController == null || playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                if (playerTransform == null)
                    playerTransform = playerObj.transform;

                if (playerController == null)
                    playerController = playerObj.GetComponent<CharacterController>();

                if (playerInventory == null)
                    playerInventory = playerObj.GetComponent<PlayerInventory>();
            }
        }
    }

    private void Start()
    {
        // Se quiser que o ponto inicial da fase já seja um checkpoint,
        // basta descomentar este bloco:

        if (playerTransform != null)
        {
            checkpointPosition = playerTransform.position;
            SaveInventorySnapshot();
            hasCheckpoint = true;
            Debug.Log("[Checkpoint] Checkpoint inicial salvo na posição de spawn.");
        }
    }

    /// <summary>
    /// Salva posição + inventário atual do player como novo checkpoint.
    /// Chamada pelo SavePoint ou por sistemas de história.
    /// </summary>
    public void SetCheckpoint(Vector3 pos)
    {
        if (playerTransform == null || playerInventory == null)
        {
            Debug.LogWarning("[Checkpoint] Referências do player não configuradas.");
            return;
        }

        checkpointPosition = pos;
        SaveInventorySnapshot();
        hasCheckpoint = true;

        Debug.Log("[Checkpoint] Novo checkpoint salvo na posição: " + pos);
    }

    /// <summary>
    /// Guarda um "snapshot" do inventário atual.
    /// </summary>
    private void SaveInventorySnapshot()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[Checkpoint] PlayerInventory não atribuído.");
            return;
        }

        // Pilhas
        checkpointBatteryCount = playerInventory.GetBatteryCount();

        // Chaves (cópia da lista, para não ficar apontando pro mesmo objeto)
        checkpointKeys = new List<string>(playerInventory.GetAllKeys());

        // Notas (também faz cópia da lista – os NoteData em si são assets)
        checkpointNotes = new List<NoteData>(playerInventory.GetAllNotes());

        Debug.Log("[Checkpoint] Snapshot de inventário salvo. Pilhas: "
                  + checkpointBatteryCount + " | Chaves: " + checkpointKeys.Count
                  + " | Notas: " + checkpointNotes.Count);
    }

    /// <summary>
    /// Teleporta o player para o último checkpoint e restaura o inventário
    /// exatamente como estava na hora do save.
    /// </summary>
    public void ReturnToLastCheckpoint()
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("[Checkpoint] Nenhum checkpoint salvo ainda.");
            return;
        }

        if (playerTransform == null || playerController == null || playerInventory == null)
        {
            Debug.LogWarning("[Checkpoint] Referências do player não configuradas.");
            return;
        }

        // Desliga o CharacterController para evitar "pulo" estranho
        bool controllerWasEnabled = playerController.enabled;
        playerController.enabled = false;

        // Teleporta o player
        playerTransform.position = checkpointPosition;

        // Restaura inventário completo
        playerInventory.LoadInventoryState(
            checkpointBatteryCount,
            checkpointKeys,
            checkpointNotes
        );

        // Religa o CharacterController
        playerController.enabled = controllerWasEnabled;

        Debug.Log("[Checkpoint] Player retornou ao último checkpoint. Inventário restaurado.");
    }

    /// <summary>
    /// Opcional: limpa dados de checkpoint (por exemplo, ao trocar de fase).
    /// </summary>
    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
        checkpointKeys.Clear();
        checkpointNotes.Clear();
        checkpointBatteryCount = 0;

        Debug.Log("[Checkpoint] Dados de checkpoint limpos.");
    }
}