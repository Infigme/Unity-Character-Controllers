using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    private PlayerInput playerInput;

    [HideInInspector]
    public InputAction move, run, interact, action, aim, pause;

    private void Awake(){
        instance = this;

        playerInput = GetComponent<PlayerInput>();

        move = playerInput.actions.FindAction("move");
        run = playerInput.actions.FindAction("run");
        interact = playerInput.actions.FindAction("interact");
        action = playerInput.actions.FindAction("action");
        aim = playerInput.actions.FindAction("aim");
        pause = playerInput.actions.FindAction("pause");
    }//awake
}//class
