using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCommandController : MonoBehaviour
{
    private static readonly List<UIState> activeUIStates = new() { UIState.HUD, UIState.Command };
    private static readonly List<PlayerState> activePlayerStates = new() { PlayerState.SteeringWheel, PlayerState.FirstPerson, PlayerState.Cannon, PlayerState.Throttle, PlayerState.Command };

    private InputSystem_Actions input;
    private PlayerAdmiralController admiralController;

    private void Awake()
    {
        input = new();
        input.Player.FirstCommand.performed += FirstCommand_performed;
        input.Player.SecondCommand.performed += SecondCommand_performed;
        input.Player.ThirdCommand.performed += ThirdCommand_performed;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void Start()
    {
        admiralController = PlayerBoatController.Instance.AdmiralController;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        input.Player.FirstCommand.performed -= FirstCommand_performed;
        input.Player.SecondCommand.performed -= SecondCommand_performed;
        input.Player.ThirdCommand.performed -= ThirdCommand_performed;

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        TryEnableInput();
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        TryEnableInput();
    }

    private void TryEnableInput()
    {
        if (activeUIStates.Contains(UIManager.Instance.State) && activePlayerStates.Contains(FirstPersonController.Instance.State))
        {
            input.Player.Enable();
        }

        else
        {
            input.Player.Disable();
        }
    }

    private void FirstCommand_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        admiralController.SetCommandForSubordinates(Command.Formation);
    }
    private void SecondCommand_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        admiralController.SetCommandForSubordinates(Command.Hold);
    }

    private void ThirdCommand_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        admiralController.SetCommandForSubordinates(Command.Charge);
    }
}
