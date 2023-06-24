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
        return actionFrames <= 0 && hitstunFrames <= 0 && !launched;
    }

    public void FireMove(InputParser.Action button, List<InputParser.Motion> motions, bool airborne) {
        MoveProperty property = GetMove(button, motions);
        
        if (property == null)
            return;

        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }
        
        activeHitbox = Instantiate(hitboxPrefab, transform);
        
        // setup for kara cancels : REMOVED KARA CANCELS (They're needlessly complex, and since Kara dash cancel and kara impact exist, they arent needed)
        //cancelFrames = 2;
        //cancelType = moveProperties.MoveLevel;

        actionFrames = property.Duration;
        activeHitbox.GetComponent<HitboxInfo>().Init(property, this);
    }

    private MoveProperty GetMove(InputParser.Action button, List<InputParser.Motion> motions) {
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
                var property = SearchMoveList(button, motion, weapon1);
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
                var property = SearchMoveList(button, motion, weapon2);
                if (property != null)
                    return property;
            }
        } else if (button == InputParser.Action.LL || button == InputParser.Action.HH) {
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, extra);
                if (property != null)
                    return property;
            }
        } else {
            foreach (var motion in motions) {
                var property = SearchMoveList(button, motion, universal);
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

    private MoveProperty SearchMoveList(InputParser.Action action, InputParser.Motion motion, List<MoveProperty> list) {
        foreach (var move in list) {
            if (move.MatchInput(action, motion) && CheckValid(move))
                return move;
        }
        return null;
    }

    private bool CheckValid(MoveProperty property) {
        if ((actionFrames > 0 || activeHitbox != null)
            && (cancelFrames <= 0 || property.MoveLevel <= cancelType || cancelType == CancelType.NONE))
            return false;
        return true;
    }

    public void SetCancellable(int frames, CancelType level) {
        cancelFrames = frames;
        cancelType = level;
    }

    public void SetCancellable(MoveProperty property) {
        cancelFrames = property.CancelWindow;
        cancelType = property.CancelLevel;
    }

    public void SetStun(int frames) {
        if (launched)
            return;
        // if you're mid move, cleanup.
        actionFrames = 0;
        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }

        cancelFrames = 0;
        cancelType = CancelType.NONE;

        //set hitstun values
        hitstunFrames = frames;
    }

    public void SetLaunched(bool state) {
        if (state)
            SetStun(0);
        launched = state;
    }
}