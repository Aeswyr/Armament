using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public class FixedBounds : MonoBehaviour
{
    public Fix64 up;
    public Fix64 down;
    public Fix64 left;
    public Fix64 right;

    void FixedUpdate() {

        Debug.DrawLine(new Vector3((float)left, (float)up, 0),
         new Vector3((float)left, (float)down, 0), Color.green);
        Debug.DrawLine(new Vector3((float)left, (float)down, 0),
         new Vector3((float)right, (float)down, 0), Color.green);
        Debug.DrawLine(new Vector3((float)right, (float)down, 0),
         new Vector3((float)right, (float)up, 0), Color.green);
        Debug.DrawLine(new Vector3((float)right, (float)up, 0),
         new Vector3((float)left, (float)up, 0), Color.green);
    }

    private void OnDrawGizmos() {
            
        Gizmos.color = Color.green;

        Gizmos.DrawLine(new Vector3((float)left, (float)up, 0),
         new Vector3((float)left, (float)down, 0));
        Gizmos.DrawLine(new Vector3((float)left, (float)down, 0),
         new Vector3((float)right, (float)down, 0));
        Gizmos.DrawLine(new Vector3((float)right, (float)down, 0),
         new Vector3((float)right, (float)up, 0));
        Gizmos.DrawLine(new Vector3((float)right, (float)up, 0),
         new Vector3((float)left, (float)up, 0));
    }
}



