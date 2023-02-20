using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rBody;
    public float rotationSpeed;

    public Transform focusedLook;
    public GameObject thirdPersonCamera;
    public GameObject focusedCamera;

    public enum CameraType
    {
        Basic,
        Focused
    }

    public CameraType currentCameraType;
    CameraType newCameraType;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchCameraType(CameraType.Basic);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchCameraType(CameraType.Focused);
        }

        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        if(currentCameraType == CameraType.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if(inputDir !=Vector3.zero)
            {
                playerObject.forward = Vector3.Slerp(playerObject.forward, inputDir.normalized.normalized, Time.deltaTime * rotationSpeed);
            }

        }
        if(currentCameraType == CameraType.Focused)
        {
            Vector3 focusedDirection = focusedLook.position - new Vector3(transform.position.x, focusedLook.position.y, transform.position.z);
            orientation.forward = focusedDirection.normalized;

            playerObject.forward = focusedDirection.normalized;
        }

    }

    private void SwitchCameraType(CameraType newCameraType)
    {
        thirdPersonCamera.SetActive(false);
        focusedCamera.SetActive(false);

        if(newCameraType == CameraType.Basic)
        {
            thirdPersonCamera.SetActive(true);
        }
        if (newCameraType == CameraType.Focused)
        {
            focusedCamera.SetActive(true);
        }

        currentCameraType = newCameraType;
    }
}
