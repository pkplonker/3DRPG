using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerInput))]
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;

		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;

		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;

		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		public AudioClip LandingAudioClip;
		public AudioClip[] FootstepAudioClips;

		[Range(0, 1)]
		public float FootstepAudioVolume = 0.5f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;

		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;

		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;

		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;

		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;

		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;

		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;

		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;

		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;

		[Tooltip("For locking the camera position on all axis")]
		public bool LockCameraPosition = false;

		// cinemachine
		private float cinemachineTargetYaw;
		private float cinemachineTargetPitch;

		// player
		private float speed;
		private float animationBlend;
		private float targetRotation = 0.0f;
		private float rotationVelocity;
		private float verticalVelocity;
		private readonly float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float jumpTimeoutDelta;
		private float fallTimeoutDelta;

		// animation IDs
		private int animIDSpeed;
		private int animIDGrounded;
		private int animIDJump;
		private int animIDFreeFall;
		private int animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput playerInput;
#endif
		private Animator animator;
		private CharacterController controller;
		private InputActions input;
		private GameObject mainCamera;

		private const float THRESHOLD = 0.01f;

		private bool hasAnimator;

		private bool IsCurrentDeviceMouse => playerInput.currentControlScheme == "KeyboardMouse";

		private void Awake()
		{
			if (mainCamera == null)
			{
				mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

			hasAnimator = TryGetComponent(out animator);
			controller = GetComponent<CharacterController>();
			input = GetComponent<InputActions>();
			playerInput = GetComponent<PlayerInput>();
			AssignAnimationIDs();
			jumpTimeoutDelta = JumpTimeout;
			fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			hasAnimator = TryGetComponent(out animator);
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void AssignAnimationIDs()
		{
			animIDSpeed = Animator.StringToHash("Speed");
			animIDGrounded = Animator.StringToHash("Grounded");
			animIDJump = Animator.StringToHash("Jump");
			animIDFreeFall = Animator.StringToHash("FreeFall");
			animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
				transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
				QueryTriggerInteraction.Ignore);

			if (hasAnimator)
			{
				animator.SetBool(animIDGrounded, Grounded);
			}
		}

		private void CameraRotation()
		{
			if (input.Look.sqrMagnitude >= THRESHOLD && !LockCameraPosition)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				cinemachineTargetYaw += input.Look.x * deltaTimeMultiplier;
				cinemachineTargetPitch += input.Look.y * deltaTimeMultiplier;
			}

			cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
			cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride,
				cinemachineTargetYaw, 0.0f);
		}

		private void Move()
		{
			float targetSpeed = input.Sprint ? SprintSpeed : MoveSpeed;

			if (input.Move == Vector2.zero) targetSpeed = 0.0f;

			float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = input.AnalogMovement ? input.Move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset ||
			    currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
					Time.deltaTime * SpeedChangeRate);

				speed = Mathf.Round(speed * 1000f) / 1000f;
			}
			else
			{
				speed = targetSpeed;
			}

			animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (animationBlend < 0.01f) animationBlend = 0f;

			Vector3 inputDirection = new Vector3(input.Move.x, 0.0f, input.Move.y).normalized;

			if (input.Move != Vector2.zero)
			{
				targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
				                 mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
					RotationSmoothTime);

				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

			controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
			                new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

			if (hasAnimator)
			{
				animator.SetFloat(animIDSpeed, animationBlend);
				animator.SetFloat(animIDMotionSpeed, inputMagnitude);
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				fallTimeoutDelta = FallTimeout;

				if (hasAnimator)
				{
					animator.SetBool(animIDJump, false);
					animator.SetBool(animIDFreeFall, false);
				}

				if (verticalVelocity < 0.0f)
				{
					verticalVelocity = -2f;
				}

				if (input.Jump && jumpTimeoutDelta <= 0.0f)
				{
					verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					if (hasAnimator)
					{
						animator.SetBool(animIDJump, true);
					}
				}

				if (jumpTimeoutDelta >= 0.0f)
				{
					jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				jumpTimeoutDelta = JumpTimeout;

				if (fallTimeoutDelta >= 0.0f)
				{
					fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					if (hasAnimator)
					{
						animator.SetBool(animIDFreeFall, true);
					}
				}

				// if we are not grounded, do not jump
				input.Jump = false;
			}

			if (verticalVelocity < _terminalVelocity)
			{
				verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			Gizmos.DrawSphere(
				new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
				GroundedRadius);
		}

		private void OnFootstep(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f)
			{
				if (FootstepAudioClips.Length > 0)
				{
					var index = Random.Range(0, FootstepAudioClips.Length);
					AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center),
						FootstepAudioVolume);
				}
			}
		}

		private void OnLand(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f)
			{
				AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center),
					FootstepAudioVolume);
			}
		}
	}
}