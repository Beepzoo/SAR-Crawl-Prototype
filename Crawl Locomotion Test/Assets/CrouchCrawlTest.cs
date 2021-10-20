using System.Collections;
using System;
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

    //Ramp friction material
    [SerializeField] private PhysicMaterial slider;
    private float savedDynamicFrictionValue;
    private float savedStaticFrictionValue;

    //Floats
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float slopeSpeedMultiplier = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float crouchHeight;

    [SerializeField] private Vector3 finalVelocity;
    private Vector3 angle;

    //Component References
    private Rigidbody playerRB;
    private CharacterController cc;
    private CapsuleCollider capsuleCollider;

    //Bools
    public bool isCrouching;

    //Slopes
    [Header("Results")]
    public float groundSlopeAngle = 0f;             //Angle of the slope in degrees
    public Vector3 groundSlopeDir = Vector3.zero;   //The calculated slope as a vector

    [Header("Settings")]
    public bool showDebug = false;                  //Show debug gizmos and lines
    public LayerMask castingMask;                   //Layer mask for casts. You'll want to ignore the player.
    public float startDistanceFromBottom = 0.2f;    //Should probably be higher than skin width.
    public float sphereCastRadius = 0.25f;
    public float sphereCastDistance = 0.75f;        //How far spherecast moves down from origin point

    public float raycastLength = 0.75f;
    public Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
    public Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);

    public bool isMoving;

    [Header("Ramp")]
    public Transform rampTransform;
    public Transform topOfRamp;
    public Vector3 directionToRamp;
    public float angleToRampInDegrees;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        //Make reference to the current physics material friction values
        savedDynamicFrictionValue = slider.dynamicFriction;
        savedStaticFrictionValue = slider.staticFriction;
    }

    public bool IsCrouching()
    {
        //If the player's head is below a local height variable, then the player is crouching
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
        Crawling();
        VelocityReset();
        CheckMovementVectorDirection();

        if (isMoving)
        {
            //friction on the ramp is reduced
            slider.dynamicFriction = 0f;
            slider.staticFriction = 0f;
        }
        else
        {
            slider.dynamicFriction = savedDynamicFrictionValue;
            slider.staticFriction = savedStaticFrictionValue;
        }
    }

    private void Crawling()
    {
        //If the player is crouching and presses a grip button, calculate the vector with that hand
        if (IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.RightHand, true))
        {
            StartCoroutine(CalculateNewPlayerPosition(rightHand));
        }
        else if (IsCrouching() & SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
        {
            StartCoroutine(CalculateNewPlayerPosition(leftHand));
        }
    }

    private IEnumerator CalculateNewPlayerPosition(GameObject hand)
    {
        //Save the position of the hand
        Vector3 initialHandPosition = hand.transform.position;

        //Wait until next frame
        yield return new WaitForFixedUpdate();

        //Save a new position of the hand
        Vector3 newHandPosition = hand.transform.position;

        //Get the difference between these two positions
        Vector3 diff = newHandPosition - initialHandPosition;

        //Create a final velocity with this difference on the X and Z only
        finalVelocity = new Vector3(-diff.x, 0, -diff.z);
    }

    private void VelocityReset()
    {
        //If the player is holding either the left or right grip button, then the player is moving
        if (SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.RightHand, true) || SteamVR_Input.GetState("GrabGrip", SteamVR_Input_Sources.LeftHand, true))
            isMoving = true;

        else //else the player is not moving and velocities should be reset to zero
        {
            isMoving = false;
            finalVelocity = Vector3.zero;
            playerRB.velocity = Vector3.zero;
        }
    }
    
    void CheckMovementVectorDirection()
    {
        //check whether the new finalvelocity is headed up or down a slope
        if (isMoving)
        {

        }

        Vector3 normalizedVelocity = new Vector3(finalVelocity.x, finalVelocity.y, finalVelocity.z).normalized;

        directionToRamp = topOfRamp.position - normalizedVelocity;

        //return an angle, angle towards the ramp
        //atan2
        angleToRampInDegrees = Mathf.Atan2(directionToRamp.z, directionToRamp.x) * Mathf.Rad2Deg;


        //what angles
        //if dragging away the angle would be 0?
        //and if dragging towards would the angle be 180?
    }

    private void FixedUpdate()
    {
        CheckGround(new Vector3(transform.position.x, transform.position.y - (capsuleCollider.height / 2) + 
            startDistanceFromBottom, transform.position.z));

        if(isMoving)
            ApplyMovement();
    }

    void ApplyMovement()
    {
        //Clamp min and max speed
        finalVelocity.x = Mathf.Clamp(finalVelocity.x, -maxSpeed, maxSpeed);
        finalVelocity.z = Mathf.Clamp(finalVelocity.z, -maxSpeed, maxSpeed);

        //Normalize the magnitude if it is too large
        if (finalVelocity.magnitude > 1.0f)
            finalVelocity.Normalize();

        //playerRB.velocity = finalVelocity * speedMultiplier;

        //create local vector from ramp slope
        if (groundSlopeAngle > 20)
        {
            //finalVelocity = rampTransform.TransformVector(finalVelocity);

            //finalVelocity.x = Mathf.Clamp(finalVelocity.x, -maxSpeed, maxSpeed);
            //finalVelocity.z = Mathf.Clamp(finalVelocity.z, -maxSpeed, maxSpeed);

            playerRB.velocity += finalVelocity * slopeSpeedMultiplier;
        }
        else
        {
            playerRB.velocity += finalVelocity * speedMultiplier;

        }
    }

    /// <summary>
    /// Checks for ground underneath, to determine some info about it, including the slope angle.
    /// </summary>
    /// <param name="origin">Point to start checking downwards from</param>
    public void CheckGround(Vector3 origin)
    {
        // Out hit point from our cast(s)
        RaycastHit hit;

        // SPHERECAST
        // "Casts a sphere along a ray and returns detailed information on what was hit."
        if (Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, castingMask))
        {
            // Angle of our slope (between these two vectors). 
            // A hit normal is at a 90 degree angle from the surface that is collided with (at the point of collision).
            // e.g. On a flat surface, both vectors are facing straight up, so the angle is 0.
            groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Find the vector that represents our slope as well. 
            //  temp: basically, finds vector moving across hit surface 
            Vector3 temp = Vector3.Cross(hit.normal, Vector3.down);
            //  Now use this vector and the hit normal, to find the other vector moving up and down the hit surface
            groundSlopeDir = Vector3.Cross(temp, hit.normal);
        }

        // Now that's all fine and dandy, but on edges, corners, etc, we get angle values that we don't want.
        // To correct for this, let's do some raycasts. You could do more raycasts, and check for more
        // edge cases here. There are lots of situations that could pop up, so test and see what gives you trouble.
        RaycastHit slopeHit1;
        RaycastHit slopeHit2;

        // FIRST RAYCAST
        if (Physics.Raycast(origin + rayOriginOffset1, Vector3.down, out slopeHit1, raycastLength))
        {
            // Debug line to first hit point
            if (showDebug) { Debug.DrawLine(origin + rayOriginOffset1, slopeHit1.point, Color.red); }
            // Get angle of slope on hit normal
            float angleOne = Vector3.Angle(slopeHit1.normal, Vector3.up);

            // 2ND RAYCAST
            if (Physics.Raycast(origin + rayOriginOffset2, Vector3.down, out slopeHit2, raycastLength))
            {
                // Debug line to second hit point
                if (showDebug) { Debug.DrawLine(origin + rayOriginOffset2, slopeHit2.point, Color.red); }
                // Get angle of slope of these two hit points.
                float angleTwo = Vector3.Angle(slopeHit2.normal, Vector3.up);
                // 3 collision points: Take the MEDIAN by sorting array and grabbing middle.
                float[] tempArray = new float[] { groundSlopeAngle, angleOne, angleTwo };
                Array.Sort(tempArray);
                groundSlopeAngle = tempArray[1];
            }
            else
            {
                // 2 collision points (sphere and first raycast): AVERAGE the two
                float average = (groundSlopeAngle + angleOne) / 2;
                groundSlopeAngle = average;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showDebug)
        {
            // Visualize SphereCast with two spheres and a line
            Vector3 startPoint = new Vector3(transform.position.x, transform.position.y - (capsuleCollider.height / 2) + startDistanceFromBottom, transform.position.z);
            Vector3 endPoint = new Vector3(transform.position.x, transform.position.y - (capsuleCollider.height / 2) + startDistanceFromBottom - sphereCastDistance, transform.position.z);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(startPoint, sphereCastRadius);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(endPoint, sphereCastRadius);

            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
