using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using BulletUnity;

public class GraphController : MonoBehaviour {

    public Text nodeCountTxt;
    public Text linkCountTxt;

    public Node nodePrefab;
    public Link linkPrefab;
    public GameObject debugRepulseSpherePrefab;
    public float nodeVectorGenRange = 7F;

    [SerializeField]
    private bool allStatic = false;
    [SerializeField]
    private bool globalGravInFixedUpdate = false;
    private bool globalGravCoRoutineIsRunning = false;
    [SerializeField]
    private bool repulseActive = true;
    [SerializeField]
    private bool debugRepulse = false;

    [SerializeField]
    private float globalGravityFactor = 0.1f;
    [SerializeField]
    private float repulseForceStrength = 0.1f;
    // [SerializeField]
    // public float nodeForceSphereRadius = 50F;                         //this does not work anymore since Bullet CollisionObjects are used, which would need removing and readding to the world. Todo: Could be implemented somewhen.
    [SerializeField]
    private float linkForceStrength = 6F;
    [SerializeField]
    private float linkIntendedLinkLength = 5F;

    public static int nodeCount;
    public static int linkCount;

    // variables for global gravity function
    private List<BRigidBody> nodeRbList = new List<BRigidBody>();
    public bool globalGravityIteratorBreakLoop = true;
    public int globalGravityIteratorBreakCount = 250;
    public float globalGravityInteratorDoneWait = 0.1f;

    public bool AllStatic
    {
        get
        {
            return allStatic;
        }
        private set
        {
            allStatic = value;
        }
    }

    public void ToggleAllStatic(Toggle tgl)
    {
        if (tgl.isOn)
        {
            AllStatic = true;
            if (GameController.verbose)
                Debug.Log("AllStatic on");
        }
        else
        {
            AllStatic = false;
            if (GameController.verbose)
                Debug.Log("AllStatic off");
        }
    }

    public bool RepulseActive
    {
        get
        {
            return repulseActive;
        }
        private set
        {
            repulseActive = value;
        }
    }

    public void ToggleRepulseActive(Toggle tgl)
    {
        if (tgl.isOn)
        {
            repulseActive = true;
            if (GameController.verbose)
                Debug.Log("AllStatic on");
        }
        else
        {
            repulseActive = false;
            if (GameController.verbose)
                Debug.Log("AllStatic off");
        }
    }

    public bool DebugRepulse
    {
        get
        {
            return debugRepulse;
        }
        private set
        {
            debugRepulse = value;
        }
    }

    public void ToggleDebugRepulse(Toggle tgl)
    {
        if (tgl.isOn)
        {
            DebugRepulse = true;
            if (GameController.verbose)
                Debug.Log("DebugRepulse on");

            foreach (BRigidBody nodeAddDebug in nodeRbList)
            {
                GameObject debugobj = Instantiate(debugRepulseSpherePrefab, nodeAddDebug.transform) as GameObject;
                debugobj.transform.localPosition = Vector3.zero;
                float sphereDiam = GameObject.FindGameObjectWithTag("repulse").GetComponent<GhostObjPiggyBack2>().SphRadius * 2;
                debugobj.transform.localScale = new Vector3(sphereDiam, sphereDiam, sphereDiam);
            }
        }
        else
        {
            DebugRepulse = false;
            if (GameController.verbose)
                Debug.Log("DebugRepulse off");

            foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
            {
                Destroy(destroyTarget);
            }
        }
    }

    public float GlobalGravityFactor
    {
        get
        {
            return globalGravityFactor;
        }
        private set
        {
            globalGravityFactor = value;
        }
    }

    public float RepulseForceStrength
    {
        get
        {
            return repulseForceStrength;
        }
        private set
        {
            repulseForceStrength = value;
        }
    }
    
    public float LinkForceStrength
    {
        get
        {
            return linkForceStrength;
        }
        private set
        {
            linkForceStrength = value;
        }
    }

    public float LinkIntendedLinkLength
    {
        get
        {
            return linkIntendedLinkLength;
        }
        private set
        {
            linkIntendedLinkLength = value;
        }
    }

    public int NodeCount
    {
        get
        {
            return nodeCount;
        }
        set
        {
            nodeCount = value;
        }
    }

    public int LinkCount
    {
        get
        {
            return linkCount;
        }
        set
        {
            linkCount = value;
        }
    }

