using UnityEngine;

public class RotateObject : MonoBehaviour
{
    float rotateSpeed = 2;
    int fingersOnScreen;

    void Update()
    {
        fingersOnScreen = 0;
        foreach (Touch touch in Input.touches)
            fingersOnScreen++;
    }

    void OnMouseDrag()
    {
        if (fingersOnScreen > 0 && fingersOnScreen < 2)
        {
            float roty = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;
            transform.RotateAround(Vector3.right, roty);
        }
    }
}
