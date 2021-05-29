using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary>Tab control. Useful for windows like settings which requires several tabs.</summary>
	public class TabControl : Modal
	{
		[SerializeField] List<GameObject> tabs = new List<GameObject>();
		[SerializeField] List<Button> buttons = new List<Button>();

		int shownTab;

		protected virtual void Start()
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				var cachedIndex = i;
				buttons[i].onClick.AddListener(delegate { ShowTab(cachedIndex); });
			}

			for (int i = 1; i < tabs.Count; i++)
				tabs[i].SetActive(false);
		}

		public virtual void ShowTab(int number)
		{
			tabs[shownTab].SetActive(false);
			tabs[number].SetActive(true);

			shownTab = number;
		}

		public void AddTab(GameObject tab) => tabs.Add(tab);
		public void AddButton(Button button) => buttons.Add(button);
	}
}