using UnityEngine;

namespace RPG
{
	[CreateAssetMenu(menuName = "Interaction/TestInteractionStrategy")]
	public abstract class InteractionStrategySO : ScriptableObject, IInteractionStrategy
	{
		public abstract bool Interact(IInteractable interactable, IInteractor interactor);
	}
}