using UnityEngine;
using System.Collections;

public class NodePhysX : MonoBehaviour {

    public string id;
    public string text;
    public string type;

    public static float globalGravity;

    private static GraphController graphControl;

    private float sqrForceSphereRadius;
    private float sqrDistNorm;
    private Rigidbody thisRigidbody;
    
    void Start()
    {
        graphControl = FindObjectOfType<GraphController>();
        thisRigidbody = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!graphControl.AllStatic)
        {
            if (graphControl.RepulseActive)
            {
                float forceSphereRadius = graphControl.NodePhysXForceSphereRadius;

                // moved from Start() in FixedUpddate(), otherwise it won't see runtime updates of forceSphereRadius
                sqrForceSphereRadius = forceSphereRadius * forceSphereRadius;

                // test which node in within forceSphere.
                Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, forceSphereRadius);

                // only apply force to nodes within forceSphere, with Falloff towards the boundary of the Sphere and no force if outside Sphere.
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != this.gameObject)
                    {
                        Rigidbody hitRigidbody = hitCollider.GetComponent<Rigidbody>();
                        if (hitRigidbody != null)
                        {
                            Vector3 forceDirection = hitCollider.transform.position - this.transform.position;

                            float sqrDist = (hitCollider.transform.position - this.transform.position).sqrMagnitude;

                            // only apply force if inside of sphere. Could be omitted, as only objects within the Sphere are considered. Take this out later, when a performance impact can be measured.
                            if (sqrDist < sqrForceSphereRadius)
                            {
                                // Normalize the distance from forceSphere Center to node into 0..1
                                sqrDistNorm = 1 - (sqrDist / sqrForceSphereRadius);

                                // apply normalized distance linearly
                                hitRigidbody.AddForce(forceDirection.normalized * graphControl.RepulseForceStrength * sqrDistNorm);
                            }
                        }
                    }
                }
            }

            // Apply global gravity pulling node towards center of universe
            Vector3 forceDirectionToCenter = Vector3.zero - this.transform.position;
            thisRigidbody.AddForce(forceDirectionToCenter.normalized * globalGravity);
        }
    }
}