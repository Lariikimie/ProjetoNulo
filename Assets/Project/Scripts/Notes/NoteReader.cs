using System.Collections;
using UnityEngine;

/// <summary>
/// Variante de leitura de nota (mesma lógica do Note.cs).
/// Use apenas se você já tiver esse script na cena.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NoteReader : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private NoteData data;

    [Header("UI de Diálogo")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Player (opcional)")]
    [SerializeField] private Transform player;

    [Header("Inputs")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool _playerInside;
    private bool _showing;
    private int _lineIndex;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Start()
    {
        if (dialogueUI == null)
            dialogueUI = Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        EnsureAtLeastOneLineFromContent();
    }

    private void Update()
    {
        if (_playerInside && !_showing && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenNoteCoroutine());
            return;
        }

        if (!_showing) return;

        if (Input.GetKeyDown(interactKey))
        {
            OnAdvancePressed();
        }

        if (Input.GetKeyDown(pauseKey))
        {
            PauseMenuController.Instance?.TogglePause();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other.transform == player) { _playerInside = true; return; }
        if (player == null && other.CompareTag("Player")) _playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (player != null && other.transform == player) { _playerInside = false; return; }
        if (player == null && other.CompareTag("Player")) _playerInside = false;
    }

    private IEnumerator OpenNoteCoroutine()
    {
        if (data == null)
        {
            Debug.LogWarning("[NoteReader] NoteData não atribuído.");
            yield break;
        }
        if (dialogueUI == null)
        {
            Debug.LogWarning("[NoteReader] DialogueUI não encontrado/atribuído.");
            yield break;
        }

        _showing = true;
        _lineIndex = 0;

        dialogueUI.Show();
        dialogueUI.SetHint($"Pressione [{interactKey}] para avançar");

        yield return ShowCurrentLine();
    }

    private IEnumerator ShowCurrentLine()
    {
        var line = GetLine(_lineIndex);
        dialogueUI.SetStyle(data, line.kind);
        dialogueUI.TypeLine(line.text);
        yield return null;
    }

    private void OnAdvancePressed()
    {
        if (!dialogueUI.IsVisible) return;

        if (dialogueUI.IsTyping())
        {
            dialogueUI.SkipTyping();
            return;
        }

        _lineIndex++;
        if (_lineIndex < GetLinesCount())
        {
            StartCoroutine(ShowCurrentLine());
        }
        else
        {
            dialogueUI.Hide();
            _showing = false;
        }
    }

    // ---------- Helpers ----------

    private void EnsureAtLeastOneLineFromContent()
    {
        if (data == null) return;
        if ((data.lines == null || data.lines.Length == 0) && !string.IsNullOrWhiteSpace(data.content))
        {
            data.lines = new NoteData.DialogueLine[1];
            data.lines[0] = new NoteData.DialogueLine
            {
                text = data.content,
                kind = NoteData.LineKind.Speech
            };
        }
    }

    private int GetLinesCount()
    {
        return (data != null && data.lines != null) ? data.lines.Length : 0;
    }

    private NoteData.DialogueLine GetLine(int index)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
            return new NoteData.DialogueLine { text = "", kind = NoteData.LineKind.Speech };

        index = Mathf.Clamp(index, 0, data.lines.Length - 1);
        return data.lines[index];
    }
}