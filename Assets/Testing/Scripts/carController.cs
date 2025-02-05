using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class carController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB, carRB;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fwdSpeed, revSpeed, turnSpeed, airDrag, groundDrag, 
    alignToGroundTime;
    [SerializeField] private float accelerationRate = 5f;
    [SerializeField] private float decelerationRate = 10f;
    private float moveInput, turnInput, currentSpeed = 0f;
    private bool isCarGrounded, isDrifting;
    IA_Player inputActions;
    
    void Start()
    {
        sphereRB.transform.parent = null;
        carRB.transform.parent = null;

        inputActions = new IA_Player();
        inputActions.Player.Movement.Enable();
        inputActions.Player.Drift.started += Drift;
        isDrifting = false;
    }

    private void Update(){

        Vector2 inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;

        Debug.Log("MoveInput: " + moveInput + " | TurnInput: " + turnInput);

        float targetSpeed = moveInput > 0 ? fwdSpeed : (moveInput < 0 ? -revSpeed : 0);
        
        if (moveInput != 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * accelerationRate);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * decelerationRate);
        }

        transform.position = sphereRB.transform.position - new Vector3(0, 0.01f, 0);
        float newRotation = turnInput * turnSpeed * Time.deltaTime * inputValue.y;

        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 2f, groundLayer);

        if(isCarGrounded)
        {   
            transform.Rotate(0, newRotation, 0, Space.World);
            Debug.DrawLine(sphereRB.position, hit.point, Color.red);
            Debug.Log("Hit Point: " + hit.point);
        }
        else
        {
            Debug.DrawLine(sphereRB.position, hit.point, Color.green);
        }

        Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);

        if (isDrifting)
        {
            
        }
    }

    void FixedUpdate()
    {
        if (isCarGrounded)
        {
            inputActions.Player.Drift.Enable();
            sphereRB.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
            sphereRB.linearDamping = groundDrag;  
        }
        else
        {
            sphereRB.AddForce(transform.up * -30f);
            sphereRB.linearDamping = airDrag;            
            inputActions.Player.Drift.Disable();
        }  

        carRB.MoveRotation(transform.rotation);
    }

    private void Drift(InputAction.CallbackContext context)
    {
        
        if(context.phase == InputActionPhase.Started)
        {
            isDrifting = true;
            float angle = turnInput > 0 ? 45 : -45;
            Quaternion driftRotation = Quaternion.Euler(0, angle, 0);
            //transform.rotation = Quaternion.Lerp(transform.rotation, driftRotation, Time.deltaTime * 0.1f);
            transform.rotation = transform.rotation * driftRotation;
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            isDrifting = false;
        }
    }
}