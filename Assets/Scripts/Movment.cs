using System.Collections.Generic;
using UnityEngine;

public enum State {
    Normal,
    Flying,
    Floating,
    InSeat
}

[RequireComponent(typeof(GravityComponent))]
public class Movment : MonoBehaviour {

    [SerializeField] State state;
    public new MouseLook camera;

    [Header("Normal")]
    public float speed = 6;
    public float sprintSpeedMultiplyer = 1;
    public float jumpPower = 300;
    public float acceleration = 0.2f;
    public float midAirControlDivisor = 6;
    public float groundForce = 1;

    [Header("Floating")]
    public float floatingSpeed = 0.05f;

    [HideInInspector] public Vector3 currentGravityDirection;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public VehicleSeat seat;

    GravityComponent gravityComponent;
    Vector3 groundSpeed;
    Vector3 groundRotation;
    bool grounded;
    bool jump;
    bool sprinting;

    void Start() {
        rb = GetComponent<Rigidbody>();
        gravityComponent = GetComponent<GravityComponent>();

        camera.character = this;
        currentGravityDirection = gravityComponent.gravityDirection;

        SetState(state);
    }

    void Update() {
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

        if (Input.GetKeyDown(KeyCode.T)) {
            if (state != State.Flying && state != State.InSeat) {
                SetState(State.Flying);
            } else if (state == State.Flying) {
                SetState(State.Normal);
            }
        }
    }

    void FixedUpdate() {
        if (gravityComponent.IsInGravityFeild() && state == State.Floating) {
            SetState(State.Normal);
        } else if (!gravityComponent.IsInGravityFeild() && state == State.Normal) {
            SetState(State.Floating);
        }

        if (state == State.Normal) {
            Vector2 movment = InputSystem.Instance.GetAxis(InputType.Movment);
            movment = movment.normalized * speed;

            Vector3 targetVelocity = transform.forward * movment.y + transform.right * movment.x;

            Vector3 combinedGroundSpeed = groundSpeed; // somehow calculate velocity
            targetVelocity += Vector3.ProjectOnPlane(combinedGroundSpeed, transform.up);

            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            Vector3 currentVelocity = transform.forward * localVelocity.z + transform.right * localVelocity.x;

            // Vector3 localGroundSpeed = transform.InverseTransformDirection(groundSpeed);
            // rb.AddForce(localGroundSpeed.z * transform.up, ForceMode.VelocityChange);

            if (sprinting && movment.y > 0) {
                targetVelocity += transform.forward * speed * (sprintSpeedMultiplyer - 1);
            }

            if (grounded) {
                if (jump) {
                    rb.AddForce(transform.up * jumpPower);

                    jump = false;
                }

                rb.velocity += (targetVelocity - currentVelocity) * acceleration;
            } else {
                if (jump) {
                    jump = false;
                }

                rb.velocity += (targetVelocity - currentVelocity) * acceleration / midAirControlDivisor;
            }
            
            if (localVelocity.y < 0) {
                rb.AddForce(gravityComponent.gravityDirection * groundForce, ForceMode.VelocityChange);
            }

            if (grounded) {
                GetComponent<MeshRenderer>().material.color = Color.white;
            } else {
                GetComponent<MeshRenderer>().material.color = Color.red;
            }

            Vector3 difference = gravityComponent.gravityDirection - currentGravityDirection;
            currentGravityDirection += difference / 10;
        } else if (state == State.Flying) {
            Vector3 movment = InputSystem.Instance.GetAxis(InputType.Movment);
            movment = movment.normalized * speed * 5;

            Vector3 targetVelocity = transform.forward * movment.y + transform.right * movment.x + transform.up * movment.z;
            Vector3 currentVelocity = rb.velocity;

            rb.velocity += (targetVelocity - currentVelocity) * acceleration;
        } else if (state == State.Floating) {
            float movment = InputSystem.Instance.GetAxis(InputType.Movment).y;

            rb.velocity += camera.transform.forward * movment * floatingSpeed;
        }
    }

    public void SetState(State _state) {
        if (_state == State.Normal) {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false;

            currentGravityDirection = -transform.up;
            // camera.cameraRotation = transform.forward;

            gravityComponent.enabled = true;
        } else if (_state == State.Floating) {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;

            gravityComponent.enabled = true;
        } else if (_state == State.InSeat) {
            camera.GetInSeat();

            rb.isKinematic = true;

            gravityComponent.enabled = false;
        } else if (_state == State.Flying) {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false;

            gravityComponent.enabled = false;

            currentGravityDirection = -transform.up;
            // camera.cameraRotation = transform.forward;
        }

        if (_state != State.InSeat) {
            if (state == State.InSeat) {
                transform.LookAt(transform.position + Vector3.ProjectOnPlane(camera.transform.forward, transform.up), transform.up);

                camera.ResetTransform();
            }
        }

        state = _state;
    }

    public State GetState() {
        return state;
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.tag != "Gravity Source") {
            grounded = true;
            
            if (other.gameObject.GetComponent<Rigidbody>()) {
                if (other.gameObject.tag == "VehiclePlayerCollider") {
                    groundSpeed = other.transform.parent.GetComponent<Rigidbody>().velocity;
                    groundRotation = other.transform.parent.GetComponent<Rigidbody>().angularVelocity;
                } else {
                    groundSpeed = other.transform.GetComponent<Rigidbody>().velocity;
                    groundRotation = other.transform.GetComponent<Rigidbody>().angularVelocity;
                }
            } else {
                groundSpeed = Vector3.zero;
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag != "Gravity Source") {
            grounded = false;
        }
    }
}