using System;
using Input;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private Transform cameraTransform = null;
        [SerializeField, Range(0f,1f)] private float shellOffsetPercent = 0.1f;
        [SerializeField] private float groundCheckDistance = 0.01f;

        private bool m_Grounded = false, m_GroundedPreviously = false, m_Jump = false, m_JumpRequest = false;
        private Vector3 m_GroundContactNormal = Vector3.up;

        private IMovementInput m_Input;
        private Transform m_Transform;
        private Rigidbody m_Rigidbody;
        private CapsuleCollider m_Collider;

        #region Unity Runtime Methods
            private void Awake()
            {
                GetComponents();
                InitValues();
            }

            private void OnEnable()
            {
                m_Input.OnJumpPressed += OnJumpPressed;
                m_Input.OnJumpReleased += OnJumpReleased;
                
                m_Input.OnRunPressed += OnRunPressed;
                m_Input.OnRunReleased += OnRunReleased;
                
                m_Input.OnCrouchPressed += OnCrouchPressed;
                m_Input.OnCrouchReleased += OnCrouchReleased;
            }

            private void OnDisable()
            {
                m_Input.OnJumpPressed -= OnJumpPressed;
                m_Input.OnJumpReleased -= OnJumpReleased;
                
                m_Input.OnRunPressed -= OnRunPressed;
                m_Input.OnRunReleased -= OnRunReleased;
                
                m_Input.OnCrouchPressed -= OnCrouchPressed;
                m_Input.OnCrouchReleased -= OnCrouchReleased;
            }

            //private void Update() { }
            
            private void FixedUpdate()
            {
                if(cameraTransform)
                    MatchRotation(cameraTransform.rotation);
                
                CheckForGround();
                
                ProcessJump(m_Input.MoveInputVector);
                ProcessMove(m_Input.MoveInputVector);
            }
        #endregion

        #region Input Callbacks
            private void OnJumpPressed() => m_JumpRequest = true;
            private void OnJumpReleased() => m_JumpRequest = false;
            
            private void OnRunPressed() { }
            
            private void OnRunReleased() { }
            
            private void OnCrouchPressed() { }
            
            private void OnCrouchReleased() { }
        #endregion

        #region Initialization Methods
            private void GetComponents()
            {
                m_Transform = GetComponent<Transform>();
                m_Rigidbody = GetComponent<Rigidbody>();
                m_Collider = GetComponent<CapsuleCollider>();
            }

            private void InitValues()
            {
                m_Input = InputManager.Instance;
            }
        #endregion

        private void MatchRotation(Quaternion rotation) =>
            m_Rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * rotation.eulerAngles.y));

        private void ProcessMove(Vector2 inputDirection)
        {
            var targetDirection = CalculateMoveDirection(inputDirection, m_GroundContactNormal);
            var targetAcceleration = CalculateAcceleration(targetDirection);
            m_Rigidbody.AddForce(targetDirection * targetAcceleration, ForceMode.Acceleration);
        }

        private void ProcessJump(Vector2 direction)
        {
            if (m_Grounded && m_JumpRequest)
            {
                var targetDirection = new Vector3(direction.x, 1f, direction.y);
                var force = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
                m_Rigidbody.AddRelativeForce(targetDirection.normalized * force, ForceMode.VelocityChange);
            }
        }

        private Vector3 CalculateMoveDirection(Vector2 input, Vector3 groundNormal)
        {
            var direction = new Vector3(input.x, 0f, input.y);
            direction = cameraTransform.TransformDirection(direction);
            var projection = Vector3.ProjectOnPlane(direction, groundNormal);
            return projection;
        }

        private float CalculateAcceleration(Vector3 moveDirection)
        {
            var targetSpeed = GetTargetSpeedBasedOnDirection(moveDirection);
            var currentSpeed = Vector3.Scale(m_Rigidbody.velocity,Vector3.right + Vector3.forward).magnitude;
            var acceleration = (targetSpeed - currentSpeed) / Time.fixedDeltaTime;
            return acceleration;
        }

        private float GetTargetSpeedBasedOnDirection(Vector3 direction)
        {
            //TODO: set target speed based on direction
            return 5f;
        }
        
        private void CheckForGround()
        {
            var radius = m_Collider.radius;
            var position = transform.position + Vector3.up * radius;
            var checkRadius = radius * (1f - shellOffsetPercent);
            m_Grounded = Physics.SphereCast(position, checkRadius, Vector3.down, out var hitInfo, shellOffsetPercent + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            m_GroundContactNormal = (m_Grounded) ? hitInfo.normal : Vector3.up;
        }
    }
}