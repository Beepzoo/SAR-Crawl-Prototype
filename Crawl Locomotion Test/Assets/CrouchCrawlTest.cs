using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class CrouchCrawlTest : MonoBehaviour
{
    //GameObject references
    [SerializeField] private GameObject VRCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject leftHand;

    //Floats
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float crouchHeight;

    //Velocity Reset Timer
    [SerializeField] private float velocityResetTime = 0.5f; //Time of inactivity until resetting the velocity to 0;
    private float timer = 0;

    [SerializeField] private Vector3 finalVelocity;

    //Component References
    private Rigidbody playerRB;

    //Bools
    public bool isCrouching;

    //Slopes
    [SerializeField] private float height = 0.5f;
    [SerializeField] private float heightPadding = 0.05f;


    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    public bool IsCrouching()
    {
        if (VRCamera.transform.localPosition.y < crouchHeight)
        {
            isCrouching = true;
            return true;
        }

        isCrouching = false;
        return false;
    }

    private void Update()
    {
        if (IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.RightHand, true))
        {
            StartCoroutine(CalculateNewPlayerPosition(rightHand));
        }
        else if(IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
        {
            StartCoroutine(CalculateNewPlayerPosition(leftHand));
        }

        VelocityReset();
    }

    private IEnumerator CalculateNewPlayerPosition(GameObject hand)
    {
        Vector3 initialHandPosition = hand.transform.position;

        //Wait until next frame
        yield return new WaitForFixedUpdate();

        Vector3 newHandPosition = hand.transform.position;

        Vector3 diff = newHandPosition - initialHandPosition;

        finalVelocity = new Vector3(-diff.x, 0, -diff.z) * speedMultiplier;

        timer = 0;
    }

    private void VelocityReset()
    {
        timer += Time.deltaTime;

        if (timer > velocityResetTime)
        {
            finalVelocity = Vector3.zero;
            playerRB.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    void ApplyMovement()
    {
        //Clamp min and max speed
        finalVelocity.x = Mathf.Clamp(finalVelocity.x, -maxSpeed, maxSpeed);
        finalVelocity.z = Mathf.Clamp(finalVelocity.z, -maxSpeed, maxSpeed);

        playerRB.velocity += finalVelocity;
    }
}
