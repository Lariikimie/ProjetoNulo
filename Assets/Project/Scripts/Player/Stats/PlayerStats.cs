using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Vida do Player")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player tomou dano. HP atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player morreu!");

        // Aqui depois você pode:
        // - Desabilitar movimento
        // - Tocar animação de morte
        // - Chamar tela de Game Over
        // Por enquanto vamos só logar.
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
}
