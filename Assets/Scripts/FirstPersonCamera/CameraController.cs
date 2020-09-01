using System;
using Input;
using UnityEngine;

namespace FirstPersonCamera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float verticalCameraSpeed = 15.0f, horizontalCameraSpeed = 15.0f;
        [SerializeField] private float minPitch = -90f, maxPitch = 90f;
        [SerializeField] private Transform cameraTransform = null;
        
        private Vector2 m_LookVectorInput;
        private Quaternion m_CameraTargetRotation;

        private float m_Pitch = 0f, m_Jaw = 0f;

        private ICameraInput m_Input;

        private void Awake()
        {
            GetComponents();
            InitValues();
        }

        private void Update()
        {
            RotateUsingEulerAngles(m_Input.LookInputVector);
        }

        private void GetComponents()
        {
            if (!cameraTransform)
                cameraTransform = Camera.main.transform;
        }

        private void InitValues()
        {
            m_Input = InputManager.Instance;
            m_CameraTargetRotation = cameraTransform.localRotation;
        }
        
        private void RotateUsingEulerAngles(Vector2 direction)
        {
            m_Jaw += direction.x * horizontalCameraSpeed * Time.deltaTime;
            m_Pitch -= direction.y * verticalCameraSpeed * Time.deltaTime;
            m_Pitch = Mathf.Clamp(m_Pitch, minPitch, maxPitch);
            cameraTransform.localRotation = Quaternion.Euler(m_Pitch, m_Jaw, 0f);
        }
    }
}