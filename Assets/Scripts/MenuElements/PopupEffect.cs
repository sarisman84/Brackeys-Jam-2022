using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PopupEffect
{
    string popupText;
    float duration;
    UIManager uiManager;
    RuntimeManager runtimeManager;
    public PopupEffect(MonoBehaviour coroutineStarter, string somePopupText, float aDuration)
    {
        runtimeManager = PollingStation.Instance.runtimeManager;
        uiManager = PollingStation.Instance.uiManager;
        popupText = somePopupText;
        duration = aDuration;

        coroutineStarter.StartCoroutine(DisplayPopup(duration));
    }

    private IEnumerator DisplayPopup(float duration)
    {

        Canvas canvas = uiManager.workEnviroment;
        TextMeshProUGUI text;
        uiManager.Create(out text);
        if (!text) yield break;
        text.text = popupText;
        text.overflowMode = TextOverflowModes.Overflow;
        text.enableWordWrapping = false;
        text.fontSize = 120;
        text.rectTransform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
        //text.rectTransform.anchorMin = canvas.GetComponent<RectTransform>().anchorMin;
        //text.rectTransform.anchorMax = canvas.GetComponent<RectTransform>().anchorMax;

        text.autoSizeTextContainer = true;

        float curDur = 0;
        while (curDur < duration)
        {
            yield return null;

            if (runtimeManager.currentState != RuntimeManager.RuntimeState.Playing)
                continue;

            curDur += Time.deltaTime;


        }

        uiManager.Clear(text);
    }
}

