using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GWBase;


public class InputController : MonoBehaviour
{
    public static InputController inputController;
    public GameObj_Creature ownedCreature;
    private MainControls controls = null;
    public void MovementTick(InputAction.CallbackContext context) {
        ownedCreature.UpdateCharacterMovement(context.ReadValue<Vector2>());
    }

    public void DashRequest(InputAction.CallbackContext context)
    {
        ownedCreature.RequestAction("Dash");
    }

    void Awake() {
        controls = new MainControls();
        inputController = this;
        ownedCreature = PlayerController.playerController.ownedCreature;
    }

    void Start() {
        inputController = this;
        ownedCreature = PlayerController.playerController.ownedCreature;
        controls.PlayerControls.Movement.started += MovementTick;
        controls.PlayerControls.Movement.performed += MovementTick;
        controls.PlayerControls.Movement.canceled += MovementTick;

        controls.PlayerControls.Dash.started += DashRequest;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

}
