using System.Collections.Generic;
using UnityEngine;

public enum GravityType {
    Point,
    Surface,
    Cylindrical,
    Ship
}

public class GravityContainer : MonoBehaviour {

    public GravityType gravityType;
    public Vector3 source;

    public static Vector3 GetGravityDirection(List<GravityContainer> gravitySources, Vector3 position) {
        if (gravitySources.Count > 0) {
            Vector3 gravityDirection = Vector3.zero;
            foreach (GravityContainer gravitySource in gravitySources) {
                Vector3 direction = Vector3.zero;
                if (gravitySource.gravityType == GravityType.Point) {
                    direction = (gravitySource.transform.position + gravitySource.source - position).normalized * 0.3f;
                } else if (gravitySource.gravityType == GravityType.Cylindrical) {
                    direction = Vector3.ProjectOnPlane(position - gravitySource.transform.position, gravitySource.source).normalized;
                    direction *= gravitySource.source.magnitude;
                } else if (gravitySource.gravityType == GravityType.Surface || gravitySource.gravityType == GravityType.Ship) {
                    direction = gravitySource.transform.TransformDirection(gravitySource.source);
                }

                gravityDirection += direction;
            }


            gravityDirection /= gravitySources.Count;

            return gravityDirection;
        } else {
            return Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<GravityComponent>()) {
            other.gameObject.GetComponent<GravityComponent>().Add(this);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.GetComponent<GravityComponent>()) {
            other.gameObject.GetComponent<GravityComponent>().Remove(this);
        }
    }
}
