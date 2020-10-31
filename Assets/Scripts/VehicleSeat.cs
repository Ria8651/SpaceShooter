using UnityEngine;

public class VehicleSeat : MonoBehaviour {

    public VehicleController vehicle;
    [HideInInspector] public Movment playerInSeat;

    float coolDown;

    void OnTriggerEnter(Collider other) {
        if (coolDown <= 0) {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
                coolDown = 5;

                Movment player = other.GetComponent<Movment>();
                ChangeSeatState(player, true);
            }
        }
    }

    void Update() {
        if (coolDown > 0) {
            coolDown -= Time.deltaTime;
        }

        if (vehicle != null) {
            if (playerInSeat != null) {
                Vector3 movement = InputSystem.Instance.GetAxis(InputType.Movment);
                vehicle.Move(movement);
            } else {
                vehicle.Move(Vector3.zero);
            }
        }
    }
    
    public void ChangeSeatState(Movment player, bool state) {
        if (state == true) {
            player.SetState(State.InSeat);
            playerInSeat = player;

            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;

            player.seat = this;

            player.transform.parent = transform;
        } else {
            player.SetState(State.Normal);
            playerInSeat = null;

            player.transform.position = transform.position + transform.forward;
            player.rb.velocity = vehicle.rb.velocity;

            player.seat = null;

            player.transform.parent = transform.root.parent;
        }
    }
}