using UnityEngine;

public class OnClickForScaling : MonoBehaviour
{
    void OnMouseDown()
    {
        CSharpScaling.ScaleTransform = transform;
    }
}
