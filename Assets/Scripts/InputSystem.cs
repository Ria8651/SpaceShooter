using UnityEngine;

public enum InputType {
    Movment,
    Up,
    Down
}

public class InputSystem : MonoBehaviour {

    public static InputSystem Instance;
    Vector3 movment;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            Debug.LogError("There is more than 1 input system!");
        } else {
            Instance = this;
        }
    }

    void Update() {
        movment.x = Input.GetAxis("Horizontal");
        movment.y = Input.GetAxis("Vertical");
        movment.z = Input.GetAxis("Other");
    }

    public bool GetInput(InputType inputType) {
        if (inputType == InputType.Up) {
            if (Input.GetKey(KeyCode.Space)) {
                return true;
            } else {
                return false;
            }
        } else if (inputType == InputType.Down) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public bool GetInputDown(InputType inputType) {
        if (inputType == InputType.Up) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                return true;
            } else {
                return false;
            }
        } else if (inputType == InputType.Down) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public bool GetInputUp(InputType inputType) {
        if (inputType == InputType.Up) {
            if (Input.GetKeyUp(KeyCode.Space)) {
                return true;
            } else {
                return false;
            }
        } else if (inputType == InputType.Down) {
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public Vector3 GetAxis(InputType inputType) {
        if (inputType == InputType.Movment) {
            return movment;
        } else {
            return new Vector3(0, 0, 0);
        }
    }
}