using System;
using System.Collections.Generic;
using System.Linq;
using RPG.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RPG
{
	[RequireComponent(typeof(SphereCollider))]
	public abstract class TriggerInteractable : MonoBehaviour, IInteractable
	{
		private readonly HashSet<IInteractor> interactorsInRange = new();

		[SerializeField]
		private InteractionCanvas Canvas;

		[SerializeField]
		private float Range = 1.5f;

		private void Awake()
		{
			var collider = GetComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = Range;
			ValidateCanvasState();
		}

		private void OnValidate()
		{
			Canvas = GetComponentInChildren<InteractionCanvas>();
		}

		protected virtual void OnTriggerEnter(Collider collider)
		{
			if (collider.TryGetComponent<IInteractor>(out var interactor))
			{
				interactor.TriggerInteractEnter(this);
				interactorsInRange.Add(interactor);
			}

			ValidateCanvasState();
		}

		private void ValidateCanvasState()
		{
			if (interactorsInRange.Any(x => x.ShowInteractionUI) && Canvas != null)
			{
				Canvas.Show();
			}
			else
			{
				Canvas.Hide();
			}
		}

		protected virtual void OnTriggerExit(Collider collider)
		{
			if (collider.TryGetComponent<IInteractor>(out var interactor))
			{
				interactor.TriggerInteractExit(this);
				interactorsInRange.Remove(interactor);
			}

			ValidateCanvasState();
		}

		public abstract bool Interact(IInteractor interactor);
		public GameObject GetGO() => gameObject;
	}
}