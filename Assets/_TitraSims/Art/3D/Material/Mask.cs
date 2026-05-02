using UnityEngine;

public class Mask : MonoBehaviour
{
    public Transform objectB;
    public Material mat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetVector ("_B_Position", objectB.position);
    }
}
