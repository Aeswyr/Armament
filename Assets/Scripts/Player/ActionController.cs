using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the activation of hitboxes based off of specific button presses and the management of those hitboxes,
// as well as move cancellation validity and current state (hitstun/blockstun)
public class ActionController : MonoBehaviour
{
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private MoveProperty property1, property2;

    int actionFrames = 0;
    int cancelFrames;
    CancelType cancelType;
    GameObject activeHitbox;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
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

        
    }

    public bool Actionable() {
        return actionFrames <= 0;
    }

    public void FireMove(InputParser.Button button, InputParser.Motion motion) {

        MoveProperty moveProperties = property1;
        if (button == InputParser.Button.H1)
            moveProperties = property2;

        if ((actionFrames > 0 || activeHitbox != null)
            && (cancelFrames <= 0 || moveProperties.MoveLevel <= cancelType || cancelType == CancelType.NONE))
            return;

        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }
        

        activeHitbox = Instantiate(hitboxPrefab, transform);
        
        // setup for kara cancels
        cancelFrames = 2;
        cancelType = moveProperties.MoveLevel;

        actionFrames = moveProperties.Duration;
        activeHitbox.GetComponent<HitboxInfo>().Init(moveProperties, this);
    }

    public void SetCancellable(int frames, CancelType level) {
        cancelFrames = frames;
        cancelType = level;
    }

    public void SetCancellable(MoveProperty property) {
        cancelFrames = property.CancelWindow;
        cancelType = property.CancelLevel;
    }
}
