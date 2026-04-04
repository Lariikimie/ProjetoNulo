using UnityEngine;

public class EnemyPOV : MonoBehaviour
{
    public Transform player;
    private bool m_IsPlayerInRange;
    private EnemyMovement enemyMovement;

    void Start()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
    }

    bool IsPlayerCollider(Collider other)
    {
        
        if (player != null && (other.transform == player || other.transform.IsChildOf(player)))
        {
            return true;
        }

        PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();
        return playerMovement != null;
    }

    void OnTriggerEnter (Collider other)
    {
        if (IsPlayerCollider(other))
        {
            m_IsPlayerInRange = true;

            if (player == null)
            {
                PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();
                if (playerMovement != null)
                {
                    player = playerMovement.transform;
                }
            }

            if (enemyMovement != null && player != null)
            {
                enemyMovement.StartChase(player);
            }
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (IsPlayerCollider(other))
        {
            m_IsPlayerInRange = false;

            if (enemyMovement != null)
            {
                enemyMovement.StopChase();
            }
        }
    }

    void Update ()
    {
        if (m_IsPlayerInRange && player != null)
        {
            Vector3 direction = player.position - transform.position + Vector3.up;
            Ray ray = new Ray(transform.position, direction);
            RaycastHit raycastHit;

            if(Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.collider.transform == player || raycastHit.collider.transform.IsChildOf(player))
                {
                    Debug.Log("Player está no campo de visão do inimigo!");
                }
            }
        }
    }
}