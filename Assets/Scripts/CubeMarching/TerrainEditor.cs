using System.Collections.Generic;
using UnityEngine;

public enum Tool {
    Add,
    Subtract,
    Flatten
}

public class TerrainEditor : MonoBehaviour {

    public Planet planet;
    public float toolRadius = 3.5f;
    public float strength = 2;

    Vector3 GizmosPosition;
    Vector3 flattenNormal;

    void Update() {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 25);

        if (hits.Length > 0) {
            RaycastHit closesetHit = hits[0];
            for (int i = 1; i < hits.Length; i++) {
                if (hits[i].distance < closesetHit.distance && hits[i].transform.tag != "Player" && closesetHit.transform.tag != "Gravity Source") {
                    closesetHit = hits[i];
                }
            }

            if (closesetHit.transform.tag != "Player" && closesetHit.transform.tag != "Gravity Source") {
                GizmosPosition = closesetHit.point;
                Debug.DrawRay(closesetHit.point, closesetHit.normal, Color.green);

                Vector3 point = closesetHit.point - planet.transform.position;

                if (Input.GetMouseButton(1)) {
                    EditTerrain(point, closesetHit.normal, Tool.Add);
                } else if (Input.GetMouseButton(0)) {
                    EditTerrain(point, closesetHit.normal, Tool.Subtract);
                } else if (Input.GetKeyDown(KeyCode.F)) {
                    flattenNormal = closesetHit.normal;
                } else if (Input.GetKey(KeyCode.F)) {
                    EditTerrain(point, flattenNormal, Tool.Flatten);
                }
            }
        }
    }

    void EditTerrain(Vector3 point, Vector3 normal, Tool tool) {
        Vector3Int[] overflowDirections = CheckForOverflow(point, planet.chunkSize);

        if (overflowDirections.Length > 0) {
            for (int i = 0; i < overflowDirections.Length; i++) {
                float[] chunkData = ItterateChunk(overflowDirections[i], point, normal, tool);

                planet.SetChunkData(overflowDirections[i], chunkData);
            }
        }
    }

    float[] ItterateChunk(Vector3Int chunkId, Vector3 point, Vector3 normal, Tool tool) {
        float[] chunkData = planet.GetChunkData(chunkId);

        for (int i = 0; i < chunkData.Length; i++) {
            Vector3 position = CubeMarching.PositionFromId(i, planet.chunkSize + 1);
            position += (Vector3)chunkId * planet.chunkSize;

            float distance = GetDistance(point, position);

            if (distance < toolRadius) {
                float value = chunkData[i];

                switch (tool) {
                    case Tool.Add:
                        value += (10 - value) * Time.deltaTime * (1 - distance / toolRadius) * strength;
                        break;
                    case Tool.Subtract:
                        value += (0 - value) * Time.deltaTime * (1 - distance / toolRadius) * strength;
                        break;
                    case Tool.Flatten:
                        Plane plane = new Plane(normal, point);

                        value += -plane.GetDistanceToPoint(position) * 5 + 5;

                        // Vector3 difference = position - point;
                        // if (Vector3.Dot(normal, difference) > 0) {
                        //     value += (0 - value) * Time.deltaTime * (1 - distance / toolRadius) * strength;
                        // } else {
                        //     value += (10 - value) * Time.deltaTime * (1 - distance / toolRadius) * strength * 5;
                        // }

                        break;
                    default:
                        break;
                }

                chunkData[i] = Mathf.Clamp(value, 0, 10);
            }
        }

        return chunkData;
    }

    Vector3Int[] CheckForOverflow(Vector3 point, int chunkSize) {
        List<Vector3Int> overflowDirections = new List<Vector3Int>();

        for (int x = 0; x < toolRadius * 2 + 1; x++) {
            for (int y = 0; y < toolRadius * 2 + 1; y++) {
                for (int z = 0; z < toolRadius * 2 + 1; z++) {
                    Vector3Int position = Vector3Int.RoundToInt(point);
                    position += new Vector3Int(x - (int)toolRadius, y - (int)toolRadius, z - (int)toolRadius);

                    Vector3Int chunkId = planet.GetChunkFromPosition(position);

                    if (!overflowDirections.Contains(chunkId)) {
                        overflowDirections.Add(chunkId);
                    }
                }
            }
        }

        return overflowDirections.ToArray();
    }

    float GetDistance(Vector3 p1, Vector3 p2) {
        return (p1 - p2).sqrMagnitude;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.grey;
        Gizmos.DrawSphere(GizmosPosition, 0.1f);
    }
}
