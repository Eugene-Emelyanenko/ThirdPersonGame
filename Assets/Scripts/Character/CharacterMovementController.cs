using System;
using UnityEngine;

namespace Character
{
    public class CharacterMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterInput characterInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera characterCamera;
        
        [Header("Movement")]
        [SerializeField, Min(0f)] private float runAcceleration = 0.25f;
        [SerializeField, Min(0f)] private float runSpeed = 4f;
        [SerializeField, Min(0f)] private float drag = 0.1f;
        [SerializeField, Min(0f)] private float rotationSpeed = 5f;

        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _velocity = Vector3.zero;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            CalculateDirection();
            
            CalculateVelocity();
            
            characterController.Move(_velocity * Time.deltaTime);
            
            HandleRotation();
        }

        private void CalculateDirection()
        {
            Vector3 cameraForwardXZ = new Vector3(characterCamera.transform.forward.x, 0f, characterCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(characterCamera.transform.right.x, 0f, characterCamera.transform.right.z).normalized;

            _movementDirection = cameraRightXZ * characterInput.MovementInput.x + cameraForwardXZ * characterInput.MovementInput.y;
        }

        private void CalculateVelocity()
        {
            Vector3 movementDelta = _movementDirection * runAcceleration * Time.deltaTime;
            _velocity = characterController.velocity + movementDelta;

            Vector3 currentDrag = _velocity.normalized * drag * Time.deltaTime;
            _velocity = (_velocity.magnitude > drag * Time.deltaTime) ? _velocity - currentDrag : Vector3.zero;

            _velocity = Vector3.ClampMagnitude(_velocity, runSpeed);
        }
        
        private void HandleRotation()
        {
            Vector3 lookDir = Vector3.ProjectOnPlane(characterCamera.transform.forward, Vector3.up);
            if (lookDir.sqrMagnitude < 1e-6f)
                return;
            
            Quaternion target = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
    }
}