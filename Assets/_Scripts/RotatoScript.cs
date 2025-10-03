using UnityEngine;

public class RotatoScript : MonoBehaviour
{
    public float rotationSpeed = 100f; 
    public bool rotateClockwise = true;

    void Update()
    {
        float direction = rotateClockwise ? -1f : 1f;
        transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
    }
}
