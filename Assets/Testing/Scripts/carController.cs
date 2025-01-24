using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class carController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform cube;
    [SerializeField] private float fwdSpeed, revSpeed, turnSpeed, airDrag, groundDrag, maxSpeed;
    private float moveInput, turnInput;
    //private PlayerInput playerInput;
    private bool isCarGrounded, isDrifting;
    IA_Player inputActions;
    
    void Start()
    {
        sphereRB.transform.parent = null;
        //playerInput = GetComponent<PlayerInput>();

        inputActions = new IA_Player();
        inputActions.Player.Movement.Enable();
    }

    private void Update(){

        Vector2 inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;
        moveInput *= moveInput > 0 ? fwdSpeed : revSpeed;
        transform.position = sphereRB.transform.position;
        float newRotation = turnInput * turnSpeed * Time.deltaTime * inputValue.y;
        transform.Rotate(0, newRotation, 0, Space.World);

        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        if (isCarGrounded)
        {
            sphereRB.linearDamping = groundDrag;
            inputActions.Player.Drift.started += Drift;
            inputActions.Player.Drift.performed += Drift;
            inputActions.Player.Drift.canceled += Drift;
            inputActions.Player.Drift.Enable();
        }
        else
        {
            sphereRB.linearDamping = airDrag;
            inputActions.Player.Drift.started += Drift;
            inputActions.Player.Drift.performed += Drift;
            inputActions.Player.Drift.canceled += Drift;            
            inputActions.Player.Drift.Disable();
        }
    }

    void FixedUpdate()
    {
        if (isCarGrounded)
        {
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            sphereRB.AddForce(transform.up * -30f);
        }
    }
    private void Drift(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started)
        {
            Debug.Log("Drift started");
            isDrifting = true;

            float driftAngle = turnInput > 0 ? 35 : -35;
            Quaternion driftRotation = Quaternion.Euler(0, driftAngle, 0);
            cube.rotation = cube.rotation * driftRotation;
        }
        //else if (context.phase == InputActionPhase.Performed)
        //{
        //    Debug.Log("Drifting in progress");
        //}
        else if (context.phase == InputActionPhase.Canceled)
        {
            Debug.Log("Drift canceled");
            isDrifting = false;

            //float driftAngle = turnInput > 0 ? -35 : 35;
            //Quaternion driftRotation = Quaternion.Euler(0, driftAngle, 0);
            //cube.rotation = cube.rotation * driftRotation;
        }
    }
}