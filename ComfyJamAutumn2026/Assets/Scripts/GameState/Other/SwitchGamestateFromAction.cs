using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchGamestateFromAction : StateBasedEnabledBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private string _sActionName;
    [SerializeField] private string _sMapName;
    [SerializeField] private IGameState _targetGameState;
    protected override void Start()
    {
        base.Start();

        InputManager.InputActions.asset.FindActionMap(_sMapName).FindAction(_sActionName).performed += SwitchGamestateFromAction_performed;
    }

    private void SwitchGamestateFromAction_performed(InputAction.CallbackContext obj)
    {
        GameStateManager.Instance.RequestGameStateSwitch(_targetGameState);
    }
}
