using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string jumpButton = "Jump";

    private IMovement movement;

    private void Awake()
    {
        movement = GetComponent<IMovement>();
    }

    private void Update()
    {
        if (movement == null)
        {
            return;
        }

        float moveInput = Input.GetAxisRaw(horizontalAxis);
        bool jumpPressed = Input.GetButtonDown(jumpButton);
        bool jumpReleased = Input.GetButtonUp(jumpButton);

        movement.SetMoveInput(moveInput);
        movement.SetJumpInput(jumpPressed, jumpReleased);
    }
}
