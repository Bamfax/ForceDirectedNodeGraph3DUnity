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
