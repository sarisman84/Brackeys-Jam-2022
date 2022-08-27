using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{

    public float movementSpeed;
    public float sprintModifier = 1.2f;
    public float crouchModifier = 0.8f;
    public float adsModifier = 0.5f;
    public float gravity = -9.0f;
    public float jumpHeight;



    [Header("Input References")]
    public InputActionReference movementReference;
    public InputActionReference sprintReference;
    public InputActionReference jumpReference;
    public InputActionReference crouchReference;

    [Header("Debug")]
    public bool activateInputOnStart = false;

    private CinemachineVirtualCamera cameraController;
    private CinemachinePOV povHandler;
    private CharacterController charController;
    private float verticalVelocity;
    private Camera mainCam;
    private CapsuleCollider capsCollider;
    private bool isCrouching;

    private float defaultColHeight;

    private LayerMask headOverlapBox;
    private Vector3 HeadBoxPos { get => transform.position + Vector3.up * ((defaultColHeight * 0.5f) + 0.2f); }//not moving
    //private Vector3 HeadBoxPos { get => transform.position + Vector3.up * (((defaultColHeight * 0.5f) + 0.2f) + (isCrouching ? defaultColHeight*0.25f : 0.0f)); }//follow the player
    private Vector3 HeadBoxSize { get => new Vector3(capsCollider.bounds.extents.x, 0.2f, capsCollider.bounds.extents.z); }
    private bool isCollidingCeiling { get; set; }
    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        mainCam = Camera.main;


        capsCollider = GetComponent<CapsuleCollider>();

        defaultColHeight = capsCollider.height;

        headOverlapBox = ~LayerMask.GetMask("Player");
    }

    private void Start()
    {
        cameraController = PollingStation.Instance.cameraController;
        povHandler = cameraController.GetCinemachineComponent<CinemachinePOV>();


        if (activateInputOnStart)
            PollingStation.Instance.runtimeManager.SetStateToPlay();
    }

    private void Update()
    {
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;



        povHandler.m_HorizontalAxis.m_InputAxisValue = PollingStation.Instance.optionsManager.currentSensitivity;
        povHandler.m_VerticalAxis.m_InputAxisValue = PollingStation.Instance.optionsManager.currentSensitivity;

        bool grounded = charController.isGrounded;
        if (grounded && verticalVelocity < 0)
            verticalVelocity = 0;

        var input = movementReference.action.ReadValue<Vector2>();

        Vector3 forwardDir = new Vector3(mainCam.transform.forward.x, 0, mainCam.transform.forward.z);
        Vector3 rightDir = new Vector3(mainCam.transform.right.x, 0, mainCam.transform.right.z);

        Vector3 move = forwardDir.normalized * input.y + rightDir.normalized * input.x;


        float finalSpeed =
            sprintReference.action.ReadValue<float>() > 0 ? movementSpeed * sprintModifier :
            crouchReference.action.ReadValue<float>() > 0 && grounded ? movementSpeed * crouchModifier :
            PollingStation.Instance.weaponManager?.adsInput.action.ReadValue<float>() > 0 ? movementSpeed * adsModifier :
            movementSpeed;

        charController.Move(move * Time.deltaTime * finalSpeed);





        isCollidingCeiling = Physics.OverlapBox(HeadBoxPos, HeadBoxSize, Quaternion.identity, headOverlapBox).Length > 0;
        if (crouchReference.action.ReadValue<float>() > 0 || isCollidingCeiling)
        {
            if (!isCrouching && grounded)
                Crouch();
        }
        else if (isCrouching && !isCollidingCeiling)
        {
            ResetCrouch();
        }

        if (jumpReference.action.ReadValue<float>() > 0 && grounded && !isCrouching)
        {
            verticalVelocity += Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        if (isCollidingCeiling && !grounded)
        {
            verticalVelocity = gravity * Time.deltaTime;
            //Debug.Log("Hitting ceiling");
        }

        verticalVelocity += gravity * Time.deltaTime;
        charController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);



    }


    void Crouch()
    {
        charController.enabled = false;
        charController.height = defaultColHeight / 2.0f;
        capsCollider.height = defaultColHeight / 2.0f;
        transform.position = new Vector3(transform.position.x, transform.position.y - defaultColHeight / 2.0f, transform.position.z);
        isCrouching = true;
        charController.enabled = true;
    }

    void ResetCrouch()
    {
        charController.enabled = false;
        charController.height = defaultColHeight;
        capsCollider.height = defaultColHeight;
        transform.position = new Vector3(transform.position.x, transform.position.y + defaultColHeight / 4.0f, transform.position.z);
        isCrouching = false;
        charController.enabled = true;
    }

    private void OnEnable()
    {
        sprintReference.action.Enable();
        movementReference.action.Enable();
        jumpReference.action.Enable();
        crouchReference.action.Enable();
    }

    private void OnDisable()
    {
        sprintReference.action.Disable();
        movementReference.action.Disable();
        jumpReference.action.Disable();
        crouchReference.action.Disable();
    }

    private void OnDrawGizmos()
    {
        if (!capsCollider) capsCollider = GetComponent<CapsuleCollider>();

        Gizmos.color = isCollidingCeiling ? Color.green : Color.red;
        Gizmos.DrawCube(HeadBoxPos, HeadBoxSize);
    }
}
