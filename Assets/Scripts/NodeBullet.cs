using UnityEngine;
using System.Collections;
using BulletUnity;

public class NodeBullet : Node
{
    private BRigidBody thisBRigidbody;
    private GhostObjPiggyBack2 thisGhostObj;

    protected override void doGravity()
    {
        /* The gravity impulse code started out locally on the node script, now its defined as a global function on the graph controller, to have control over timing (slower=better) and easy control over it.
        * Having the impulses node-local and in FixedUpdate() was eating performance and becoming slow with many cubes, so next iteration was to move the function global and step down timing.
        * As these impulses here were nice tests (e.g. resulting in orbital spinning around center of universe, or framerate-independent braking), keeping them for reference
        * 
        * // Prepared basic data. We want to apply global gravity pulling node towards center of universe
        * Vector3 dirToCenter = -1f * this.transform.position;
        * float distToCenter = dirToCenter.magnitude;
        * 
        * // Variant 1:
        * // For usage in FixedUpdate. Simplified versions of dampening by cube self velocity. Did have some periodic pulsing effects.
        * //Vector3 impulse = dirToCenter - thisBRigidbody.velocity * globalGravity * globalGravityBrake;
        * //Vector3 impulse = dirToCenter - thisBRigidbody.velocity;
        *
        * // Variant 2:
        * // Apply brake in relation to distance, nearer is more braking power. Needs limiting max for distcenter<1. Using globalGravityBrake is easier, so this was given preference.
        * // Good for using in FixedUpdate() without need for m_linear_damping, as these permanent pulses will send the cube to universe center.
        * if (distToCenter < 1)
        *    distToCenter = 1;
        * Vector3 impulse = dirToCenter - thisBRigidbody.velocity * globalGravity * (1 - 1 / distToCenter);
        *
        * // Variant 3:
        * // This iteration works as oneshot impulse that impulses the cube to the center when using m_linear_damping of 0.63. For using as oneshot, or in a custom-timed gravity function.
        * Vector3 impulse = dirToCenter * thisBRigidbody.mass;
        * 
        * //Debug.Log("(FixedUpdate) hasRun: " + hasRun + ". thisBRigidbody: " + thisBRigidbody + ". Position: " + thisBRigidbody.transform.position + ". DistToCenter: " + distToCenter + ". Velocity: " + thisBRigidbody.velocity + ". globalGravity: " + globalGravity + ". nodeGravityBrake: " + globalGravityBrake + "; Adding impulse: " + impulse);
        * Debug.Log("(FixedUpdate) hasRun: " + hasRun + ". thisBRigidbody: " + thisBRigidbody + ". Position: " + thisBRigidbody.transform.position + ". dirToCenter: " + dirToCenter + ". DistToCenter: " + distToCenter + ". Velocity: " + thisBRigidbody.velocity + "; Adding impulse: " + impulse);
        * thisBRigidbody.AddImpulse(impulse);
        */

        // See comments above for first version of node-local gravity impulses. These were nice tries @impulses. Node-local seemed too slow, so second version was moved in GraphController as one global function and test performance.
        // As it turned out that doing Gravity globally does not seem to make much difference, it was moved back local to the nodescript. The global routine did result in a different physical layout. And having it node-local is more elegant OO-wise.
        // BUT: Doing it a central place to be much more controllable.
        // Prepared basic data. We want to apply global gravity pulling node towards center of universe
        Vector3 dirToCenter = - thisBRigidbody.transform.position;
        float distToCenter = dirToCenter.magnitude;

        // This iteration works as oneshot impulse that impulses the cube to the center when using m_linear_damping of 0.63. Idea is to use it as oneshot in a custom-timed gravity function which runs less frequent.
        // Vector3 impulse = dirToCenter * nodeToApplyGravity.mass * globalGravityFactor;

        // This iteration pulls with equal force, regardless of distance. Slower and smoother. Could maybe still use some falloff towards center.
        Vector3 impulse = dirToCenter.normalized * thisBRigidbody.mass * graphControl.GlobalGravityBullet * (1 - 1 / distToCenter);

        // Debug.Log("(GraphController.ApplyGlobalGravity) nodeToGetGravity: " + nodeToApplyGravity + ". Position: " + nodeToApplyGravity.transform.position + ". dirToCenter: " + dirToCenter + ". DistToCenter: " + distToCenter + ". Velocity: " + nodeToApplyGravity.velocity + "; Adding impulse: " + impulse);
        thisBRigidbody.AddImpulse(impulse);
    }

    protected override void doRepulse()
    {
        thisGhostObj.doGravity();
    }

    protected override void Start()
    {
        base.Start();
        thisBRigidbody = this.GetComponent<BRigidBody>();
        thisGhostObj = this.transform.FindChild("nodeRepulser").GetComponent<GhostObjPiggyBack2>();
    }
}