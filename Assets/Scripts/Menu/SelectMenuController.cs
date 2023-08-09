using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMenuController : Singleton<SelectMenuController>
{
    [SerializeField] private Transform playerList;
    [SerializeField] private Transform p1Pos;
    [SerializeField] private Transform p2Pos;
    [SerializeField] private GameObject p1Ready;
    [SerializeField] private GameObject p2Ready;
    [SerializeField] private GameObject allReady;

    public Transform GetP1Pos() {
        return p1Pos;
    }

    public Transform GetP2Pos() {
        return p2Pos;
    }

    public Transform GetPlayerList() {
        return playerList;
    }

    public void ToggleReady(TargetPlayer target, bool ready) {
        switch(target) {
            case TargetPlayer.P1:
                p1Ready.SetActive(ready);
                break;
            case TargetPlayer.P2:
                p2Ready.SetActive(ready);
                break;
        }
    }

    public void ToggleAllReady(bool ready) {
        allReady.SetActive(ready);
    }
}
