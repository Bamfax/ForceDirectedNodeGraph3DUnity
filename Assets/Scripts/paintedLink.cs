using UnityEngine;
using System.Collections;

public class paintedLink : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public GameObject sourceObj;
    public Vector3 targetVector;

    // Use this for initialization
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        //color link according to status
        Color c;
        c = Color.cyan;
        c.a = 0.5f;

        //draw line
        lineRenderer.material = new Material(Shader.Find("Self-Illumin/Diffuse"));
        lineRenderer.material.SetColor("_Color", c);
        lineRenderer.SetWidth(0.2f, 0.2f);
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, sourceObj.transform.position);
        lineRenderer.SetPosition(1, targetVector);
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, sourceObj.transform.position);
        lineRenderer.SetPosition(1, targetVector);
    }
}
