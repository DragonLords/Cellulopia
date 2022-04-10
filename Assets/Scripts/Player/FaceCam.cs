using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FaceCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos=Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dir=new(mousePos.x-transform.position.x,mousePos.y-transform.position.y);
        transform.up=dir;
    }
}
