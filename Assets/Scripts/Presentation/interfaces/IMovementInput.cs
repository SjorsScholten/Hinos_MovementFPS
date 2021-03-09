using System;
using UnityEngine;

namespace Character
{
    public interface IMovementInput
    {
        Vector2 MoveInputVector { get; }

        event Action OnJumpPressed;
        event Action OnJumpReleased;

        event Action OnRunPressed;
        event Action OnRunReleased;

        event Action OnCrouchPressed;
        event Action OnCrouchReleased;
    }
}