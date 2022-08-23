using System;
using System.Collections;
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
                Debug.Log("[Log]<PollingStation>: Initialized Polling Station!");
            }

            return m_Ins;
        }
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
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();


        foreach (var gameObject in gos)
        {
            FetchComponents(gameObject);
        }

    }

    private void FetchComponents(GameObject gameObject)
    {
        if (gameObject.CompareTag("PlayerCam"))
            cameraController = gameObject.GetComponent<CinemachineVirtualCamera>();


        var components = gameObject.GetComponents<Component>();
        foreach (var component in components)
        {
            //Debug.Log($"{component.GetType().Name} on {component.gameObject.name}");

            switch (component)
            {
                case MapGenerator mg:
                    if (!mapGenerator)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Map Generator");
                        mapGenerator = mg;

                    }
                    break;
                case OptionsManager om:
                    if (!optionsManager)
                    {
                        Debug.Log("[Log]<PollingStation>: Found Options Manager");
                        optionsManager = om;

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
