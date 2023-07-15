using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

// handles the activation of hitboxes based off of specific button presses and the management of those hitboxes,
// as well as move cancellation validity and current state (hitstun/blockstun)
public class ActionController : MonoBehaviour
{
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private List<MoveProperty> weapon1;
    [SerializeField] private List<MoveProperty> weapon2;
    [SerializeField] private List<MoveProperty> extra;
    [SerializeField] private List<MoveProperty> universal;
    [SerializeField] private MoveProperty currentProperty;

    bool launched = false;
    int hitstunFrames = 0;
    int blockstunFrames = 0;
    int actionFrames = 0;
    int cancelFrames;
    int karaFrames;
    CancelType cancelType;
    GameObject activeHitbox;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (hitstunFrames > 0)
            hitstunFrames--;

        if (blockstunFrames > 0)
            blockstunFrames--;

        if (actionFrames > 0)
            actionFrames--;
        else {
            Destroy(activeHitbox);
            activeHitbox = null;
        }

        if (cancelFrames > 0)
            cancelFrames--;
        else
            cancelType = CancelType.NONE;

        if (karaFrames > 0)
            karaFrames--;
    }

    public bool Actionable() {
        return actionFrames <= 0 && blockstunFrames <= 0 && hitstunFrames <= 0 && !launched;
    }

    public MoveProperty FireMove(InputParser.Action button, List<InputParser.Motion> motions, bool airborne) {
        MoveProperty property = GetMove(button, motions, airborne);
        
        if (property == null)
            return null;

        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }

        actionFrames = property.Duration;
        
        activeHitbox = Instantiate(hitboxPrefab, transform);
        
        // setup for kara cancels : REMOVED KARA CANCELS (They're needlessly complex, and since Kara dash cancel and kara impact exist, they arent needed)
        //cancelFrames = 2;
        //cancelType = moveProperties.MoveLevel;

        activeHitbox.GetComponent<HitboxInfo>().Init(property, this);

        return property;
    }

    private MoveProperty GetMove(InputParser.Action button, List<InputParser.Motion> motions, bool airborne) {
        if (button == InputParser.Action.L1 || button == InputParser.Action.H1) {
            switch (button) {
                case InputParser.Action.L1:
                    button = InputParser.Action.L;
                    break;
                case InputParser.Action.H1:
                    button = InputParser.Action.H;
                    break;
            }
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, airborne, weapon1);
                if (property != null)
                    return property;
            }
        } else if (button == InputParser.Action.L2 || button == InputParser.Action.H2) {
            switch (button) {
                case InputParser.Action.L2:
                    button = InputParser.Action.L;
                    break;
                case InputParser.Action.H2:
                    button = InputParser.Action.H;
                    break;
            }
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, airborne, weapon2);
                if (property != null)
                    return property;
            }
        } else if (button == InputParser.Action.LL || button == InputParser.Action.HH) {
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, airborne, extra);
                if (property != null)
                    return property;
            }
        } else {
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, airborne, universal);
                if (property != null)
                    return property;
            }
        }
        return null;
    }

    public void UpdateMomentum(ref Vec2Fix movement) {
        movement.x = (Fix64)0.7f * movement.x;
        if (Fix64.Abs(movement.x) < (Fix64)0.1f)
            movement.x = Fix64.Zero;
    }

    private MoveProperty SearchMoveList(InputParser.Action action, InputParser.Motion motion, bool airborne, List<MoveProperty> list) {
        foreach (var move in list) {
            if (move.MatchInput(action, motion, airborne) && CheckValid(move))
                return move;
        }
        return null;
    }

    private bool CheckValid(MoveProperty property) {
        if ((actionFrames > 0 || activeHitbox != null)
            && (cancelFrames <= 0 || property.CancelData.MoveLevel <= cancelType || cancelType == CancelType.NONE))
            return false;
        return true;
    }

    public void SetCancellable(int frames, CancelType level) {
        cancelFrames = frames;
        cancelType = level;
    }

    public void SetCancellable(MoveProperty property) {
        if (!property.CancelData.HasWindow)
            cancelFrames = actionFrames;
        else
            cancelFrames = property.CancelData.CancelWindow;
        cancelType = property.CancelData.CancelLevel;
    }

    public void SetHitstun(int frames) {
        if (launched)
            return;
        // if you're mid move, cleanup.
        actionFrames = 0;
        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }

        blockstunFrames = 0;

        cancelFrames = 0;
        cancelType = CancelType.NONE;

        //set hitstun values
        hitstunFrames = frames;
    }

    public void SetBlockstun(int frames) {
        // if you're mid move, cleanup.
        actionFrames = 0;
        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }

        cancelFrames = 0;
        cancelType = CancelType.NONE;

        //set hitstun values
        blockstunFrames = frames;
    }

    public void SetLaunched(bool state) {
        if (state)
            SetHitstun(0);
        launched = state;
    }

    public bool IsInCombo() {
        return launched || hitstunFrames > 0;
    }
}