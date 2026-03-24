using UnityEngine;

public class NoteCineController : MonoBehaviour
{
    public static NoteCineController Instance { get; private set; }

    [Header("Referências principais")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Camera noteCamera;
    [SerializeField] private GameObject notePanel;
    [SerializeField] private NoteWorldView noteWorldView; // Squad genérico
    [SerializeField] private Transform cameraPoint; // Ponto fixo para a câmera

    private int currentNoteIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowNoteArea()
    {
        Debug.Log("[NoteCineController] ShowNoteArea chamado.");

        if (playerInventory == null)
        {
            Debug.LogWarning("[NoteCineController] playerInventory não atribuído!");
            return;
        }

        var notes = playerInventory.GetAllNotes();
        if (notes == null)
        {
            Debug.LogWarning("[NoteCineController] Lista de notas é nula!");
            return;
        }
        if (notes.Count == 0)
        {
            Debug.LogWarning("[NoteCineController] Nenhuma nota no inventário!");
            return;
        }

        if (currentNoteIndex < 0 || currentNoteIndex >= notes.Count)
        {
            Debug.LogWarning("[NoteCineController] currentNoteIndex fora do range! Index: " + currentNoteIndex);
            return;
        }

        NoteData noteToShow = notes[currentNoteIndex];
        Debug.Log("[NoteCineController] Exibindo nota: " + (noteToShow != null ? noteToShow.name : "null"));

        if (noteWorldView == null)
        {
            Debug.LogWarning("[NoteCineController] noteWorldView não atribuído!");
            return;
        }

        noteWorldView.SetNoteVisual(noteToShow);

        if (cameraPoint != null)
        {
            noteCamera.transform.position = cameraPoint.position;
            noteCamera.transform.rotation = cameraPoint.rotation;
            Debug.Log("[NoteCineController] Câmera posicionada em: " + cameraPoint.position);
        }
        else
        {
            Debug.LogWarning("[NoteCineController] cameraPoint não atribuído!");
        }

        if (notePanel != null)
        {
            notePanel.SetActive(true);
            Debug.Log("[NoteCineController] notePanel ativado.");
        }
        else
        {
            Debug.LogWarning("[NoteCineController] notePanel não atribuído!");
        }

        if (noteCamera != null)
        {
            noteCamera.enabled = true;
            Debug.Log("[NoteCineController] noteCamera ativada.");
        }
        else
        {
            Debug.LogWarning("[NoteCineController] noteCamera não atribuída!");
        }
    }
}