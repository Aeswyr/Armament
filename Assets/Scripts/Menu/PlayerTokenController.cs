using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTokenController : MonoBehaviour
{
    private InputHandler input;
    public TargetPlayer target = TargetPlayer.NONE;
    public bool ready;

    public void SetInputs(InputHandler input) {
        this.input = input;
    }

    void FixedUpdate() {
        if (target == TargetPlayer.NONE) {
            if (input.move.pressed) {
                if (input.dir.x > 0) {
                    if (!InputDeviceManager.Instance.HasPlayer(TargetPlayer.P2)) {
                        this.transform.SetParent(SelectMenuController.Instance.GetP2Pos());
                        this.transform.localPosition = Vector3.zero;
                        target = TargetPlayer.P2;
                    }
                } else if (input.dir.x < 0) {
                    if (!InputDeviceManager.Instance.HasPlayer(TargetPlayer.P1)) {
                        this.transform.SetParent(SelectMenuController.Instance.GetP1Pos());
                        this.transform.localPosition = Vector3.zero;
                        target = TargetPlayer.P1;
                    }
                }
            }
        } else {
            if (!ready && input.move.pressed && ((input.dir.x > 0 && target == TargetPlayer.P1) 
            || (input.dir.x < 0 && target == TargetPlayer.P2))) {
                this.transform.SetParent(SelectMenuController.Instance.GetPlayerList());
                target = TargetPlayer.NONE;
            } else if (input.heavy2.pressed) {
                if (ready && InputDeviceManager.Instance.IsReady()) {
                    SceneManager.LoadScene("GameScene");
                } else {
                    ready = true;
                    SelectMenuController.Instance.ToggleReady(target, ready);

                    input.transform.GetComponent<InputLink>().target = target;

                    if (InputDeviceManager.Instance.IsReady()) {
                        SelectMenuController.Instance.ToggleAllReady(true);
                    }
                }
            } else if (input.heavy1.pressed && ready) {
                ready = false;

                input.transform.GetComponent<InputLink>().target = TargetPlayer.NONE;

                SelectMenuController.Instance.ToggleReady(target, ready);
                SelectMenuController.Instance.ToggleAllReady(false);
            }
        }
    }

    public bool UsingInputs(InputHandler input) {
        return input == this.input;
    }
}
