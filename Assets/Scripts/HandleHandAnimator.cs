using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class HandleHandAnimator : MonoBehaviour
{
    private Hand hand;
    [SerializeField] private GameObject doorKnob;

    public SortingLayer handLayer;

    void OnTriggerEnter(Collider other)
    {
        //If the gameobject is on the hand layer
        if(other.gameObject.layer == 7)
        {
            //if the hand is holding down the button, play the animation
            Hand hand = other.gameObject.GetComponent<Hand>();

            //If the right hand is colliding and the player presses the right grip button
            if (hand.handType == SteamVR_Input_Sources.RightHand && SteamVR_Input.GetStateDown("GrabGrip", SteamVR_Input_Sources.RightHand, true))
            {
                //change right hand anim
                hand.AttachObject(doorKnob, GrabTypes.Scripted, 0);
                Debug.Log("hello");

            }
            //else if the left hand is colliding and the player presses the left grip button
            else if (hand.handType == SteamVR_Input_Sources.LeftHand && SteamVR_Input.GetStateDown("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
            {

            }

        }
    }
}
