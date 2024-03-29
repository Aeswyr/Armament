using FixMath.NET;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


public static class FixedPhysics {

    private const int FPS = 60;
    public static readonly Fix64 fixedTimestep = Fix64.One / (Fix64)FPS;
    private static List<FixedCollider> dirty = new();
    private static List<FixedCollider> colliders = new();
    private static List<FixedBoxCollider> physicsColliders = new();
    private static List<FixedBody> physicsBodies = new();

    // updates the physics system, performing rigidbody movement and testing for collisions
    public static void Update() {
        for (int i = 0; i < physicsBodies.Count; i++)
            physicsBodies[i].PhysicsFrame();
        
        for (int i = 0; i < dirty.Count; i++) {
            var collider = dirty[i];
            for (int j = 0; j < colliders.Count; j++) {
                var target = colliders[j];
                if (collider != target && collider.Colliding(target)) {
                    collider.Collide(target);
                }
            }
            collider.Undirty();
        }

        for (int i = 0; i < colliders.Count; i++) {
            colliders[i].CheckCollisions();
        }

        dirty.Clear();
    }

    // called automatically when a collider is moved or otherwise needs to be rechecked for collisions
    public static void MarkDirty(FixedCollider collider) {
        dirty.Add(collider);
    }

    // registers a collider with the physics system
    public static void RegisterCollider(FixedCollider collider) {
        if (!colliders.Contains(collider)) {
            colliders.Add(collider);
            if (collider is FixedBoxCollider && !((FixedBoxCollider)collider).isTrigger)
                physicsColliders.Add((FixedBoxCollider)collider);
        }
    }

    public static void DeregisterCollider(FixedCollider collider) {
        colliders.Remove(collider);
        if (dirty.Contains(collider))
            dirty.Remove(collider);
        if (collider is FixedBoxCollider && physicsColliders.Contains((FixedBoxCollider)collider)) {
            physicsColliders.Remove((FixedBoxCollider)collider);
        }

        foreach (var col in colliders)
            col.CleanCollider(collider);
    }

    public static void RegisterBody(FixedBody fBody) {
        if (!physicsBodies.Contains(fBody))
            physicsBodies.Add(fBody);
    }

    public static void DeregisterBody(FixedBody fBody) {
        physicsBodies.Remove(fBody);
    }

    public static FixedBoxCollider IsPhysicsColliding(FixedBoxCollider primary, Vec2Fix offset) {
        foreach (var collider in physicsColliders) {
            if (collider != primary && FixedCollider.AABB(primary.ColliderPos + offset, primary.Size, collider.ColliderPos, collider.Size))
                return collider;
        }
        return null;
    }

}

[Serializable]
public struct Vec2Fix : IEquatable<Vec2Fix>, IComparable<Vec2Fix> {

    public static Vec2Fix zero = new Vec2Fix((Fix64)0, (Fix64)0);
    public static Vec2Fix one = new Vec2Fix((Fix64)1, (Fix64)1);
    public static Vec2Fix up = new Vec2Fix((Fix64)0, (Fix64)1);
    public static Vec2Fix down = new Vec2Fix((Fix64)0, (Fix64)(-1));
    public static Vec2Fix left = new Vec2Fix((Fix64)(-1), (Fix64)0);
    public static Vec2Fix right = new Vec2Fix((Fix64)1, (Fix64)0);
    public Fix64 x;
    public Fix64 y;

    public Vec2Fix(Fix64 x, Fix64 y) {
        this.x = x;
        this.y = y;
    }
    
    public static Vec2Fix operator-(Vec2Fix a) {
        return new Vec2Fix(-a.x, -a.y);
    }

    public static Vec2Fix operator+(Vec2Fix a, Vec2Fix b) {
        return new Vec2Fix(a.x + b.x, a.y + b.y);
    }

    public static Vec2Fix operator-(Vec2Fix a, Vec2Fix b) {
        return new Vec2Fix(a.x - b.x, a.y - b.y);
    }

    public static Vec2Fix operator*(Fix64 a, Vec2Fix b) {
        return new Vec2Fix(a * b.x, a * b.y);
    }

    public static Vec2Fix operator*(Vec2Fix a, Vec2Fix b) {
        return new Vec2Fix(a.x * b.x, a.y * b.y);
    }

    public static bool operator ==(Vec2Fix a, Vec2Fix b) {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vec2Fix a, Vec2Fix b) {
        return a.x != b.x || a.y != b.y;
    }

    public override bool Equals(object o) {
        if (o.GetType() != typeof(Vec2Fix))
            return false;
        return (Vec2Fix)o == this;
    }

    public static explicit operator Vec2Fix(Vector2 a) {
        return new Vec2Fix((Fix64)a.x, (Fix64)a.y);
    }

    public static implicit operator Vector2(Vec2Fix a) {
        return new Vector2((float)a.x, (float)a.y);
    }

    public Fix64 Magnitude() {
        return Fix64.Sqrt(x * x + y * y);
    }

    public bool Equals(Vec2Fix other)
    {
        
        if (GetType() != other.GetType())
        {
            return false;
        }
        
        return x == other.x && y == other.y;
    }

    public int CompareTo(Vec2Fix other) {
        return (int)(Magnitude() - other.Magnitude());
    }
    
    public override int GetHashCode()
    {
        return x.GetHashCode() + y.GetHashCode();
    }

    public override string ToString() {
        return $"{x.ToString()}, {y.ToString()}";
    }


    [CustomPropertyDrawer(typeof(Vec2Fix))]
    public class Vec2FixDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            Rect xpos = new Rect(position.x + 125, position.y, -1, position.height);
            Rect ypos = new Rect(position.x + 125 + 75, position.y, -1, position.height);

            string name = property.name;
            if (name.IndexOf("m_") == 0)
                name = name.Substring(2);
            name = name[0].ToString().ToUpper() + name.Substring(1);


            EditorGUI.LabelField(position, name);
            EditorGUI.PropertyField(xpos, property.FindPropertyRelative(nameof(Vec2Fix.x)));
            EditorGUI.PropertyField(ypos, property.FindPropertyRelative(nameof(Vec2Fix.y)));

            EditorGUI.EndProperty();
        }
    }
}

[Serializable]
public struct FixedBox {
    public Vec2Fix position;
    public Vec2Fix size;
}