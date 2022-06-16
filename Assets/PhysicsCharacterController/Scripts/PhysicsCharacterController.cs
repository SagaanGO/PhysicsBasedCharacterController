using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsCharacterController : MonoBehaviour
{
    private Rigidbody rgbody;
    private PhysicsCharacterState currentState;
    private PhysicsCharacterActions inputActions;
    private InputAction _movementAction;

    [Header("Float")]
    public float rayCheckLength;
    public float rideHeight;
    public float rideSpringStrength;
    public float rideSpringDamper;
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

    [Header("Jump")]
    public float jumpForce;
    public float delayBetweenJumps;
    public float coyoteTimeDuration;

    public event Action<Rigidbody, Vector2> OnFixedUpdate;
    public event Action<Rigidbody> OnJump;
    public event Action<Rigidbody> OnCancelJump;

    private void Awake()
    {
        inputActions = new PhysicsCharacterActions();
        _movementAction = inputActions.Player.Move;
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.CancelJump.performed += CancelJump;
        ChangeState(new PhysicsCharacterFalling(this));

        rgbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void FixedUpdate()
    {
        //DO ALL THE PHYSICS UPDATES
        Vector2 moveInput = _movementAction.ReadValue<Vector2>();
        UpdateMove(moveInput);
        UpdateRotation();
        OnFixedUpdate?.Invoke(rgbody, moveInput);
    }

    public bool NearGround(out RaycastHit rayHit) => Physics.Raycast(transform.position, Vector3.down, out rayHit, rayCheckLength);

    public void ChangeState(PhysicsCharacterState state)
    {
        if (currentState != null)
        {
            Debug.Log($"Exit state: {currentState.GetType()}");
            currentState.ClearState();
        }
        currentState = state;
        currentState.InitializeState();
        Debug.Log($"Enter state: {state.GetType()}");
    }

    void UpdateMove(Vector2 moveInput)
    {
        //Get raw input
        move = new Vector3(moveInput.x, 0, moveInput.y);

        //Normalize to account for diagonals being stronger than simple directions.
        if (move.magnitude > 1.0f)
            move.Normalize();

        //do dotproduct of input to current goal velocity to apply acceleration based on dot to direction (makes sharp turns better).
        Vector3 unitVel = storedGoalVel.normalized;
        float velDot = Vector3.Dot(move, unitVel);
        float accel = acceleration * accelerationFactorFromDot.Evaluate(velDot);
        Vector3 goalVel = move * maxSpeed;

        //Lerp goal velocity towards new calculated goal velocity.
        storedGoalVel = Vector3.MoveTowards(storedGoalVel, goalVel + currentState.MoveModifier, accel * Time.fixedDeltaTime);

        //calculate needed acceleration to reach goal velocity in a single fixed update.
        Vector3 neededAccel = (storedGoalVel - rgbody.velocity) / Time.fixedDeltaTime;

        //clamp the needed acceleration to max possible acceleration.
        float maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot.Evaluate(velDot) ;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        //Add the final force.
        rgbody.AddForce(Vector3.Scale(neededAccel * rgbody.mass, forceScale));
    }

    void UpdateRotation()
    {
        //if current input is out of deadzone, get an upright position facing movement input.
        Quaternion characterCurrent = transform.rotation;
        Quaternion goalRotation = Quaternion.LookRotation(move.magnitude>.05f? move: transform.forward, Vector3.up);
        Quaternion toGoal = goalRotation.ShortestRotation(characterCurrent);

        Vector3 rotAxis;
        float rotDegrees;

        //calculate radians to goal angle.
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        //add torque to rotate character towards the correct upright movement facing position while taking spring strength and damper into account.
        rgbody.AddTorque((rotAxis * (rotRadians* rotationSpringStrength)) - (rgbody.angularVelocity* rotationSpringDamper));
    }

    void Jump(InputAction.CallbackContext obj) => OnJump?.Invoke(rgbody);

    void CancelJump(InputAction.CallbackContext obj) => OnCancelJump?.Invoke(rgbody);

}