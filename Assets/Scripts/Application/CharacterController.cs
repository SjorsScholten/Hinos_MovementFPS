using System;
using Character;
using Input;
using UnityEngine;
using Character = Domain.Character;

namespace Application
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        private Domain.Character m_Character = new Domain.Character();
        
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private Transform cameraTransform = null;

        [Serializable]
        private class GroundCheckSettings
        {
            [SerializeField, Range(0f,1f)] private float shellOffsetPercent = 0.1f;
            [SerializeField] private float groundCheckDistance = 0.01f;

            public bool Grounded { get; private set; } = false;
            public bool GroundedPreviously { get; private set; } = false;
            public Vector3 GroundContactNormal { get; private set; } = Vector3.up;
        
            public void CheckForGround(Vector3 position, float radius)
            {
                Grounded = Physics.SphereCast(position, radius * (1f - shellOffsetPercent), Vector3.down, out var hitInfo, radius + shellOffsetPercent + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                GroundContactNormal = (Grounded) ? hitInfo.normal : Vector3.up;
            }
        }

        [Serializable]
        private class WallCheckSettings
        {
            [SerializeField] private LayerMask wallMask = Physics.AllLayers;
            [SerializeField] private float wallCheckRadius = 0.1f;
            [SerializeField, Range(0f, 1f)] private float sector = 0.5f;
            
            public Collider[] WallsAroundPlayer { get; private set; } = new Collider[4];

            private Collider wallFront, wallLeft, wallRight, wallBack;

            private Collider t_Wall = null;
            
            public void CheckForWall(Transform entity, Vector3 offset, float radius)
            {
                //TODO: allocate memory for overal sphere
                WallsAroundPlayer = Physics.OverlapSphere(entity.position + offset, radius + wallCheckRadius, wallMask, QueryTriggerInteraction.Ignore);

                for (int i = 0; i < WallsAroundPlayer.Length; i++)
                {
                    t_Wall = WallsAroundPlayer[i];
                    var wallDirection = t_Wall.transform.position - entity.position;
                    wallDirection.y = 0;
                    
                    var dotOrientationZ = Vector3.Dot(entity.forward, wallDirection);
                    var dotOrientationX = Vector3.Dot(entity.right, wallDirection);

                    wallFront = (dotOrientationZ > sector)? t_Wall : null;
                    wallBack = (dotOrientationZ < -sector)? t_Wall : null;
                    wallRight = (dotOrientationX > 1 - sector)? t_Wall : null;
                    wallLeft = (dotOrientationX > -1 + sector)? t_Wall : null;
                }
            }

            public void DebugWall(Vector3 position, float radius)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, radius + wallCheckRadius);
            }
        }
        
        [SerializeField] private GroundCheckSettings groundCheckSettings = new GroundCheckSettings();
        [SerializeField] private WallCheckSettings wallCheckSettings = new WallCheckSettings();

        private bool m_Jump = false, m_JumpRequest = false;
        public float speedSmoothTime = 0.2f;
        private float m_SpeedSmoothRefVelocity;

        private Vector3 m_CameraDirection;

        private Vector3 m_MoveDirection = Vector3.zero;
        private float m_CurrentSpeed = 0f, m_TargetSpeed = 0f, m_Acceleration = 0f;

        private IMovementInput m_Input;
        private Transform m_Transform;
        private Rigidbody m_Rigidbody;
        private CapsuleCollider m_Collider;
        private Ray m_Ray = new Ray();

        private Vector3 WorldColliderCenter => m_Transform.position + m_Collider.center;
        private Vector2 MoveInput => m_Input.MoveInputVector;

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
                if (cameraTransform)
                {
                    MatchRotation(cameraTransform.rotation);
                }

                m_CameraDirection = GetCameraDirection(MoveInput);
                
                //Update Ray
                m_Ray.origin = WorldColliderCenter;
                m_Ray.direction = m_CameraDirection;

                CheckForGround();
                CheckForWall();
                
                ProcessJump();
                ProcessMove();
            }

            private void OnDrawGizmos()
            {
                if (m_Collider)
                {
                    Gizmos.DrawRay(WorldColliderCenter, m_CameraDirection);
                    wallCheckSettings.DebugWall(WorldColliderCenter, m_Collider.radius);
                }
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

        private void MatchRotation(Quaternion rotation) => m_Rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * rotation.eulerAngles.y));

        private void ProcessMove()
        {
            CalculateMoveDirection();
            CalculateAcceleration(m_MoveDirection);
            m_Rigidbody.AddForce(m_MoveDirection * m_Acceleration, ForceMode.Acceleration);
        }

        private void ProcessJump()
        {
            if (!groundCheckSettings.Grounded) return;
            
            if (m_JumpRequest)
            {
                var targetDirection = m_CameraDirection + Vector3.up;
                var force = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
                m_Rigidbody.AddForce(targetDirection.normalized * force, ForceMode.VelocityChange);
            }
        }

        private void CalculateMoveDirection()
        {
            var projection = Vector3.zero;
            //projection += Vector3.ProjectOnPlane(m_CameraDirection, wallCheckSettings.WallNormal(m_Ray));
            projection += Vector3.ProjectOnPlane(m_CameraDirection, groundCheckSettings.GroundContactNormal);
            m_MoveDirection = projection.normalized;
        }

        //TODO: Move to Camera Domain as property, Camera controller can move the camera
        private Vector3 GetCameraDirection(Vector2 input)
        {
            var direction = cameraTransform.TransformDirection(new Vector3(input.x, 0f, input.y));
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            return direction.normalized;
        }

        private void CalculateAcceleration(Vector3 moveDirection)
        {
            m_CurrentSpeed = Vector3.Scale(m_Rigidbody.velocity,Vector3.right + Vector3.forward).magnitude;
            
            m_TargetSpeed = GetTargetSpeedBasedOnDirection(moveDirection);
            m_TargetSpeed = Mathf.SmoothDamp(m_CurrentSpeed, m_TargetSpeed, ref m_SpeedSmoothRefVelocity, speedSmoothTime);
            
            m_Acceleration = m_CurrentSpeed > m_TargetSpeed ? 0f : (m_TargetSpeed - m_CurrentSpeed) / Time.fixedDeltaTime;
        }

        private float GetTargetSpeedBasedOnDirection(Vector3 direction)
        {
            //TODO: set target speed based on direction
            return 5f;
        }

        private void CheckForGround() => groundCheckSettings.CheckForGround(WorldColliderCenter, m_Collider.radius);
        private void CheckForWall() => wallCheckSettings.CheckForWall(m_Transform, m_Collider.center, m_Collider.radius);
    }
}