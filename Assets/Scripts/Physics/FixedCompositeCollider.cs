using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

[RequireComponent(typeof(FixedTransform))]
public class FixedCompositeCollider : FixedCollider
{
    [SerializeField] private List<FixedBox> boxes;
    [SerializeField] private Material mat;

    private Vec2Fix boundingOffset;
    private Vec2Fix boundingSize;
    // checks if this collider is colliding with the provided other collider
    public override bool Colliding(FixedCollider other) {
        if (other is FixedBoxCollider) {
            if (CompositeAABB((FixedBoxCollider)other))
                return true;
        } else if (other is FixedCompositeCollider){
            FixedCompositeCollider col = (FixedCompositeCollider)other;

            if (boxes == null || col.boxes == null)
                return false;

            if (FixedCollider.AABB(fTransform.position + boundingOffset, boundingSize, col.fTransform.position + col.boundingOffset, col.boundingSize)) 
                foreach (var box in boxes) {
                    foreach (var oBox in col.boxes) {
                        if (FixedCollider.AABB(fTransform.position + box.position - (Fix64)0.5f * box.size, box.size,
                                        col.fTransform.position + oBox.position - (Fix64)0.5f * oBox.size, oBox.size))
                            return true;
                    }
                }
            return false;
        }
        return false;
    }

    new void Awake() {
        base.Awake();
        CalculateBounding();
    }

    public bool CompositeAABB(FixedBoxCollider other) {
        if (boxes == null || boxes.Count == 0)
                return false;
        
        if (FixedCollider.AABB(fTransform.position + boundingOffset, boundingSize, other.fixedTransform.position - (Fix64)0.5f * other.Size, other.Size))
            foreach (var box in boxes) {
                if (FixedCollider.AABB(fTransform.position + box.position - (Fix64)0.5f * box.size, box.size,
                                        other.fixedTransform.position - (Fix64)0.5f * other.Size, other.Size))
                    return true;
            }
        return false;
    }

    void FixedUpdate() {

        foreach (var box in boxes) {
            Vec2Fix size = box.size;
            Vec2Fix pos = fTransform.position + box.position - (Fix64)0.5 * size;

            Debug.DrawLine(new Vector3((float)pos.x, (float)pos.y, 0),
            new Vector3((float)(pos.x + size.x), (float)pos.y, 0), Color.red);
            Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)pos.y, 0),
            new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0), Color.red);
            Debug.DrawLine(new Vector3((float)(pos.x + size.x), (float)(pos.y + size.y), 0),
            new Vector3((float)pos.x, (float)(pos.y + size.y), 0), Color.red);
            Debug.DrawLine(new Vector3((float)pos.x, (float)(pos.y + size.y), 0),
            new Vector3((float)pos.x, (float)pos.y, 0), Color.red);
        }

        Vec2Fix bsize = boundingSize;
        Vec2Fix bpos = fTransform.position + boundingOffset + (Fix64)0.5 * boundingSize;

        Debug.DrawLine(new Vector3((float)bpos.x, (float)bpos.y, 0),
        new Vector3((float)(bpos.x + bsize.x), (float)bpos.y, 0), Color.cyan);
        Debug.DrawLine(new Vector3((float)(bpos.x + bsize.x), (float)bpos.y, 0),
        new Vector3((float)(bpos.x + bsize.x), (float)(bpos.y + bsize.y), 0), Color.cyan);
        Debug.DrawLine(new Vector3((float)(bpos.x + bsize.x), (float)(bpos.y + bsize.y), 0),
        new Vector3((float)bpos.x, (float)(bpos.y + bsize.y), 0), Color.cyan);
        Debug.DrawLine(new Vector3((float)bpos.x, (float)(bpos.y + bsize.y), 0),
        new Vector3((float)bpos.x, (float)bpos.y, 0), Color.cyan);
        
    }

    private void OnDrawGizmos() {
        CalculateBounding();

        Gizmos.color = Color.cyan;
        FixedTransform fTransform  = GetComponent<FixedTransform>();

        Vec2Fix bsize = boundingSize;
        Vec2Fix bpos = fTransform.position + boundingOffset + (Fix64)0.5 * boundingSize;

        Gizmos.DrawLine(new Vector3((float)bpos.x, (float)bpos.y, 0),
        new Vector3((float)(bpos.x + bsize.x), (float)bpos.y, 0));
        Gizmos.DrawLine(new Vector3((float)(bpos.x + bsize.x), (float)bpos.y, 0),
        new Vector3((float)(bpos.x + bsize.x), (float)(bpos.y + bsize.y), 0));
        Gizmos.DrawLine(new Vector3((float)(bpos.x + bsize.x), (float)(bpos.y + bsize.y), 0),
        new Vector3((float)bpos.x, (float)(bpos.y + bsize.y), 0));
        Gizmos.DrawLine(new Vector3((float)bpos.x, (float)(bpos.y + bsize.y), 0),
        new Vector3((float)bpos.x, (float)bpos.y, 0));

        Gizmos.color = Color.red;

        foreach (var box in boxes) {
            Vec2Fix size = box.size;
            Vec2Fix pos = fTransform.position + box.position - (Fix64)0.5 * size;

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

    private void CalculateBounding() {
        if (boxes == null || boxes.Count == 0)
            return;
        Fix64[] corners = new Fix64[] {
            boxes[0].position.x - (Fix64)0.5f * boxes[0].size.x,
            boxes[0].position.x + (Fix64)0.5f * boxes[0].size.x,
            boxes[0].position.y - (Fix64)0.5f * boxes[0].size.y,
            boxes[0].position.y + (Fix64)0.5f * boxes[0].size.y

        };
        foreach (var box in boxes) {
            if (box.position.x - (Fix64)0.5f * box.size.x < corners[0])
                corners[0] = box.position.x - (Fix64)0.5f * box.size.x;
            if (box.position.x + (Fix64)0.5f * box.size.x > corners[1])
                corners[1] = box.position.x + (Fix64)0.5f * box.size.x;
            if (box.position.y - (Fix64)0.5f * box.size.y < corners[2])
                corners[2] = box.position.y - (Fix64)0.5f * box.size.y;
            if (box.position.y + (Fix64)0.5f * box.size.y > corners[3])
                corners[3] = box.position.y + (Fix64)0.5f * box.size.y;
        }

        boundingSize = new Vec2Fix(corners[1] - corners[0], corners[3] - corners[2]);
        boundingOffset = new Vec2Fix (corners[0] - (Fix64)0.5f * boundingSize.x, corners[2] - (Fix64)0.5f * boundingSize.y);
    }
}
