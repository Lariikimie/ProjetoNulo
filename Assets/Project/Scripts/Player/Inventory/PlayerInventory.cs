using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Pilhas (Bateria extra)")]
    [SerializeField] private int batteryCount = 0;

    [Header("Munição")]
    [SerializeField] private int ammoCount = 0;
    [SerializeField] private int maxAmmo = 30;

    [Header("Itens de Cura")]
    [SerializeField] private int healthItemCount = 0;

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

    public int GetBatteryCount() => batteryCount;

    // ==== MUNIÇÃO =====================================================
    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        ammoCount = Mathf.Clamp(ammoCount, 0, maxAmmo);
        Debug.Log("[Inventário] Munição: " + ammoCount);
    }

    public bool UseAmmo(int amount = 1)
    {
        if (ammoCount >= amount)
        {
            ammoCount -= amount;
            return true;
        }
        return false;
    }

    public int GetAmmoCount() => ammoCount;
    public int GetMaxAmmo() => maxAmmo;

    // ==== ITENS DE CURA ==============================================
    public void AddHealthItem(int amount = 1)
    {
        healthItemCount += amount;
        Debug.Log("[Inventário] Itens de cura: " + healthItemCount);
    }

   
    public int GetHealthItemCount() => healthItemCount;

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

    public bool HasKey(string keyId) => keys.Contains(keyId);
    public List<string> GetAllKeys() => keys;

    // ==== NOTAS =====================================================
    public void AddNote(NoteData note)
    {
        if (!notes.Contains(note))
        {
            notes.Add(note);
            Debug.Log("[Inventário] Pegou nota: " + note.title);
        }
        else
        {
            Debug.Log("[Inventário] Já tinha a nota: " + note.title);
        }
    }

    public List<NoteData> GetAllNotes() => notes;

    // ==== ESTADO DO INVENTÁRIO (para checkpoints/salvar) ============
    public void LoadInventoryState(int loadedBatteries, List<string> loadedKeys, List<NoteData> loadedNotes, int loadedAmmo)
    {
        batteryCount = loadedBatteries;
        keys = new List<string>(loadedKeys);
        notes = new List<NoteData>(loadedNotes);
        ammoCount = loadedAmmo;
    }
    public bool UseHealthItem()
{
    if (healthItemCount > 0)
    {
        healthItemCount--;
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            int quantidadeDeCura = 25; // Defina aqui o valor de cura desejado
            stats.AddHealth(quantidadeDeCura);
            Debug.Log("[Inventário] Usou item de cura! Restam: " + healthItemCount);
        }
        else
        {
            Debug.LogWarning("[Inventário] PlayerStats não encontrado no GameObject do Player!");
        }
        return true;
    }
    Debug.Log("[Inventário] Nenhum item de cura disponível!");
    return false;
}
}