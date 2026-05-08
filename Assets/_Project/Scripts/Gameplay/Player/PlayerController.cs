using UnityEngine;

namespace LibraryGame.Gameplay.Player
{
    /// <summary>
    /// Touchscreen first-person walker. Two virtual sticks fed by TouchHud:
    ///  - moveInput: x = strafe, y = forward
    ///  - lookInput: x = yaw, y = pitch (delta-style)
    /// Has its own CharacterController for collision.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 3.0f;
        public float lookSpeed = 0.25f;       // degrees per pixel of drag
        public float gravity = -9.81f;
        public Vector3 startPosition = new(0f, 0.1f, -4f);
        public float pitchMin = -75f;
        public float pitchMax = 75f;

        public Transform CameraMount { get; private set; }

        private CharacterController _cc;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;

        public Vector2 MoveInput { set => _moveInput = value; }
        public Vector2 LookInput { set => _lookInput = value; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.height = 1.7f;
            _cc.radius = 0.3f;
            _cc.center = new Vector3(0, 0.85f, 0);

            var mount = new GameObject("CameraMount");
            mount.transform.SetParent(transform, false);
            mount.transform.localPosition = new Vector3(0, 1.6f, 0);
            CameraMount = mount.transform;

            Teleport(startPosition);
        }

        private void Update()
        {
            // Look (delta consumed each frame)
            _yaw += _lookInput.x * lookSpeed;
            _pitch -= _lookInput.y * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
            _lookInput = Vector2.zero;

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            CameraMount.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            // Move
            var forward = transform.forward;
            var right = transform.right;
            var move = (right * _moveInput.x + forward * _moveInput.y) * moveSpeed;

            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _cc.Move(move * Time.deltaTime);
        }

        public void Teleport(Vector3 worldPos)
        {
            _cc.enabled = false;
            transform.position = worldPos;
            _cc.enabled = true;
        }
    }
}
