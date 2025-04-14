using System;
using UnityEngine;

namespace RPG
{
	public class PlayerInteractor : MonoBehaviour, IInteractor
	{
		private IInteractable currentInteractable;
		private InputActions inputActions;

		private void Awake()
		{
			inputActions = GetComponent<InputActions>();
			inputActions.OnInteractAction += TryInteract;
		}

		private void OnDestroy()
		{
			inputActions.OnInteractAction += TryInteract;
		}

		private void TryInteract()
		{
			if (currentInteractable != null && CanInteract())
			{
				currentInteractable.Interact(this);
			}
		}

		protected virtual bool CanInteract() => true;

		public bool ShowInteractionUI => true;

		public void TriggerInteractEnter(IInteractable interactable)
		{
			currentInteractable = interactable;
		}

		public void TriggerInteractExit(IInteractable interactable)
		{
			currentInteractable = interactable;
		}
	}
}