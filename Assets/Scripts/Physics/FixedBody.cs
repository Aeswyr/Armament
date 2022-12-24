using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FixedTransform))]
public class FixedBody : MonoBehaviour
{
    FixedTransform fTransform;

    void Awake()
    {
        fTransform = GetComponent<FixedTransform>();
    }
}
