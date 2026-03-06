using System.Collections.Generic;
using UnityEngine;

// Este script guarda o inventário lógico do jogador:
// - Quantidade de pilhas
// - Lista de chaves
// - Lista de notas (NoteData)
public class PlayerInventory : MonoBehaviour
{
    [Header("Pilhas (Bateria extra)")]
    [SerializeField] private int batteryCount = 0;

    [Header("Chaves")]
    [SerializeField] private List<string> keys = new List<string>();

    [Header("Notas (Diário)")]
    [SerializeField] private List<NoteData> notes = new List<NoteData>();

    // ==== PILHAS =====================================================

    public void AddBattery(int amount)
    {
        batteryCount += amount;
        if (batteryCount < 0)
            batteryCount = 0;

        Debug.Log("[Inventário] Pilhas: " + batteryCount);
    }

    public bool UseBattery(int amount = 1)
    {
        if (batteryCount >= amount)
        {
            batteryCount -= amount;
            Debug.Log("[Inventário] Usou pilha. Restam: " + batteryCount);
            return true;
        }

        Debug.Log("[Inventário] Tentou usar pilha, mas não tem suficientes.");
        return false;
    }

    public int GetBatteryCount()
    {
        return batteryCount;
    }

    // ==== CHAVES =====================================================

    public void AddKey(string keyId)
    {
        if (!keys.Contains(keyId))
        {
            keys.Add(keyId);
            Debug.Log("[Inventário] Pegou chave: " + keyId);
        }
        else
        {
            Debug.Log("[Inventário] Já tinha a chave: " + keyId);
        }
    }

    public bool HasKey(string keyId)
    {
        return keys.Contains(keyId);
    }

    public List<string> GetAllKeys()
    {
        return keys;
    }

    // ==== NOTAS ======================================================

    public void AddNote(NoteData note)
    {
        if (note == null)
        {
            Debug.LogWarning("[Inventário] Tentou adicionar nota nula.");
            return;
        }

        if (!notes.Contains(note))
        {
            notes.Add(note);
            Debug.Log("[Inventário] Pegou nota: " + note.title);
        }
        else
        {
            Debug.Log("[Inventário] Já tinha essa nota: " + note.title);
        }
    }

    public List<NoteData> GetAllNotes()
    {
        return notes;
    }

    // ==== CARREGAR ESTADO (USADO PELO CHECKPOINT) =====================

    /// <summary>
    /// Carrega um estado completo de inventário.
    /// Usado pelo CheckpointManager ao dar Respawn.
    /// </summary>
    public void LoadInventoryState(int newBatteryCount, List<string> newKeys, List<NoteData> newNotes)
    {
        // Pilhas
        batteryCount = Mathf.Max(0, newBatteryCount);

        // Chaves
        keys.Clear();
        if (newKeys != null)
            keys.AddRange(newKeys);

        // Notas
        notes.Clear();
        if (newNotes != null)
            notes.AddRange(newNotes);

        Debug.Log("[Inventário] Estado restaurado pelo Checkpoint.");
    }
}
