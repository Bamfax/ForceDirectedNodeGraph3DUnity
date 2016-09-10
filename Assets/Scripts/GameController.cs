using UnityEngine;

[RequireComponent(typeof(GameCtrlUI))]
[RequireComponent(typeof(GameCtrlHID))]
[RequireComponent(typeof(GameCtrlHelper))]
[RequireComponent(typeof(GameCtrlInputReader))]
[RequireComponent(typeof(GraphController))]

public class GameController : MonoBehaviour {

    // use BulletUnity or PhysX?
    private bool engineBulletUnity = true;
    
    private static GameCtrlHID gameCtrlHID;

    [SerializeField]
    private bool verbose = true;

    public bool EngineBulletUnity
    {
        get
        {
            return engineBulletUnity;
        }
        private set
        {
            engineBulletUnity = value;
        }
    }

    void Start()
    {
        gameCtrlHID = GetComponent<GameCtrlHID>();
    }

    void Update()
    {
        gameCtrlHID.PaintModeController();
    }
}
