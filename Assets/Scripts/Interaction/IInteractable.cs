
using RPG;
using UnityEngine;

public interface IInteractable
{
	bool Interact(IInteractor interactor);
	GameObject GetGO();
}