using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class CrouchCrawlTest : MonoBehaviour
{
    [SerializeField] private GameObject VRCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject leftHand;

    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float crouchHeight;

    [SerializeField] private float velocityResetTime = 0.5f; //Time of inactivity until resetting the velocity to 0;
    private float timer = 0;

    private Vector3 finalVelocity;

    public bool isCrouching;

    public bool IsCrouching()
    {
        //if(VRCamera.transform.position.y < crouchHeight)
        //{
        //    isCrouching = true;
        //    return true;
        //}

        //isCrouching = false;
        //return false;

        return true;
    }

    private void Update()
    {
        if (IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.RightHand, true))
        {
            StartCoroutine(CalculateNewPlayerPosition(rightHand));
        }
         
        if(IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
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

        //apply difference in reverse to the player
        //player.transform.position += new Vector3(-diff.x, 0, -diff.z);

        finalVelocity = new Vector3(-diff.x, 0, -diff.z) * speedMultiplier;

        timer = 0;
    }

    private void VelocityReset()
    {
        timer += Time.deltaTime;

        if (timer > velocityResetTime)
        {
            finalVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    void ApplyMovement()
    {
        playerRB.velocity = finalVelocity;
    }
}
