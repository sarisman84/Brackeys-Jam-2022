using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{

    public float movementSpeed;
    public float sprintModifier = 1.2f;
    public float gravity = -9.0f;

    public InputActionReference movementReference;
    public InputActionReference sprintReference;


    private CharacterController charController;
    private float verticalVelocity;
    private Camera mainCam;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        mainCam = Camera.main;

    }

    private void Update()
    {
        if (charController.isGrounded && verticalVelocity < 0)
            verticalVelocity = 0;

        var input = movementReference.action.ReadValue<Vector2>();

        Vector3 forwardDir = new Vector3(mainCam.transform.forward.x, 0, mainCam.transform.forward.z);
        Vector3 rightDir = new Vector3(mainCam.transform.right.x, 0, mainCam.transform.right.z);

        Vector3 move = forwardDir * input.y + rightDir * input.x;
        charController.Move(move * Time.deltaTime * movementSpeed);


        verticalVelocity += gravity * Time.deltaTime;
        charController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    private void OnEnable()
    {
        sprintReference.action.Enable();
        movementReference.action.Enable();
    }

    private void OnDisable()
    {
        sprintReference.action.Enable();
        movementReference.action.Disable();
    }
}
