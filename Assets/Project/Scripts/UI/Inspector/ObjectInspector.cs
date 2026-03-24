using UnityEngine;
using TMPro;

/// <summary>
/// Sistema universal de inspeção com câmera dedicada.
/// Coloque no Player. Usa uma câmera separada + ponto de inspeção,
/// sem conflito com Cinemachine.
/// </summary>
public class ObjectInspector : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("A Main Camera (a que o Cinemachine controla). Usada apenas para o raycast. Se vazio, usa Camera.main.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Câmera dedicada para inspeção (InspectCamera). Começa DESLIGADA.")]
    [SerializeField] private Camera inspectCamera;

    [Tooltip("Ponto onde o objeto será posicionado na frente da InspectCamera.")]
    [SerializeField] private Transform inspectPoint;

    [SerializeField] private PlayerInventory playerInventory;

    [Header("Raycast")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask inspectableLayer;

    [Header("UI — Hint (mesmo padrão do HintTrigger)")]
    [Tooltip("Painel de hint (GameObject) que será ativado/desativado.")]
    [SerializeField] private GameObject hintPanel;

    [Tooltip("Texto dentro do painel de hint.")]
    [SerializeField] private TMP_Text hintText;

    [Header("Inspeção")]
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Inputs")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Inputs — Controle (opcional)")]
    [SerializeField] private bool usarControle = false;
    [SerializeField] private string interactButton = "Submit";

    // Estado
    private InspectableObject inspectingObject;
    private bool isInspecting = false;
    private bool wasHandling = false;
    private float previousTimeScale = 1f;

    public static bool IsInspecting { get; private set; } = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("[ObjectInspector] Main Camera não encontrada! Arraste no Inspector ou garanta que existe uma câmera com tag MainCamera.");

        if (inspectCamera == null)
            Debug.LogError("[ObjectInspector] InspectCamera NÃO atribuída! Arraste a câmera de inspeção no Inspector.");

        if (inspectPoint == null)
            Debug.LogError("[ObjectInspector] InspectPoint NÃO atribuído! Arraste o ponto de inspeção no Inspector.");

        // Garante que a câmera de inspeção começa desligada
        if (inspectCamera != null)
            inspectCamera.gameObject.SetActive(false);

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void Update()
    {
        if (isInspecting)
        {
            HandleInspection();
            return;
        }

        HandleDetection();
    }

    // ═══════════════ DETECÇÃO (RAYCAST) ═══════════════

    private void HandleDetection()
    {
        // Não detecta se outro painel estiver aberto
        if (UIPanelManager.Instance != null && UIPanelManager.Instance.AnyPanelOpen)
        {
            // HideHint();
            // return;
        }

        if (mainCamera == null) return;

        // Raycast do CENTRO da Main Camera
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, inspectableLayer))
        {
            var inspectable = hit.collider.GetComponent<InspectableObject>();
            if (inspectable == null)
                inspectable = hit.collider.GetComponentInParent<InspectableObject>();

            if (inspectable != null)
            {
                ShowHint($"[E] Inspecionar {inspectable.displayName}");

                if (GetInteractDown())
                    StartInspection(inspectable);

                return;
            }
        }

        // HideHint();
    }

    // ═══════════════ INSPEÇÃO ═══════════════

    private void StartInspection(InspectableObject obj)
    {
        if (inspectCamera == null || inspectPoint == null)
        {
            Debug.LogError("[ObjectInspector] Faltam referências (InspectCamera ou InspectPoint). Inspeção cancelada.");
            return;
        }

        isInspecting = true;
        IsInspecting = true;
        inspectingObject = obj;

        obj.SaveOriginalTransform();

        // Desabilita colliders para não colidir
        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Desabilita Rigidbody
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // Desparentar e TELEPORTAR para o ponto de inspeção
        obj.transform.SetParent(null);
        obj.transform.position = inspectPoint.position;

        // Liga a câmera de inspeção
        inspectCamera.gameObject.SetActive(true);

        // PAUSA — usa SetPaused que já seta timeScale = 0
        previousTimeScale = Time.timeScale;
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetPaused(true);
        else
            Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        obj.PlayPickupSound();

        ShowHint("[E] Soltar  |  Arraste para girar");

        Debug.Log($"[ObjectInspector] Inspeção INICIADA: {obj.displayName}. Camera ativa: {inspectCamera.gameObject.activeSelf}");
    }

    private void HandleInspection()
    {
        if (inspectingObject == null)
        {
            EndInspection(false);
            return;
        }

        // Mantém o objeto na posição do ponto de inspeção
        inspectingObject.transform.position = inspectPoint.position;

        // Rotaciona com o mouse
        bool isHandling = Input.GetMouseButton(0);

        if (isHandling)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * inspectingObject.rotationSpeed * Time.unscaledDeltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * inspectingObject.rotationSpeed * Time.unscaledDeltaTime;

            inspectingObject.transform.Rotate(inspectCamera.transform.up, -mouseX, Space.World);
            inspectingObject.transform.Rotate(inspectCamera.transform.right, mouseY, Space.World);

            if (!wasHandling)
                inspectingObject.StartHandleSound();
        }
        else
        {
            if (wasHandling)
                inspectingObject.StopHandleSound();
        }

        wasHandling = isHandling;

        if (GetInteractDown() || Input.GetKeyDown(cancelKey))
        {
            EndInspection(inspectingObject.collectAfterInspect);
        }
    }

    private void EndInspection(bool collect)
    {
        if (inspectingObject != null)
        {
            inspectingObject.StopHandleSound();
            inspectingObject.PlayReleaseSound();

            // Reabilita colliders
            foreach (var col in inspectingObject.GetComponentsInChildren<Collider>())
                col.enabled = true;

            if (collect && playerInventory != null)
            {
                CollectObject(inspectingObject);
                Destroy(inspectingObject.gameObject, 0.1f);
            }
            else
            {
                inspectingObject.RestoreOriginalTransform();
            }
        }

        inspectingObject = null;
        isInspecting = false;
        IsInspecting = false;
        wasHandling = false;

        // Desliga a câmera de inspeção
        if (inspectCamera != null)
            inspectCamera.gameObject.SetActive(false);

        // DESPAUSA — volta para Gameplay com timeScale = 1
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetPaused(false);
        else
            Time.timeScale = previousTimeScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        HideHint();

        Debug.Log("[ObjectInspector] Inspeção ENCERRADA.");
    }

    private void CollectObject(InspectableObject obj)
    {
        switch (obj.collectItemType)
        {
            case ItemType.Battery:
                playerInventory.AddBattery(obj.collectBatteryAmount);
                break;
            case ItemType.Key:
                if (!string.IsNullOrEmpty(obj.collectKeyId))
                    playerInventory.AddKey(obj.collectKeyId);
                break;
            case ItemType.Note:
                if (obj.collectNoteData != null)
                    playerInventory.AddNote(obj.collectNoteData);
                break;
        }
    }

    private bool GetInteractDown()
    {
        if (Input.GetKeyDown(interactKey)) return true;
        if (usarControle && Input.GetButtonDown(interactButton)) return true;
        return false;
    }

    // ═══════════════ UI HINT (padrão HintTrigger) ═══════════════

    private void ShowHint(string message)
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = message;
            hintPanel.SetActive(true);
        }
    }

    private void HideHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
}