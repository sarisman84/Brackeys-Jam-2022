using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;


public class MenuManager : MonoBehaviour
{
    public Canvas startingCanvas;


    private Canvas previousCanvas;
    private Canvas currentCanvas;

    private void Awake()
    {
        if (startingCanvas)
            OpenCanvas(startingCanvas.gameObject);
    }

    public void OpenCanvas(GameObject targetCanvas)
    {
        previousCanvas = currentCanvas;
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(false);

        currentCanvas = GetCanvas(targetCanvas);
        currentCanvas.gameObject.SetActive(true);
    }


    public void OpenCanvas(Canvas canvas)
    {
        previousCanvas = currentCanvas;
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(false);

        currentCanvas = canvas;
        currentCanvas.gameObject.SetActive(true);
    }

    public bool IsCurrentCanvasOpen()
    {
        return currentCanvas && currentCanvas.gameObject.activeSelf;
    }


    public void ExitCurrentCanvas()
    {
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(true);

       

        currentCanvas.gameObject.SetActive(false);

        currentCanvas = previousCanvas;
    }

    public void ExitCurrentCanvas(Action<Canvas> onExitEvent)
    {
        ExitCurrentCanvas();
        onExitEvent?.Invoke(currentCanvas);

    }


    private Canvas GetCanvas(GameObject obj)
    {
        return obj.GetComponent<Canvas>();
    }



}
