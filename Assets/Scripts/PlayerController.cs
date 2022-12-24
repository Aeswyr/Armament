using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private FixedTransform fTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FixedPhysics.Update();
        
        fTransform.position += (Fix64)0.1f * (Vec2Fix)input.dir;
    }
}
