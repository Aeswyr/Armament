using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

[RequireComponent(typeof(FixedTransform))]
public class FixedBody : MonoBehaviour
{
    private FixedTransform fTransform;
    private FixedBounds bounds;
    public Vec2Fix velocity {
        get;
        set;
    }

    public Fix64 gravity;
    void Awake()
    {
        bounds = GetComponent<FixedBounds>();
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

        if (primary.fixedTransform.position.x > secondary.fixedTransform.position.x) {
            offset.x = secondary.ColliderPos.x - (primary.Size.x + primary.ColliderPos.x);
        } else {
            offset.x = (secondary.Size.x + secondary.ColliderPos.x) - primary.ColliderPos.x;
        }
        offset.y = Fix64.Zero;

        fTransform.position += offset;
        
    }

    public void PhysicsFrame() {
        velocity += FixedPhysics.fixedTimestep * gravity * Vec2Fix.down;
        if (velocity != Vec2Fix.zero)
            fTransform.position += FixedPhysics.fixedTimestep * velocity;
        

        if (bounds != null) {

            Vec2Fix size = Vec2Fix.zero;
            Vec2Fix bound = fTransform.position;
            Vec2Fix offset = Vec2Fix.zero;

            foreach (var collider in GetComponentsInChildren<FixedBoxCollider>(true))
                if (!collider.isTrigger) {
                    size = Fix64.Half * collider.Size;
                    offset = collider.fixedTransform.localPosition;
                    break;
                }


            
            if (fTransform.position.x + offset.x + size.x > bounds.right) {
                bound.x = bounds.right - size.x;
            } else if (fTransform.position.x + offset.x - size.x < bounds.left) {
                bound.x = bounds.left + size.x;
            }

            if (fTransform.position.y + offset.y + size.y > bounds.up) {
                bound.y = bounds.up;
            } else if (fTransform.position.y + offset.y - size.y < bounds.down) {
                bound.y = bounds.down;
            }
            fTransform.position = bound;
        }
    }
}
