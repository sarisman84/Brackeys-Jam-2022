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
            HardOpenCanvas(startingCanvas);
    }




    public void OpenCanvas(Canvas canvas)
    {
        previousCanvas = currentCanvas;
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(false);

        currentCanvas = canvas;
        currentCanvas.gameObject.SetActive(true);
    }

    public void HardOpenCanvas(Canvas canvas)
    {
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(false);
        previousCanvas = canvas;

        currentCanvas = canvas;
        currentCanvas.gameObject.SetActive(true);
    }

    public bool IsCurrentCanvasOpen()
    {
        return currentCanvas && currentCanvas.gameObject.activeSelf;
    }


    public void ExitCurrentCanvas()
    {
        HardExitCurrentCanvas();
        currentCanvas = previousCanvas;
    }


    public void HardExitCurrentCanvas()
    {
        if (previousCanvas)
            previousCanvas.gameObject.SetActive(true);



        currentCanvas.gameObject.SetActive(false);
    }

    public void ExitCurrentCanvas(Func<Canvas, bool> onPreExitEvent = null, Action<Canvas> onPostExitEvent = null)
    {
        if (onPreExitEvent == null || onPreExitEvent.Invoke(currentCanvas))
        {
            ExitCurrentCanvas();
            onPostExitEvent?.Invoke(currentCanvas);
        }


    }




}
