using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PollingStation
{
    public static PollingStation Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void OnRuntimeStart()
    {
        Instance = new PollingStation();
        Debug.Log("[Log]<PollingStation>: Initialized Polling Station!");
    }

    #region Getter Properties
    public OptionsManager optionsManager { get; private set; }
    public FPSController fpsController { get; private set; }
    public InteractionManager interactionManager { get; private set; }
    public WeaponManager weaponManager { get; private set; }
    public RuntimeManager runtimeManager { get; private set; }
    public MapGenerator mapGenerator { get; private set; }

    public CinemachineVirtualCamera cameraController { get; private set; }
    #endregion

    public PollingStation()
    {
        GameObject managerHolder = new GameObject("Managers");
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();


        foreach (var gameObject in gos)
        {
            FetchComponents(managerHolder, gameObject);
        }

    }

    private void FetchComponents(GameObject managerHolder, GameObject gameObject)
    {
        if (gameObject.CompareTag("PlayerCam"))
            cameraController = gameObject.GetComponent<CinemachineVirtualCamera>();


        var components = gameObject.GetComponents<Component>();
        for (int c = 0; c < components.Length; c++)
        {
            switch (components[c])
            {
                case OptionsManager om:
                    if (!optionsManager)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Options Manager");
                        optionsManager = om;
                        om.transform.SetParent(managerHolder.transform);
                    }

                    break;

                case FPSController fpsC:
                    if (!fpsController)
                    {
                        Debug.Log("[Log]<PollingStation>: Found FPSController");
                        fpsController = fpsC;

                    }

                    break;

                case InteractionManager im:
                    if (!interactionManager)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Interaction Manager");
                        interactionManager = im;

                    }

                    break;

                case WeaponManager wm:
                    if (!weaponManager)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Weapon Manager");
                        weaponManager = wm;

                    }
                    break;
                case RuntimeManager rm:
                    if (!runtimeManager)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Runtime Manager");
                        runtimeManager = rm;
                        rm.transform.SetParent(managerHolder.transform);
                    }

                    break;


                case MapGenerator mg:
                    if (!mapGenerator)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Map Generator");
                        mapGenerator = mg;
                        mg.transform.SetParent(managerHolder.transform);
                    }
                    break;
                default:
                    break;
            }
        }


        for (int child = 0; child < gameObject.transform.childCount; child++)
        {
            FetchComponents(managerHolder, gameObject.transform.GetChild(child).gameObject);
        }
    }
}
