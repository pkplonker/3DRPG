namespace RPG
{
	public interface IInteractionStrategy
	{
		bool Interact(IInteractable interactable, IInteractor interactor);
	}
}