    void ApplyGlobalGravityToNode(BRigidBody nodeToApplyGravity)
    {
        // See comments in node script for first version of node-local gravity impulses. These were nice tries @impulses. Node-local was too slow, so second version was moved in GraphController as one global function and test performance.
        // Prepared basic data. We want to apply global gravity pulling node towards center of universe
        Vector3 dirToCenter = -1f * nodeToApplyGravity.transform.position;
        //float distToCenter = dirToCenter.magnitude;

        // This iteration works as oneshot impulse that impulses the cube to the center when using m_linear_damping of 0.63. Idea is to use it as oneshot in a custom-timed gravity function which runs less frequent.
        // Vector3 impulse = dirToCenter * nodeToApplyGravity.mass * globalGravityFactor;

        // This iteration pulls with equal force, regardless of distance. Slower and smoother. Could maybe still use some falloff towards center.
        Vector3 impulse = dirToCenter.normalized * nodeToApplyGravity.mass * globalGravityFactor;

        // Debug.Log("(GraphController.ApplyGlobalGravity) nodeToGetGravity: " + nodeToApplyGravity + ". Position: " + nodeToApplyGravity.transform.position + ". dirToCenter: " + dirToCenter + ". DistToCenter: " + distToCenter + ". Velocity: " + nodeToApplyGravity.velocity + "; Adding impulse: " + impulse);
        nodeToApplyGravity.AddImpulse(impulse);
    }

    void ApplyGlobalGravityFixedUpdate()
    {
        // apply Global Gravity once in FixedUpdate(). For comparing to ApplyGlobalGravityCoroutine, to see if this is less bouncy.
        // ToDo: Apply center gravity better. It has two goals: a) keep single nodes from wandering off into space. b) give order to the whole system, condesing it.
        if (!AllStatic)
        {
            // need to loop over it without foreach(=with enumerator), otherwise "InvalidOperationException: Collection was modified; enumeration operation may not execute". see http://answers.unity3d.com/questions/290595/enumeration-operation-may-not-execute-problem-with.html
            for (int i = 0; i < nodeRbList.Count; i++)
            {
                ApplyGlobalGravityToNode(nodeRbList[i]);
            }
        }
    }

    IEnumerator ApplyGlobalGravityCoroutine()
    {
        // apply Global Gravity forever, at custom interval. Test if that works, as it is easier on performance, but probably it also introduces too much bounce.
        // ToDo: Apply center gravity better. It has two goals: a) keep single nodes from wandering off into space. b) give order to the whole system, condesing it.
        for (;;)
        {
            if (!AllStatic)
            {
                // need to loop over it without foreach(=with enumerator), otherwise "InvalidOperationException: Collection was modified; enumeration operation may not execute". see http://answers.unity3d.com/questions/290595/enumeration-operation-may-not-execute-problem-with.html
                for (int i=0 ; i < nodeRbList.Count; i++)
                {
                    ApplyGlobalGravityToNode(nodeRbList[i]);

                    // do a hard yield every globalGravityIteratorBreakCount impulses, too ease it on the cpu. E.g. all cubes stacked in the center of the world being impulsed upon (10 times a second) slows things considerably down when reaching 500 cubes.
                    // So first try here is to limit to 250 impulses every frame. Would allow 12.5k impulses every second when running with 50 FPS.
                    // Whereas some testing shows that the slowdown in the "all cubes in center" mostly comes from one collision causing many subsequent collisions, when all cubes are consended at the center. At 700 cubes in the center breakCount needs to be as low as 1 for smooth fps.
                    // A FPS-dependent breakCount would be an idea. Maybe later, lets get the whole thing running first.

                    if (globalGravityIteratorBreakLoop && i % globalGravityIteratorBreakCount == 0)
                        yield return null;
                }
            }
            yield return new WaitForSeconds(globalGravityInteratorDoneWait);
        }
    }

    public void ResetWorld()
    {
        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("link"))
        {
            Destroy(destroyTarget);
            LinkCount -= 1;
            linkCountTxt.text = "Linkcount: " + LinkCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("node"))
        {
            Destroy(destroyTarget);
            NodeCount -= 1;
            nodeCountTxt.text = "Nodecount: " + NodeCount;
        }

