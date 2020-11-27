using System.Collections.Generic;
using UnityEngine;

// enum to store vechcile types
public enum VehicleType {
    SpaceShip,
    HoverCar,
    Mech
}

// this component reqires accsess to a gravity component
[RequireComponent(typeof(GravityComponent))]
public class VehicleController : MonoBehaviour {
    // instace of vehicle type enum
    public VehicleType vehicleType;
    // refrence to collider that the player will interact with
    public Collider playerCollider;
    // variables that control how the ship moves
    public float speed = 32;
    public float acceleration = 0.02f;
    public float rotationalSpeed = 1.5f;
    public float angularAcceleration = 0.04f;
    public float hoverForce = 13;
    // refrence to hoverpad location
    public Transform[] hoverpads = new Transform[4];
    // refrence to center of mass transform
    public Transform centerOfMass;

    // refrence to rigidbody avliable to other scripts but not visible in the inspector
    [HideInInspector] public Rigidbody rb;

    // player input sent from the player
    Vector3 movement;
    // runtime reference to the gravity component required on line 12
    GravityComponent gravityComponent;

    // start method called when the game starts
    void Start() {
        // getting refrences too required components
        rb = GetComponent<Rigidbody>();
        gravityComponent = GetComponent<GravityComponent>();

        // set rigidbodys center of mass to center of mass empty
        rb.centerOfMass = centerOfMass.localPosition;

        // ignore collition between the ship collider and the player collider
        if (playerCollider != null) {
            Physics.IgnoreCollision(GetComponent<Collider>(), playerCollider);
        }
    }

    // update method called 50 times a second - used for pysics
    void FixedUpdate() {
        // caclulates target velocity based on input
        Vector3 targetVelocity = transform.forward * movement.y * speed;
        // get global velocity
        Vector3 currentVelocity = rb.velocity;
        // Vector3 currentVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
        // change velocity based on the difference of targetvelocity and currentvelocity
        Vector3 difference = (targetVelocity - currentVelocity) * acceleration;
        rb.velocity += difference;

        // calculate target angular velocity
        Vector3 targetAngularVelocity = (transform.up * movement.x + transform.right * -movement.z) * rotationalSpeed;
        // change angular velocity based on the difference of target angular velocity and current anular velocity
        Vector3 angularDifference = (targetAngularVelocity - rb.angularVelocity) * angularAcceleration;
        rb.angularVelocity += angularDifference;

        // add forces from the hover thrusters if vehcile is a hover car
        if (vehicleType == VehicleType.HoverCar) {
            // loop over thrusters
            foreach (Transform hoverpad in hoverpads) {
                // draw ray for debuging
                Debug.DrawRay(hoverpad.position, -hoverpad.up * 5, Color.green, 0, false);

                // create constant for ray length
                const float rayLength = 10;
                // get distance using distance function
                float distance = GetGroundDistance(hoverpad, rayLength);

                // calculate force using hyperbola to get expeonential growth
                Vector3 force = transform.up * (1 / (distance + 1)) * hoverForce;

                // add the force at the position of the thruster
                rb.AddForceAtPosition(force, hoverpad.position);
            }
        }
    }

    // function for getting input forwared from the player
    public void Move(Vector3 movement) {
        this.movement = movement;
    }

    // utillity function for getting the distance to the ground
    float GetGroundDistance(Transform hoverpad, float rayLength) {
        // caclulate direction thruster is facing
        Vector3 direction = -hoverpad.up * rayLength;

        // array to store all raycast hits
        RaycastHit[] hits;
        // get raycast hits using pysics sytem
        hits = Physics.RaycastAll(hoverpad.position, direction, rayLength);

        // loop over hits to get the closest one that isnt a gravity source
        float distance = 1000000000;
        for (int i = 0; i < hits.Length; i++) {
            // check if hit is a gravity source and closer than the current closest hit
            if (hits[i].transform.tag != "Gravity Source" && hits[i].distance < distance) {
                // set closest hit to hit distance
                distance = hits[i].distance;
            }
        }

        // return the closest hit that isnt a gravity source
        return distance;
    }
}