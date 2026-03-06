using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Alvo a ser perseguido (Player). Se vazio, procura por tag \"Player\".")]
    [SerializeField] private Transform target;
    [SerializeField] private EnemyStats enemyStats;

    [Header("Detecção")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float stopDistance = 2.5f;

    [Header("Configuração de movimento")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float angularSpeed = 120f;

    private NavMeshAgent agent;
    private bool isStunned = false;

    // flags pra não spamar log
    private bool loggedNoTarget = false;
    private bool loggedNoNavMesh = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyStats == null)
            enemyStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        // Encontrar o player se não estiver setado
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                Debug.Log("[ENEMY CHASE] Target encontrado por tag 'Player': " + target.name);
            }
            else
            {
                Debug.LogWarning("[ENEMY CHASE] NÃO encontrei nenhum objeto com tag 'Player'.");
            }
        }
        else
        {
            Debug.Log("[ENEMY CHASE] Target já atribuído no Inspector: " + target.name);
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.stoppingDistance = stopDistance;
        }

        // ?? Tentar garantir que o Agent fique em cima do NavMesh
        if (agent != null && !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log("[ENEMY CHASE] Agent não estava em NavMesh, fiz Warp para: " + hit.position);
            }
            else
            {
                Debug.LogWarning("[ENEMY CHASE] Agent NÃO está em NavMesh e não encontrei área próxima. Verifique o Bake e a posição do inimigo.");
            }
        }
        else if (agent != null)
        {
            Debug.Log("[ENEMY CHASE] Iniciou em NavMesh válido.");
        }
    }

    private void Update()
    {
        if (target == null)
        {
            if (!loggedNoTarget)
            {
                Debug.LogWarning("[ENEMY CHASE] Sem target. Certifique-se que o Player tem tag 'Player' ou arraste o Transform para o campo Target.");
                loggedNoTarget = true;
            }
            return;
        }

        if (agent == null || !agent.enabled)
        {
            if (!loggedNoNavMesh)
            {
                Debug.LogWarning("[ENEMY CHASE] NavMeshAgent nulo ou desabilitado no inimigo.");
                loggedNoNavMesh = true;
            }
            return;
        }

        if (!agent.isOnNavMesh)
        {
            if (!loggedNoNavMesh)
            {
                Debug.LogWarning("[ENEMY CHASE] Agent NÃO está em NavMesh (Update). Confirme o bake e a posição do inimigo.");
                loggedNoNavMesh = true;
            }
            return;
        }

        // Atordoado = parado
        if (isStunned || (enemyStats != null && enemyStats.IsStunned))
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                Debug.Log("[ENEMY CHASE] Parando agente porque está ATORDOADO.");
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // Linha magenta Player <-> Inimigo (debug visual)
        Debug.DrawLine(transform.position + Vector3.up, target.position + Vector3.up, Color.magenta);

        if (distance <= detectionRadius)
        {
            if (agent.isStopped)
                agent.isStopped = false;

            agent.SetDestination(target.position);
            // Debug.Log("[ENEMY CHASE] Perseguindo player. Distância: " + distance.ToString("F2"));

            // Rotaciona para olhar o player (opcional)
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }
        else
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                Debug.Log("[ENEMY CHASE] Player fora do raio. Distância: " + distance.ToString("F2"));
            }
        }
    }

    public void SetStunned(bool value)
    {
        isStunned = value;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = value;
        Debug.Log("[ENEMY CHASE] SetStunned(" + value + ")");
    }
}