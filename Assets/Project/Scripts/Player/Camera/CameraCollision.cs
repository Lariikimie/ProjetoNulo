using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public Transform cameraTransform;
    public LayerMask collisionMask;

    public float cameraDistance = 3f;
    public float cameraRadius = 0.2f;
    public float smooth = 10f;

    private float currentDistance;

    void Start()
    {
        currentDistance = cameraDistance;
    }

    void LateUpdate()
    {
        Vector3 direction = cameraTransform.localPosition.normalized;
        float targetDistance = cameraDistance;

        if (Physics.SphereCast(transform.position, cameraRadius, direction, out RaycastHit hit, cameraDistance, collisionMask))
        {
            targetDistance = hit.distance - 0.2f;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smooth);
        cameraTransform.localPosition = direction * currentDistance;
    }
}

