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
    }

    void OnEnable() {
        FixedPhysics.RegisterBody(this);
    }

    void OnDisable() {
        FixedPhysics.DeregisterBody(this);
    }

    void OnDestroy() {
        FixedPhysics.DeregisterBody(this);
    }

    public void PhysicsFrame() {
        velocity += gravity * Vec2Fix.down;
        if (velocity != Vec2Fix.zero)
            fTransform.position += velocity;
    }
}
