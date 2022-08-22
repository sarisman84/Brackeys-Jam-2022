using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PollingStation
{
    public static PollingStation Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void OnRuntimeStart()
    {
        Instance = new PollingStation();
        Debug.Log("[Log]<PollingStation>: Initialized Polling Station!");
    }


    public OptionsManager optionsManager { get; private set; }

    public PollingStation()
    {
        GameObject managerHolder = new GameObject("Managers");
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();

        for (int i = 0; i < gos.Length; i++)
        {
            var components = gos[i].GetComponents<Component>();
            for (int c = 0; c < components.Length; c++)
            {
                switch (components[c])
                {
                    case OptionsManager om:
                        if (!optionsManager)
                        {
                            optionsManager = om;
                            om.transform.SetParent(managerHolder.transform);
                        }

                        break;

                    default:
                        break;
                }
            }
        }



 
    }
}
