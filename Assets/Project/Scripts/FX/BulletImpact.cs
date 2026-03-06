using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.0f; // tempo até desaparecer

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}