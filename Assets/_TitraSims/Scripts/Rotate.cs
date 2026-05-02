using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            float touchDeltaX = touch.deltaPosition.x;
            transform.Rotate(0, -touchDeltaX * rotationSpeed * Time.deltaTime, 0);
        }
    }
}
