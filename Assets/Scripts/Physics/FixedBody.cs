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

        Vec2Fix offset = new Vec2Fix();

        if (velocity.x > Fix64.Zero)
            offset.x = info.primary.fixedTransform.position.x + ((FixedBoxCollider)info.primary).Size.x - info.secondary.fixedTransform.position.x;
        else if (velocity.x < Fix64.Zero)
            offset.x = info.primary.fixedTransform.position.x - (info.secondary.fixedTransform.position.x + ((FixedBoxCollider)info.secondary).Size.x);

        if (velocity.y > Fix64.Zero)
            offset.y = info.primary.fixedTransform.position.y + ((FixedBoxCollider)info.primary).Size.y - info.secondary.fixedTransform.position.y;
        else if (velocity.y < Fix64.Zero)
            offset.y = info.primary.fixedTransform.position.y - (info.secondary.fixedTransform.position.y + ((FixedBoxCollider)info.secondary).Size.y);

        if (Fix64.Abs(info.primary.fixedTransform.position.x - info.secondary.fixedTransform.position.x) > Fix64.Abs(info.primary.fixedTransform.position.y - info.secondary.fixedTransform.position.y)) {
            offset.y = Fix64.Zero;
        } else if (Fix64.Abs(info.primary.fixedTransform.position.x - info.secondary.fixedTransform.position.x) < Fix64.Abs(info.primary.fixedTransform.position.y - info.secondary.fixedTransform.position.y)) {
            offset.x = Fix64.Zero;
        }

        fTransform.position -= offset;

        Vec2Fix vel = new Vec2Fix();
        if (offset.x != Fix64.Zero)
            vel.x = Fix64.Zero;
        if (offset.y != Fix64.Zero)
            vel.y = Fix64.Zero;

        velocity = velocity;
        
    }

    public void PhysicsFrame() {
        velocity += gravity * Vec2Fix.down;
        if (velocity != Vec2Fix.zero)
            fTransform.position += velocity;
    }
}
