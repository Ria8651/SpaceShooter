using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Thruster : MonoBehaviour {

    public Rigidbody ship;
    public float rate = 20;

    Vector3 currentVelocity;
    new ParticleSystem particleSystem;

    void Start() {
        currentVelocity = ship.velocity;
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update() {
        Vector3 acelleration = currentVelocity - ship.velocity;

        float ammount = Mathf.Clamp(Vector3.Dot(transform.forward, acelleration), 0, 1);

        var emission = particleSystem.emission;
        emission.rateOverTime = ammount * rate;

        currentVelocity = ship.velocity;
    }
}
