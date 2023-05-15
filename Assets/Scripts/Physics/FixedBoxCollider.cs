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

    public Vec2Fix ColliderPos => fTransform.position - Fix64.Half * size;
    public bool isTrigger;
    [SerializeField] private Material mat;

    // checks if this collider is colliding with the provided other collider
    public override bool Colliding(FixedCollider other) {
        if (other.transform.root == transform.root) // prevent collisions between related objects
            return false;

        if (Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer))
            return false;

        if (other is FixedBoxCollider) {
            FixedBoxCollider col = (FixedBoxCollider)other;
            return FixedCollider.AABB(ColliderPos, size, col.ColliderPos, col.size);
        } else if (other is FixedCompositeCollider){
            FixedCompositeCollider col = (FixedCompositeCollider)other;
            return col.CompositeAABB(this);
        }
        return false;
    }

    void FixedUpdate() {
        Vec2Fix pos = ColliderPos;

        Color color = Color.green;
        if (isTrigger)
            color = Color.red;

        Debug.DrawLine(new Vector3((float)pos.x, (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)pos.y, 0), color);
        Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)pos.y, 0),
         new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0), color);
        Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)(pos.y + size.y), 0), color);
        Debug.DrawLine(new Vector3((float)pos.x, (float)(pos.y + size.y), 0),
         new Vector3((float)pos.x, (float)pos.y, 0), color);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        if (isTrigger)
            Gizmos.color = Color.red;

        Vec2Fix pos = (Vec2Fix)(Vector2)transform.position - Fix64.Half * size;

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
