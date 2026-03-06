using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool canDie = true;

    [Header("Atordoamento (Taser)")]
    [SerializeField] private float defaultStunDuration = 3f;
    [SerializeField] private Color stunnedColor = Color.cyan;

    [Header("Referências")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private EnemyChase enemyChase;

    private float currentHealth;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private Color originalColor;

    public bool IsStunned => isStunned;

    // ?? ADICIONA ISSO AQUI
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (enemyChase == null)
            enemyChase = GetComponent<EnemyChase>();

        if (enemyRenderer == null)
            enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
    }

    private void Update()
    {
        HandleStun();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth -= amount;
        Debug.Log($"[ENEMY] {gameObject.name} tomou {amount} de dano. HP atual: {currentHealth}");

        if (canDie && currentHealth <= 0f)
        {
            Die();
        }
    }

    public void ApplyStun(float duration)
    {
        float stunDuration = duration > 0f ? duration : defaultStunDuration;

        isStunned = true;
        stunTimer = stunDuration;

        if (enemyChase != null)
            enemyChase.SetStunned(true);

        if (enemyRenderer != null)
            enemyRenderer.material.color = stunnedColor;

        Debug.Log($"[ENEMY] {gameObject.name} ATORDOADO por {stunDuration}s.");
    }

    private void HandleStun()
    {
        if (!isStunned)
            return;

        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            isStunned = false;

            if (enemyChase != null)
                enemyChase.SetStunned(false);

            if (enemyRenderer != null)
                enemyRenderer.material.color = originalColor;

            Debug.Log($"[ENEMY] {gameObject.name} voltou do atordoamento.");
        }
    }

    private void Die()
    {
        Debug.Log($"[ENEMY] {gameObject.name} morreu.");
        gameObject.SetActive(false);
    }
}