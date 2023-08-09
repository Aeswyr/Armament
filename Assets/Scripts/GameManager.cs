using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private List<PlayerController> players = new List<PlayerController>();

    void Start() {
        players = new List<PlayerController>(FindObjectsOfType<PlayerController>());

        var inputs = FindObjectsOfType<InputLink>();
        foreach (var input in inputs) {
            if (input.target == TargetPlayer.P1) {
                players[0].GetComponent<InputParser>().BindInputHandler(input.transform.GetComponent<InputHandler>());
            } else if (input.target == TargetPlayer.P2) {
                players[1].GetComponent<InputParser>().BindInputHandler(input.transform.GetComponent<InputHandler>());
            }
        }
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
}
