using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;

        [Header("Speeds")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 5.5f;
        [SerializeField, Min(0f)] private float acceleration = 12f;
        [SerializeField, Min(0f)] private float deceleration = 16f;

        [Header("Gravity / Jump")]
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedGravity = -2f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.6f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float terminalVelocity = 50f;

        [Header("Input Actions (by name in Input System)")]
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string sprintActionName = "Sprint";
        [SerializeField] private string jumpActionName = "Jump";

        private InputAction _moveAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;

        private Vector2 _moveInput;
        private Vector3 _moveDirWorld;
        private float _currentSpeed;
        private float _targetSpeed;

        private bool _isSprinting;
        private Vector3 _verticalVelocity;
        
        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private float CurrentMaxSpeed => _isSprinting ? sprintSpeed : walkSpeed;

        private void Awake()
        {
            _moveAction   = InputSystem.actions.FindAction(moveActionName);
            _sprintAction = InputSystem.actions.FindAction(sprintActionName);
            _jumpAction   = InputSystem.actions.FindAction(jumpActionName);
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _sprintAction?.Enable();
            _jumpAction?.Enable();

            _sprintAction.performed += OnSprintPerformed;
            _sprintAction.canceled  += OnSprintCanceled;

            _jumpAction.performed   += OnJumpPerformed;
            _jumpAction.canceled    += OnJumpCanceled;
        }

        private void OnDisable()
        {
            _sprintAction.performed -= OnSprintPerformed;
            _sprintAction.canceled  -= OnSprintCanceled;

            _jumpAction.performed   -= OnJumpPerformed;
            _jumpAction.canceled    -= OnJumpCanceled;

            _moveAction?.Disable();
            _sprintAction?.Disable();
            _jumpAction?.Disable();
        }

        private void OnSprintPerformed(InputAction.CallbackContext _) => _isSprinting = true;
        private void OnSprintCanceled (InputAction.CallbackContext _) => _isSprinting = false;

        private void OnJumpPerformed(InputAction.CallbackContext _)
        {
            _jumpBufferTimer = jumpBuffer;
        }

        private void OnJumpCanceled(InputAction.CallbackContext _)
        {
            if (_verticalVelocity.y > 0f)
                _verticalVelocity.y *= jumpCutMultiplier;
        }

        private void Update()
        {
            ReadInput();
            ComputeWorldDirection();
            HandleSpeed();
            HandleGroundAndTimers();
            HandleJump();
            HandleGravity();
            MoveCharacter();
        }

        private void ReadInput()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
        }

        private void ComputeWorldDirection()
        {
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 camRight   = Vector3.ProjectOnPlane(cameraTransform.right,   Vector3.up).normalized;

            _moveDirWorld = camForward * _moveInput.y + camRight * _moveInput.x;
            if (_moveDirWorld.sqrMagnitude > 1e-4f)
                _moveDirWorld.Normalize();
        }

        private void HandleSpeed()
        {
            float inputMagnitude = Mathf.Clamp01(_moveInput.magnitude);
            _targetSpeed = CurrentMaxSpeed * inputMagnitude;

            float accel = (_targetSpeed > _currentSpeed) ? acceleration : deceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, accel * Time.deltaTime);
        }

        private void HandleGroundAndTimers()
        {
            if (characterController.isGrounded)
            {
                _coyoteTimer = coyoteTime;
            }
            else
            {
                _coyoteTimer -= Time.deltaTime;
            }

            if (_jumpBufferTimer > 0f)
                _jumpBufferTimer -= Time.deltaTime;
        }

        private void HandleJump()
        {
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                _verticalVelocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
                
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }
        }

        private void HandleGravity()
        {
            if (characterController.isGrounded && _verticalVelocity.y <= 0f)
            {
                _verticalVelocity.y = groundedGravity;
            }
            else
            {
                _verticalVelocity.y += gravity * Time.deltaTime;
                if (_verticalVelocity.y < -terminalVelocity)
                    _verticalVelocity.y = -terminalVelocity;
            }
        }

        private void MoveCharacter()
        {
            Vector3 horizontal = _moveDirWorld * _currentSpeed;
            Vector3 velocity   = horizontal + _verticalVelocity;

            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
