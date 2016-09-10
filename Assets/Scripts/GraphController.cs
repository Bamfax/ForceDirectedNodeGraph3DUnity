using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using BulletUnity;

public class GraphController : MonoBehaviour {

    [SerializeField]
    private static bool verbose = true;

    private static GameController gameControl;
    private static GameCtrlUI gameCtrlUI;
    private static GameCtrlHelper gameCtrlHelper;

    [SerializeField]
    private bool allStatic = false;
    [SerializeField]
    private bool paintMode = false;
    [SerializeField]
    private bool repulseActive = true;
    [SerializeField]
    private bool debugRepulse = false;

    [SerializeField]
    private GameObject nodePrefabBullet;
    [SerializeField]
    private GameObject nodePrefabPhysX;
    [SerializeField]
    private Link linkPrefab;
    [SerializeField]
    private float nodeVectorGenRange = 7F;

    [SerializeField]
    private bool globalGravInFixedUpdate = false;
    private bool globalGravCoRoutineIsRunning = false;

    [SerializeField]
    private float globalGravityFactor = 0.1f;
    [SerializeField]
    private float repulseForceStrength = 0.1f;
    [SerializeField]
    private float nodePhysXForceSphereRadius = 50F;                         // only works in PhysX; in BulletUnity CollisionObjects are used, which would need removing and readding to the world. Todo: Could implement it somewhen.
    [SerializeField]
    private float linkForceStrength = 6F;
    [SerializeField]
    private float linkIntendedLinkLength = 5F;

    private static int nodeCount;
    private static int linkCount;
    private List<GameObject> debugObjects = new List<GameObject>();

    // variables for global gravity function
    // private List<BRigidBody> nodeRbList = new List<BRigidBody>();
    private List<Component> nodeGenRbList = new List<Component>();
    public bool globalGravityIteratorBreakLoop = true;
    public int globalGravityIteratorBreakCount = 250;
    public float globalGravityInteratorDoneWait = 0.1f;

    public bool AllStatic
    {
        get
        {
            return allStatic;
        }
        set
        {
            allStatic = value;
        }
    }

    public bool PaintMode
    {
        get
        {
            return paintMode;
        }
        set
        {
            paintMode = value;
        }
    }

    public bool RepulseActive
    {
        get
        {
            return repulseActive;
        }
        set
        {
            repulseActive = value;
        }
    }

