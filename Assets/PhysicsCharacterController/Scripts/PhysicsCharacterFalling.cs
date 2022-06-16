using UnityEngine;

public class PhysicsCharacterFalling : PhysicsCharacterState {

    public override Vector3 MoveModifier => Vector3.zero;

    public override void InitializeState() {
        pc.OnFixedUpdate += OnFixedUpdate;
    }

    public override void ClearState() {
        pc.OnFixedUpdate -= OnFixedUpdate;
    }

    protected void OnFixedUpdate(Rigidbody rb, Vector2 moveInput) {
        bool nearGround = pc.NearGround(out _);

        if (!nearGround) {
            rb.AddForce(Vector3.down * pc.fallForce);   // Add force toward ground
        } else {
            pc.ChangeState(new PhysicsCharacterGrounded(pc));     // Change to grounded state
        }
    }

    public PhysicsCharacterFalling(PhysicsCharacterController pc) : base(pc) { }
}
