using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacterController : MonoBehaviour
{
    [Header("Setup")]
    public Rigidbody rgbody;

    [Header("Float")]
    public float rayCheckLength;
    public float rideHeight;
    public float rideSpringStrength;
    public float rideSpringDamper;
    private bool nearGround;
    public float fallForce;

    [Header("Rotation")]
    public float rotationSpringStrength;
    public float rotationSpringDamper;

    [Header("Movement")]
    public float maxSpeed;
    private Vector3 move;
    public float acceleration;
    public AnimationCurve accelerationFactorFromDot;
    public float maxAccelForce;
    public AnimationCurve maxAccelerationForceFactorFromDot;

    public Vector3 forceScale;
    private Vector3 storedGoalVel;
    private Vector3 groundVel;

    [Header("Jump")]
    public float jumpForce;
    private bool canJump=true;
    public float delayBetweenJumps;
    public float coyoteTimeDuration;
    private bool isInCoyoteTime=false;

    private void Update()
    {
        //You can jump whenever you're near the ground or in coyote time. The difference between your ground check and ride height acts as an input buffering distance.
        if (Input.GetButtonDown("Jump") && (nearGround || isInCoyoteTime) && canJump)
            StartCoroutine(Jump());
    }

    void FixedUpdate()
    {
        //DO ALL THE PHYSICS UPDATES
        UpdateMove();
        UpdateFloat();
        UpdateRotation();
        UpdateFall();
    }

    void UpdateMove()
    {
        //Get raw input
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //Normalize to account for diagonals being stronger than simple directions.
        if (move.magnitude > 1.0f)
            move.Normalize();

        //do dotproduct of input to current goal velocity to apply acceleration based on dot to direction (makes sharp turns better).
        Vector3 unitVel = storedGoalVel.normalized;
        float velDot = Vector3.Dot(move, unitVel);
        float accel = acceleration * accelerationFactorFromDot.Evaluate(velDot);
        Vector3 goalVel = move * maxSpeed;

        //Lerp goal velocity towards new calculated goal velocity.
        storedGoalVel = Vector3.MoveTowards(storedGoalVel, goalVel + groundVel, accel * Time.fixedDeltaTime);

        //calculate needed acceleration to reach goal velocity in a single fixed update.
        Vector3 neededAccel = (storedGoalVel - rgbody.velocity) / Time.fixedDeltaTime;

        //clamp the needed acceleration to max possible acceleration.
        float maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot.Evaluate(velDot) ;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        //Add the final force.
        rgbody.AddForce(Vector3.Scale(neededAccel * rgbody.mass, forceScale));
    }

    void UpdateFloat()
    {
        //Raycast down.
        Vector3 rayDir = Vector3.down;

        RaycastHit _rayHit;

        //Check if near ground. If isn't but was on last frame, activate coyote time.
        bool cachedNearGround = nearGround;
        nearGround = Physics.Raycast(transform.position, rayDir, out _rayHit, rayCheckLength);
        if (!nearGround && cachedNearGround)
            StartCoroutine(CoyoteTime());
        
        //If raycast touches ground
        if (nearGround)
        {
            Vector3 vel = rgbody.velocity;

            Vector3 otherVel = Vector3.zero;
            groundVel = Vector3.zero;

            //if rigidbody found, get its velocity (useful for moving platforms and the like)
            Rigidbody hitBody = _rayHit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
                groundVel = hitBody.velocity;
            }
            
            //calculate spring force to add to reach ride height with spring strength and damper taken into account.
            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;
            float x = _rayHit.distance - rideHeight;
            float springForce = (x * rideSpringStrength) - (relVel * rideSpringDamper);

            //add spring force.
            rgbody.AddForce(rayDir * springForce);

            //Add reciprocal force to object under feet.
            if (hitBody != null)
                hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
        }
    }

    void UpdateFall()
    {
        //add a downwards force whenever the player is in the air and isn't ascending.
        if (!nearGround && rgbody.velocity.y <= 0)
            rgbody.AddForce(Vector3.down * fallForce);
    }

    void UpdateRotation()
    {
        //if current input is out of deadzone, get an upright position facing movement input.
        Quaternion characterCurrent = transform.rotation;
        Quaternion goalRotation = Quaternion.LookRotation(move.magnitude>.05f? move: transform.forward, Vector3.up);
        Quaternion toGoal = StaticFunctions_Quaternions.ShortestRotation(goalRotation, characterCurrent);

        Vector3 rotAxis;
        float rotDegrees;

        //calculate radians to goal angle.
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        //add torque to rotate character towards the correct upright movement facing position while taking spring strength and damper into account.
        rgbody.AddTorque((rotAxis * (rotRadians* rotationSpringStrength)) - (rgbody.angularVelocity* rotationSpringDamper));
    }

    IEnumerator Jump()
    {
        //Zero current Y velocity
        rgbody.velocity = new Vector3(rgbody.velocity.x, 0, rgbody.velocity.z);

        //Add vertical jump force
        rgbody.AddForce(Vector3.up * jumpForce);

        //Delay until next jump
        canJump = false;
        yield return new WaitForSeconds(delayBetweenJumps);
        canJump = true;
    }

    IEnumerator CoyoteTime()
    {
        isInCoyoteTime = true;
        yield return new WaitForSeconds(coyoteTimeDuration);
        isInCoyoteTime = false;
    }

}