using TMPro;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> This class allows to show panel with info near cursor. </summary>
	public sealed class Hint : MonoBehaviour, IHideable
	{
		[SerializeField] Modal modal;
		[SerializeField] Transform canvasTransform;
		[SerializeField] TMP_Text nameText;

		RectTransform rectTransform;

		void Awake()
		{
			rectTransform = modal.SelfObject.GetComponent<RectTransform>();
		}

		void Start() => Hide();

		void Update()
		{ 
			if (modal.IsShown)
				rectTransform.anchoredPosition = Input.mousePosition;
		}

		public void ShowOnCursor(string text, Vector2 offset)
		{
			Show(text, (Vector2)Input.mousePosition + offset);
		}
		
		public void Show(string text, Vector2 position)
		{
			Show();

			rectTransform.anchoredPosition = position * canvasTransform.localScale.x;

			nameText.text = text;
		}

		public void Show() => modal.Show();
		public void Hide() => modal.Hide();
	}
}