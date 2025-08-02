using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;


public class InputManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    private int playerCount;


    private void Start()
    {
        var gamepads = Gamepad.all;
        Debug.Log("Controllers connected: " + gamepads.Count);

        //Pair controller 1 to player 1
        var p1Controller = player1.GetComponent<PlayerController>();
        var player1Controls = new PlayerControls();
        player1Controls.Player.Enable();

        var user1 = InputUser.CreateUserWithoutPairedDevices();
        user1.AssociateActionsWithUser(player1Controls);
        if (gamepads.Count > 0)
            InputUser.PerformPairingWithDevice(gamepads[0], user1);
        else
        {
            InputUser.PerformPairingWithDevice(Keyboard.current, user1);
            InputUser.PerformPairingWithDevice(Mouse.current, user1);
        }
        p1Controller.Initialize(player1Controls);

        //Pair controller 2 to player 2
        if (gamepads.Count >= 2)
        {
            var p2Controller = player2.GetComponent<PlayerController>();
            var player2Controls = new PlayerControls();
            player2Controls.Player.Enable();

            var user2 = InputUser.CreateUserWithoutPairedDevices();
            user2.AssociateActionsWithUser(player2Controls);
            InputUser.PerformPairingWithDevice(gamepads[1], user2);
            p2Controller.Initialize(player2Controls);
        }
        else
        {
            Destroy(player2);
            /*GameObject.Find("Divider").SetActive(false);
            GameObject.Find("Camera 2").SetActive(false);
            Rect r = GameObject.Find("Camera 1").GetComponent<Camera>().rect;
            r.width = 1;
            GameObject.Find("Camera 1").GetComponent<Camera>().rect = r;*/
        }
    }
}