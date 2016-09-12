using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;

/* Some Experiences:
 * - Having an empty GhostObject with cleared CollisionGroup + Mask is fast (Sphere used here). Activating the correct group+mask kills framerate (dropping to 5 fps) with 100 of these objects sitting condensed.
 * - Using a Box instead of the Sphere gives a framerate of ~12 fps. Deactivating the BGhostObj keeps the framerate stable at 150 fps. So the BGhostobject is too slow for this topic.
 */

public class GhostObjPiggyBack2 : BGhostObject
{
    private static GraphController graphControl;

    private GameObject go;
    private BGhostObject goBGhost;
    private CollisionObject goCo;
    private GhostObject goGhost;
    private GameObject goParent;
    private BRigidBody goParentRb;
    private CollisionObject goParentCo;

    private static float repulseForceStrength;
    private static bool repulseActive;

    private static float sphRadius;
    private float sphRadiusSqr;

    public float SphRadius
    {
        get
        {
            return sphRadius;
        }
    }

    internal override void Start()
    {
        base.Start();

        graphControl = FindObjectOfType<GraphController>();

        go = this.gameObject;
        goBGhost = go.GetComponent<BGhostObject>();
        goCo = goBGhost.GetCollisionObject();
        goGhost = (GhostObject)goCo;
        goParent = go.transform.parent.gameObject;
        goParentRb = goParent.GetComponent<BRigidBody>();
        goParentCo = goParentRb.GetCollisionObject();

        if (goCo.CollisionShape is SphereShape)
        {
            sphRadius = this.gameObject.GetComponent<BSphereShape>().Radius;                      // when using a Sphere as CollisionObject
        } else if (goCo.CollisionShape is BoxShape)
        {
            sphRadius = this.gameObject.GetComponent<BBoxShape>().Extents.x;                        // when using a Box as CollisionObject. Box is assumed to have equal sides.
        } else
        {
            Debug.LogError("GhostObjPiggyBack2: CollisionObject not Sphere or Box. sphRadius will be empty, repulse not working.");
        }

        sphRadiusSqr = sphRadius * sphRadius;
    }

    internal void doGravity()
    {
        m_collisionObject.WorldTransform = goParentCo.WorldTransform;
        //m_collisionObject.WorldTransform = ((CollisionObject)thisRb as CollisionObject);
        //transform.position = goParent.transform.position;

        for (int i = 0; i < goGhost.NumOverlappingObjects; i++)
        {
            CollisionObject otherGoCo = goGhost.GetOverlappingObject(i);

            // Todo: Check if BRigidBody can be disabled if everything works. Should be filtered by CollisionGroup+Mask
            if (otherGoCo.UserObject is BRigidBody)
            {
                BRigidBody hitRb = (BRigidBody)otherGoCo.UserObject as BRigidBody;

                if (hitRb != goParentRb)
                {
                    //Debug.Log("BOnTriggerStay: this: " + this + ". go: " + go + ". goParentRb: " + goParentRb + ". / other: " + other + ". otherGoRb: " + otherGoRb);

                    //Vector3 direction = otherGoCo.WorldTransform.Origin.ToUnity() - goGhost.WorldTransform.Origin.ToUnity();
                    //Vector3 direction = otherGoRb.gameObject.transform.position - this.transform.position;
                    //Vector3 direction = otherGoRb.transform.position - goParentRb.transform.position;
                    Vector3 direction = (otherGoCo.WorldTransform.Origin - goGhost.WorldTransform.Origin).ToUnity();
                    float distSqr = direction.sqrMagnitude;

                    // only repulse if within Repulse Sphere Radius
                    // if distSqr > sphRadiusSqr, the following impulseExpoFalloffByDist calc becomes negative. So only use it if distSqr was checked before or add a min/max limit. Otherwise nodes will be applied a negative impulse and go visit Voyager.
                    float impulseExpoFalloffByDist = Mathf.Clamp(1 - (distSqr / sphRadiusSqr), 0, 1);

                    Vector3 impulse = direction.normalized * graphControl.RepulseForceStrength * impulseExpoFalloffByDist;
                    //Debug.Log("goGhostParent: " + goParent + ". goGhostPos: " + goGhost.WorldTransform.Origin.ToUnity() + ". otherGoRb: " + otherGoRb + ". otherGoRbPos: " + otherGoCo.WorldTransform.Origin.ToUnity() +
                    //          ". direction: " + direction + ". distanceSqr: " + distSqr + ". sphRadiusSqr: " + sphRadiusSqr + ". RepulseForceStrength: " + graphControl.RepulseForceStrength + ". linearImpulseFalloffByDist: " + impulseExpoFalloffByDist.ToString("F4") + ". Applying Impulse: " + impulse.ToString("F4"));
                    hitRb.AddImpulse(impulse);
                    //hitRb.AddImpulse(impulse * .5f);
                    //thisRb.AddImpulse(-impulse * .5f);
                }
            }
            else
            {
                Debug.LogError("other.UserObject: " + otherGoCo + " is not a BRigidbody. Check CollisionGroups/Masks!");
            }
        }
    }
}