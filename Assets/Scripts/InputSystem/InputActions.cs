using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG
{
	public class InputActions : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 Move;

		public Vector2 Look;

		public bool Jump;

		public bool Sprint;
		public bool Interact;

		[Header("Movement Settings")]
		public bool AnalogMovement;

		[Header("Mouse Cursor Settings")]
		public bool CursorLocked = true;

		public bool CursorInputForLook = true;
		public Action OnInteractAction { get; set; }

		public void OnMove(InputValue value)
		{
			Move = value.Get<Vector2>();
		}

		public void OnLook(InputValue value)
		{
			if (CursorInputForLook)
			{
				Look = value.Get<Vector2>();
			}
		}

		public void OnJump(InputValue value)
		{
			Jump = value.isPressed;
		}

		public void OnSprint(InputValue value)
		{
			Sprint = value.isPressed;
		}

		public void OnInteract(InputValue value)
		{
			Interact = value.isPressed;
			OnInteractAction?.Invoke();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			Cursor.lockState = CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}