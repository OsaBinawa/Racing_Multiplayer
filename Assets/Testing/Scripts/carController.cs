using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class carController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB; //initialize rigidbody sphere
    [SerializeField] private LayerMask groundLayer; //initialize ground layer
    [SerializeField] private float fwdSpeed, revSpeed, turnSpeed, airDrag, groundDrag, 
    alignToGroundTime, driftSpeed = 5f, driftAngle = 45f;
    [SerializeField] private Transform carNormal;
    private float moveInput, turnInput;
    private bool isCarGrounded, isDrifting;
    private Quaternion driftRotation;
    private RaycastHit hit;
    IA_Player inputActions;
    
    void Start()
    {
        sphereRB.transform.parent = null;

        inputActions = new IA_Player();
        inputActions.Player.Movement.Enable();
        inputActions.Player.Drift.started += Drift; 
        //inputActions.Player.Drift.performed += Drift; 
        inputActions.Player.Drift.canceled += Drift; 
    }

    private void Update(){

        Vector2 inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;
        moveInput *= moveInput > 0 ? fwdSpeed : revSpeed;
        transform.position = sphereRB.transform.position;
        float newRotation = turnInput * turnSpeed * Time.deltaTime * inputValue.y;
        transform.Rotate(0, newRotation, 0, Space.World);

        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);
        Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        if (isCarGrounded)
        {
            Accelerate();
            sphereRB.linearDamping = groundDrag;
            inputActions.Player.Drift.Enable();
            Debug.DrawLine(sphereRB.position, hit.point, Color.red);
        }
        else
        {
            sphereRB.AddForce(transform.up * -30f);
            sphereRB.linearDamping = airDrag;            
            inputActions.Player.Drift.Disable();
            Debug.DrawLine(sphereRB.position, hit.point, Color.green);
        }  
    }

    private void Accelerate(){
        if (!isDrifting)
        {
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            sphereRB.AddForce(-transform.right * moveInput, ForceMode.Acceleration);
        }
    }

    private void Drift(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started)
        {
            Debug.Log("Drift started");
            isDrifting = true;

            float angle = turnInput > 0 ? driftAngle : -driftAngle;
            Quaternion driftRotation = Quaternion.Euler(0, angle, 0);
            //transform.rotation = Quaternion.Slerp(transform.rotation, driftRotation, Time.deltaTime * driftSpeed);
            carNormal.rotation = carNormal.rotation * driftRotation;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            Debug.Log("Drift canceled");
            isDrifting = false;

            float angle = turnInput > 0 ? -driftAngle : driftAngle;
            Quaternion driftRotation = Quaternion.Euler(0, angle, 0);
            //transform.rotation = Quaternion.Slerp(transform.rotation, driftRotation, Time.deltaTime * driftSpeed);
            carNormal.rotation = carNormal.rotation * driftRotation;
        }
    }
}