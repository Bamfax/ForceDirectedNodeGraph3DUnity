using UnityEngine;
using BulletUnity;
using BulletSharp;

public class GameCtrlHelper : MonoBehaviour {

    [SerializeField]
    private bool verbose = true;

    [SerializeField]
    private BPhysicsWorld bPhysicsWorld;

    GameController gameControl;
    GraphController graphControl;

    private GameObject hitGo;

    public GameObject ScreenPointToRaySingleHitBullet(Camera cam, Vector3 pointerCoords)
    {
        CollisionObject hitCo = BCamera.ScreenPointToRay(cam, pointerCoords, CollisionFilterGroups.SensorTrigger, CollisionFilterGroups.DefaultFilter);

        if (hitCo != null && hitCo.UserObject is BCollisionObject)
        {
            BCollisionObject hitBCo = (BCollisionObject)hitCo.UserObject as BCollisionObject;
            return hitBCo.gameObject;
        }
        else
        {
            return null;
        }
    }

    public GameObject ScreenPointToRaySingleHitNative(Camera cam, Vector3 pointerCoords)
    {
        RaycastHit hitInfo;
        Ray ray = cam.ScreenPointToRay(pointerCoords);

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject hitGo = hitInfo.collider.gameObject;

            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonDown: Ray did hit. On Object: " + hitGo.name);

            return hitGo;
        }
        else
        {
            return null;
        }
    }

    public GameObject ScreenPointToRaySingleHitWrapper(Camera cam, Vector3 pointerCoords)
    {
        if (gameControl.EngineBulletUnity)
        {
            hitGo = ScreenPointToRaySingleHitBullet(cam, pointerCoords);
        } else
        {
            hitGo = ScreenPointToRaySingleHitNative(cam, pointerCoords);
        }

        if (hitGo != null)
            return hitGo;
        else
            return null;
    }

    public float GetRepulseSphereDiam()
    {
        float sphereDiam;

        if (gameControl.EngineBulletUnity)
        {
            sphereDiam = GameObject.FindGameObjectWithTag("repulse").GetComponent<GhostObjPiggyBack2>().SphRadius * 2;
        }
        else
        {
            sphereDiam = graphControl.NodePhysXForceSphereRadius * 2;
        }

        return sphereDiam;
    }

    public void ApplyGlobalGravityToNode(Component nodeToApplyGravity)
    {
        if (gameControl.EngineBulletUnity)
        {
            BRigidBody nodeBRb = (BRigidBody)nodeToApplyGravity as BRigidBody;

            // See comments in node script for first version of node-local gravity impulses. These were nice tries @impulses. Node-local was too slow, so second version was moved in GraphController as one global function and test performance.
            // Prepared basic data. We want to apply global gravity pulling node towards center of universe
            Vector3 dirToCenter = -1f * nodeBRb.transform.position;
            //float distToCenter = dirToCenter.magnitude;

            // This iteration works as oneshot impulse that impulses the cube to the center when using m_linear_damping of 0.63. Idea is to use it as oneshot in a custom-timed gravity function which runs less frequent.
            // Vector3 impulse = dirToCenter * nodeToApplyGravity.mass * globalGravityFactor;

            // This iteration pulls with equal force, regardless of distance. Slower and smoother. Could maybe still use some falloff towards center.
            Vector3 impulse = dirToCenter.normalized * nodeBRb.mass * graphControl.GlobalGravityFactor;

            // Debug.Log("(GraphController.ApplyGlobalGravity) nodeToGetGravity: " + nodeToApplyGravity + ". Position: " + nodeToApplyGravity.transform.position + ". dirToCenter: " + dirToCenter + ". DistToCenter: " + distToCenter + ". Velocity: " + nodeToApplyGravity.velocity + "; Adding impulse: " + impulse);
            nodeBRb.AddImpulse(impulse);
        }
        else
        {
            //Rigidbody nodeRb = (Rigidbody)nodeToApplyGravity as Rigidbody;
            //nodeRb.AddForce(impulse);
        }
    }

    public Component getRb(GameObject go)
    {
        if (gameControl.EngineBulletUnity)
        {
            return (Component)go.GetComponent<BRigidBody>() as Component;
        } else
        {
            return (Component)go.GetComponent<Rigidbody>() as Component;
        }
    }

    // Use this for initialization
    void Start ()
    {
        gameControl = GetComponent<GameController>();
        graphControl = GetComponent<GraphController>();

        if (gameControl.EngineBulletUnity)
        {
            Instantiate(bPhysicsWorld);
        }
    }
}
