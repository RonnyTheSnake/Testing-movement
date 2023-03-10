using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] 
    private float movementSpeed;
    public float MovementSpeed { get { return movementSpeed; } }
    [Header("Movement")]    

    public float walkSpeed;
    public float sprintSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    public Transform orientation;

    [Header("Keybindings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask IsGround;

    bool grounded;

    [Header("Slope Handler")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    float horizontalInput;
    float verticalInput;
    Vector3 movementDirection;

    Rigidbody rBody;


    public enum MovementState
    {
        Walk,
        Sprint,
        Dash,
        Airial
    }
    public bool dashing;

    [Header("Movement State")]
    public MovementState currentMovementState;    

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true;
    }

    private float desiredMovementSpeed;
    private float previouslyDesiredMovementSpeed;
    private MovementState previousMovementState;
    private bool keepMomentum;


    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, IsGround);

        if (currentMovementState == MovementState.Walk || currentMovementState == MovementState.Sprint)
        {
            rBody.drag = groundDrag;
        }
        else
        {
            rBody.drag = 0;
        }

        PlayerInput();
        SpeedControl();
        MovementHandler();
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

    private void MovementHandler()
    {

        if (dashing)
        {
            currentMovementState = MovementState.Dash;
            desiredMovementSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if(grounded && Input.GetKey(sprintKey))
        {
            currentMovementState = MovementState.Sprint;
            desiredMovementSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            currentMovementState = MovementState.Walk;
            desiredMovementSpeed = walkSpeed;
        }
        else
        {
            currentMovementState = MovementState.Airial;
            if(desiredMovementSpeed < sprintSpeed)
            {
                desiredMovementSpeed = walkSpeed;
            }
            else
            {
                desiredMovementSpeed = sprintSpeed;
            }
        }

        bool desiredMovementSpeedHasChanged = desiredMovementSpeed != previouslyDesiredMovementSpeed;
        if(previousMovementState == MovementState.Dash)
        {
            keepMomentum = true;
        }

        if (desiredMovementSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMovementSpeed());
            }
            else
            {
                StopAllCoroutines();
                movementSpeed = desiredMovementSpeed;
            }
        }

        previouslyDesiredMovementSpeed = desiredMovementSpeed;
        previousMovementState = currentMovementState;
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMovementSpeed() //Take a deeper look into this method
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - movementSpeed);
        float startValue = movementSpeed;

        float boostFactor = speedChangeFactor;

        while(time < difference)
        {
            movementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time/difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        movementSpeed = desiredMovementSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
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

        else if (OnSlope() && !exitingSlope)
        {
            rBody.AddForce(GetSlopeMovementDirection() * movementSpeed * 20f, ForceMode.Force);

            if(rBody.velocity.y > 0) //to prevent slope "Bumping"
            {
                rBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        rBody.useGravity = !OnSlope(); //To prevent slope sliding
    }

    private void SpeedControl()
    {
        
        //Adjust speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if(rBody.velocity.magnitude > movementSpeed)
            {
                rBody.velocity = rBody.velocity.normalized * movementSpeed;
            }
        }
        else //Adjust speed on ground/in air
        {
            Vector3 velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);

            if (velocity.magnitude > movementSpeed)
            {
                Vector3 limitVelocity = velocity.normalized * movementSpeed;
                rBody.velocity = new Vector3(limitVelocity.x, rBody.velocity.y, limitVelocity.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if(angle < maxSlopeAngle && angle != 0)
            {
                return true;
            }

        }
            return false;
    }

    private Vector3 GetSlopeMovementDirection()
    {
        return Vector3.ProjectOnPlane(movementDirection, slopeHit.normal).normalized;
    }

}
