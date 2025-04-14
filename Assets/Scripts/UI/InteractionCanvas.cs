using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG.UI
{
	public class InteractionCanvas : MonoBehaviour
	{
		[SerializeField]
		private List<UIBehaviour> UIElements;

		public void Show()
		{
			SetVisability(true);
		}

		private void SetVisability(bool state)
		{
			foreach (var element in UIElements)
			{
				element.gameObject.SetActive(state);
			}
		}

		public void Hide()
		{
			SetVisability(false);
		}
	}
}