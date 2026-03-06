using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    float rotationSpeed = 50.0f;
    

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
