using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 20f;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float touchDeltaX = touch.deltaPosition.x;
            transform.Rotate(0, -touchDeltaX * rotationSpeed * Time.deltaTime, 0);
        }
    }
}
