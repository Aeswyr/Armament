using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

[RequireComponent(typeof(FixedTransform))]
public class FixedBody : MonoBehaviour
{
    private FixedTransform fTransform;
    public Vec2Fix velocity {
        get;
        set;
    }

    public Fix64 gravity;
    void Awake()
    {
        fTransform = GetComponent<FixedTransform>();
        FixedPhysics.RegisterBody(this);
        foreach (var collider in GetComponentsInChildren<FixedBoxCollider>(true))
            if (!collider.isTrigger)
                collider.onPhysicsResolution += Resolve;
    }

    void OnEnable() {
        FixedPhysics.RegisterBody(this);
        foreach (var collider in GetComponentsInChildren<FixedBoxCollider>(true))
            if (!collider.isTrigger)
                collider.onPhysicsResolution += Resolve;
    }

    void OnDisable() {
        FixedPhysics.DeregisterBody(this);
        foreach (var collider in GetComponentsInChildren<FixedBoxCollider>(true))
            collider.onPhysicsResolution -= Resolve;
    }

    void OnDestroy() {
        FixedPhysics.DeregisterBody(this);
        foreach (var collider in GetComponentsInChildren<FixedBoxCollider>(true))
            collider.onPhysicsResolution -= Resolve;
    }

    private void Resolve(CollisionInfo info) {

        Vec2Fix offset = -velocity;
        FixedBoxCollider primary = (FixedBoxCollider)info.primary;
        FixedBoxCollider secondary = (FixedBoxCollider)info.secondary;

        FixedBoxCollider col;
        if ((col = FixedPhysics.IsPhysicsColliding(primary, velocity.y * Vec2Fix.down)) == null) {
            offset.x = Fix64.Zero;
        } else if (col != info.secondary) {
        } else {
            if (velocity.x > Fix64.Zero) {
                offset.x = secondary.ColliderPos.x - (primary.Size.x + primary.ColliderPos.x);
            } else if (velocity.x < Fix64.Zero) {
                offset.x = (secondary.Size.x + secondary.ColliderPos.x) - primary.ColliderPos.x;
            }
        }

        if ((col = FixedPhysics.IsPhysicsColliding(primary, velocity.x * Vec2Fix.left)) == null) {
            offset.y = Fix64.Zero;
        } else if (col != info.secondary) {
        } else {
            if (velocity.y > Fix64.Zero) {
                offset.y = secondary.ColliderPos.y - (primary.Size.y + primary.ColliderPos.y);
            } else if (velocity.y < Fix64.Zero) {
                offset.y = (secondary.Size.y + secondary.ColliderPos.y) - primary.ColliderPos.y;
            }
        }

        fTransform.position += offset;

        velocity = new Vec2Fix(velocity.x * (Fix64)Fix64.Sign(offset.x), velocity.y * (Fix64)Fix64.Sign(offset.y));
        
    }

    public void PhysicsFrame() {
        velocity += gravity * Vec2Fix.down;
        if (velocity != Vec2Fix.zero)
            fTransform.position += velocity;
    }
}
