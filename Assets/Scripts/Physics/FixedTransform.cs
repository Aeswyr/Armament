using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Events;
using FixMath.NET;

/***
* Controles the object's base transform position using fixed point representations
* Required for any sort of fixed point physics interactions
*/
public class FixedTransform : MonoBehaviour
{
    [SerializeField] private Vec2Fix m_position;
    public delegate void OnDirty();
    public OnDirty onDirty {
        get;
        set;
    }

    private Vec2Fix m_worldBasePos = new Vec2Fix((Fix64)0, (Fix64)0);

    private Vec2Fix worldBasePos {
        get {
            return m_worldBasePos;
        }
        set {
            if (value != m_worldBasePos) {
                onDirty?.Invoke();
                m_worldBasePos = value;
                UpdateTransform();
            }
            
        }
    }
    public Vec2Fix position {
        get {
            return m_worldBasePos + m_position;
        }
        set {
            if (value != position) {
                onDirty?.Invoke();
                localPosition = value - m_worldBasePos;
                UpdateTransform();
            }
        }
    }

    public Vec2Fix localPosition {
        get {
            return m_position;
        }
        set {
            if (m_position != value) {
                onDirty?.Invoke();
                m_position = value;
                foreach (var fTransform in GetComponentsInChildren<FixedTransform>()) {
                    if (fTransform.transform.parent == transform)
                        fTransform.worldBasePos = worldBasePos + m_position;
                }
                UpdateTransform();
            }
        }
    }

    public void UpdateTransform() {
        transform.position = new Vector3((float)position.x, (float)position.y, transform.position.z);
    }


    [CustomEditor(typeof(FixedTransform))]
    public class FixedTransformEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUI.changed) {
                var fTransform = (FixedTransform)target;
                fTransform.UpdateTransform();
            }
        }
    }
}
