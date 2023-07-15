using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using System;

[CreateAssetMenu(fileName = "MoveProperty", menuName = "Armament/MoveProperty", order = 0)]
public class MoveProperty : ScriptableObject {

    [SerializeField] private string moveName;
    public string MoveName {
        get {return moveName;}
    }

    [SerializeField] private MoveType moveType;
    public MoveType MoveType {
        get {return moveType;}
    }

    [SerializeField] private InputParser.Action action;
    [SerializeField] private InputParser.Motion motion;
    [SerializeField] private bool airOk;

    [SerializeField] private int duration;
    public int Duration {
        get {return duration;}
    }

    [SerializeField] private int counterhitDuration;
    public int CounterhitDuration {
        get {return counterhitDuration;}
    }

    [SerializeField] private HitProperty hitData;
    public HitProperty HitData {
        get {return hitData;}
    }
    [SerializeField] private CancelProperty cancelData;
    public CancelProperty CancelData {
        get {return cancelData;}
    }
    
    public bool MatchInput(InputParser.Action action, InputParser.Motion motion, bool airborne) {
        return this.action == action && this.motion == motion && airborne == airOk;
    }
}

[Serializable] public class ResourceProperty {

}

[Serializable] public struct HitProperty {
    [SerializeField] private int damage;
    public int Damage {
        get {return damage;}
    }
    [SerializeField] private int minimumDamage;
    public int MinimumDamage {
        get {return minimumDamage;}
    }
    [SerializeField] private Fix64 proration;
    public Fix64 Proration {
        get {return proration;}
    }
    [SerializeField] private Fix64 forcedProration;
    public Fix64 ForcedProration {
        get {return forcedProration;}
    }
    [SerializeField] private BlockPropertyType blockProperty;
    public BlockPropertyType BlockProperty {
        get {return blockProperty;}
    }

    [Header("On Hit")]

    [Header("On Counterhit")]

    [Header("On Air Hit")]
    
    [Header("On Air Counterhit")]

    [Header("On Block")]
    [SerializeField] private BlockStunType blockType;
}

[Serializable] public struct MotionProperty {

}

[Serializable] public struct SpawnProperty {

}

[Serializable] public struct ColliderProperty {

}

[Serializable] public struct CancelProperty {
    // what level should this move be treated as when determining what can cancel into it. This should always be the move's actual level 
    [SerializeField] private CancelType moveLevel;
    public CancelType MoveLevel {
        get {return moveLevel;}
    }

    // what level should this move be treated as when determining what it can cancel into. Usually this will be the same as the move level
    [SerializeField] private CancelType cancelLevel;
    public CancelType CancelLevel {
        get {return cancelLevel;}
    }
    
    // True if there is a custom cancel window, false otherwise
    [SerializeField] private bool hasWindow;
    public bool HasWindow {
        get {return hasWindow;}
    }

    // base cancel window after hitbox lands connects on hit or block
    [SerializeField] private int cancelWindow;
    public int CancelWindow {
        get {return cancelWindow;}
    }
}

[Serializable] public struct FollowupProperty {
    
}

public enum StunType {
    HITSTUN, STAGGER, LAUNCH, TUMBLE, CRUMPLE
}

public enum BlockStunType {
    BLOCKSTUN, GUARDCRUSH, REEL
}

public enum MoveType {
    NONE, NORMAL, SPECIAL, SUPER, EX
}

public enum TargetZoneType {
    ALL, HEAD, BODY, FEET
}

public enum BlockPropertyType {
    MID, LOW, HIGH, THROW
}

public enum CancelType {
    NONE, ANY, LIGHT, HEAVY, SPECIAL, SUPER, UNCANCELABLE
}