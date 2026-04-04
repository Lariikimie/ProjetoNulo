using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Referências")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public AudioSource audioSource;

    [Header("Configuração")]
    public float dialogueDuration = 3f; // Tempo que o diálogo fica visível

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Reproduz um diálogo após ler uma nota
    /// </summary>
    public void PlayNoteDialogue(NoteData.DialogueLine dialogue)
    {
        if (dialogue.text == null || dialogue.text.Length == 0)
            return;

        StartCoroutine(DialogueRoutine(dialogue));
    }

    private IEnumerator DialogueRoutine(NoteData.DialogueLine dialogue)
    {
        Debug.Log("[DialogueManager] Reproduzindo diálogo: " + dialogue.text);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
        {
            dialogueText.text = dialogue.text;
        }

        // Toca o áudio se houver
        float waitTime = dialogueDuration;
        if (dialogue.audioClip != null && audioSource != null)
        {
            audioSource.clip = dialogue.audioClip;
            audioSource.Play();
            waitTime = dialogue.audioClip.length; // Aguarda o áudio terminar
            Debug.Log("[DialogueManager] Tocando áudio: " + dialogue.audioClip.name + " (" + waitTime + "s)");
        }

        yield return new WaitForSeconds(waitTime);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";
    }
}
