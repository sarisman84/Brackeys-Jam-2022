using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public Canvas workEnviroment;

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
}
