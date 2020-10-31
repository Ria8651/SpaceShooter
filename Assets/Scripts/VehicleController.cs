using System.Collections.Generic;
using UnityEngine;

public enum VehicleType {
    SpaceShip,
    HoverCar,
    Mech
}

[RequireComponent(typeof(GravityComponent))]
public class VehicleController : MonoBehaviour {
    
    public VehicleType vehicleType;
    public Collider playerCollider;
    public float speed = 32;
    public float acceleration = 0.02f;
    public float rotationalSpeed = 1.5f;
    public float angularAcceleration = 0.04f;
    public float hoverForce = 13;
    public Transform[] hoverpads = new Transform[4];
    public Transform centerOfMass;

    [HideInInspector] List<GravityContainer> gravitySources;
    [HideInInspector] public Rigidbody rb;

    Vector3 movement;
    GravityComponent gravityComponent;

    void Start() {
        rb = GetComponent<Rigidbody>();
        gravityComponent = GetComponent<GravityComponent>();

        rb.centerOfMass = centerOfMass.localPosition;

        if (playerCollider != null) {
            Physics.IgnoreCollision(GetComponent<Collider>(), playerCollider);
        }
    }

    void FixedUpdate() {
        Vector3 targetVelocity = transform.forward * movement.y * speed;
        Vector3 currentVelocity = rb.velocity;
        // Vector3 currentVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
        Vector3 difference = (targetVelocity - currentVelocity) * acceleration;
        rb.velocity += difference;
        
        Vector3 targetAngularVelocity = (transform.up * movement.x + transform.right * -movement.z) * rotationalSpeed;
        Vector3 angularDifference = (targetAngularVelocity - rb.angularVelocity) * angularAcceleration;
        rb.angularVelocity += angularDifference;

        if (vehicleType == VehicleType.HoverCar) {
            foreach (Transform hoverpad in hoverpads) {
                Debug.DrawRay(hoverpad.position, -hoverpad.up * 5, Color.green, 0, false);

                const float rayLength = 10;
                float distance = GetGroundDistance(hoverpad, rayLength);

                Vector3 force = transform.up * (1 / (distance + 1)) * hoverForce;
                
                rb.AddForceAtPosition(force, hoverpad.position);
            }
        }
    }

    public void Move(Vector3 movement) {
        this.movement = movement;
    }

    float GetGroundDistance(Transform hoverpad, float rayLength) {
        Vector3 direction = -hoverpad.up * rayLength;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(hoverpad.position, direction, rayLength);

        float distance = 1000000000;
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].transform.tag != "Gravity Source" && hits[i].distance < distance) {
                distance = hits[i].distance;
            }
        }

        return distance;
    }
}