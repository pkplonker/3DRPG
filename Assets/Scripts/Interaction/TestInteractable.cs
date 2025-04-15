using UnityEngine;

namespace RPG
{
	public class TestInteractable : TriggerInteractable
	{
		[SerializeField]
		private InteractionStrategySO strategy;

		public override bool Interact(IInteractor interactor) =>
			strategy != null && strategy.Interact(this, interactor);
	}
}