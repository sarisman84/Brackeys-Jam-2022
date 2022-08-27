using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Canvas workEnviroment;

    public void Start() {
        PollingStation.Instance.runtimeManager.onPostStateChangeCallback += (RuntimeManager.RuntimeState previousState, RuntimeManager.RuntimeState state) =>
        {
            if (state == RuntimeManager.RuntimeState.MainMenu)
                ClearAll();
        };
    }

    public UIManager Create<T>(out T value) where T : Component
    {
        GameObject obj = new GameObject($"<UIManager/Procedual Element>: {typeof(T).Name}");
        obj.transform.SetParent(workEnviroment.transform);
        obj.AddComponent<RectTransform>();
        value = obj.AddComponent<T>();
        return this;
    }

    public UIManager CreateTextElement<T>(out T value) where T : TMPro.TextMeshProUGUI
    {
        return Create(out value);
    }

    public void Clear<T>(T element) where T: Component
    {
        Destroy(element.gameObject);
    }

    public void ClearAll() {//Destroy all children of workEnvironment
        for(int i = workEnviroment.transform.childCount-1; i >= 0; i--)
            Destroy(workEnviroment.transform.GetChild(i).gameObject);
    }
}
