using UnityEngine;

public abstract class PhysicsCharacterState {

    protected PhysicsCharacterController pc;

    public abstract Vector3 MoveModifier { get; }
    public abstract void InitializeState();
    public abstract void ClearState();

    public PhysicsCharacterState() { }
    public PhysicsCharacterState(PhysicsCharacterController pc) {
        this.pc = pc;
    }
}
