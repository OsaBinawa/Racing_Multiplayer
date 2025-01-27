using UnityEngine;

public class CarCon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    private void Start() {
        carRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        Suspension();
    }

    private void Suspension(){
        foreach (Transform rayPoints in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;
            if (Physics.Raycast(rayPoints.position, -rayPoints.up, out hit, maxLength + wheelRadius, drivable))
            {
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints.position), rayPoints.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoints.up, rayPoints.position);

                Debug.DrawLine(rayPoints.position, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(rayPoints.position, rayPoints.position + (wheelRadius + maxLength) * -rayPoints.up, Color.green);
            }   
        }
    }
}
