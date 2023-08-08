using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public abstract class FixedCollider : MonoBehaviour
{
    protected FixedTransform fTransform;
    public FixedTransform fixedTransform {
        get => fTransform;
    }
    protected List<FixedCollider> collisions = new();
    protected List<FixedCollider> thisFrame = new();
    protected bool dirty;
    public delegate void OnCollisionEnter(CollisionInfo info);
    public OnCollisionEnter onCollisionEnter {
        get;
        set;
    }
    public delegate void OnCollisionStay(CollisionInfo info);
    public OnCollisionStay onCollisionStay {
        get;
        set;
    }
    public delegate void OnCollisionExit(CollisionInfo info);
    public OnCollisionExit onCollisionExit {
        get;
        set;
    }

    //dont interact with this. if you do and stuff breaks, thats your problem, not mine
    public delegate void OnPhysicsResolution(CollisionInfo info);
    public OnPhysicsResolution onPhysicsResolution {
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
            CollisionInfo info = new CollisionInfo(this, other);
            onCollisionEnter?.Invoke(info);

            if (this is FixedBoxCollider && !((FixedBoxCollider)this).isTrigger
                && other is FixedBoxCollider && !((FixedBoxCollider)other).isTrigger) {
                onPhysicsResolution?.Invoke(info);
                
                other.onCollisionEnter?.Invoke(new CollisionInfo(other, this));
                other.thisFrame.Add(this);
                return;
            }
            
            thisFrame.Add(other);
            other.Collide(this);
        }
    }

    // checks if there are any colliders that were not collided with this frame and calls on collider exit or stay depending
    public void CheckCollisions() {
        for (int i = 0; i < collisions.Count; i++) {

            if (Colliding(collisions[i])) {
                onCollisionStay?.Invoke(new CollisionInfo(this, collisions[i]));
            } else {
                onCollisionExit?.Invoke(new CollisionInfo(this, collisions[i]));
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
    protected void MarkDirty() {
        if (!dirty) {
            FixedPhysics.MarkDirty(this);
            dirty = true;
        }
    }

    // marks this collider as no longer dirty for internal operations
    public void Undirty() {
        dirty = false;
    }

    public void CleanCollider(FixedCollider collider) {
        collisions.Remove(collider);
        thisFrame.Remove(collider);
    }
}

public struct CollisionInfo {
    public FixedCollider primary, secondary;
    
    public CollisionInfo(FixedCollider primary, FixedCollider secondary) {
        this.primary = primary;
        this.secondary = secondary;
    }
}
