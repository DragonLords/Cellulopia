using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public class DummyPlayer : MonoBehaviour
{
    CharacterController controller;
    Rigidbody rb;
    InputAction _movePress;
    Vector3 mousePos = Vector2.one;
    Vector3 worldPos = Vector3.one;
    PlayerInput playerInput;

    #region new
    Vector2 _moveInput;
    bool _sprint = false;
    float _moveSpeed = 1f;
    float _sprintSpeed = 3f;
    bool _isMoving;
    float _speed;
    Camera _mainCamera;
    float _targetRotation = 0f;
    float RotationSmoothTime = 0;
    private float _verticalVelocity;
    float _rotationVelocity;
    #endregion 
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        playerInput=GetComponent<PlayerInput>();
        _mainCamera = Camera.main;
        // Binding();
    }
    void Binding()
    {
        _movePress = playerInput.actions["MovePress"];
    }
    // Update is called once per frame
    void Update()
    {
        // if (_movePress.inProgress)
        // {
        //     _movePress_performed();
        // }
        // MoveCharacter();
        MoveSolved();
    }

        void MoveCharacter()
        {
            float targetSpeed = _sprint ? _sprintSpeed : _moveSpeed;

            if (_moveInput == Vector2.zero) targetSpeed = 0f;
            float currentHorizontalspeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
            float speedOffset = .1f;
            float inputMagnitude = _isMoving ? _moveInput.magnitude : 1f;
            if (currentHorizontalspeed < targetSpeed - speedOffset || currentHorizontalspeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalspeed, targetSpeed * inputMagnitude, Time.deltaTime * 10);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
            Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
            if (_moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            }
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        void MoveSolved(){
            float targetSpeed=_sprint?_sprintSpeed:_moveSpeed;
            if(_moveInput==Vector2.zero) targetSpeed=0f;
            controller.Move(new(_moveInput[0]*.05f,0,_moveInput[1]*.05f));
        }

    private void _movePress_performed()
        {
            Move();
        }

    public void MoveInputReceived(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            mousePos = ctx.ReadValue<Vector2>();
            worldPos = Camera.main.ScreenToWorldPoint(mousePos);
#if UNITY_IOS
                Move();
                // Rot();
#endif
        }
    }

    void Move()
    {
        
    }



    public void ClickInputReceived(InputAction.CallbackContext ctx)
    {

    }

    public void KeyPress(InputAction.CallbackContext ctx){
        _moveInput=ctx.ReadValue<Vector2>();
    }
}
