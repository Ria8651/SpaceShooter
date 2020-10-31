using UnityEngine;
using UnityEngine.EventSystems;

public class MouseLook : MonoBehaviour {

    public float sensitivity = 5;

    [HideInInspector] public Movment character;
    [HideInInspector] public Vector3 cameraRotation;

    float Yvalue;
    float distance;

    Vector2 mouseInput = new Vector2(0, 0);

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        cameraRotation = character.transform.forward;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None) { // && !EventSystem.current.IsPointerOverGameObject()) {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void FixedUpdate() {
        mouseInput.x = Input.GetAxis("Mouse X") * sensitivity;
        mouseInput.y = Input.GetAxis("Mouse Y") * -sensitivity;

        if (character.GetState() == State.Normal || character.GetState() == State.Flying) {
            cameraRotation = Quaternion.AngleAxis(mouseInput.x, character.transform.up) * cameraRotation;

            Vector3 newRotation = Quaternion.AngleAxis(mouseInput.y, transform.right) * cameraRotation;

            if (Vector3.Dot(newRotation, character.transform.forward) > 0) {
                cameraRotation = newRotation;
            }

            character.transform.LookAt(character.transform.position + Vector3.ProjectOnPlane(cameraRotation, character.currentGravityDirection), -character.currentGravityDirection);
            transform.LookAt(transform.position + Vector3.ProjectOnPlane(cameraRotation, transform.right), -character.currentGravityDirection);
        } else if (character.GetState() == State.Floating) {
            cameraRotation += (character.transform.forward - cameraRotation) / 10;

            transform.LookAt(transform.position + cameraRotation, character.transform.up);

            character.rb.AddTorque(character.transform.up * mouseInput.x / 14);

            character.rb.AddTorque(character.transform.right * mouseInput.y / 6);
        } else if (character.GetState() == State.InSeat) {
            cameraRotation = Quaternion.AngleAxis(mouseInput.x, character.transform.up) * cameraRotation;

            Vector3 newRotation = Quaternion.AngleAxis(mouseInput.y, transform.right) * cameraRotation;

            if (Vector3.Dot(newRotation, character.transform.forward) > 0) {
                cameraRotation = newRotation;
            }

            Rigidbody vehicleRb = character.seat.vehicle.rb;
            if (vehicleRb != null) {
                cameraRotation = Quaternion.Euler(vehicleRb.angularVelocity) * cameraRotation;
            }

            transform.LookAt(transform.position + cameraRotation, character.transform.up);

            float change = Input.GetAxis("Mouse ScrollWheel") * -2;
            if (distance + change < 3) {
                character.seat.ChangeSeatState(character, false);
                distance = 0;
            } else if (distance + change < 15) {
                distance += change;
            }

            transform.position += ((character.transform.position + -transform.forward * distance) - transform.position) / 5;
        }
    }

    public void ResetTransform() {
        transform.localPosition = new Vector3(0, 0.7f, 0);
        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void GetInSeat() {
        Yvalue = transform.localEulerAngles.x;

        distance = 10;
    }
}
