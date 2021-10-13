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

    //Floats
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float crouchHeight;

    //Velocity Reset Timer
    [SerializeField] private float velocityResetTime = 0.5f; //Time of inactivity until resetting the velocity to 0;
    private float timer = 0;

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


    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        isMoving = false;
        
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
        Crawling();
        VelocityReset();
    }

    private void Crawling()
    {
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
        Vector3 initialHandPosition = hand.transform.position;

        //Wait until next frame
        yield return new WaitForFixedUpdate();

        Vector3 newHandPosition = hand.transform.position;

        Vector3 diff = newHandPosition - initialHandPosition;

        finalVelocity = new Vector3(-diff.x, 0, -diff.z);

        timer = 0;
        isMoving = true;
    }

    private void VelocityReset()
    {
        timer += Time.deltaTime;
        

        if (timer > velocityResetTime)
        {
            finalVelocity = Vector3.zero;
            playerRB.velocity = Vector3.zero;
            isMoving = false;
        }
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

        //cc.Move(finalVelocity * speedMultiplier * Time.deltaTime);
        //playerRB.velocity += finalVelocity;

        //If player is on a slope
        if (groundSlopeAngle > 10)
        {
            playerRB.velocity = -groundSlopeDir * speedMultiplier;
        }
        else
        {
            playerRB.velocity = finalVelocity * speedMultiplier;

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
