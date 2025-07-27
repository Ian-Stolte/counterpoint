using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;


public class InputManager : MonoBehaviour
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    private int playerCount;


    private void Start()
    {
        var gamepads = Gamepad.all;
        if (gamepads.Count < 2)
        {
            Debug.LogError("Two controllers required!");
            return;
        }

        //Pair controller 1 to player 1
        GameObject p1 = Instantiate(player1Prefab, new Vector3(-2, 1, 0), Quaternion.identity);
        var p1Controller = p1.GetComponent<Player1Controller>();
        var player1Controls = new PlayerControls();
        player1Controls.Player1.Enable();

        var user1 = InputUser.CreateUserWithoutPairedDevices();
        user1.AssociateActionsWithUser(player1Controls);
        InputUser.PerformPairingWithDevice(gamepads[0], user1);
        p1Controller.Initialize(player1Controls);

        //Pair controller 2 to player 2
        GameObject p2 = Instantiate(player2Prefab, new Vector3(2, 1, 0), Quaternion.identity);
        var p2Controller = p2.GetComponent<Player2Controller>();
        var player2Controls = new PlayerControls();
        player2Controls.Player2.Enable();

        var user2 = InputUser.CreateUserWithoutPairedDevices();
        user2.AssociateActionsWithUser(player2Controls);
        InputUser.PerformPairingWithDevice(gamepads[1], user2);
        p2Controller.Initialize(player2Controls);
    }
}