using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController inputController;
    public GameObj_Creature ownedCreature;
    private MainControls controls = null;
    public void MovementTick(InputAction.CallbackContext context) {
        ownedCreature.UpdateCharacterMovement(context.ReadValue<Vector2>());
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
