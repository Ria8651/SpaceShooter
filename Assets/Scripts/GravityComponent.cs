using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityComponent : MonoBehaviour {

    public bool enabled = true;
    [HideInInspector] public Vector3 gravityDirection;
    List<GravityContainer> gravitySources;
    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();

        gravitySources = new List<GravityContainer>();
    }

    void FixedUpdate() {
        if (enabled) {
            gravityDirection = GravityContainer.GetGravityDirection(gravitySources, transform.position);
            rb.AddForce(gravityDirection, ForceMode.VelocityChange);
        }
    }

    public bool IsInGravityFeild() {
        if (gravitySources.Count > 0) {
            return true;
        } else {
            return false;
        }
    }

    public void Add(GravityContainer container) {
        gravitySources.Add(container);
    }

    public void Remove(GravityContainer container) {
        gravitySources.Remove(container);
    }
}