    public bool DebugRepulse
    {
        get
        {
            return debugRepulse;
        }
        set
        {
            if (debugRepulse != value)
            {
                debugRepulse = value;
                DebugAllNodes();
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

    public float NodePhysXForceSphereRadius
    {
        get
        {
            return nodePhysXForceSphereRadius;
        }
        set
        {
            nodePhysXForceSphereRadius = value;
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

    void DebugAllNodes()
    {
        if (DebugRepulse)
        {
            foreach (GameObject debugObj in debugObjects)
            {
                debugObj.SetActive(true);
                if (debugObj.name == "debugRepulseObj")
                {
                    float sphereDiam = gameCtrlHelper.GetRepulseSphereDiam();
                    debugObj.transform.localScale = new Vector3(sphereDiam, sphereDiam, sphereDiam);
                }
            }
        }
        else
        {
            foreach (GameObject debugObj in debugObjects)
            {
                debugObj.SetActive(false);
            }
        }
    }

    void ApplyGlobalGravityFixedUpdate()
    {
        // apply Global Gravity once in FixedUpdate(). For comparing to ApplyGlobalGravityCoroutine, to see if this is less bouncy.
        // ToDo: Apply center gravity better. It has two goals: a) keep single nodes from wandering off into space. b) give order to the whole system, condesing it.
        if (!AllStatic)
        {
            // need to loop over it without foreach(=with enumerator), otherwise "InvalidOperationException: Collection was modified; enumeration operation may not execute". see http://answers.unity3d.com/questions/290595/enumeration-operation-may-not-execute-problem-with.html
            for (int i = 0; i < nodeGenRbList.Count; i++)
            {
                gameCtrlHelper.ApplyGlobalGravityToNode(nodeGenRbList[i]);
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
                for (int i=0 ; i < nodeGenRbList.Count; i++)
                {
                    gameCtrlHelper.ApplyGlobalGravityToNode(nodeGenRbList[i]);

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
            gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("node"))
        {
            Destroy(destroyTarget);
            NodeCount -= 1;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
        {
            Destroy(destroyTarget);
        }

        debugObjects.Clear();
        nodeGenRbList.Clear();
    }

    private GameObject InstObj(Vector3 createPos)
    {
        if (gameControl.EngineBulletUnity)
        {
            return Instantiate(nodePrefabBullet, createPos, Quaternion.identity) as GameObject;
        }
        else
        {
            return Instantiate(nodePrefabPhysX, createPos, Quaternion.identity) as GameObject;
        }
    }

    public GameObject GenerateNode()
    {
        // Method for creating a Node on random coordinates, e.g. when spawning multiple new nodes

        GameObject nodeCreated = null;

        Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange));

        nodeCreated = InstObj(createPos);

        if (nodeCreated != null)
        {
            nodeCreated.name = "node_" + nodeCount;
            nodeCount++;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;

            //nodeRbList.Add(nodeCreated.GetComponent<BRigidBody>());
            nodeGenRbList.Add(gameCtrlHelper.getRb(nodeCreated.gameObject));

            GameObject debugObj = nodeCreated.transform.FindChild("debugRepulseObj").gameObject;
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);

            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Node created: " + nodeCreated.gameObject.name);

        } else
        {
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Something went wrong, did not get a Node Object returned.");
        }

        return nodeCreated.gameObject;
    }

    public GameObject GenerateNode(Vector3 createPos)
    {
        // Method for creating a Node on specific coordinates, e.g. in Paintmode when a node is created at the end of a paintedLink

        GameObject nodeCreated = null;

        //nodeCreated = Instantiate(nodePrefabBullet, createPos, Quaternion.identity) as Node;
        nodeCreated = InstObj(createPos);

        if (nodeCreated != null)
        {
            nodeCreated.name = "node_" + nodeCount;
            nodeCount++;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;

            //nodeRbList.Add(nodeCreated.GetComponent<BRigidBody>());
            nodeGenRbList.Add(gameCtrlHelper.getRb(nodeCreated.gameObject));

            GameObject debugObj = nodeCreated.transform.FindChild("debugRepulseObj").gameObject;
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);

            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Node created: " + nodeCreated.gameObject.name);
        }
        else
        {
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Something went wrong, did not get a Node Object returned.");
        }

        return nodeCreated.gameObject;
    }

    public GameObject GenerateNode(string name, string id, string type)
    {
        // Method for creating a Node on random coordinates, but with defined labels. E.g. when loaded from a file which contains these label.

        GameObject nodeCreated = null;

        Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange));

        //nodeCreated = Instantiate(nodePrefabBullet, createPos, Quaternion.identity) as Node;
        nodeCreated = InstObj(createPos);

        if (nodeCreated != null)
        {
            if (gameControl.EngineBulletUnity)
            {
                Node nodeNode = nodeCreated.GetComponent<Node>();
                nodeNode.name = id;
                nodeNode.text = name;
                nodeNode.type = type;
            }
            else
            {
                NodePhysX nodeNode = nodeCreated.GetComponent<NodePhysX>();
                nodeNode.name = id;
                nodeNode.text = name;
                nodeNode.type = type;
            }

            nodeCount++;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;

            //nodeGenRbList.Add(nodeCreated.GetComponent<BRigidBody>());
            nodeGenRbList.Add(gameCtrlHelper.getRb(nodeCreated.gameObject));

            GameObject debugObj = nodeCreated.transform.FindChild("debugRepulseObj").gameObject;
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);

            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Node created: " + nodeCreated.gameObject.name);
        }
        else
        {
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Something went wrong, no node created.");
        }

        return nodeCreated.gameObject;
    }

    public bool CreateLink(GameObject source, GameObject target)
    {
        if (source == null || target == null)
        {
            if (verbose)
            {
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": source or target does not exist. Link not created.");
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
                    gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;

                    return true;
                }
                else
                {
                    if (verbose)
                    {
                        Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Link between source " + source.name + " and target " + target.name + " already exists. Link not created.");
                    }
                    return false;
                }
            }
            else
            {
                if (verbose)
                {
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": source " + source.name + " and target " + target.name + " are the same. Link not created.");
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
                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Too many unsuccessful tries, limit reached. Bailing out of GenerateLink run with mode=random. TryCounter: " + tryCounter + " Limit: " + nodeCount * 5);
        }
    }

    public void GenerateLink(string mode, GameObject source, GameObject target)
    {
        if (mode == "specific_src_tgt")
        {
            bool success = false;

            success = CreateLink(source, target);

            if (!success)
                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Problem with creating link. Link not created.");
        }
    }

    public void GenNodes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create a node on random Coordinates
            GenerateNode();
        }
    }

    public void GenLinks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create a link on random Coordinates
            GenerateLink("random");
        }
    }

    void Start()
    {
        gameControl = GetComponent<GameController>();
        gameCtrlUI = GetComponent<GameCtrlUI>();
        gameCtrlHelper = GetComponent<GameCtrlHelper>();

        nodeCount = 0;
        linkCount = 0;
        nodeGenRbList.Clear();
        debugObjects.Clear();

        foreach (GameObject debugObj in GameObject.FindGameObjectsWithTag("debug"))
        {
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);
        }
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