        nodeRbList.Clear();
    }

    public GameObject GenerateNode(string mode)
    {
        Node nodeCreated = null;

        if (mode == "random")
        {
            Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange));

            nodeCreated = Instantiate(nodePrefab, createPos, Quaternion.identity) as Node;
            nodeCreated.name = "node_" + nodeCount;
            nodeCount++;
            nodeCountTxt.text = "Nodecount: " + NodeCount;

        }

        if (nodeCreated != null)
        {
            nodeRbList.Add(nodeCreated.GetComponent<BRigidBody>());

            if (GameController.verbose)
                Debug.Log("GraphController.CreateNode: Node created: " + nodeCreated.gameObject.name);

        } else
        {
            if (GameController.verbose)
                Debug.Log("GraphController.GenerateNode: Something went wrong, did not get a Node Object returned.");
        }

        return nodeCreated.gameObject;
    }

    public GameObject GenerateNode(string mode, Vector3 createPos)
    {
        Node nodeCreated = null;

        if (mode == "specific_xyz")
        {
            nodeCreated = Instantiate(nodePrefab, createPos, Quaternion.identity) as Node;
            nodeCreated.name = "node_" + nodeCount;
            nodeCount++;
            nodeCountTxt.text = "Nodecount: " + NodeCount;
        }

        if (nodeCreated != null)
        {
            nodeRbList.Add(nodeCreated.GetComponent<BRigidBody>());

            if (GameController.verbose)
                Debug.Log("GraphController.CreateNode: Node created: " + nodeCreated.gameObject.name);
        }
        else
        {
            if (GameController.verbose)
                Debug.Log("GraphController.GenerateNode: Something went wrong, did not get a Node Object returned.");
        }

        return nodeCreated.gameObject;
    }

    public GameObject GenerateNode(string mode, string name, string id, string type)
    {
        Node nodeCreated = null;

        if (mode == "specific_initset")
        {
            Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange));

            nodeCreated = Instantiate(nodePrefab, createPos, Quaternion.identity) as Node;
            nodeCreated.name = id;
            nodeCreated.text = name;
            nodeCreated.type = type;
            nodeCount++;
            nodeCountTxt.text = "Nodecount: " + NodeCount;

            nodeRbList.Add(nodeCreated.GetComponent<BRigidBody>());

            if (GameController.verbose)
                Debug.Log("GraphController.CreateNode: Node created: " + nodeCreated.gameObject.name);

            if (nodeCreated == null)
            {
                if (GameController.verbose)
                    Debug.Log("GraphController.GenerateNode: Something went wrong, no node created.");
            }
        }

        return nodeCreated.gameObject;
    }

    public bool CreateLink(GameObject source, GameObject target)
    {
        if (source == null || target == null)
        {
            if (GameController.verbose)
            {
                Debug.Log("GraphController.CreateLink: source or target does not exist. Link not created.");
            }
            return false;
        }
        else
        {
            if (source != target)
            {
                bool alreadyExists = false;
                foreach (GameObject checkObj in GameObject.FindGameObjectsWithTag("link"))
                {
                    Link checkLink = checkObj.GetComponent<Link>();
                    if (checkLink.source == source && checkLink.target == target)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    Link linkObject = Instantiate(linkPrefab, new Vector3(0, 0, 0), Quaternion.identity) as Link;
                    linkObject.name = "link_" + linkCount;
                    linkObject.source = source;
                    linkObject.target = target;
                    linkCount++;
                    linkCountTxt.text = "Linkcount: " + LinkCount;

                    return true;
                }
                else
                {
                    if (GameController.verbose)
                    {
                        Debug.Log("GraphController.CreateLink: Link between source " + source.name + " and target " + target.name + " already exists. Link not created.");
                    }
                    return false;
                }
            }
            else
            {
                if (GameController.verbose)
                {
                    Debug.Log("GraphController.CreateLink: source " + source.name + " and target " + target.name + " are the same. Link not created.");
                }
                return false;
            }
        }
    }

    public void GenerateLink(string mode)
    {
        if (mode == "random")
        {
            bool success = false;
            int tryCounter = 0;
            int tryLimit = nodeCount * 5;

            while (!success && tryCounter < tryLimit)
            {
                tryCounter++;

                int sourceRnd = UnityEngine.Random.Range(0, nodeCount);
                int targetRnd = UnityEngine.Random.Range(0, nodeCount);

                GameObject source = GameObject.Find("node_" + sourceRnd);
                GameObject target = GameObject.Find("node_" + targetRnd);

                success = CreateLink(source, target);
            }
            if (!success)
                if (GameController.verbose)
                    Debug.Log("GraphController.GenerateLink: Too many unsuccessful tries, limit reached. Bailing out of GenerateLink run with mode=random. TryCounter: " + tryCounter + " Limit: " + nodeCount * 5);
        }
    }

    public void GenerateLink(string mode, GameObject source, GameObject target)
    {
        if (mode == "specific_src_tgt")
        {
            bool success = false;

            success = CreateLink(source, target);

            if (!success)
                if (GameController.verbose)
                    Debug.Log("GraphController.GenerateLink: Problem with creating link. Link not created.");
        }
    }

    void Awake()
    {
        nodeCount = 0;
        linkCount = 0;
        nodeRbList.Clear();
    }

    void Update()
    {
        //Node.forceSphereRadius = nodeForceSphereRadius;
        //Node.forceStrength = nodeForceStrength;
        //Node.globalGravity = globalGravity;
        Link.intendedLinkLength = linkIntendedLinkLength;
        Link.forceStrength = linkForceStrength;
    }

    void FixedUpdate()
    {
        // To compare ApplyGlobalGravityCoroutine() with ApplyGlobalGravityFixedUpdate(), test bounciness/performance of partial updates against full updates.
        if (globalGravInFixedUpdate)
        {
            if (globalGravCoRoutineIsRunning)
            {
                StopCoroutine(ApplyGlobalGravityCoroutine());
                globalGravCoRoutineIsRunning = false;
            }

            ApplyGlobalGravityFixedUpdate();
        }
        else
        {
            if (!globalGravCoRoutineIsRunning)
            {
                globalGravCoRoutineIsRunning = true;
                StartCoroutine(ApplyGlobalGravityCoroutine());
            }
        }
    }
}
