using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum HotbarItemType
{
    None,
    Battery,
    Ammo,      // novo tipo para munição
    Health,    // novo tipo para cura/vida
    Notes,
    NotesCamera
}

public class HotbarSlotUI : MonoBehaviour
{
    [Header("Configuração do slot")]
    [SerializeField] private HotbarItemType itemType = HotbarItemType.None;
    [SerializeField] private string displayName = "Item";

    [Header("Dados de nota (apenas se ItemType = Notes)")]
    [Tooltip("Se quiser que este slot abra SEMPRE uma nota específica (no NoteViewerUI), arraste aqui. Se deixar vazio, ele abre a primeira nota do inventário.")]
    [SerializeField] private NoteData fixedNote;

    [Header("Sistema de notas (NoteViewerUI)")]
    [Tooltip("Arraste aqui o NoteViewerUI (para ItemType = Notes).")]
    [SerializeField] private NoteViewerUI noteViewerUI;

    [Header("Referências visuais")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image selectionHighlight;

    public HotbarItemType ItemType => itemType;

    private void Awake()
    {
        if (nameText != null)
            nameText.text = displayName;
        else
            Debug.LogWarning("[HotbarSlotUI] nameText não atribuído no slot: " + gameObject.name);

        UpdateQuantity(0);
        SetSelected(false);
    }

    /// <summary>
    /// Atualiza quantidade a partir do inventário lógico.
    /// Battery -> número de pilhas
    /// Ammo    -> número de munição
    /// Health  -> vida atual do player
    /// Notes / NotesCamera   -> quantidade de notas
    /// </summary>
    public void RefreshFromInventory(PlayerInventory inv)
    {
        if (inv == null)
        {
            Debug.LogWarning("[HotbarSlotUI] PlayerInventory nulo em RefreshFromInventory no slot: " + gameObject.name);
            UpdateQuantity(0);
            return;
        }

        int qty = 0;

        switch (itemType)
        {
            case HotbarItemType.Battery:
                qty = inv.GetBatteryCount();
                break;
            case HotbarItemType.Ammo:
                qty = inv.GetAmmoCount();
                break;
            case HotbarItemType.Health:
                // Busca o PlayerStats no mesmo GameObject do PlayerInventory
                PlayerStats stats = inv.GetComponent<PlayerStats>();
                qty = stats != null ? stats.CurrentHealth : 0;
                break;
            case HotbarItemType.Notes:
            case HotbarItemType.NotesCamera:
                qty = inv.GetAllNotes().Count;
                break;
        }

        UpdateQuantity(qty);
    }

    public void UpdateQuantity(int q)
    {
        if (quantityText != null)
            quantityText.text = q.ToString();
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;
    }

    public NoteData GetNoteData() => fixedNote;

    /// <summary>
    /// Chamado pelo InventoryUI quando o slot atual é do tipo Notes (UI de texto).
    /// O próprio slot sabe como abrir o NoteViewerUI.
    /// </summary>
    public void OpenNotePanelFromSlot()
    {
        if (itemType != HotbarItemType.Notes)
        {
            Debug.Log("[HotbarSlotUI] OpenNotePanelFromSlot chamado em slot que não é Notes: " + gameObject.name);
            return;
        }

        if (noteViewerUI == null)
        {
            Debug.LogWarning("[HotbarSlotUI] noteViewerUI NÃO atribuído no slot de notas (UI): " + gameObject.name +
                             ". Arraste o objeto que tem o NoteViewerUI aqui no Inspector.");
            return;
        }

        Debug.Log("[HotbarSlotUI] Abrindo painel de notas (UI) a partir do slot: " + gameObject.name +
                  ". fixedNote=" + (fixedNote != null ? fixedNote.title : "null (usar primeira nota do inventário)"));

        // Se fixedNote for null, o NoteViewerUI.ShowNote(null) abre a primeira nota do inventário.
        noteViewerUI.ShowNote(fixedNote);
    }

    /// <summary>
    /// Chamado pelo InventoryUI quando o slot atual é do tipo NotesCamera (slot 4).
    /// Usa o NoteCineController para ativar a Cinemachine da área de notas.
    /// </summary>
    public void OpenNoteCineFromSlot()
    {
        if (itemType != HotbarItemType.NotesCamera)
        {
            Debug.Log("[HotbarSlotUI] OpenNoteCineFromSlot chamado em slot que não é NotesCamera: " + gameObject.name);
            return;
        }

        if (NoteCineController.Instance == null)
        {
            Debug.LogWarning("[HotbarSlotUI] NoteCineController.Instance é nulo. Verifique se existe um NoteCineController na cena.");
            return;
        }

        Debug.Log("[HotbarSlotUI] Abrindo área de notas (Cinemachine) a partir do slot: " + gameObject.name);
        NoteCineController.Instance.ShowNoteArea();
    }
}