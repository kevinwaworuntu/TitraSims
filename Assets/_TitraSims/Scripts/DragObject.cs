using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    Vector3 dist;
    float posX;
    float posY;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        dist = Camera.main.WorldToScreenPoint(transform.position);
        posX = Input.mousePosition.x - dist.x;
        posY = Input.mousePosition.y - dist.y; 
    }
 
    void OnMouseDrag(){
        int fingersOnScreen = 0;
  
        foreach(Touch touch in Input.touches) {
            fingersOnScreen++; //Count fingers (or rather touches) on screen as you iterate through all screen touches.
    
            //You need two fingers on screen to pinch.
            if(fingersOnScreen == 2){
                Vector3 curPos = new Vector3(Input.mousePosition.x - posX, Input.mousePosition.y - posY, dist.z);  
                
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(curPos);
                transform.position = worldPos;
                
            }
        }     
    }
}
