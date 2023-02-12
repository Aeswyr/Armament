using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HitProperty", menuName = "Armament/HitProperty", order = 0)]
public class HitProperty : ScriptableObject {
    
}

enum StunType {
    HITSTUN, STAGGER, LAUNCH, BLOWBACK, CRUMPLE
}

enum BlockStunType {
    BLOCKSTUN, GUARDCRUSH, REEL
}


