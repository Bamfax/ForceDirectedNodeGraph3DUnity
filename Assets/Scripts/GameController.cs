using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ProgressBar;
using BulletUnity;
using BulletSharp;

[RequireComponent(typeof(GraphController))]

public class GameController : MonoBehaviour {

    public Text statusText;

    public static GameController gameControl;
    public static GraphController graphControl;

    public paintedLink paintedLinkPrefab;

    public static bool verbose = true;
    private static bool paintMode = false;

    private Vector3 posOnBtnDown = new Vector3();
    // private RaycastHit hitInfoBtnDown;
    // private GameObject hitObjBtnDown;
    private Vector3 posOnBtnUp = new Vector3();
    private RaycastHit hitInfoBtnUp;
    //private GameObject hitObjBtnUp;
    private paintedLink paintedLinkObject;

    private RectTransform panelrecttrans;

    private ProgressBarBehaviour progressBar;
    private GameObject progressBarObj;

    private CollisionObject hitObjBtnDown;
    private BRigidBody hitRbBtnDown;
    private CollisionObject hitObjBtnUp;
    private BRigidBody hitRbBtnUp;

    class nodeListObj
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }
    private List<nodeListObj> nodesList = new List<nodeListObj>();

    class linkListObj
    {
        public string id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string target { get; set; }
    }
    private List<linkListObj> linksList = new List<linkListObj>();

    public bool PaintMode
    {
        get
        {
            return paintMode;
        }
        private set
        {
            paintMode = value;
        }
    }

    //Method for loading the GraphML layout file
    private IEnumerator LoadInputFile(string sourceFile)
    {
        progressBarObj.SetActive(true);
        statusText.text = "Loading file: " + sourceFile;
        progressBar.Value = 0F;

        //determine which platform to load for
        string xml = null;
        if (Application.isWebPlayer)
        {
            WWW www = new WWW(sourceFile);
            yield return www;
            xml = www.text;
        }
        else
        {
            StreamReader sr = new StreamReader(sourceFile);
            xml = sr.ReadToEnd();
            sr.Close();
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        statusText.text = "Loading Topology";

        XmlElement root = xmlDoc.FirstChild as XmlElement;
        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlElement xmlGraph = root.ChildNodes[i] as XmlElement;

            int myNodeCount = 0;
            int myLinkCount = 0;

            int childCount = xmlGraph.ChildNodes.Count;

            for (int j = 0; j < childCount; j++)
            {

                progressBar.Value = (float) j / (float) childCount * 100F;

                XmlElement xmlNode = xmlGraph.ChildNodes[j] as XmlElement;

                //create nodes
                statusText.text = "Loading: " + xmlNode.Attributes["id"].Value;
                if (xmlNode.Name == "node" && myNodeCount < 50)
                {
                    myNodeCount++;
                    nodesList.Add(new nodeListObj { id      = xmlNode.Attributes["id"].Value,
                                                    name    = xmlNode.Attributes["name"].Value,
                                                    type    = xmlNode.Attributes["type"].Value,
                                                    x       = float.Parse(xmlNode.Attributes["x"].Value),
                                                    y       = float.Parse(xmlNode.Attributes["y"].Value),
                                                    z       = float.Parse(xmlNode.Attributes["z"].Value) });
                }

                //create links
                if (xmlNode.Name == "edge" && myLinkCount < 100)
                {
                    myLinkCount++;
                    linksList.Add(new linkListObj { id      = xmlNode.Attributes["id"].Value,
                                                    name    = xmlNode.Attributes["label"].Value,
                                                    source  = xmlNode.Attributes["source"].Value,
                                                    target  = xmlNode.Attributes["target"].Value });
                }

                //every 100 cycles return control to unity
                if (j % 100 == 0)
                    yield return true;
            }
        }

        progressBar.Value = 100F;
        statusText.text = "Loading done.";
        progressBar.Value = 0F;
        progressBarObj.SetActive(false);

        statusText.text = "Generating graph...";

        foreach (nodeListObj genNode in nodesList)
        {
            graphControl.GenerateNode("specific_initset", genNode.name, genNode.id, genNode.type);
        }

        foreach (linkListObj genLink in linksList)
        {
            graphControl.GenerateLink("specific_src_tgt", GameObject.Find(genLink.source), GameObject.Find(genLink.target));
        }

        statusText.text = "Graph done.";
    }

    public void LoadandGenWorld()
    {
        nodesList.Clear();
        linksList.Clear();

        string sourceFile = EditorUtility.OpenFilePanel("Open Input File", Application.dataPath + "/Data", "");
        if (sourceFile.Length != 0)
        {
            StartCoroutine(LoadInputFile(sourceFile));
        }
    }

    public void GenNodes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            graphControl.GenerateNode("random");
        }
    }

    public void GenLinks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            graphControl.GenerateLink("random");
        }
    }

    public void TogglePaintMode(Toggle tgl)
    {
        if (tgl.isOn)
        {
            PaintMode = true;
            if (verbose)
                Debug.Log("PaintMode active");
        }
        else
        {
            PaintMode = false;
            if (verbose)
                Debug.Log("PaintMode inactivate");
        }

    }

    public void paintNewLinkedNode(GameObject nodeClickedOn)
    {
        if (GameController.verbose)
            Debug.Log("GameController.paintNewLinkedNode: Entered. Node given: " + nodeClickedOn.name);

        GameObject nodeCreated = graphControl.GenerateNode("random");

        if (nodeCreated!= null)
        {
            graphControl.CreateLink(nodeClickedOn, nodeCreated);
        }
    }

    void InputControl()
    {
        if (Input.mousePosition.x > panelrecttrans.rect.xMax)
        {
            if (Input.GetMouseButtonDown(0))
            {
                posOnBtnDown = Input.mousePosition;

                if (verbose)
                    Debug.Log("GameController.Update: Detected MouseButtonDown. mousePosition: x: " + posOnBtnDown.x + "   y: " + posOnBtnDown.y + "  z: " + posOnBtnDown.z);

                if (paintMode)
                {
                    //Ray ray = Camera.main.ScreenPointToRay(posOnBtnDown);
                    //hitObjBtnDown = null;
                    //if (Physics.Raycast(ray, out hitInfoBtnDown))

                    hitObjBtnDown = null; hitRbBtnDown = null;
                    hitObjBtnDown = BCamera.ScreenPointToRay(Camera.main, posOnBtnDown, CollisionFilterGroups.SensorTrigger, CollisionFilterGroups.DefaultFilter);

                    if (hitObjBtnDown != null)
                    {
                        //hitObjBtnDown = hitInfoBtnDown.collider.gameObject;
                        hitRbBtnDown = (BRigidBody)hitObjBtnDown.UserObject as BRigidBody;

                        if (GameController.verbose)
                            Debug.Log("GameController.Update: GetMouseButtonDown: Ray did hit. On Object: " + hitObjBtnDown + ". hitRb: " + hitRbBtnDown);

                        paintedLinkObject = Instantiate(paintedLinkPrefab, new Vector3(0, 0, 0), Quaternion.identity) as paintedLink;
                        paintedLinkObject.name = "paintedLink";
                        //paintedLinkObject.sourceObj = hitObjBtnDown;
                        //paintedLinkObject.targetVector = hitObjBtnDown.transform.position;
                        paintedLinkObject.sourceObj = hitRbBtnDown.gameObject;
                        paintedLinkObject.targetVector = hitRbBtnDown.transform.position;
                    }
                }
            }

            if (Input.GetMouseButton(0) && paintedLinkObject != null && paintMode)
            {
                if (GameController.verbose)
                    Debug.Log("GameController.Update: Entered GetMouseButton, about to start paintlink()");

                float lazyZ = Camera.main.nearClipPlane + 28;
                Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lazyZ));

                if (verbose)
                    Debug.Log("GameController.paintLink: Painting Link. Current mousePosition is: x: " + Input.mousePosition.x + "   y: " + Input.mousePosition.y + "  z: " + lazyZ + "Converted worldPosition is: x: " + mousePosWorld.x + "   y: " + mousePosWorld.y + "  z: " + mousePosWorld.z);

                paintedLinkObject.targetVector = mousePosWorld;
            }

            if (Input.GetMouseButtonUp(0))
            {
                posOnBtnUp = Input.mousePosition;

                if (verbose)
                    Debug.Log("GameController.Update: Detected MouseButtonUp. mousePosition: x: " + posOnBtnUp.x + "   y: " + posOnBtnUp.y + "  z: " + posOnBtnUp.z);

                if (paintMode)
                {
                    // Ray ray = Camera.main.ScreenPointToRay(posOnBtnUp);
                    // hitObjBtnUp = null;
                    // if (Physics.Raycast(ray, out hitInfoBtnUp))

                    hitObjBtnUp = null; hitRbBtnUp = null;
                    hitObjBtnUp = BCamera.ScreenPointToRay(Camera.main, posOnBtnUp, CollisionFilterGroups.SensorTrigger, CollisionFilterGroups.DefaultFilter);

                    if (hitObjBtnUp != null)
                    {
                        //hitObjBtnUp = hitInfoBtnUp.collider.gameObject;
                        hitRbBtnUp = (BRigidBody)hitObjBtnUp.UserObject as BRigidBody;

                        if (GameController.verbose)
                            Debug.Log("GameController.Update: GetMouseButtonUp: Ray did hit. On Object: " + hitRbBtnUp);

                        // If on ButtonDown a node was rayhit, and on ButtonUp a different nodes, we just want to link these nodes together
                        if (hitRbBtnDown != null && (hitRbBtnDown != hitRbBtnUp))
                        {
                            if (GameController.verbose)
                                Debug.Log("GameController.Update: GetMouseButtonUp: ButtonDown node and a differing ButtonUp was selected. This linking the ButtonDown node: " + hitRbBtnDown + " with the ButtonUp node: " + hitRbBtnUp);

                            graphControl.GenerateLink("specific_src_tgt", hitRbBtnDown.gameObject, hitRbBtnUp.gameObject);
                        }
                        // if on ButtonDown no node was rayhit, only on ButtonUp, we create new random node, which is connected to the node that was rayhit on ButtonUp
                        else
                        {
                            if (GameController.verbose)
                                Debug.Log("GameController.Update: GetMouseButtonUp: No ButtonUp node found. Creating new random node connected to this ButtonUp node: " + hitRbBtnUp);

                            //paintNewLinkedNode(hitObjBtnUp);
                            paintNewLinkedNode(hitRbBtnUp.gameObject); 
                        }

                    }
                    else
                    {
                        float lazyZ = Camera.main.nearClipPlane + 28;
                        Vector3 clickPosWorld = Camera.main.ScreenToWorldPoint(new Vector3(posOnBtnUp.x, posOnBtnUp.y, lazyZ));

                        if (GameController.verbose)
                            Debug.Log("GameController.Update: GetMouseButtonDown: Ray did not hit. Creating a node on position: x: " + clickPosWorld.x + "   y: " + clickPosWorld.y + "  z: " + lazyZ);

                        GameObject yetFreeNode = graphControl.GenerateNode("specific_xyz", clickPosWorld);
                        if (paintedLinkObject != null)
                        {
                            //graphControl.GenerateLink("specific_src_tgt", hitObjBtnDown, yetFreeNode);
                            graphControl.GenerateLink("specific_src_tgt", hitRbBtnDown.gameObject, yetFreeNode);
                        }
                    }

                    if (paintedLinkObject != null)
                        GameObject.Destroy(paintedLinkObject.gameObject);
                }

            }
        }
    }

    void Awake()
    {
        gameControl = GetComponent<GameController>();
        graphControl = GetComponent<GraphController>();
        progressBar = FindObjectOfType<ProgressBarBehaviour>();
        progressBarObj = progressBar.gameObject;
        panelrecttrans = GameObject.Find("PanelLeft").GetComponent<RectTransform>();
        progressBarObj.SetActive(false);
    }

    void Update()
    {
        InputControl();
    }
}
