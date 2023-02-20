using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    public Transform orientation;

    [Header("Keybindings")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask IsGround;

    bool grounded;

    float horizontalInput;
    float verticalInput;
    Vector3 movementDirection;

    Rigidbody rBody;


    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, IsGround);

        if (grounded)
        {
            rBody.drag = groundDrag;
        }
        else
        {
            rBody.drag = 0;
        }

        PlayerInput();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


        if (grounded)
        {
            rBody.AddForce(movementDirection.normalized * movementSpeed * 10f, ForceMode.Force);            
        }
        else if (!grounded)
        {
            rBody.AddForce(movementDirection.normalized * movementSpeed * airMultiplier * 10f, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);

        if(velocity.magnitude > movementSpeed)
        {
            Vector3 limitVelocity = velocity.normalized * movementSpeed;
            rBody.velocity = new Vector3(limitVelocity.x, rBody.velocity.y, limitVelocity.z);
        }
    }

    private void Jump()
    {
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }


}
