using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FixedCollider : MonoBehaviour
{
    protected FixedTransform fTransform;
    public FixedTransform fixedTransform {
        get => fTransform;
    }
    protected List<FixedCollider> collisions = new();
    protected List<FixedCollider> thisFrame = new();
    protected bool dirty;
    public delegate void OnCollisionEnter(FixedCollider other);
    public OnCollisionEnter onCollisionEnter {
        get;
        set;
    }
    public delegate void OnCollisionStay(FixedCollider other);
    public OnCollisionStay onCollisionStay {
        get;
        set;
    }
    public delegate void OnCollisionExit(FixedCollider other);
    public OnCollisionExit onCollisionExit {
        get;
        set;
    }

    public static bool AABB(Vec2Fix aPos, Vec2Fix aSize, Vec2Fix bPos, Vec2Fix bSize) {
        return  bPos.x < aPos.x + aSize.x 
                && aPos.x < bPos.x + bSize.x
                && bPos.y < aPos.y + aSize.y 
                && aPos.y < bPos.y + bSize.y;
    }

    protected void Awake()
    {
        fTransform = GetComponent<FixedTransform>();
        MarkDirty();
        fTransform.onDirty += MarkDirty;
        FixedPhysics.RegisterCollider(this);

        MarkDirty();
    }

    protected void OnEnable() {
        fTransform.onDirty += MarkDirty;
        FixedPhysics.RegisterCollider(this);

        MarkDirty();
    }

    protected void OnDisable() {
        fTransform.onDirty -= MarkDirty;
        FixedPhysics.DeregisterCollider(this);
    }

    protected void OnDestroy() {
        fTransform.onDirty -= MarkDirty;
        FixedPhysics.DeregisterCollider(this);
    }

    public abstract bool Colliding(FixedCollider other);

    // called automatically when a collision occurs, and invokes either onColliderEnter if needed
    public void Collide(FixedCollider other) {
        if (!thisFrame.Contains(other) && !collisions.Contains(other)) {
            onCollisionEnter?.Invoke(other); 
            
            thisFrame.Add(other);
            other.Collide(this);
        }
    }

    // checks if there are any colliders that were not collided with this frame and calls on collider exit or stay depending
    public void CheckCollisions() {
        for (int i = 0; i < collisions.Count; i++) {

            if (Colliding(collisions[i])) {
                onCollisionStay?.Invoke(collisions[i]);
            } else {
                onCollisionExit?.Invoke(collisions[i]);
                collisions.RemoveAt(i);
                i--;
            }

        }

        foreach (var collider in thisFrame) {
            collisions.Add(collider);
        }
        thisFrame.Clear();
    }

    // marks this collider as dirty in the physics system, meaning it is checked for new collisions
    // this is done automatically whenever the transform this object is linked to is edited
    private void MarkDirty() {
        if (!dirty) {
            FixedPhysics.MarkDirty(this);
            dirty = true;
        }
    }

    // marks this collider as no longer dirty for internal operations
    public void Undirty() {
        dirty = false;
    }
}
