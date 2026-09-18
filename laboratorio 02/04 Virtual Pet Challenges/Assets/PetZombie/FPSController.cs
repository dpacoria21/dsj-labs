using UnityEngine;
using UnityEngine.InputSystem;

namespace Holistic3D 
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 4.5f;
        [SerializeField] private float _sprintSpeed = 7.5f;
        [SerializeField, Tooltip("Height (m) to reach at jump apex.")]
        private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -18f;
        [SerializeField, Tooltip("Extra downward force to keep grounded on slopes.")]
        private float _groundSnap = -4f;

        [Header("Look")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _mouseSensitivity = 0.12f;   // scales raw Look delta
        [SerializeField] private float _controllerSensitivity = 120f; // deg/sec for gamepad
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;

        [Header("Grounding")]
        [SerializeField] private float _groundCheckRadius = 0.25f;
        [SerializeField] private float _groundCheckOffset = 0.1f;
        [SerializeField] private LayerMask _groundMask = ~0; // everything by default

        // Input (polled from PlayerInput's current actions)
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;

        // State
        private CharacterController _cc;
        private float _pitch;
        private Vector3 _velocity; // y used for vertical motion
        private bool _isGrounded;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
                Debug.LogWarning("PlayerInput not found. Add a PlayerInput and assign your FPS.inputactions.");

            if (_playerCamera == null)
                Debug.LogError("Assign the player Camera in the inspector.");

            CacheActions();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            EnableActions(true);
        }

        private void OnDisable()
        {
            EnableActions(false);
        }

        private void Update()
        {
            ReadGrounding();
            LookUpdate();
            MoveUpdate();
        }

        private void CacheActions()
        {
            if (_playerInput == null || _playerInput.actions == null) return;
            var map = _playerInput.actions.FindActionMap("Gameplay", throwIfNotFound: false);
            if (map == null)
            {
                Debug.LogWarning("Gameplay action map not found. Check your Input Actions asset.");
                return;
            }
            _moveAction   = map.FindAction("Move",   throwIfNotFound: false);
            _lookAction   = map.FindAction("Look",   throwIfNotFound: false);
            _jumpAction   = map.FindAction("Jump",   throwIfNotFound: false);
            _sprintAction = map.FindAction("Sprint", throwIfNotFound: false);
        }

        private void EnableActions(bool enable)
        {
            if (_moveAction   != null) { if (enable) _moveAction.Enable();   else _moveAction.Disable(); }
            if (_lookAction   != null) { if (enable) _lookAction.Enable();   else _lookAction.Disable(); }
            if (_jumpAction   != null) { if (enable) _jumpAction.Enable();   else _jumpAction.Disable(); }
            if (_sprintAction != null) { if (enable) _sprintAction.Enable(); else _sprintAction.Disable(); }
        }

        private void ReadGrounding()
        {
            // CharacterController.isGrounded is OK, but a small sphere check is more reliable on uneven ground
            Vector3 checkPos = transform.position + Vector3.down * (_cc.height * 0.5f - _cc.radius + _groundCheckOffset);
            _isGrounded = Physics.CheckSphere(checkPos, _groundCheckRadius, _groundMask, QueryTriggerInteraction.Ignore);

            if (_isGrounded && _velocity.y < 0f)
                _velocity.y = _groundSnap; // keeps us glued to the floor on gentle slopes
        }

        private void LookUpdate()
        {
            if (_playerCamera == null) return;

            Vector2 look = _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;

            // Mouse provides delta; gamepad expects time-scaled degrees
            bool usingGamepad = Gamepad.current != null && Gamepad.current.rightStick.ReadValue() != Vector2.zero;

            if (usingGamepad)
            {
                // rightStick is -1..1; scale to deg/sec
                Vector2 stick = look * _controllerSensitivity * Time.deltaTime;
                _pitch -= stick.y;
                transform.Rotate(0f, stick.x, 0f, Space.Self);
            }
            else
            {
                Vector2 mouse = look * _mouseSensitivity; // already a delta
                _pitch -= mouse.y;
                transform.Rotate(0f, mouse.x, 0f, Space.Self);
            }

            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            _playerCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void MoveUpdate()
        {
            Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            bool sprinting = _sprintAction != null && _sprintAction.IsPressed();

            float speed = sprinting ? _sprintSpeed : _walkSpeed;

            // Convert input to world space along the player's yaw
            Vector3 planeforward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 planeright   = Vector3.ProjectOnPlane(transform.right,   Vector3.up).normalized;
            Vector3 move = (planeforward * moveInput.y + planeright * moveInput.x) * speed;

            // Jump
            if (_isGrounded && _jumpAction != null && _jumpAction.WasPressedThisFrame())
            {
                // v = sqrt(2 g h)  (g is positive magnitude)
                float vJump = Mathf.Sqrt(Mathf.Abs(_gravity) * 2f * Mathf.Max(0.01f, _jumpHeight));
                _velocity.y = vJump;
            }

            // Gravity
            _velocity.y += _gravity * Time.deltaTime;

            // Compose and move
            Vector3 displacement = move * Time.deltaTime + new Vector3(0f, _velocity.y, 0f) * Time.deltaTime;
            _cc.Move(displacement);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_cc == null) return;
            Gizmos.color = Color.cyan;
            Vector3 checkPos = transform.position + Vector3.down * (_cc.height * 0.5f - _cc.radius + _groundCheckOffset);
            Gizmos.DrawWireSphere(checkPos, _groundCheckRadius);
        }
#endif
    }
}
