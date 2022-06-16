using System.Collections;
using UnityEngine;

public class PhysicsCharacterGrounded : PhysicsCharacterState {

    public PhysicsCharacterGrounded(PhysicsCharacterController pc) : base(pc) { }

    public override Vector3 MoveModifier => _groundVel;
    private Vector3 _groundVel;
    private bool _nearGround;
    private Coroutine _coyoteTime;

    public override void InitializeState() {
        pc.OnJump += Jump;
        pc.OnFixedUpdate += OnFixedUpdate;
    }

    public override void ClearState() {
        ResetCoyoteTime();
        pc.OnJump -= Jump;
        pc.OnFixedUpdate -= OnFixedUpdate;
    }

    protected void OnFixedUpdate(Rigidbody rb, Vector2 moveInput) {

        _nearGround = pc.NearGround(out var rayHit);

        if (!_nearGround) {
            _coyoteTime = pc.StartCoroutine(CoyoteTime());      // Start Coyote Time

        } else {
            ResetCoyoteTime();    // Reset coyote time

            Vector3 vel = rb.velocity;

            Vector3 otherVel = Vector3.zero;
            _groundVel = Vector3.zero;

            //if rigidbody found, get its velocity (useful for moving platforms and the like)
            Rigidbody hitBody = rayHit.rigidbody;
            if (hitBody != null) {
                otherVel = hitBody.velocity;
                _groundVel = hitBody.velocity;
            }

            //calculate spring force to add to reach ride height with spring strength and damper taken into account.
            float rayDirVel = Vector3.Dot(Vector3.down, vel);
            float otherDirVel = Vector3.Dot(Vector3.down, otherVel);

            float relVel = rayDirVel - otherDirVel;
            float x = rayHit.distance - pc.rideHeight;
            float springForce = (x * pc.rideSpringStrength) - (relVel * pc.rideSpringDamper);

            //add spring force.
            rb.AddForce(Vector3.down * springForce);

            //Add reciprocal force to object under feet.
            if (hitBody != null)
                hitBody.AddForceAtPosition(Vector3.down * -springForce, rayHit.point);
        }
    }

    protected void Jump(Rigidbody rb) {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);     // Zero current Y velocity
        rb.AddForce(Vector3.up * pc.jumpForce);         // Add vertical jump force
        pc.ChangeState(new PhysicsCharacterJumping(pc));          // Change state to jump state
    }

    protected void ResetCoyoteTime() {
        if (_coyoteTime != null) {
            pc.StopCoroutine(_coyoteTime);
            _coyoteTime = null;
        }
    }

    protected IEnumerator CoyoteTime() {
        yield return new WaitForSeconds(pc.coyoteTimeDuration);     // Wait for coyote time duration
        pc.ChangeState(new PhysicsCharacterFalling(pc));          // Change state to fall state
    }
}
