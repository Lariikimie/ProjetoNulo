using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static bool IsInventoryOpen = false;   // usado pelo PauseMenu para bloquear, se quiser

    [Header("Referências principais")]
    [SerializeField] private GameObject inventoryPanel;     // InventoryPanel
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private NoteViewerUI noteViewerUI;

    [Header("Hotbar")]
    [SerializeField] private List<HotbarSlotUI> hotbarSlots = new List<HotbarSlotUI>();

    [Header("UI – Lado Esquerdo")]
    [SerializeField] private TMP_Text keysListText;
    [SerializeField] private TMP_Text batteryCountText;     // pode ficar null, não causa erro

    [Header("UI – Lista de Notas")]
    [SerializeField] private Transform notesListContainer;   // NotesListContainer
    [SerializeField] private GameObject noteButtonPrefab;    // Prefab com Button + Text/TMP

    [Header("Inputs (abrir/fechar e fallback)")]
    [SerializeField] private KeyCode openInventoryKey = KeyCode.Tab;
    [SerializeField] private KeyCode closeInventoryKey = KeyCode.Tab; // ESC ou Tab – configurável

    [Tooltip("Tecla de usar item (fallback caso não exista UINavigationInput na cena).")]
    [SerializeField] private KeyCode useItemKey = KeyCode.Q;

    [Header("Inputs de navegação (fallback)")]
    [SerializeField] private KeyCode previousKey1 = KeyCode.A;
    [SerializeField] private KeyCode previousKey2 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode nextKey1 = KeyCode.D;
    [SerializeField] private KeyCode nextKey2 = KeyCode.RightArrow;

    [Header("Lanterna / Pilhas")]
    [SerializeField] private BatteryRechargeController batteryRechargeController;

    private int currentIndex = 0;
    private float previousTimeScale = 1f;

    private bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        else
            Debug.LogError("[InventoryUI] InventoryPanel NÃO atribuído no Inspector!");

        if (playerInventory == null)
            Debug.LogWarning("[InventoryUI] PlayerInventory NÃO atribuído no Inspector.");
        if (noteViewerUI == null)
            Debug.LogWarning("[InventoryUI] NoteViewerUI NÃO atribuído no Inspector.");

        RefreshLeftPanel();
        RefreshAllSlots();
        UpdateSelectionVisual();

        Debug.Log("[InventoryUI] Start() concluído. Painel inicial desativado.");
    }

    private void Update()
    {
        HandleOpenCloseInput();

        if (!IsOpen)
            return;

        HandleNavigationInput();
        HandleUseItemInput();
    }

    // ───────────────────────────────── ABRIR / FECHAR ─────────────────────────────────

    private void HandleOpenCloseInput()
    {
        // Toggle pela tecla de abrir
        if (Input.GetKeyDown(openInventoryKey))
        {
            if (!IsOpen) OpenInventory();
            else CloseInventory();
            return;
        }

        // Fechar explicitamente (ESC/Tab – você decide no Inspector)
        if (Input.GetKeyDown(closeInventoryKey))
        {
            if (IsOpen) CloseInventory();
        }
    }

    private void OpenInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryUI] InventoryPanel não atribuído. Não é possível abrir o inventário.");
            return;
        }

        // Pergunta ao UIPanelManager se pode abrir o Inventário
        if (UIPanelManager.Instance != null)
        {
            if (!UIPanelManager.Instance.TryOpen(UIPanelType.Inventory))
            {
                Debug.Log("[InventoryUI] Não abriu inventário, outro painel já ativo: " +
                          UIPanelManager.Instance.CurrentPanel);
                return;
            }
        }

        inventoryPanel.SetActive(true);
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        IsInventoryOpen = true;

        if (hotbarSlots.Count > 0)
            currentIndex = Mathf.Clamp(currentIndex, 0, hotbarSlots.Count - 1);

        Debug.Log($"[InventoryUI] Inventário ABERTO. TimeScale antes={previousTimeScale}, depois={Time.timeScale}");

        RefreshLeftPanel();
        RefreshAllSlots();
        UpdateSelectionVisual();
    }

    private void CloseInventory()
    {
        if (inventoryPanel == null)
            return;

        inventoryPanel.SetActive(false);
        Time.timeScale = previousTimeScale;

        IsInventoryOpen = false;

        // Informa o UIPanelManager que o inventário foi fechado
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.Close(UIPanelType.Inventory);
            Debug.Log($"[InventoryUI] Inventário FECHADO. UIPanelManager painel atual: {UIPanelManager.Instance.CurrentPanel}");
        }
        else
        {
            Debug.Log("[InventoryUI] Inventário FECHADO (sem UIPanelManager).");
        }

        Debug.Log($"[InventoryUI] Inventário FECHADO. TimeScale restaurado para {previousTimeScale}");
    }

    // ───────────────────────────────── NAVEGAÇÃO HOTBAR ─────────────────────────────────

    private void HandleNavigationInput()
    {
        if (hotbarSlots.Count == 0)
            return;

        bool moved = false;

        // Se existir UINavigationInput, usamos ele (A/D + setas + controle)
        if (UINavigationInput.Instance != null)
        {
            if (UINavigationInput.Instance.LeftPressedThisFrame())
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = hotbarSlots.Count - 1;

                moved = true;
                Debug.Log("[InventoryUI] Slot anterior: " + currentIndex);
            }

            if (UINavigationInput.Instance.RightPressedThisFrame())
            {
                currentIndex++;
                if (currentIndex >= hotbarSlots.Count)
                    currentIndex = 0;

                moved = true;
                Debug.Log("[InventoryUI] Próximo slot: " + currentIndex);
            }
        }
        else
        {
            // Fallback: usa as teclas configuradas nesse script
            if (Input.GetKeyDown(previousKey1) || Input.GetKeyDown(previousKey2))
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = hotbarSlots.Count - 1;

                moved = true;
                Debug.Log("[InventoryUI] Slot anterior (fallback): " + currentIndex);
            }

            if (Input.GetKeyDown(nextKey1) || Input.GetKeyDown(nextKey2))
            {
                currentIndex++;
                if (currentIndex >= hotbarSlots.Count)
                    currentIndex = 0;

                moved = true;
                Debug.Log("[InventoryUI] Próximo slot (fallback): " + currentIndex);
            }
        }

        if (moved)
            UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] != null)
            {
                bool selected = (i == currentIndex);
                hotbarSlots[i].SetSelected(selected);
            }
        }
    }

    // ───────────────────────────────── USO DE ITENS (pilha / notas / câmera) ─────────────────────────────────

    private void HandleUseItemInput()
    {
        bool usePressed = false;

        if (UINavigationInput.Instance != null)
        {
            // Confirmar (Q no teclado / botão no controle configurado no UINavigationInput)
            usePressed = UINavigationInput.Instance.ConfirmPressedThisFrame();
        }
        else
        {
            // Fallback: tecla local
            usePressed = Input.GetKeyDown(useItemKey);
        }

        if (usePressed)
        {
            Debug.Log($"[InventoryUI] Usar item (Confirm/Use). currentIndex={currentIndex}");
            UseCurrentSlotItem();
        }
    }

    private void UseCurrentSlotItem()
    {
        if (hotbarSlots.Count == 0 || playerInventory == null)
        {
            if (hotbarSlots.Count == 0)
                Debug.LogWarning("[InventoryUI] Nenhum slot configurado na hotbar.");
            if (playerInventory == null)
                Debug.LogWarning("[InventoryUI] PlayerInventory não atribuído em UseCurrentSlotItem.");
            return;
        }

        if (currentIndex < 0 || currentIndex >= hotbarSlots.Count)
        {
            Debug.LogWarning("[InventoryUI] currentIndex fora dos limites da hotbar: " + currentIndex);
            return;
        }

        HotbarSlotUI slot = hotbarSlots[currentIndex];
        if (slot == null)
        {
            Debug.LogWarning("[InventoryUI] Slot atual é nulo.");
            return;
        }

        Debug.Log($"[InventoryUI] Slot atual index={currentIndex}, tipo={slot.ItemType}");

        switch (slot.ItemType)
        {
            case HotbarItemType.Battery:
                UseBatteryFromSlot(slot);
                break;

            case HotbarItemType.Notes:
                slot.OpenNotePanelFromSlot();
                break;

            case HotbarItemType.NotesCamera:
                slot.OpenNoteCineFromSlot();
                break;

            default:
                Debug.Log("[InventoryUI] Slot sem item configurado (ItemType=None ou outro).");
                break;
        }
    }

    private void UseBatteryFromSlot(HotbarSlotUI slot)
    {
        if (batteryRechargeController == null)
        {
            Debug.LogWarning("[InventoryUI] BatteryRechargeController não atribuído.");
            return;
        }

        bool used = batteryRechargeController.TryUseBattery();
        if (!used)
        {
            Debug.Log("[InventoryUI] Tentativa de usar pilha, mas não foi possível (sem pilhas ou lanterna cheia).");
            return;
        }

        int newCount = playerInventory.GetBatteryCount();
        slot.UpdateQuantity(newCount);
        UpdateBatteryText();

        Debug.Log($"[InventoryUI] Pilha usada. Novas pilhas no inventário: {newCount}");
    }

    // ───────────────────────────────── ATUALIZAÇÃO DO LADO ESQUERDO ─────────────────────────────────

    private void RefreshLeftPanel()
    {
        UpdateKeysText();
        UpdateBatteryText();
        UpdateNotesList();
    }

    private void UpdateKeysText()
    {
        if (keysListText == null || playerInventory == null)
            return;

        List<string> keys = playerInventory.GetAllKeys();

        if (keys.Count == 0)
        {
            keysListText.text = "Chaves:\n- Nenhuma";
            return;
        }

        keysListText.text = "Chaves:\n";
        foreach (string keyId in keys)
            keysListText.text += "- " + keyId + "\n";
    }

    private void UpdateBatteryText()
    {
        if (batteryCountText == null || playerInventory == null)
            return;

        batteryCountText.text = "Pilhas: " + playerInventory.GetBatteryCount();
    }

    private void UpdateNotesList()
    {
        if (notesListContainer == null || noteButtonPrefab == null || playerInventory == null)
        {
            Debug.LogWarning("[InventoryUI] NotesListContainer, NoteButtonPrefab ou PlayerInventory NÃO atribuídos.");
            return;
        }

        // Limpa a lista
        for (int i = notesListContainer.childCount - 1; i >= 0; i--)
            Destroy(notesListContainer.GetChild(i).gameObject);

        List<NoteData> notes = playerInventory.GetAllNotes();
        if (notes == null || notes.Count == 0)
        {
            Debug.Log("[InventoryUI] Nenhuma nota para listar.");
            return;
        }

        foreach (NoteData note in notes)
        {
            if (note == null)
                continue;

            GameObject buttonObj = Instantiate(noteButtonPrefab, notesListContainer);
            buttonObj.SetActive(true);

            // Texto do botão
            TMP_Text tmp = buttonObj.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = note.title;
            else
            {
                Text txt = buttonObj.GetComponentInChildren<Text>();
                if (txt != null) txt.text = note.title;
                else Debug.LogWarning("[InventoryUI] NoteButtonPrefab NÃO tem TMP_Text nem Text!");
            }

            // Clique abre no NoteViewerUI
            Button btn = buttonObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("[InventoryUI] NoteButtonPrefab NÃO tem componente Button!");
                continue;
            }

            NoteData captured = note;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[InventoryUI] Nota selecionada na lista: '{captured.title}'");
                if (noteViewerUI != null) noteViewerUI.ShowNote(captured);
                else Debug.LogWarning("[InventoryUI] noteViewerUI não atribuído.");
            });
        }
    }

    public void RefreshAllSlots()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[InventoryUI] PlayerInventory não atribuído em RefreshAllSlots.");
            return;
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot != null)
                slot.RefreshFromInventory(playerInventory);
        }
    }
}