using UnityEngine;

public class PhysicsCharacterJumping : PhysicsCharacterState {
    public override Vector3 MoveModifier => Vector3.zero;

    public override void InitializeState() {
        pc.OnFixedUpdate += OnFixedUpdate;
        pc.OnCancelJump += CancelJump;
    }

    public override void ClearState() {
        pc.OnFixedUpdate -= OnFixedUpdate;
        pc.OnCancelJump -= CancelJump;
    }

    protected void OnFixedUpdate(Rigidbody rb, Vector2 moveInput) {
        if (rb.velocity.y < 0) {
            pc.ChangeState(new PhysicsCharacterFalling(pc));          // Change to falling state
        }
    }

    protected void CancelJump(Rigidbody rb) {
        pc.ChangeState(new PhysicsCharacterFalling(pc));          // Change to falling state
    }

    public PhysicsCharacterJumping(PhysicsCharacterController pc) : base(pc) { }
}
