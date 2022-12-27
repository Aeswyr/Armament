using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixMath.NET;

[RequireComponent(typeof(FixedTransform))]
public class FixedCollider : MonoBehaviour
{
    private FixedTransform fTransform;
    [SerializeField] private Vec2Fix size;
    [SerializeField] private bool isTrigger;
    [SerializeField] private Material mat;

    private List<FixedCollider> collisions = new();
    private List<FixedCollider> thisFrame = new();

    private bool dirty;
    
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

    void Awake()
    {
        fTransform = GetComponent<FixedTransform>();
        MarkDirty();
        fTransform.onDirty += MarkDirty;
        FixedPhysics.RegisterCollider(this);

        MarkDirty();
    }

    void OnEnable() {
        fTransform.onDirty += MarkDirty;
        FixedPhysics.RegisterCollider(this);

        MarkDirty();
    }

    void OnDisable() {
        fTransform.onDirty -= MarkDirty;
        FixedPhysics.DeregisterCollider(this);
    }

    void OnDestroy() {
        fTransform.onDirty -= MarkDirty;
        FixedPhysics.DeregisterCollider(this);
    }

    // checks if this collider is colliding with the provided other collider
    public bool Colliding(FixedCollider other) {
        return AABB(fTransform.position - (Fix64)0.5f * size, size, other.fTransform.position - (Fix64)0.5f * other.size, other.size);
    }

    private bool AABB(Vec2Fix aPos, Vec2Fix aSize, Vec2Fix bPos, Vec2Fix bSize) {
        return  bPos.x < aPos.x + aSize.x 
                && aPos.x < bPos.x + bSize.x
                && bPos.y < aPos.y + aSize.y 
                && aPos.y < bPos.y + bSize.y;
    }

    void Update() {
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
        Gizmos.color = new Color(1, 0, 0, 1f);
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {new Vector3(transform.position.x, transform.position.y),
                                        new Vector3(transform.position.x + (float)size.x, transform.position.y),
                                        new Vector3(transform.position.x + (float)size.x, transform.position.y + (float)size.y),
                                        new Vector3(transform.position.x, transform.position.y + (float)size.y)};
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0};
        mesh.RecalculateNormals();
        Gizmos.DrawMesh(mesh, transform.position);
        Gizmos.color = new Color(1, 0, 0, 1f);
        //Gizmos.DrawWireMesh(mesh, transform.position);
    }

    // called automatically when a collision occurs, and invokes either onColliderEnter if needed
    public void Collide(FixedCollider other) {
        if (!thisFrame.Contains(other) && !collisions.Contains(other)) {
            onCollisionEnter.Invoke(other); 
            
            thisFrame.Add(other);
            other.Collide(this);
        }
    }

    // checks if there are any colliders that were not collided with this frame and calls on collider exit or stay depending
    public void CheckCollisions() {
        for (int i = 0; i < collisions.Count; i++) {

            if (Colliding(collisions[i])) {
                onCollisionStay.Invoke(collisions[i]);
            } else {
                onCollisionExit.Invoke(collisions[i]);
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
