using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class OpenDoor : MonoBehaviour
{
    private Vector3 force;
    private Vector3 cross;
    public bool holdingHandle;
    private float angle;
    private const float forceMultiplier = 150f;

    private void HandHoverUpdate(Hand hand)
    {
        if (hand.handType == SteamVR_Input_Sources.RightHand && SteamVR_Input.GetStateDown("GrabGrip", SteamVR_Input_Sources.RightHand, true) 
            || hand.handType == SteamVR_Input_Sources.LeftHand && SteamVR_Input.GetStateDown("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
        {
            holdingHandle = true;

            // Direction vector from the door's pivot point to the hand's current position
            Vector3 doorPivotToHand = hand.transform.position - transform.parent.position;

            // Ignore the y axis of the direction vector
            doorPivotToHand.y = 0;

            // Direction vector from door handle to hand's current position
            force = hand.transform.position - transform.position;

            // Cross product between force and direction. 
            cross = Vector3.Cross(doorPivotToHand, force);
            angle = Vector3.Angle(doorPivotToHand, force);
        }
        else if (hand.handType == SteamVR_Input_Sources.RightHand && SteamVR_Input.GetStateUp("GrabGrip", SteamVR_Input_Sources.RightHand, true) 
            || hand.handType == SteamVR_Input_Sources.LeftHand && SteamVR_Input.GetStateUp("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
        {
            holdingHandle = false;
        }
    }

    void Update()
    {
        if (holdingHandle)
        {
            // Apply cross product and calculated angle to
            GetComponentInParent<Rigidbody>().angularVelocity = cross * angle * forceMultiplier;

            //If holding the handle but the buttons are not pressed, then holding handle is false
            if (!(SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.RightHand, false) && SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.LeftHand, false)))
                holdingHandle = false;

            //if (SteamVR_Input.GetStateUp("GrabGrip", SteamVR_Input_Sources.RightHand, true) && SteamVR_Input.GetStateUp("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
            //    holdingHandle = false;
        }
    }

    private void OnHandHoverEnd()
    {
        // Set angular velocity to zero if the hand stops hovering
        GetComponentInParent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
