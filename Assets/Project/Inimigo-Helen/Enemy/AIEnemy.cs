using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private EnemyMovement enemyMovement;
    public float wanderRadius = 20f;
    public float wanderTimer = 5f;
    private float timer;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyMovement = GetComponent<EnemyMovement>();
        timer = wanderTimer;
    }

    void Update()
    {
        if (enemyMovement != null && enemyMovement.IsChasing)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavPosition(transform.position, wanderRadius, NavMesh.AllAreas);
            navMeshAgent.SetDestination(newPos);
            timer = 0;
        }
    }

    Vector3 RandomNavPosition(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        if (NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask))
        {
            return navHit.position;
        }
        else
        {
            return origin;
        }
    }
}
