namespace RPG
{
	public interface IInteractor
	{
		public void TriggerInteractEnter(IInteractable interactable);
		public void TriggerInteractExit(IInteractable interactable);
		public bool ShowInteractionUI => false;
	}
}