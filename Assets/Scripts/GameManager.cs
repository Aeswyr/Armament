using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private List<PlayerController> players = new List<PlayerController>();

    void Start() {
        players = new List<PlayerController>(FindObjectsOfType<PlayerController>());
    }
    void FixedUpdate()
    {
        FixedPhysics.Update();
    }

    public PlayerController GetOtherPlayer(PlayerController primary) {
        foreach (var player in players) {
            if (player != primary)
                return player;
        }
        return null;
    }

    public void ChangeMomentumBar(int amt, PlayerController primary) { 
        primary.AdjustMomentumBar(amt);
        GetOtherPlayer(primary).AdjustMomentumBar(-amt);
    }

    public void ResetMomentum(PlayerController primary) {
        primary.ResetMomentum();
        GetOtherPlayer(primary).ResetMomentum();
    }
}
