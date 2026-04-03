using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent navMeshAgent;

    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;

    public bool IsChasing { get; private set; }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = patrolSpeed;
    }

    void Update()
    {
        if (IsChasing && player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    public void StartChase(Transform target)
    {
        if (target == null)
        {
            return;
        }

        player = target;
        IsChasing = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);
    }

    public void StopChase()
    {
        IsChasing = false;
        navMeshAgent.speed = patrolSpeed;
    }
}
