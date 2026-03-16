public interface IMovement
{
    void SetMoveInput(float moveInput);
    void SetJumpInput(bool jumpPressed, bool jumpReleased);
    void SetDashInput(bool dashPressed);
}
