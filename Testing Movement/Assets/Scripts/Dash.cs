using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rBody;
    private PlayerMovement playerMovement;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float dashCooldown;
    private float dashCooldownTimer;

    [Header("Effects")]
    public ThirdPersonCamera camera;
    public float dashFov;
    private float resetFov = 85f;
    private TrailRenderer trail;

    public float trailTimerReset;
    public float trailTimer;

    public ParticleSystem particles;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;



    // Start is called before the first frame update
    void Start()
    {
        trailTimer = trailTimerReset;

        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;

        rBody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void PlayerDash()
    {
        if (dashCooldownTimer > 0)
        {
            return;
        }
        else
        {
            dashCooldownTimer = dashCooldown;

            playerMovement.dashing = true;

            camera.DashFOV(dashFov);
            particles.Play();

            Vector3 applyForce = orientation.forward * dashForce + orientation.up * dashUpwardForce;

            //rBody.AddForce(applyForce, ForceMode.Impulse);

            delayedApplyForce = applyForce;
            Invoke(nameof(DelayedDashForce), 0.025f);

            Invoke(nameof(ResetPlayerDash), dashDuration);
        }

    }

    private Vector3 delayedApplyForce;

    private void DelayedDashForce()
    {
        rBody.AddForce(delayedApplyForce, ForceMode.Impulse);
    }

    private void ResetPlayerDash()
    {
        playerMovement.dashing = false;
        camera.DashFOV(resetFov);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            PlayerDash();
            trail.enabled = false;
            trailTimer = trailTimerReset;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        trailTimer -= Time.deltaTime;
        if (trail.enabled)
        {
            if(trailTimer <= 0)
            {
                trail.enabled = false;
            }
        }
    }
}
