using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

[CreateAssetMenu(fileName = "MoveProperty", menuName = "Armament/MoveProperty", order = 0)]
public class MoveProperty : ScriptableObject {

    [SerializeField] private string moveName;
    public string MoveName {
        get {return moveName;}
    }

    [SerializeField] private InputParser.Action action;
    [SerializeField] private InputParser.Motion motion;

    [SerializeField] private int damage;
    public int Damage {
        get {return damage;}
    }
    [SerializeField] private int momentumDamage;
    public int MomentumDamage {
        get {return momentumDamage;}
    }
    [SerializeField] private int duration;
    public int Duration {
        get {return duration;}
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
    // default cancel window after hitbox lands connects on hit or block
    [SerializeField] private int cancelWindow;
    public int CancelWindow {
        get {return cancelWindow;}
    }


    [Header("On Hit")]

    [Header("On Counterhit")]

    [Header("On Air Hit")]
    
    [Header("On Air Counterhit")]

    [Header("On Block")]
    [SerializeField] private BlockStunType blockType;


    public bool MatchInput(InputParser.Action action, InputParser.Motion motion) {
        return this.action == action && this.motion == motion;
    }
}

public enum StunType {
    HITSTUN, STAGGER, LAUNCH, TUMBLE, CRUMPLE
}

public enum BlockStunType {
    BLOCKSTUN, GUARDCRUSH, REEL
}

public enum MoveType {
    NONE, NORMAL, SPECIAL, SUPER, EXHAUST
}

public enum TargetZoneType {
    ALL, HEAD, BODY, FEET
}

public enum BlockPropertyType {
    MID, LOW, HIGH, THROW
}

public enum CancelType {
    NONE, LIGHT, HEAVY, SPECIAL, SUPER, UNCANCELABLE
}