using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance { get; private set; }

    [System.Serializable]
    public class HintPanel
    {
        public string hintId;
        public GameObject panelObject;
    }

    public List<HintPanel> hintPanels;

    private Dictionary<string, GameObject> hintDict;
    private Dictionary<string, bool> hintShownTracker; // Rastreia quais dicas foram marcadas como mostradas

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            hintDict = new Dictionary<string, GameObject>();
            hintShownTracker = new Dictionary<string, bool>();
            
            foreach (var hint in hintPanels)
            {
                hintDict[hint.hintId] = hint.panelObject;
                Debug.Log($"[HintManager] Dica registrada: '{hint.hintId}'");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Mostra uma dica no painel correspondente (sem bloqueio de "já foi mostrada")
    /// </summary>
    public void ShowHint(string hintId)
    {
        if (hintDict.TryGetValue(hintId, out var panel))
        {
            panel.SetActive(true);
            Debug.Log($"[HintManager] Dica '{hintId}' MOSTRADA");
        }
        else
        {
            Debug.LogWarning($"[HintManager] Dica '{hintId}' NÃO ENCONTRADA no dicionário");
        }
    }

    /// <summary>
    /// Marca uma dica como mostrada (completa), destroi o painel e registra no tracking
    /// </summary>
    public void MarkHintAsShown(string hintId)
    {
        PlayerPrefs.SetInt("hint_" + hintId, 1);
        hintShownTracker[hintId] = true;
        
        if (hintDict.TryGetValue(hintId, out var panel))
        {
            panel.SetActive(false);
            Destroy(panel);
            Debug.Log($"[HintManager] Dica '{hintId}' DESTRUÍDA e marcada como mostrada");
        }
        else
        {
            Debug.LogWarning($"[HintManager] Tentativa de destruir dica '{hintId}' mas não foi encontrada");
        }
    }

    /// <summary>
    /// Verifica se uma dica já foi completada (marcada como mostrada em uma sesão anterior)
    /// </summary>
    public bool HasHintBeenShown(string hintId)
    {
        return PlayerPrefs.GetInt("hint_" + hintId, 0) == 1;
    }

    /// <summary>
    /// Verifica se uma dica foi marcada como mostrada na sessão atual
    /// </summary>
    public bool IsHintMarkedAsShown(string hintId)
    {
        return hintShownTracker.ContainsKey(hintId) && hintShownTracker[hintId];
    }
}