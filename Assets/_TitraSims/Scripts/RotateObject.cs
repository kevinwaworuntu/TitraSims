using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    float rotateSpeed = 2;
    int fingersOnScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fingersOnScreen = 0;

        foreach(Touch touch in Input.touches) {
            fingersOnScreen++; //Count fingers (or rather touches) on screen as you iterate through all screen touches.
        }
    }

    void OnMouseDrag(){

        //You need two fingers on screen to pinch.
        if(fingersOnScreen > 0 && fingersOnScreen < 2){
            //float rotX = Input.GetAxis("Mouse X")*rotateSpeed*Mathf.Deg2Rad;
            float roty = Input.GetAxis("Mouse Y")*rotateSpeed*Mathf.Deg2Rad;

            //transform.RotateAround(Vector3.up, -rotX);
            transform.RotateAround(Vector3.right, roty);     
        }
    }
}
