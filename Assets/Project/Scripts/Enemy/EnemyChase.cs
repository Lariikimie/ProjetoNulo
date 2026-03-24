using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Alvo a ser perseguido (Player). Se vazio, procura por tag 'Player'.")]
    [SerializeField] private Transform target;
    [SerializeField] private EnemyStats enemyStats;

    [Header("Detecção")]
    [SerializeField] private float detectionRadius = 30f;
    [SerializeField] private float loseRadius = 45f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Configuração de movimento")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float angularSpeed = 360f;
    [SerializeField] private float acceleration = 40f;

    private NavMeshAgent agent;
    private bool isStunned = false;
    private bool isChasing = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyStats == null)
            enemyStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stopDistance;
        }

        if (agent != null && !agent.isOnNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
                agent.Warp(navHit.position);
        }
    }

    private void Update()
    {
        if (target == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (isStunned || (enemyStats != null && enemyStats.IsStunned))
        {
            if (!agent.isStopped)
                agent.isStopped = true;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (!isChasing && distance <= detectionRadius)
        {
            isChasing = true;
            agent.isStopped = false;
        }

        if (isChasing && distance > loseRadius)
        {
            isChasing = false;
            agent.isStopped = true;
        }

        if (isChasing)
        {
            agent.SetDestination(target.position);

            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }
    }

    public void SetStunned(bool value)
    {
        isStunned = value;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        agent.isStopped = value;
    }
}