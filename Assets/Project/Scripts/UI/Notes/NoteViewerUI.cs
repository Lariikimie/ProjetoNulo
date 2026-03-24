using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoteViewerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject notePanel;   // NotePanel
    [SerializeField] private TMP_Text titleText;     // TitleText
    [SerializeField] private TMP_Text contentText;   // ContentText

    [Header("Botões (opcional, pode deixar null se não usar)")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button closeButton;

    [Header("Dados")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Visual 2D da Nota")]
    [Tooltip("RawImage de fundo dentro do NotePanel (o 'papel' da nota).")]
    [SerializeField] private RawImage backgroundRawImage;
    [Tooltip("Cor padrão do fundo.")]
    [SerializeField] private Color defaultBackgroundColor = Color.white;

    [Header("Visual 3D (NoteWorld)")]
    [Tooltip("Referência ao NoteWorldView genérico (squad 3D).")]
    [SerializeField] private NoteWorldView noteWorldView;
    [Tooltip("Transform para posicionar a câmera do mundo 3D.")]
    [SerializeField] private Transform cameraPoint;
    [Tooltip("Câmera que mostra o squad 3D.")]
    [SerializeField] private Camera noteCamera;

    private List<NoteData> notes;
    private int currentIndex = -1;
    private bool isOpen = false;
    private float previousTimeScale = 1f;

    private void Start()
    {
        if (notePanel != null)
            notePanel.SetActive(false);
        else
            Debug.LogError("[NoteViewerUI] notePanel NÃO atribuído no Inspector.");

        if (nextButton != null)
            nextButton.onClick.AddListener(NextNote);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousNote);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseNote);

        if (playerInventory == null)
            Debug.LogWarning("[NoteViewerUI] PlayerInventory NÃO atribuído no Inspector (arraste o Player aqui).");
        else
            Debug.Log("[NoteViewerUI] Start() OK. PlayerInventory atribuído: " + playerInventory.name);

        if (backgroundRawImage == null)
            Debug.LogWarning("[NoteViewerUI] backgroundRawImage NÃO atribuído (arraste o RawImage de fundo do NotePanel).");

        if (noteWorldView == null)
            Debug.LogWarning("[NoteViewerUI] noteWorldView NÃO atribuído (arraste o NoteWorldView do squad 3D).");

        if (noteCamera == null)
            Debug.LogWarning("[NoteViewerUI] noteCamera NÃO atribuída (arraste a câmera do mundo 3D).");
    }

    /// <summary>
    /// Chamado pelo InventoryUI/HotbarSlotUI.
    /// Se 'note' for null, tenta abrir a PRIMEIRA nota válida do inventário.
    /// </summary>
    public void ShowNote(NoteData note)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[NoteViewerUI] PlayerInventory não atribuído em ShowNote. Não é possível carregar notas.");
            return;
        }

        // Pega todas as notas atuais do inventário
        List<NoteData> allNotes = playerInventory.GetAllNotes();

        if (allNotes == null)
        {
            Debug.LogWarning("[NoteViewerUI] Lista de notas retornada pelo PlayerInventory é nula.");
            return;
        }

        if (allNotes.Count == 0)
        {
            Debug.LogWarning("[NoteViewerUI] PlayerInventory não possui nenhuma nota para exibir.");
            return;
        }

        // Filtra apenas notas não nulas
        notes = new List<NoteData>();
        foreach (var n in allNotes)
            if (n != null)
                notes.Add(n);

        if (notes.Count == 0)
        {
            Debug.LogWarning("[NoteViewerUI] Todas as notas na lista do inventário são nulas. Nada para exibir.");
            return;
        }

        // Decide qual nota mostrar
        if (note != null)
        {
            currentIndex = notes.IndexOf(note);
            if (currentIndex < 0)
            {
                Debug.Log("[NoteViewerUI] Nota recebida não está na lista filtrada. Caindo para a primeira nota válida.");
                currentIndex = 0;
            }

            Debug.Log($"[NoteViewerUI] ShowNote chamado com nota '{note.title}'. Index final: {currentIndex}. Total notas válidas: {notes.Count}");
        }
        else
        {
            currentIndex = 0;
            Debug.Log($"[NoteViewerUI] ShowNote chamado com note=null. Abrindo primeira nota válida: '{notes[0].title}'. Total notas válidas: {notes.Count}");
        }

        UpdateUI();
        OpenPanel();
    }

    private void OpenPanel()
    {
        if (notePanel != null)
            notePanel.SetActive(true);

        // Ativa a câmera do mundo 3D e posiciona
        if (noteCamera != null && cameraPoint != null)
        {
            noteCamera.enabled = true;
            noteCamera.transform.position = cameraPoint.position;
            noteCamera.transform.rotation = cameraPoint.rotation;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;

        Debug.Log($"[NoteViewerUI] Painel de nota ABERTO. previousTimeScale={previousTimeScale}, TimeScale agora={Time.timeScale}");
    }

    public void CloseNote()
    {
        if (notePanel != null)
            notePanel.SetActive(false);

        if (noteCamera != null)
            noteCamera.enabled = false;

        Time.timeScale = previousTimeScale;
        isOpen = false;

        Debug.Log($"[NoteViewerUI] Nota fechada. TimeScale restaurado para {previousTimeScale}");
    }

    private void Update()
    {
        if (!isOpen)
            return;

        bool closeKey = Input.GetKeyDown(KeyCode.Escape);
        bool closeButton = Input.GetButtonDown("Cancel");

        if (closeKey || closeButton)
        {
            Debug.Log("[NoteViewerUI] Input de fechar nota detectado (ESC ou Cancel).");
            CloseNote();
        }
    }

    private void UpdateUI()
    {
        if (notes == null || notes.Count == 0 || currentIndex < 0 || currentIndex >= notes.Count)
        {
            Debug.LogWarning("[NoteViewerUI] Não há nota válida para exibir. notes=null? " +
                             (notes == null) + ", count=" + (notes != null ? notes.Count : 0) +
                             ", currentIndex=" + currentIndex);
            return;
        }

        NoteData note = notes[currentIndex];
        if (note == null)
        {
            Debug.LogWarning("[NoteViewerUI] Nota em notes[" + currentIndex + "] é nula.");
            return;
        }

        // Texto
        if (titleText != null)
            titleText.text = note.title;
        if (contentText != null)
            contentText.text = note.content;

        // Visual 2D
        Apply2DBackground(note);

        // Visual 3D: troca textura do squad genérico
        if (noteWorldView != null)
        {
            noteWorldView.SetNoteVisual(note);
            Debug.Log("[NoteViewerUI] Atualizando squad 3D para nota: " + note.title);
        }

        bool canPrev = currentIndex > 0;
        bool canNext = currentIndex < notes.Count - 1;

        if (previousButton != null)
            previousButton.interactable = canPrev;

        if (nextButton != null)
            nextButton.interactable = canNext;

        Debug.Log($"[NoteViewerUI] UpdateUI exibindo índice {currentIndex} de {notes.Count}. canPrev={canPrev}, canNext={canNext}");
    }

    private void Apply2DBackground(NoteData note)
    {
        if (backgroundRawImage == null)
            return;

        if (note.backgroundSprite != null)
            backgroundRawImage.texture = note.backgroundSprite.texture;
        else
            backgroundRawImage.texture = null; // Ou uma textura padrão, se desejar

        if (note.backgroundColor.a > 0f ||
            note.backgroundColor.r > 0f ||
            note.backgroundColor.g > 0f ||
            note.backgroundColor.b > 0f)
        {
            backgroundRawImage.color = note.backgroundColor;
        }
        else
        {
            backgroundRawImage.color = defaultBackgroundColor;
        }
    }

    public void NextNote()
    {
        if (notes == null) return;

        if (currentIndex < notes.Count - 1)
        {
            currentIndex++;
            Debug.Log("[NoteViewerUI] NextNote -> novo index = " + currentIndex);
            UpdateUI();
        }
    }

    public void PreviousNote()
    {
        if (notes == null) return;

        if (currentIndex > 0)
        {
            currentIndex--;
            Debug.Log("[NoteViewerUI] PreviousNote -> novo index = " + currentIndex);
            UpdateUI();
        }
    }
}