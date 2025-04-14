using UnityEngine;

namespace RPG
{
	public class TestInteractable : TriggerInteractable
	{
		public override bool Interact(IInteractor interactor)
		{
			Debug.Log("Interacting with TestInteractable");
			Destroy(gameObject);
			return true;
		}
	}
}