using System.Collections.Generic;
using UnityEngine;

// enum for player states
public enum State {
    Normal,
    Flying,
    Floating,
    InSeat
}

// this component reqires accsess to a gravity component
[RequireComponent(typeof(GravityComponent))]
public class Movment : MonoBehaviour {

    // instance of player state enum
    [SerializeField] State state;
    // refernce to camera script
    public new MouseLook camera;

    // variables that control player movment in the normal player state
    [Header("Normal")]
    public float speed = 6;
    public float sprintSpeedMultiplyer = 1;
    public float jumpPower = 300;
    public float acceleration = 0.2f;
    public float midAirControlDivisor = 6;
    public float groundForce = 1;

    // variables that control player movment when floating in space
    [Header("Floating")]
    public float floatingSpeed = 0.05f;

    // public variables that dont show in the inspector (so other scripts can accsess them)
    [HideInInspector] public Vector3 currentGravityDirection;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public VehicleSeat seat;

    // runtime reference to the gravity component required on line 13
    GravityComponent gravityComponent;
    // private variables to store the players current state
    Vector3 groundSpeed;
    Vector3 groundRotation;
    bool grounded;
    bool jump;
    bool sprinting;

    // start method called when the game starts
    void Start() {
        // getting refrences too required components
        rb = GetComponent<Rigidbody>();
        gravityComponent = GetComponent<GravityComponent>();

        // initilising variables
        camera.character = this;
        currentGravityDirection = gravityComponent.gravityDirection;

        // initilising state
        SetState(state);
    }

    // update method called every frame
    void Update() {
        // input is controlled in update instead of fixed update to avoid input loss
        if (state == State.Normal) {
            if (InputSystem.Instance.GetInputDown(InputType.Up)) {
                jump = true;
            }

            if (InputSystem.Instance.GetInputDown(InputType.Up)) {
                sprinting = true;
            } else if (InputSystem.Instance.GetInputUp(InputType.Up)) {
                sprinting = false;
            }
        }

        // flight feature for debuging
        if (Input.GetKeyDown(KeyCode.T)) {
            if (state != State.Flying && state != State.InSeat) {
                SetState(State.Flying);
            } else if (state == State.Flying) {
                SetState(State.Normal);
            }
        }
    }

    // update method called 50 times a second - used for pysics
    void FixedUpdate() {
        // update player state
        if (gravityComponent.IsInGravityFeild() && state == State.Floating) {
            SetState(State.Normal);
        } else if (!gravityComponent.IsInGravityFeild() && state == State.Normal) {
            SetState(State.Floating);
        }

        // switch for each player state
        switch (state) {
            // if player is in normal state
            case (State.Normal):
                // get input from input system and scale it by speed
                Vector2 movment = InputSystem.Instance.GetAxis(InputType.Movment);
                movment = movment.normalized * speed;

                // caclulates target velocity based on input
                Vector3 targetVelocity = transform.forward * movment.y + transform.right * movment.x;

                // get speed of the ground to add to target velocity
                Vector3 combinedGroundSpeed = groundSpeed; // somehow add rotational velocity
                targetVelocity += Vector3.ProjectOnPlane(combinedGroundSpeed, transform.up);

                // turn velocity from global space to local space
                Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
                // remove vertical component of velocity
                Vector3 currentVelocity = transform.forward * localVelocity.z + transform.right * localVelocity.x;

                // Vector3 localGroundSpeed = transform.InverseTransformDirection(groundSpeed);
                // rb.AddForce(localGroundSpeed.z * transform.up, ForceMode.VelocityChange);

                // increase speed if walking forwad and sprinting
                if (sprinting && movment.y > 0) {
                    targetVelocity += transform.forward * speed * (sprintSpeedMultiplyer - 1);
                }

                // jump if player is grounded
                if (grounded) {
                    if (jump) {
                        rb.AddForce(transform.up * jumpPower);

                        jump = false;
                    }

                    // change player velocity based on the difference of targetvelocity and currentvelocity
                    rb.velocity += (targetVelocity - currentVelocity) * acceleration;
                } else {
                    // acknowledge input from update loop without jumping
                    if (jump) {
                        jump = false;
                    }

                    // change player velocity slow when in mid air
                    rb.velocity += (targetVelocity - currentVelocity) * acceleration / midAirControlDivisor;
                }

                // make player fall faster on the way down. common techqniqe to make movment feel nicer
                if (localVelocity.y < 0) {
                    rb.AddForce(gravityComponent.gravityDirection * groundForce, ForceMode.VelocityChange);
                }

                // change player color for debuging
                if (grounded) {
                    GetComponent<MeshRenderer>().material.color = Color.white;
                } else {
                    GetComponent<MeshRenderer>().material.color = Color.red;
                }

                // interpolate players gravity vector
                Vector3 difference = gravityComponent.gravityDirection - currentGravityDirection;
                currentGravityDirection += difference / 10;

                break;

            // if player is in floating state
            case (State.Floating):
                // get only forwad and back input
                float floatingInput = InputSystem.Instance.GetAxis(InputType.Movment).y;

                // accelerate player based on forwad and back input
                rb.velocity += camera.transform.forward * speed * floatingInput;

                break;

            // if player is in flying state
            case (State.Flying):
                // get input from input system
                Vector3 flyingInput = InputSystem.Instance.GetAxis(InputType.Movment);
                // scale input by speed
                flyingInput = flyingInput.normalized * speed * 5;

                // calculate target velocity based on input
                Vector3 flyingTargetVelocity = transform.forward * flyingInput.y + transform.right * flyingInput.x + transform.up * flyingInput.z;
                // get current velocity
                Vector3 flyingCurrentVelocity = rb.velocity;

                // change player velocity based on the difference of targetvelocity and currentvelocity
                rb.velocity += (flyingTargetVelocity - flyingCurrentVelocity) * acceleration;

                break;
        }
    }

