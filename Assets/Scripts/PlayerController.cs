using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private FixedTransform fTransform;
    [SerializeField] private FixedBody fBody;
    [SerializeField] private FixedCollider fCollider;
    // Start is called before the first frame update
    void Start()
    {
        fCollider.onCollisionEnter += OnCollide;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FixedPhysics.Update();
        
        fBody.velocity = (Fix64)0.1f * (Vec2Fix)input.dir;
    }

    void OnCollide(FixedCollider col) {
        Debug.Log("did a collision");
    }
}
