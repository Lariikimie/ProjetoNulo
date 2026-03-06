using System.Collections.Generic;
using UnityEngine;

public class NoteCineController : MonoBehaviour
{
    public static NoteCineController Instance { get; private set; }

    [Header("Referências principais")]
    [SerializeField] private PlayerInventory playerInventory;

    [Tooltip("Camera que renderiza a nota para a Render Texture (NoteRenderCamera).")]
    [SerializeField] private Camera noteCamera;

    [Header("UI")]
    [Tooltip("Painel de notas (NotePanel) que contém a RawImage com a RT_Nota. ATENÇÃO: este painel é filho do Canvas, NÃO é o InventoryPanel.")]
    [SerializeField] private GameObject notePanel;

    [Header("Notas no mundo (NoteWorldView)")]
    [Tooltip("Lista de notas 3D no 'mundo da nota'. Se deixar vazio, o script tenta encontrar automaticamente.")]
    [SerializeField] private List<NoteWorldView> noteWorldViews = new List<NoteWorldView>();

    private bool isShowing = false;
    private int currentNoteIndex = 0;   // índice da nota atual na lista do inventário

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[NoteCine] Já existe uma instância, destruindo duplicata.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (noteCamera == null)
            Debug.LogWarning("[NoteCine] noteCamera NÃO atribuída no Inspector.");
        else
            Debug.Log("[NoteCine] Awake: noteCamera = " + noteCamera.name);

        if (notePanel != null)
        {
            notePanel.SetActive(false);
            Debug.Log("[NoteCine] NotePanel inicial DESATIVADO.");
        }
        else
        {
            Debug.LogWarning("[NoteCine] notePanel NÃO atribuído no Inspector.");
        }
    }

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            if (playerInventory != null)
                Debug.Log("[NoteCine] PlayerInventory encontrado automaticamente: " + playerInventory.name);
            else
                Debug.LogWarning("[NoteCine] PlayerInventory NÃO encontrado automaticamente na cena.");
        }

        // Se a lista estiver vazia, tenta encontrar automaticamente todos NoteWorldView na cena
        if (noteWorldViews == null || noteWorldViews.Count == 0)
        {
            noteWorldViews = new List<NoteWorldView>(FindObjectsOfType<NoteWorldView>());
            Debug.Log("[NoteCine] NoteWorldViews encontrados automaticamente: " + noteWorldViews.Count);
        }
    }

    /// <summary>
    /// Abre a tela da nota (NotePanel com RT_Nota) mostrando a primeira nota do inventário
    /// para a qual exista um NoteWorldView configurado.
    /// Chamado pelo slot de NotesCamera (slot 4) ENQUANTO o inventário já está aberto.
    /// NÃO mexe em Time.timeScale nem em InventoryPanel.
    /// </summary>
    public void ShowNoteArea()
    {
        Debug.Log("[NoteCine] ShowNoteArea chamado.");

        if (isShowing)
        {
            Debug.Log("[NoteCine] Já está mostrando a área de nota. Ignorando.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("[NoteCine] PlayerInventory não atribuído. Não é possível verificar notas.");
            return;
        }

        var notes = playerInventory.GetAllNotes();
        if (notes == null || notes.Count == 0)
        {
            Debug.Log("[NoteCine] Player NÃO possui notas no inventário. Tela de nota NÃO será aberta.");
            return;
        }

        if (noteCamera == null)
        {
            Debug.LogWarning("[NoteCine] noteCamera não atribuída. Não é possível mostrar a tela da nota.");
            return;
        }

        if (notePanel == null)
        {
            Debug.LogWarning("[NoteCine] notePanel não atribuído. Não é possível mostrar a tela da nota.");
            return;
        }

        if (noteWorldViews == null || noteWorldViews.Count == 0)
        {
            Debug.LogWarning("[NoteCine] Nenhum NoteWorldView configurado na cena. Nada para mostrar.");
            return;
        }

        // Escolhe a primeira nota do inventário que tenha um NoteWorldView correspondente
        NoteData noteToShow = null;
        NoteWorldView viewToUse = null;

        for (int i = 0; i < notes.Count; i++)
        {
            NoteData candidate = notes[i];
            if (candidate == null) continue;

            NoteWorldView view = FindViewForNote(candidate);
            if (view != null)
            {
                noteToShow = candidate;
                viewToUse = view;
                currentNoteIndex = i;
                break;
            }
        }

        if (noteToShow == null || viewToUse == null)
        {
            Debug.LogWarning("[NoteCine] Nenhuma nota do inventário possui um NoteWorldView correspondente na cena.");
            return;
        }

        // Posiciona a câmera no ponto dessa nota
        PositionCameraAtView(viewToUse);

        // Garante que a camera da nota está renderizando (na RT_Nota)
        noteCamera.enabled = true;
        Debug.Log("[NoteCine] noteCamera habilitada.");

        // Ativa NotePanel (tela com RT_Nota) por cima do inventário
        notePanel.SetActive(true);
        Debug.Log("[NoteCine] NotePanel ATIVADO para nota: " + noteToShow.title);

        isShowing = true;
        Debug.Log("[NoteCine] Tela da nota ATIVADA (inventário continua aberto e jogo já está pausado pelo InventoryUI).");
    }

    private NoteWorldView FindViewForNote(NoteData note)
    {
        if (noteWorldViews == null) return null;

        foreach (var view in noteWorldViews)
        {
            if (view == null || view.NoteData == null) continue;

            // 1) Tenta por referência (mesmo asset)
            if (view.NoteData == note)
                return view;

            // 2) Se os noteId não forem vazios, tenta casar pelo ID
            if (!string.IsNullOrEmpty(view.NoteData.noteId) &&
                !string.IsNullOrEmpty(note.noteId) &&
                view.NoteData.noteId == note.noteId)
                return view;
        }

        return null;
    }

    private void PositionCameraAtView(NoteWorldView view)
    {
        Transform p = view.CameraPoint;
        noteCamera.transform.position = p.position;
        noteCamera.transform.rotation = p.rotation;

        Debug.Log("[NoteCine] Camera posicionada para nota: " +
                  (view.NoteData != null ? view.NoteData.title : "sem título") +
                  " em " + p.position);
    }

    private void CloseNoteArea()
    {
        if (!isShowing)
        {
            Debug.Log("[NoteCine] CloseNoteArea chamado, mas não estava mostrando. Ignorando.");
            return;
        }

        Debug.Log("[NoteCine] Fechando área de nota...");

        isShowing = false;

        // Desativa NotePanel
        if (notePanel != null)
        {
            notePanel.SetActive(false);
            Debug.Log("[NoteCine] NotePanel DESATIVADO.");
        }

        // Opcionalmente desativa a câmera da nota
        if (noteCamera != null)
        {
            noteCamera.enabled = false;
            Debug.Log("[NoteCine] noteCamera desabilitada.");
        }

        Debug.Log("[NoteCine] Área de nota FECHADA (Inventário continua aberto; TimeScale é controlado pelo InventoryUI).");
    }

    private void Update()
    {
        if (!isShowing)
            return;

        bool closeKey = Input.GetKeyDown(KeyCode.Escape);
        bool closeButton = Input.GetButtonDown("Cancel");

        if (closeKey || closeButton)
        {
            Debug.Log("[NoteCine] Input de fechar área de nota detectado (ESC ou Cancel).");
            CloseNoteArea();
        }
    }
}