    // function for updating the players state
    public void SetState(State _state) {
        // switch for player states
        switch (_state) {
            case (State.Normal):
                // change rigidbody contraints to let player move properly
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                // makes player active so it can colllide and respond
                rb.isKinematic = false;

                // reset current gravity direction
                currentGravityDirection = -transform.up;
                // camera.cameraRotation = transform.forward;

                // renables gravity
                gravityComponent.enabled = true;

                break;

                // the other cases are the oppisite or simmillar

            case (State.Floating):
                rb.constraints = RigidbodyConstraints.None;
                rb.isKinematic = false;

                gravityComponent.enabled = true;

                break;

            case (State.InSeat):
                camera.GetInSeat();

                rb.isKinematic = true;

                gravityComponent.enabled = false;

                break;

            case (State.Flying):
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.isKinematic = false;

                gravityComponent.enabled = false;

                currentGravityDirection = -transform.up;
                // camera.cameraRotation = transform.forward;

                break;
        }

        // to remove duplicate code
        if (_state != State.InSeat) {
            if (state == State.InSeat) {
                // make sure camera is looking in the same direction
                transform.LookAt(transform.position + Vector3.ProjectOnPlane(camera.transform.forward, transform.up), transform.up);

                // put camera back inside the player
                camera.ResetTransform();
            }
        }

        // set state to new state
        state = _state;
    }

    // exposing private variable through public method
    public State GetState() {
        return state;
    }

    // method called on fixed update when the ground trigger is intersecting somthing
    void OnTriggerStay(Collider other) {
        // so you cant walk on gravity sources
        if (other.gameObject.tag != "Gravity Source") {
            // set grounded to true so rest of the script knows whats happening
            grounded = true;

            // if other object is rigidbody extract their velocity and rotation
            if (other.gameObject.GetComponent<Rigidbody>()) {
                if (other.gameObject.tag == "VehiclePlayerCollider") {
                    groundSpeed = other.transform.parent.GetComponent<Rigidbody>().velocity;
                    groundRotation = other.transform.parent.GetComponent<Rigidbody>().angularVelocity;
                } else {
                    groundSpeed = other.transform.GetComponent<Rigidbody>().velocity;
                    groundRotation = other.transform.GetComponent<Rigidbody>().angularVelocity;
                }
            } else {
                // if no on ground ground speed is zero
                groundSpeed = Vector3.zero;
            }
        }
    }

    // method called when the on fixed update when the ground trigger leaves the ground
    void OnTriggerExit(Collider other) {
        // so you cant walk on gravity sources
        if (other.gameObject.tag != "Gravity Source") {
            // set grounded to false so rest of the script knows whats happening
            grounded = false;
        }
    }
}