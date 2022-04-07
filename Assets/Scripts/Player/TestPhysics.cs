using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPhysics : MonoBehaviour
{
    Vector2 mousePos=Vector2.zero;
    Vector3 worldPos=Vector3.zero;
    Mouse mouse=Mouse.current;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos=mouse.position.ReadValue();
        worldPos=Camera.main.ScreenToWorldPoint(mousePos);
        if(mouse.leftButton.isPressed)
            Move();
    }

    private void Move()
    {
        rb.AddForce(new(worldPos.x-transform.position.x,worldPos.y-transform.position.y));
    }
}
