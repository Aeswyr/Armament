using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixMath.NET;

[RequireComponent(typeof(FixedTransform))]
public class FixedBoxCollider : FixedCollider
{
    [SerializeField] private Vec2Fix size;
    public Vec2Fix Size {
        get => size;
    }
    public bool isTrigger;
    [SerializeField] private Material mat;

    // checks if this collider is colliding with the provided other collider
    public override bool Colliding(FixedCollider other) {
        if (other.transform.root == transform.root) // prevent collisions between related objects
            return false;

        if (other is FixedBoxCollider) {
            FixedBoxCollider col = (FixedBoxCollider)other;
            return FixedCollider.AABB(fTransform.position - (Fix64)0.5f * size, size, col.fTransform.position - (Fix64)0.5f * col.size, col.size);
        } else if (other is FixedCompositeCollider){
            if (((FixedCompositeCollider)other).CompositeAABB(this))
                return true;
        }
        return false;
    }

    void FixedUpdate() {
        Vec2Fix pos = fTransform.position - (Fix64)0.5 * size;

        Debug.DrawLine(new Vector3((float)pos.x, (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)pos.y, 0), Color.green);
        Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0), Color.green);
        Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)(pos.y + size.y), 0), Color.green);
        Debug.DrawLine(new Vector3((float)pos.x, (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)pos.y, 0), Color.green);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;

        Vec2Fix pos = GetComponent<FixedTransform>().position - (Fix64)0.5 * size;

        Gizmos.DrawLine(new Vector3((float)pos.x, (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)pos.y, 0));
        Gizmos.DrawLine(new Vector3((float)(pos.x + size.x), (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0));
        Gizmos.DrawLine(new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)(pos.y + size.y), 0));
        Gizmos.DrawLine(new Vector3((float)pos.x, (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)pos.y, 0));
    }

}
