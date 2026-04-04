using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Cutscenes disponíveis")]
    public List<CutsceneData> cutscenes; // Configure no Inspector

    [Header("Referências")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public AudioSource audioSource;
    public MonoBehaviour playerMovementScript; // arraste o script PlayerMovement aqui
    public Camera playerCamera; // câmera principal do jogador

    private Dictionary<string, List<CutsceneStep>> cutsceneDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Monta o dicionário para lookup rápido
        cutsceneDict = new Dictionary<string, List<CutsceneStep>>();
        foreach (var c in cutscenes)
        {
            if (!cutsceneDict.ContainsKey(c.cutsceneName))
                cutsceneDict.Add(c.cutsceneName, c.steps);
        }
    }

    public void PlayCutsceneByName(string name)
    {
        if (cutsceneDict.TryGetValue(name, out var steps))
        {
            StartCoroutine(CutsceneRoutine(steps));
        }
        else
        {
            Debug.LogWarning("Cutscene não encontrada: " + name);
        }
    }

    private IEnumerator CutsceneRoutine(List<CutsceneStep> steps)
    {
        Debug.Log("Iniciando cutscene com " + steps.Count + " steps");

        // Desativa câmera do player
        if (playerCamera != null)
        {
            Debug.Log("Desativando câmera do player: " + playerCamera.gameObject.name);
            playerCamera.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("playerCamera não foi atribuída no Inspector!");
        }

        if (playerMovementScript != null)
        {
            Debug.Log("Desativando movimento do player");
            playerMovementScript.enabled = false;
        }
        else
        {
            Debug.LogError("playerMovementScript não foi atribuído no Inspector!");
        }

        for (int i = 0; i < steps.Count; i++)
        {
            Debug.Log("Executando step " + i + " de " + steps.Count);

            // Desativa todas as câmeras de cutscene antes de ativar a atual
            foreach (var step in steps)
                if (step.camera != null && step.camera != steps[i].camera)
                    step.camera.gameObject.SetActive(false);

            // Ativa câmera atual
            if (steps[i].camera != null)
            {
                Debug.Log("Ativando câmera: " + steps[i].camera.gameObject.name);
                steps[i].camera.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Step " + i + " não tem câmera atribuída!");
            }

            // Diálogo
            if (!string.IsNullOrEmpty(steps[i].dialogueText))
            {
                Debug.Log("Mostrando diálogo: " + steps[i].dialogueText);
                dialoguePanel.SetActive(true);
                dialogueText.text = steps[i].dialogueText;
            }
            else
            {
                dialoguePanel.SetActive(false);
            }

            // Áudio
            if (steps[i].audioClip != null)
            {
                Debug.Log("Tocando áudio: " + steps[i].audioClip.name);
                audioSource.clip = steps[i].audioClip;
                audioSource.Play();
            }

            Debug.Log("Aguardando " + steps[i].duration + " segundos");
            yield return new WaitForSeconds(steps[i].duration);
        }

        Debug.Log("Cutscene finalizada - desativando câmeras e reativando player");

        // Desativa todas as câmeras de cutscene
        foreach (var step in steps)
        {
            if (step.camera != null)
            {
                Debug.Log("Desativando câmera: " + step.camera.gameObject.name);
                step.camera.gameObject.SetActive(false);
            }
        }

        dialoguePanel.SetActive(false);

        // Reativa câmera do player
        if (playerCamera != null)
        {
            Debug.Log("Reativando câmera do player");
            playerCamera.gameObject.SetActive(true);
        }

        if (playerMovementScript != null)
        {
            Debug.Log("Reativando movimento do player");
            playerMovementScript.enabled = true;
        }
    }
}