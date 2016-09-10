using UnityEngine;
using System.Collections;
using BulletUnity;

public class Link : MonoBehaviour
{

    public string id;
    public GameObject source;
    public GameObject target;
    public static float intendedLinkLength;
    public static float forceStrength;

    private static GraphController graphControl;

    private BRigidBody sourceBRigidbody;
    private BRigidBody targetBRigidbody;
    private LineRenderer lineRenderer;
    private float intendedLinkLengthSqr;
    private float distSqrNorm;

    // Use this for initialization
    void Start()
    {
        graphControl = FindObjectOfType<GraphController>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();

        //color link according to status
        Color c;
        c = Color.gray;
        c.a = 0.5f;

        //draw line
        lineRenderer.material = new Material(Shader.Find("Self-Illumin/Diffuse"));
        lineRenderer.material.SetColor("_Color", c);
        lineRenderer.SetWidth(0.3f, 0.3f);
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, source.transform.position);
        lineRenderer.SetPosition(1, target.transform.position);

        sourceBRigidbody = source.GetComponent<BRigidBody>();
        targetBRigidbody = target.GetComponent<BRigidBody>();

        intendedLinkLengthSqr = intendedLinkLength * intendedLinkLength;
    }

    void FixedUpdate()
    {
        if (!graphControl.AllStatic)
        {
            Vector3 forceDirection = sourceBRigidbody.transform.position - targetBRigidbody.transform.position;
            float distSqr = forceDirection.sqrMagnitude;

            if (distSqr > intendedLinkLengthSqr)
            {
                //Debug.Log("(Link.FixedUpdate) distSqr: " + distSqr + "/ intendedLinkLengthSqr: " + intendedLinkLengthSqr + " = distSqrNorm: " + distSqrNorm);
                distSqrNorm = distSqr / intendedLinkLengthSqr;

                Vector3 targetRbImpulse = forceDirection.normalized * forceStrength * distSqrNorm;
                //Debug.Log("(Link.FixedUpdate) targetBRigidbody: " + targetBRigidbody + ". forceDirection.normalized: " + forceDirection.normalized + ". distSqrNorm: " + distSqrNorm + ". Applying Impulse: " + targetRbImpulse);
                targetBRigidbody.AddImpulse(targetRbImpulse);
                Vector3 sourceRbImpulse = forceDirection.normalized * -1 * forceStrength * distSqrNorm;
                //Debug.Log("(Link.FixedUpdate) targetBRigidbody: " + sourceBRigidbody + ". forceDirection.normalized: " + forceDirection.normalized + "  * -1 * distSqrNorm: " + distSqrNorm + ". Applying Impulse: " + sourceRbImpulse);
                sourceBRigidbody.AddImpulse(sourceRbImpulse);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // moved from Start() in Update(), otherwise it won't see runtime updates of intendedLinkLength
        intendedLinkLengthSqr = intendedLinkLength * intendedLinkLength;

        lineRenderer.SetPosition(0, source.transform.position);
        lineRenderer.SetPosition(1, target.transform.position);
    }
}
