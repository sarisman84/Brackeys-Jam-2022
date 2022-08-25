using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PollingStation
{
    static PollingStation m_Ins;
    public static PollingStation Instance
    {
        get
        {
            if (m_Ins == null)
            {
                m_Ins = new PollingStation();
                if (m_Ins.debugMode)
                    Debug.Log("[Log]<PollingStation>: Initialized Polling Station!");
            }

            return m_Ins;
        }
    }

    #region Getter Properties
    public OptionsManager optionsManager { get; private set; }

    public MenuManager menuManager { get; private set; }
    public FPSController fpsController { get; private set; }
    public InteractionManager interactionManager { get; private set; }
    public WeaponManager weaponManager { get; private set; }
    public RuntimeManager runtimeManager { get; private set; }
    public MapGenerator mapGenerator { get; private set; }
    public GameManager gameManager { get; private set; }

    public CinemachineVirtualCamera cameraController { get; private set; }


    public GameObject player { get; private set; }
    #endregion

    public bool debugMode { private get; set; } = false;

    public PollingStation(bool showDebug = false)
    {
        debugMode = showDebug;
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();


        foreach (var gameObject in gos)
        {
            FetchComponents(gameObject);
        }

    }

    private void FetchComponents(GameObject gameObject)
    {
        switch (gameObject.tag)
        {
            case "PlayerCam":
                cameraController = gameObject.GetComponent<CinemachineVirtualCamera>();
                break;
            case "Player":
                player = gameObject;
                break;
            default:
                break;
        }





        var components = gameObject.GetComponents<Component>();
        foreach (var component in components)
        {
            //Debug.Log($"{component.GetType().Name} on {component.gameObject.name}");

            switch (component)
            {
                case MapGenerator mg:
                    if (!mapGenerator)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Map Generator");
                        mapGenerator = mg;
                    }

                    break;

                case GameManager gm:
                    if (!gameManager) {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Game Manager");
                        gameManager = gm;
                    }

                    break;

                case OptionsManager om:
                    if (!optionsManager)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Options Manager");
                        optionsManager = om;
                    }

                    break;

                case FPSController fpsC:
                    if (!fpsController)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found FPSController");
                        fpsController = fpsC;
                    }

                    break;

                case InteractionManager im:
                    if (!interactionManager)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Interaction Manager");
                        interactionManager = im;
                    }

                    break;

                case WeaponManager wm:
                    if (!weaponManager)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Weapon Manager");
                        weaponManager = wm;
                    }
                    break;
                case RuntimeManager rm:
                    if (!runtimeManager)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Runtime Manager");
                        runtimeManager = rm;
                    }

                    break;

                case MenuManager mm:
                    if (!menuManager)
                    {
                        if (debugMode)
                            Debug.Log("[Log]<PollingStation>: Found Menu Manager");
                        menuManager = mm;
                    }
                    break;

                default:
                    break;

            }
        }




        for (int child = 0; child < gameObject.transform.childCount; child++)
        {
            FetchComponents(gameObject.transform.GetChild(child).gameObject);
        }
    }
}
