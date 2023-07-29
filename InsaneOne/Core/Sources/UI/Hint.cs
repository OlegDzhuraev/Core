using TMPro;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> This class allows to show panel with info near cursor. </summary>
	[RequireComponent(typeof(Panel))]
	public sealed class Hint : MonoBehaviour
	{
		public Panel SelfPanel => panel;
		
		[SerializeField] Panel panel;
		[SerializeField] Transform canvasTransform;
		[SerializeField] TMP_Text nameText;

		RectTransform rectTransform;

		void Awake()
		{
			rectTransform = panel.SelfObject.GetComponent<RectTransform>();
		}

		void Start() => Hide();

		void Update()
		{ 
			if (panel.IsShown)
				rectTransform.anchoredPosition = Input.mousePosition;
		}

		public void ShowOnCursor(string text, Vector2 offset)
		{
			Show(text, (Vector2)Input.mousePosition + offset);
		}
		
		public void Show(string text, Vector2 position)
		{
			SelfPanel.Show();

			rectTransform.anchoredPosition = position * canvasTransform.localScale.x;

			nameText.text = text;
		}

		public void Hide() => panel.Hide();
	}
}