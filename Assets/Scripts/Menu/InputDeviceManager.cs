using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceManager : Singleton<InputDeviceManager>
{
    [SerializeField] private GameObject playerMenuPrefab;
    private List<PlayerTokenController> players = new();
    public void OnJoin(PlayerInput input) {
        var controller = Instantiate(playerMenuPrefab, SelectMenuController.Instance.GetPlayerList()).GetComponent<PlayerTokenController>();
        controller.SetInputs(input.transform.GetComponent<InputHandler>());
        players.Add(controller);
    }

    public void OnLeave(PlayerInput input) {
        var handler = input.transform.GetComponent<InputHandler>();
        for (int i = 0; i < players.Count; i++) {
            if (players[i].UsingInputs(handler)) {
                Destroy(players[i].gameObject);
                players.RemoveAt(i);
                i--;
            }
        }
    }

    public bool HasPlayer(TargetPlayer type) {
        foreach (var player in players)
            if (player.target == type)
                return true;
        return false;
    }

    public bool IsReady() {
        foreach (var player in players) {
            if ((player.target == TargetPlayer.P1 || player.target == TargetPlayer.P2) && !player.ready)
                return false;
        }
        return true;
    }
}