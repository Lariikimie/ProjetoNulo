using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoteViewerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Button closeButton;

    [Header("Lista de Notas")]
    [SerializeField] private Transform notesListContainer;
    [SerializeField] private GameObject noteButtonPrefab;

    [Header("Dados")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Visual 2D da Nota")]
    [SerializeField] private RawImage backgroundRawImage;
    [SerializeField] private Color defaultBackgroundColor = Color.white;

    [Header("Visual 3D (NoteWorld)")]
    [SerializeField] private NoteWorldView noteWorldView;
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private Camera noteCamera;

    [Header("Cores de Destaque")]
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.92f, 0.016f, 1f); // Amarelo
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.black;
    [SerializeField] private Color normalTextColor = Color.black;

    private List<NoteData> notes;
    private int selectedNoteIndex = 0;
    private bool isOpen = false;
    private float previousTimeScale = 1f;
    private List<Button> noteButtons = new List<Button>();

    private void Start()
    {
        if (notePanel != null)
            notePanel.SetActive(false);
        else
            Debug.LogError("[NoteViewerUI] notePanel NÃO atribuído no Inspector.");

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseNote);

        if (playerInventory == null)
            Debug.LogWarning("[NoteViewerUI] PlayerInventory NÃO atribuído no Inspector (arraste o Player aqui).");

        if (backgroundRawImage == null)
            Debug.LogWarning("[NoteViewerUI] backgroundRawImage NÃO atribuído (arraste o RawImage de fundo do NotePanel).");

        if (noteWorldView == null)
            Debug.LogWarning("[NoteViewerUI] noteWorldView NÃO atribuído (arraste o NoteWorldView do squad 3D).");

        if (noteCamera == null)
            Debug.LogWarning("[NoteViewerUI] noteCamera NÃO atribuída (arraste a câmera do mundo 3D).");
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void ShowNote(NoteData note)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[NoteViewerUI] PlayerInventory não atribuído em ShowNote. Não é possível carregar notas.");
            return;
        }

        List<NoteData> allNotes = playerInventory.GetAllNotes();

        if (allNotes == null || allNotes.Count == 0)
        {
            Debug.LogWarning("[NoteViewerUI] PlayerInventory não possui nenhuma nota para exibir.");
            return;
        }

        notes = new List<NoteData>();
        foreach (var n in allNotes)
            if (n != null)
                notes.Add(n);

        if (notes.Count == 0)
        {
            Debug.LogWarning("[NoteViewerUI] Todas as notas na lista do inventário são nulas. Nada para exibir.");
            return;
        }

        if (note != null)
        {
            selectedNoteIndex = notes.IndexOf(note);
            if (selectedNoteIndex < 0)
                selectedNoteIndex = 0;
        }
        else
        {
            selectedNoteIndex = 0;
        }

        OpenPanel();
        PopulateNotesList();
        ShowSelectedNote();
        UpdateNoteButtonHighlight();
    }

    private void OpenPanel()
    {
        if (notePanel != null)
            notePanel.SetActive(true);

        if (noteCamera != null && cameraPoint != null)
        {
            noteCamera.enabled = true;
            noteCamera.transform.position = cameraPoint.position;
            noteCamera.transform.rotation = cameraPoint.rotation;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;
    }

    public void CloseNote()
    {
        if (notePanel != null)
            notePanel.SetActive(false);

        if (noteCamera != null)
            noteCamera.enabled = false;

        Time.timeScale = previousTimeScale;
        isOpen = false;
    }

    private void PopulateNotesList()
    {
        foreach (Transform child in notesListContainer)
            Destroy(child.gameObject);
        noteButtons.Clear();

        for (int i = 0; i < notes.Count; i++)
        {
            NoteData note = notes[i];
            GameObject buttonObj = Instantiate(noteButtonPrefab, notesListContainer);
            buttonObj.SetActive(true);

            TMP_Text tmp = buttonObj.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = note.title;
                tmp.enableAutoSizing = true;
                tmp.alignment = TextAlignmentOptions.Midline;
                tmp.color = normalTextColor;
                tmp.fontSizeMin = 18;
                tmp.fontSizeMax = 36;
            }
            else
            {
                Text txt = buttonObj.GetComponentInChildren<Text>();
                if (txt != null) txt.text = note.title;
                else Debug.LogWarning("[NoteViewerUI] NoteButtonPrefab NÃO tem TMP_Text nem Text!");
            }

            // LayoutElement para responsividade
            LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
            if (layout == null)
                layout = buttonObj.AddComponent<LayoutElement>();
            layout.minHeight = 40;
            layout.preferredHeight = 50;
            layout.flexibleWidth = 1;

            Button btn = buttonObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("[NoteViewerUI] NoteButtonPrefab NÃO tem componente Button!");
                continue;
            }

            int capturedIndex = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                selectedNoteIndex = capturedIndex;
                ShowSelectedNote();
                UpdateNoteButtonHighlight();
                Debug.Log($"[NoteViewerUI] Nota selecionada: '{note.title}' (índice {capturedIndex})");
            });

            noteButtons.Add(btn);
        }
    }

    private void UpdateNoteButtonHighlight()
    {
        for (int i = 0; i < noteButtons.Count; i++)
        {
            Button btn = noteButtons[i];
            if (btn != null)
            {
                Image bg = btn.GetComponent<Image>();
                TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
                bool selected = (i == selectedNoteIndex);

                if (bg != null)
                    bg.color = selected ? selectedButtonColor : normalButtonColor;

                if (tmp != null)
                    tmp.color = selected ? selectedTextColor : normalTextColor;

                // Opcional: aumentar levemente o tamanho do botão selecionado
                btn.transform.localScale = selected ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
            }
        }
    }

    private void ShowSelectedNote()
    {
        if (notes == null || notes.Count == 0 || selectedNoteIndex < 0 || selectedNoteIndex >= notes.Count)
            return;

        NoteData note = notes[selectedNoteIndex];
        if (titleText != null)
            titleText.text = note.title;
        if (contentText != null)
            contentText.text = note.content;

        Apply2DBackground(note);

        if (noteWorldView != null)
            noteWorldView.SetNoteVisual(note);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (UINavigationInput.Instance != null && noteButtons.Count > 0)
        {
            if (UINavigationInput.Instance.UpPressedThisFrame())
            {
                selectedNoteIndex = (selectedNoteIndex - 1 + noteButtons.Count) % noteButtons.Count;
                ShowSelectedNote();
                UpdateNoteButtonHighlight();
            }
            if (UINavigationInput.Instance.DownPressedThisFrame())
            {
                selectedNoteIndex = (selectedNoteIndex + 1) % noteButtons.Count;
                ShowSelectedNote();
                UpdateNoteButtonHighlight();
            }
            if (UINavigationInput.Instance.ConfirmPressedThisFrame())
            {
                noteButtons[selectedNoteIndex].onClick.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
            CloseNote();
    }

    private void Apply2DBackground(NoteData note)
    {
        if (backgroundRawImage == null)
            return;

        if (note.backgroundSprite != null)
            backgroundRawImage.texture = note.backgroundSprite.texture;
        else
            backgroundRawImage.texture = null;

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
}