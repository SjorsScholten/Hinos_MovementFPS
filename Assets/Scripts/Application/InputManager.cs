using System;
using Character;
using FirstPersonCamera;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace Input
{
    public class InputManager : Singleton<InputManager>, Player_Controls.IPlayerActions, IMovementInput, ICameraInput
    {
        private Player_Controls m_Player_Controls;

        #region Movement Input Interface Implementation
            public Vector2 MoveInputVector { get; private set; }
            
            public event Action OnJumpPressed = delegate { };
            public event Action OnJumpReleased = delegate { };
            
            public event Action OnRunPressed = delegate { };
            public event Action OnRunReleased = delegate { };
            
            public event Action OnCrouchPressed = delegate { };
            public event Action OnCrouchReleased = delegate { };
        #endregion

        public Vector2 LookInputVector { get; private set; }

        #region Unity Runtime Methods
            private void Awake()
            {
                m_Player_Controls = new Player_Controls();
                m_Player_Controls.Player.SetCallbacks(this);
            }

            private void OnEnable()
            {
                m_Player_Controls.Player.Enable();
            }

            private void OnDisable()
            {
                m_Player_Controls.Player.Disable();
            }
        #endregion

        #region Player Actions Interface Implementation
            public void OnWalk(InputAction.CallbackContext context)
            {
                MoveInputVector = context.ReadValue<Vector2>();
            }

            public void OnLook(InputAction.CallbackContext context)
            {
                LookInputVector = context.ReadValue<Vector2>();
            }

            public void OnJump(InputAction.CallbackContext context)
            {
                if (context.started)
                    OnJumpPressed();

                if (context.canceled)
                    OnJumpReleased();
            }

            public void OnRun(InputAction.CallbackContext context)
            {
                if (context.started)
                    OnRunPressed();

                if (context.canceled)
                    OnRunReleased();
            }

            public void OnCrouch(InputAction.CallbackContext context)
            {
                if (context.started)
                    OnCrouchPressed();

                if (context.canceled)
                    OnCrouchReleased();
            }
        #endregion
    }
}
