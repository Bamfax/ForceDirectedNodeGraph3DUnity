using UnityEngine;

public class GameCtrlHID : MonoBehaviour {

    [SerializeField]
    private bool verbose = true;

    private static GraphController graphControl;
    private static GameCtrlUI gameCtrlUI;
    private static GameCtrlHelper gameCtrlHelper;

    private Vector3 btnDownPointerPos;
    private Vector3 btnUpPointerPos;
    private GameObject btnDownHitGo;
    private GameObject btnUpHitGo;

    public paintedLink paintedLinkPrefab;
    private paintedLink paintedLinkObject;

    public void paintNewLinkedNode(GameObject nodeClickedOn)
    {
        if (verbose)
            Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Entered. Node given: " + nodeClickedOn.name);

        // Create a node on random Coordinates
        GameObject nodeCreated = graphControl.GenerateNode();

        if (nodeCreated != null)
        {
            graphControl.CreateLink(nodeClickedOn, nodeCreated);
        }
    }

    public void PaintModeController()
    {
        // Paint only if not over Panel
        if (!gameCtrlUI.PanelIsPointeroverPanel(Input.mousePosition))
        {
            // Check what was clicked on when MouseButtonDown
            if (Input.GetMouseButtonDown(0))
            {
                btnDownPointerPos = Input.mousePosition;

                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Detected MouseButtonDown. mousePosition: x: " + btnDownPointerPos.x + "   y: " + btnDownPointerPos.y + "  z: " + btnDownPointerPos.z);

                if (graphControl.PaintMode)
                {
                    //Ray ray = Camera.main.ScreenPointToRay(btnDownPointerPos);
                    //hitObjBtnDown = null;
                    //if (Physics.Raycast(ray, out hitInfoBtnDown))

                    btnDownHitGo = null;
                    btnDownHitGo = gameCtrlHelper.ScreenPointToRaySingleHitWrapper(Camera.main, btnDownPointerPos);

                    if (btnDownHitGo != null)
                    {
                        //hitObjBtnDown = hitInfoBtnDown.collider.gameObject;

                        if (verbose)
                            Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonDown: Ray did hit. On Object: " + btnDownHitGo);

                        paintedLinkObject = Instantiate(paintedLinkPrefab, new Vector3(0, 0, 0), Quaternion.identity) as paintedLink;
                        paintedLinkObject.name = "paintedLink";
                        //paintedLinkObject.sourceObj = hitObjBtnDown;
                        //paintedLinkObject.targetVector = hitObjBtnDown.transform.position;
                        paintedLinkObject.sourceObj = btnDownHitGo;
                        paintedLinkObject.targetVector = btnDownHitGo.transform.position;
                    }
                }
            }
            
            if (Input.GetMouseButton(0) && paintedLinkObject != null && graphControl.PaintMode)
            {
                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Entered GetMouseButton, about to start paintlink()");

                float lazyZ = Camera.main.nearClipPlane + 28;
                Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lazyZ));

                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Painting Link. Current mousePosition is: (" + Input.mousePosition.x + ", " + Input.mousePosition.y + ", " + lazyZ + "). Converted worldPosition is: " + mousePosWorld);

                paintedLinkObject.targetVector = mousePosWorld;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                btnUpPointerPos = Input.mousePosition;

                if (verbose)
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Detected MouseButtonUp. mousePosition: x: " + btnUpPointerPos.x + "   y: " + btnUpPointerPos.y + "  z: " + btnUpPointerPos.z);

                if (graphControl.PaintMode)
                {
                    // Ray ray = Camera.main.ScreenPointToRay(btnUpPointerPos);
                    // hitObjBtnUp = null;
                    // if (Physics.Raycast(ray, out hitInfoBtnUp))

                    btnUpHitGo = null;
                    btnUpHitGo = gameCtrlHelper.ScreenPointToRaySingleHitWrapper(Camera.main, btnUpPointerPos);

                    if (btnUpHitGo != null)
                    {
                        //hitObjBtnUp = hitInfoBtnUp.collider.gameObject;

                        if (verbose)
                            Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonUp: Ray did hit. On Object: " + btnUpHitGo);
                        
                        // If on ButtonDown a node was rayhit, and on ButtonUp a different nodes, we just want to link these nodes together
                        if (btnDownHitGo != null && (btnDownHitGo != btnUpHitGo))
                        {
                            if (verbose)
                                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonUp: ButtonDown node and a differing ButtonUp was selected. This linking the ButtonDown node: " + btnDownHitGo + " with the ButtonUp node: " + btnUpHitGo);

                            graphControl.GenerateLink("specific_src_tgt", btnDownHitGo, btnUpHitGo);
                        }
                        // if on ButtonDown no node was rayhit, only on ButtonUp, we create new random node, which is connected to the node that was rayhit on ButtonUp
                        else
                        {
                            if (verbose)
                                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonUp: No ButtonUp node found. Creating new random node connected to this ButtonUp node: " + btnUpHitGo);

                            //paintNewLinkedNode(hitObjBtnUp);
                            paintNewLinkedNode(btnUpHitGo);
                        }

                    }
                    else
                    {
                        float lazyZ = Camera.main.nearClipPlane + 28;
                        Vector3 clickPosWorld = Camera.main.ScreenToWorldPoint(new Vector3(btnUpPointerPos.x, btnUpPointerPos.y, lazyZ));

                        if (verbose)
                            Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": GetMouseButtonDown: Ray did not hit. Creating a node on position: x: " + clickPosWorld.x + "   y: " + clickPosWorld.y + "  z: " + lazyZ);

                        // Create a node on specific Coordinates
                        GameObject yetFreeNode = graphControl.GenerateNode(clickPosWorld);
                        if (paintedLinkObject != null)
                        {
                            //graphControl.GenerateLink("specific_src_tgt", hitObjBtnDown, yetFreeNode);
                            graphControl.GenerateLink("specific_src_tgt", btnDownHitGo, yetFreeNode);
                        }
                    }

                    if (paintedLinkObject != null)
                        GameObject.Destroy(paintedLinkObject.gameObject);
                }

            }
        }
    }

    void Start()
    {
        graphControl = GetComponent<GraphController>();
        gameCtrlUI = GetComponent<GameCtrlUI>();
        gameCtrlHelper = GetComponent<GameCtrlHelper>();
    }
}
