using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    [SerializeField] private Transform kartModel;
    [SerializeField] private Transform kartNormal;
    [SerializeField] private Rigidbody sphere;

    private float speed, currentSpeed;
    private float rotate, currentRotate;
    private int driftDirection;
    private float driftPower;
    private int driftMode = 0;
    private bool first, second, third;
    private float moveInput, turnInput;

    [Header("Bools")]
    [SerializeField] private bool drifting;

    [Header ("Parameters")]
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float decceleration = -15f;
    [SerializeField] private float steering = 80f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private LayerMask layerMask;

    IA_Player inputActions;
    
    private void Start() {
        inputActions = new IA_Player();
        inputActions.Player.Movement.Enable();
        inputActions.Player.Drift.Enable();
        inputActions.Player.Drift.started += Drift;
    }

    private void Update() {
        Vector2 inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;

        transform.position = sphere.transform.position; //- new Vector3(0, 0.4f, 0);

        moveInput *= moveInput > 0 ? acceleration : decceleration;

        if (turnInput != 0)
        {
            int dir = turnInput > 0 ? 1 : -1;
            float amount = Mathf.Abs(turnInput);
            Steer(dir, amount);
        }

        if (drifting)
        {
            float control = (driftDirection == 1) 
                ? ExtensionMethods.Remap(turnInput, -1, 1, 0, 2) 
                : ExtensionMethods.Remap(turnInput, -1, 1, 2, 0);
            float powerControl = (driftDirection == 1)
                ? ExtensionMethods.Remap(turnInput, -1, 1, .2f, 1)
                : ExtensionMethods.Remap(turnInput, -1, 1, 1, .2f);
            Steer(driftDirection, control);
            driftPower += powerControl;
        }

        if (!drifting)
        {
            kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, turnInput * 15, kartModel.localEulerAngles.z), .2f);
        }
        else
        {
            float control = (driftDirection == 1) ? ExtensionMethods.Remap(turnInput, -1, 1, .5f, 2) : ExtensionMethods.Remap(turnInput, -1, 1, 2, .5f);
            kartModel.parent.localRotation = Quaternion.Euler(0, Mathf.LerpAngle(kartModel.parent.localEulerAngles.y,(control * 15) * driftDirection, .2f), 0);
        }
    }

    private void FixedUpdate() {
        if(drifting)
            sphere.AddForce(-kartModel.transform.right * moveInput, ForceMode.Acceleration);
        else
            sphere.AddForce(transform.forward * moveInput, ForceMode.Acceleration);

        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);

        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(transform.position + (transform.up*.1f), Vector3.down, out hitOn, 1.1f,layerMask);
        Physics.Raycast(transform.position + (transform.up * .1f)   , Vector3.down, out hitNear, 2.0f, layerMask);

        kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
    }

    private void Drift(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && turnInput != 0){
            drifting = true;
            driftDirection = turnInput > 0 ? 1 : -1;
        }
    }

    private void Movement(InputAction.CallbackContext context){
        
    }

    private void Steer(int direction, float amount)
    {
        rotate = (steering * direction) * amount;
    }

    private void Speed(float x)
    {
        currentSpeed = x;
    }
}
