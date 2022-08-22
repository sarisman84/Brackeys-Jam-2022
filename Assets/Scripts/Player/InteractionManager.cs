using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask interactionLayer;
    public InputActionReference interactionInput;



    private void Update()
    {
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;

        if (interactionInput.action.ReadValue<float>() > 0 && interactionInput.action.triggered)
        {
            bool intersected = InteractionUtilities.IntersectFromCamera(Camera.main, 10.0f, interactionLayer, out var hitInfo);

            if (intersected && hitInfo.collider.GetComponent<IInteractable>() is IInteractable interactable)
            {
                interactable.OnInteraction(gameObject);
            }
        }
    }

    private void OnEnable()
    {
        interactionInput.action.Enable();
    }

    private void OnDisable()
    {
        interactionInput.action.Disable();
    }

}
