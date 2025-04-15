using UnityEngine;

namespace RPG
{
	[CreateAssetMenu(menuName = "Interaction/TestInteractionStrategy")]
	public class TestInteractionStrategySO : InteractionStrategySO
	{
		public override bool Interact(IInteractable interactable, IInteractor interactor)
		{
			Debug.Log("Interacting with TestInteractable");
			Destroy(interactable.GetGO());
			return true;
		}
	}
}